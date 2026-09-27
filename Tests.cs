using System;

namespace CarsApp
{
    // Simple test runner: dotnet run -- test
    // Test names refer to the test plan in the design document (chapter 13, T-01..T-28).
    public class Tests
    {
        private static int passed = 0;
        private static int failed = 0;

        public static void Check(bool condition, string testName)
        {
            if (condition)
            {
                passed++;
                Console.WriteLine("PASS: " + testName);
            }
            else
            {
                failed++;
                Console.WriteLine("FAIL: " + testName);
            }
        }

        public static void RunAll()
        {
            passed = 0;
            failed = 0;

            RunInfrastructureTests();
            RunRegisterTests();
            RunConfirmOrderTests();

            Console.WriteLine();
            Console.WriteLine("Passed: " + passed + ", Failed: " + failed);
        }

        // Helper: a car in the given dealership
        public static Car MakeCar(CarDealerShipSystem system, string license, double price, string dealType, CarDealership dealership)
        {
            return new Car(system.GetNextCarId(), "Sedan", "Toyota", "Corolla", 2024, 1000, license, price, dealType, "Haifa", dealership);
        }

        // VFDN-133: classes and system skeleton (design chapters 6 and 12)
        private static void RunInfrastructureTests()
        {
            Console.WriteLine("--- Infrastructure (VFDN-133) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            Check(system.GetUserCount() == 0 && system.GetCarCount() == 0 && system.GetOrderCount() == 0,
                  "constructor creates no users, cars or orders");
            Check(system.GetCurrentUser() == null, "no user is logged in at start");

            CarDealership haifa = system.FindDealershipById(1);
            Check(haifa != null && haifa.GetName() == "Toyota Haifa" && haifa.GetDealershipType() == "Cars", "seed dealership 1");
            Check(system.FindDealershipById(5) != null && system.FindDealershipById(5).GetDealershipType() == "Motorcycles", "seed dealership 5 is motorcycles");
            Check(system.FindDealershipById(6) == null, "exactly 5 seed dealerships");
            Check(!haifa.HasOwner(), "seed dealerships start without a manager");

            Car car = MakeCar(system, "1234567", 100000, "Both", haifa);
            Check(car.IsAvailable(), "new car is Available");
            Check(car.SupportsDealType("Sale") && car.SupportsDealType("Rental"), "Both supports Sale and Rental");
            Check(!car.MarkAsSold() && car.IsAvailable(), "Available cannot go straight to Sold");
            Check(car.MarkAsReserved() && car.GetStatus() == "Reserved", "Available -> Reserved");
            Check(car.MarkAsSold() && car.GetStatus() == "Sold", "Reserved -> Sold");
            Check(!car.MakeAvailable() && car.GetStatus() == "Sold", "T-25 MakeAvailable on a Sold car fails");
            Check(!car.SetPrice(0) && !car.SetPrice(-5) && car.GetPrice() == 100000, "T-09 price <= 0 is rejected");

            User customer = new User(1, "dana", "Dana_123", "0521234567", "dana@mail.com", "Customer");
            Order order = new Order(1, customer, "Sale");
            Check(order.IsPending() && order.GetDealership() == null, "new order is Pending with no cars");
            order.AddCar(MakeCar(system, "1111111", 100000, "Sale", haifa));
            order.AddCar(MakeCar(system, "2222222", 50000, "Sale", haifa));
            order.AddCar(MakeCar(system, "3333333", 25000, "Sale", haifa));
            Check(!order.AddCar(MakeCar(system, "4444444", 1, "Sale", haifa)), "an order holds at most 3 cars");
            Check(order.GetTotalPrice() == 175000, "order total price");
            Check(order.GetDealership() == haifa && order.BelongsTo(customer), "order -> dealership and customer");
            Check(order.Approve() && !order.Cancel() && order.GetStatus() == "Approved", "Approved is final");
        }

        // VFDN-93: REQ-001 registration (design 7.1)
        private static void RunRegisterTests()
        {
            Console.WriteLine("--- REQ-001 Register (VFDN-93) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            Check(system.IsStrongPassword("Dana_123"), "password with a digit and _ is strong");
            Check(!system.IsStrongPassword("Dana1234"), "password without a special character is weak");
            Check(!system.IsStrongPassword("Dana$abc"), "password without a digit is weak");
            Check(!system.IsStrongPassword("Da_1"), "short password is weak");
            Check(system.IsValidEmail("dana@mail.com") && !system.IsValidEmail("dana.mail.com") && !system.IsValidEmail("dana@mail"),
                  "email needs @ and a dot after it");
            Check(system.IsValidPhone("0521234567") && !system.IsValidPhone("0421234567") && !system.IsValidPhone("05212"),
                  "phone: 10 digits starting with 05");

            Check(system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null), "customer registers");
            User dana = system.FindUserByUsername("dana");
            Check(dana != null && dana.IsCustomer() && dana.GetDealership() == null, "customer has no dealership");
            Check(dana.GetId() == 1, "first user gets id 1");

            int count = system.GetUserCount();
            Check(!system.TryCreateUser("dana", "Dana_123", "other@mail.com", "0521234567", "Customer", null) && system.GetUserCount() == count,
                  "T-01 taken username is rejected");
            Check(!system.TryCreateUser("a1", "weak", "a1@mail.com", "0521234567", "Customer", null)
                  && !system.TryCreateUser("a2", "Dana_123", "bad-email", "0521234567", "Customer", null)
                  && !system.TryCreateUser("a3", "Dana_123", "a3@mail.com", "123", "Customer", null)
                  && system.GetUserCount() == count,
                  "T-02 weak password / bad email / bad phone create no user");
            Check(!system.TryCreateUser("a4", "Dana_123", "a4@mail.com", "0521234567", "Salesperson", system.FindDealershipById(1)),
                  "salesperson cannot self-register");

            CarDealership haifa = system.FindDealershipById(1);
            Check(system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa), "manager registers to a free dealership");
            User boss = system.FindUserByUsername("boss");
            Check(boss.IsManager() && boss.GetDealership() == haifa && haifa.IsOwner(boss), "manager and dealership are linked both ways");
            Check(!system.TryCreateUser("boss2", "Boss_123", "boss2@cars.com", "0502222222", "Manager", haifa), "dealership that has a manager is rejected");

            for (int i = 2; i <= 5; i++)
            {
                system.TryCreateUser("m" + i, "Boss_123", "m" + i + "@cars.com", "0500000000", "Manager", system.FindDealershipById(i));
            }
            count = system.GetUserCount();
            Check(!system.TryCreateUser("m6", "Boss_123", "m6@cars.com", "0500000000", "Manager", system.FindDealershipById(1))
                  && system.GetUserCount() == count,
                  "T-03 manager registration when all 5 dealerships are taken creates no user");

            CarDealerShipSystem full = new CarDealerShipSystem();
            for (int i = 0; i < CarDealerShipSystem.USERS_MAX; i++)
            {
                full.TryCreateUser("u" + i, "Pass_123", "u" + i + "@m.com", "0500000000", "Customer", null);
            }
            Check(!full.HasFreeUserSlot() && !full.TryCreateUser("last", "Pass_123", "last@m.com", "0500000000", "Customer", null),
                  "registration fails when the users array is full");
        }

        // Helper: a customer registered in the system
        public static User MakeCustomer(CarDealerShipSystem system, string username)
        {
            system.TryCreateUser(username, "Pass_123", username + "@mail.com", "0521234567", "Customer", null);
            return system.FindUserByUsername(username);
        }

        // Helper: a salesperson of the dealership. Built directly because AddSalesperson (REQ-014) is on another branch.
        public static User MakeSalesperson(string username, CarDealership dealership)
        {
            User salesperson = new User(900, username, "Pass_123", "0521234567", username + "@cars.com", "Salesperson");
            salesperson.SetDealership(dealership);
            return salesperson;
        }

        // Helper: a car added to the system inventory; returns its id
        public static int AddCar(CarDealerShipSystem system, string license, string dealType, CarDealership dealership)
        {
            Car car = MakeCar(system, license, 100000, dealType, dealership);
            system.AddCarToInventory(car);
            return car.GetId();
        }

        // VFDN-104: REQ-011 place an order (design 7.5)
        private static void RunConfirmOrderTests()
        {
            Console.WriteLine("--- REQ-011 Confirm order (VFDN-104) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            User dana = MakeCustomer(system, "dana");
            int sale1 = AddCar(system, "1000001", "Sale", haifa);
            int sale2 = AddCar(system, "1000002", "Sale", haifa);
            int both1 = AddCar(system, "1000003", "Both", haifa);
            int rent1 = AddCar(system, "1000004", "Rental", haifa);
            int rent2 = AddCar(system, "1000005", "Rental", haifa);
            int rent3 = AddCar(system, "1000006", "Rental", haifa);
            int rentTa = AddCar(system, "2000001", "Rental", telAviv);

            Check(system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale1 }, 1), "customer places a purchase order");
            Order first = system.FindOrderByNumber(1);
            Check(first != null && first.IsPending() && first.BelongsTo(dana) && first.GetCarCount() == 1,
                  "new order is Pending and belongs to the customer");
            Check(system.FindCarById(sale1).GetStatus() == "Reserved", "ordered car is Reserved, not Sold");
            Check(system.CountActivePurchaseCars(dana) == 1 && system.GetNextOrderNumber() == 2, "purchase counter and order number");

            User noa = MakeCustomer(system, "noa");
            int orders = system.GetOrderCount();
            Check(!system.TryConfirmOrder(noa, noa, "Sale", new int[] { sale1 }, 1) && system.GetOrderCount() == orders,
                  "T-10 a car that is not Available is not added and no order is created");

            Check(!system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale2 }, 1)
                  && system.FindCarById(sale2).IsAvailable() && system.GetOrderCount() == orders,
                  "T-11 a second active purchase is blocked and no car is Reserved");

            Check(!system.TryConfirmOrder(noa, noa, "Sale", new int[] { sale2, both1 }, 2), "a purchase is always one car");
            Check(!system.TryConfirmOrder(noa, noa, "Lease", new int[] { rent1 }, 1), "unknown order type is rejected");
            Check(!system.TryConfirmOrder(noa, noa, "Rental", new int[] { sale2 }, 1) && system.FindCarById(sale2).IsAvailable(),
                  "T-24 a Sale-only car cannot join a rental order");

            Check(system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent1, rentTa }, 2), "T-23 order with cars from two dealerships is created");
            Order mixed = system.FindOrderByNumber(2);
            Check(mixed.GetCarCount() == 1 && mixed.GetDealership() == haifa && system.FindCarById(rentTa).IsAvailable(),
                  "T-23 only the first dealership's car is in the order; the other stays Available");

            Check(system.TryConfirmOrder(noa, noa, "Rental", new int[] { both1 }, 1) && system.CountActiveRentalCars(noa) == 2,
                  "a Both car can be rented; rental counter counts cars");
            orders = system.GetOrderCount();
            Check(!system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent2, rent3 }, 2)
                  && system.FindCarById(rent2).IsAvailable() && system.GetOrderCount() == orders,
                  "T-27 2 active rentals + 2 more is blocked before any car is chosen");
            Check(system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent2 }, 1) && system.CountActiveRentalCars(noa) == 3,
                  "2 active rentals + 1 more is allowed (exactly 3)");
            orders = system.GetOrderCount();
            Check(!system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent3 }, 1) && system.GetOrderCount() == orders,
                  "T-12 a fourth rented car is blocked");

            User omer = MakeCustomer(system, "omer");
            Check(!system.TryConfirmOrder(dana, omer, "Rental", new int[] { rent3 }, 1), "a customer cannot order for another customer");
            Check(!system.TryConfirmOrder(omer, omer, "Rental", new int[] { 999 }, 1), "unknown car id creates no order");

            User seller = MakeSalesperson("seller", telAviv);
            orders = system.GetOrderCount();
            Check(!system.TryConfirmOrder(seller, omer, "Rental", new int[] { rent3 }, 1)
                  && system.FindCarById(rent3).IsAvailable() && system.GetOrderCount() == orders,
                  "T-28 salesperson cannot order a car of another dealership");
            Check(system.TryConfirmOrder(seller, omer, "Rental", new int[] { rentTa }, 1)
                  && system.FindOrderByNumber(5).BelongsTo(omer),
                  "salesperson orders for a customer from his own dealership");
            Check(!system.TryConfirmOrder(seller, seller, "Rental", new int[] { rent3 }, 1), "an order is only for a customer");

            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            User boss = system.FindUserByUsername("boss");
            Check(!system.TryConfirmOrder(boss, omer, "Rental", new int[] { rent3 }, 1), "a manager cannot place orders");
        }
    }
}
