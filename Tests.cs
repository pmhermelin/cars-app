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
            RunCancelOrderTests();
            RunApproveRejectTests();
            RunSoldOrRentedTests();
            RunCustomerOrdersTests();

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

        // VFDN-98: REQ-004 cancel an order (design 7.6)
        private static void RunCancelOrderTests()
        {
            Console.WriteLine("--- REQ-004 Cancel order (VFDN-98) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            User dana = MakeCustomer(system, "dana");
            User noa = MakeCustomer(system, "noa");
            int rent1 = AddCar(system, "1000001", "Rental", haifa);
            int rent2 = AddCar(system, "1000002", "Rental", haifa);
            int sale1 = AddCar(system, "1000003", "Sale", haifa);
            system.TryConfirmOrder(dana, dana, "Rental", new int[] { rent1, rent2 }, 2);

            Check(!system.TryCancelOrder(noa, 1) && system.FindOrderByNumber(1).IsPending()
                  && system.FindCarById(rent1).GetStatus() == "Reserved",
                  "T-14 a customer cannot cancel another customer's order");
            Check(!system.TryCancelOrder(dana, 99), "unknown order number is rejected");

            Check(system.TryCancelOrder(dana, 1) && system.FindOrderByNumber(1).GetStatus() == "Cancelled",
                  "customer cancels a Pending order");
            Check(system.FindCarById(rent1).IsAvailable() && system.FindCarById(rent2).IsAvailable(),
                  "all the cars of a cancelled order are Available again");
            Check(system.CountActiveRentalCars(dana) == 0, "a cancelled order is not active");
            Check(!system.TryCancelOrder(dana, 1), "a cancelled order cannot be cancelled again");

            system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale1 }, 1);
            Order approved = system.FindOrderByNumber(2);
            approved.Approve();
            system.FindCarById(sale1).MarkAsSold();
            Check(!system.TryCancelOrder(dana, 2) && approved.GetStatus() == "Approved"
                  && system.FindCarById(sale1).GetStatus() == "Sold",
                  "T-13 an Approved order cannot be cancelled and its car stays Sold");
        }

        // VFDN-99: REQ-005 approve or reject an order (design 7.7-7.8)
        private static void RunApproveRejectTests()
        {
            Console.WriteLine("--- REQ-005 Approve / reject (VFDN-99) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            system.TryCreateUser("boss2", "Boss_123", "boss2@cars.com", "0502222222", "Manager", telAviv);
            User boss = system.FindUserByUsername("boss");
            User otherBoss = system.FindUserByUsername("boss2");
            User dana = MakeCustomer(system, "dana");
            User noa = MakeCustomer(system, "noa");
            int sale1 = AddCar(system, "1000001", "Sale", haifa);
            int rent1 = AddCar(system, "1000002", "Rental", haifa);
            int rent2 = AddCar(system, "1000003", "Both", haifa);
            int rent3 = AddCar(system, "1000004", "Rental", haifa);
            system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale1 }, 1);
            system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent1, rent2 }, 2);
            system.TryConfirmOrder(dana, dana, "Rental", new int[] { rent3 }, 1);

            User seller = MakeSalesperson("seller", haifa);
            Check(!system.TryApproveOrder(seller, 1) && !system.TryRejectOrder(seller, 1)
                  && system.FindOrderByNumber(1).IsPending() && system.FindCarById(sale1).GetStatus() == "Reserved",
                  "T-17 a salesperson cannot approve or reject");
            Check(!system.TryApproveOrder(dana, 1), "a customer cannot approve");
            Check(!system.TryApproveOrder(otherBoss, 1) && !system.TryRejectOrder(otherBoss, 1)
                  && system.FindOrderByNumber(1).IsPending(),
                  "a manager of another dealership cannot approve or reject");
            Check(system.PrintPendingOrdersOfDealership(otherBoss) == 0, "orders of another dealership are not shown to a manager");
            Check(!system.TryApproveOrder(boss, 99), "unknown order number is rejected");

            Check(system.TryApproveOrder(boss, 1) && system.FindOrderByNumber(1).GetStatus() == "Approved"
                  && system.FindCarById(sale1).GetStatus() == "Sold",
                  "T-15 approving a purchase: order Approved and its car Sold");
            Check(system.TryApproveOrder(boss, 2) && system.FindCarById(rent1).GetStatus() == "Rented"
                  && system.FindCarById(rent2).GetStatus() == "Rented",
                  "approving a rental: every car in the order is Rented, none stays Reserved");
            Check(!system.TryApproveOrder(boss, 1) && !system.TryRejectOrder(boss, 1)
                  && system.FindCarById(sale1).GetStatus() == "Sold",
                  "an Approved order cannot be approved or rejected again; cars unchanged");

            Check(system.TryRejectOrder(boss, 3) && system.FindOrderByNumber(3).GetStatus() == "Rejected"
                  && system.FindCarById(rent3).IsAvailable(),
                  "T-16 rejecting: order Rejected and its car Available again");
            Check(!system.TryApproveOrder(boss, 3) && system.FindCarById(rent3).IsAvailable(),
                  "a Rejected order cannot be approved; car stays Available");
            Check(system.PrintPendingOrdersOfDealership(boss) == 0, "no Pending orders are left");
        }

        // VFDN-102: REQ-008 sold or rented cars (design 7.11)
        private static void RunSoldOrRentedTests()
        {
            Console.WriteLine("--- REQ-008 Sold or rented cars (VFDN-102) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            system.TryCreateUser("boss2", "Boss_123", "boss2@cars.com", "0502222222", "Manager", telAviv);
            User boss = system.FindUserByUsername("boss");
            User otherBoss = system.FindUserByUsername("boss2");
            User dana = MakeCustomer(system, "dana");
            User noa = MakeCustomer(system, "noa");

            Check(system.CountSoldOrRentedCars(haifa) == 0, "no closed deals at start");

            int sale1 = AddCar(system, "1000001", "Sale", haifa);
            int rent1 = AddCar(system, "1000002", "Rental", haifa);
            int rent2 = AddCar(system, "1000003", "Rental", haifa);
            int rentTa = AddCar(system, "2000001", "Rental", telAviv);
            system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale1 }, 1);
            system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent1 }, 1);
            system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent2 }, 1);
            system.TryConfirmOrder(dana, dana, "Rental", new int[] { rentTa }, 1);

            Check(system.CountSoldOrRentedCars(haifa) == 0, "Pending orders are not shown");

            system.TryApproveOrder(boss, 1);
            system.TryApproveOrder(boss, 2);
            system.TryRejectOrder(boss, 3);
            system.TryApproveOrder(otherBoss, 4);

            Check(system.CountSoldOrRentedCars(haifa) == 2, "the sold car and the rented car of the dealership are counted");
            Check(system.FindCarById(rent2).IsAvailable(), "a rejected order is not counted");
            Check(system.CountSoldOrRentedCars(telAviv) == 1, "each dealership sees only its own cars");
        }

        // VFDN-106: REQ-012 my orders (design 7.14)
        private static void RunCustomerOrdersTests()
        {
            Console.WriteLine("--- REQ-012 My orders (VFDN-106) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            User dana = MakeCustomer(system, "dana");
            User noa = MakeCustomer(system, "noa");
            Order[] results = new Order[CarDealerShipSystem.ORDERS_MAX];

            Check(system.GetCustomerOrders(dana, results) == 0 && results[0] == null, "customer without orders gets 0 and the array is unchanged");

            int rent1 = AddCar(system, "1000001", "Rental", haifa);
            int rent2 = AddCar(system, "1000002", "Rental", haifa);
            int rent3 = AddCar(system, "1000003", "Rental", haifa);
            int sale1 = AddCar(system, "1000004", "Sale", haifa);
            system.TryConfirmOrder(dana, dana, "Rental", new int[] { rent1 }, 1);
            system.TryConfirmOrder(noa, noa, "Rental", new int[] { rent2 }, 1);
            system.TryConfirmOrder(dana, dana, "Rental", new int[] { rent3 }, 1);
            system.TryConfirmOrder(dana, dana, "Sale", new int[] { sale1 }, 1);
            system.TryCancelOrder(dana, 3);

            int count = system.GetCustomerOrders(dana, results);
            Check(count == 3, "the customer sees all his orders, in every status");
            Check(results[0].GetOrderNumber() == 4 && results[1].GetOrderNumber() == 3 && results[2].GetOrderNumber() == 1,
                  "orders are sorted from the newest to the oldest");
            bool onlyMine = true;
            for (int i = 0; i < count; i++)
            {
                if (!results[i].BelongsTo(dana))
                {
                    onlyMine = false;
                }
            }
            Check(onlyMine, "orders of another customer are not shown");
            Check(system.GetOrderAt(0).GetOrderNumber() == 1 && system.GetOrderAt(1).GetOrderNumber() == 2
                  && system.GetOrderAt(3).GetOrderNumber() == 4,
                  "showing orders does not reorder the orders array");
            Check(system.GetCustomerOrders(system.FindUserByUsername("noa"), results) == 1, "each customer sees only his own orders");
        }
    }
}
