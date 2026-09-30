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
            RunLoginTests();
            RunLogoutTests();
            RunAddSalespersonTests();
            RunInventoryReportTests();
            RunAddCarTests();
            RunChangePriceTests();
            RunSearchTests();
            RunAvailableCarsTests();
            RunDealershipInventoryTests();
            RunConfirmOrderTests();
            RunCancelOrderTests();
            RunApproveRejectTests();
            RunSoldOrRentedTests();

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

        // VFDN-94: REQ-002 login (design 7.2)
        private static void RunLoginTests()
        {
            Console.WriteLine("--- REQ-002 Login (VFDN-94) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", system.FindDealershipById(2));

            Check(system.LoginWith("dana", "wrong_12") == null && system.GetCurrentUser() == null, "T-04 wrong password - currentUser stays null");
            Check(system.LoginWith("nobody", "Dana_123") == null && system.GetCurrentUser() == null, "unknown username - currentUser stays null");
            Check(system.LoginWith("dana", "dana_123") == null, "password is case sensitive");

            User dana = system.LoginWith("dana", "Dana_123");
            Check(dana != null && system.GetCurrentUser() == dana && dana.IsCustomer(), "customer logs in and becomes currentUser");

            CarDealerShipSystem other = new CarDealerShipSystem();
            other.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", other.FindDealershipById(2));
            User boss = other.LoginWith("boss", "Boss_123");
            Check(boss != null && boss.IsManager() && boss.GetDealership().GetName() == "Kia Tel Aviv", "manager logs in with his dealership");
        }

        // VFDN-95: REQ-015 logout (design 7.17)
        private static void RunLogoutTests()
        {
            Console.WriteLine("--- REQ-015 Logout (VFDN-95) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", system.FindDealershipById(1));
            system.LoginWith("dana", "Dana_123");
            int users = system.GetUserCount();

            system.Logout();
            Check(system.GetCurrentUser() == null, "currentUser is reset");
            Check(system.GetUserCount() == users && system.FindUserByUsername("dana") != null, "T-22 data is kept after logout");
            Check(system.LoginWith("dana", "Dana_123") != null, "the same user can log in again");
            system.Logout();
            Check(system.LoginWith("boss", "Boss_123") != null && system.GetCurrentUser().IsManager(), "another user can log in after logout");
        }

        // VFDN-107: REQ-014 add salesperson (design 7.16)
        private static void RunAddSalespersonTests()
        {
            Console.WriteLine("--- REQ-014 Add Salesperson (VFDN-107) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership mazda = system.FindDealershipById(3);
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", mazda);
            User dana = system.FindUserByUsername("dana");
            User boss = system.FindUserByUsername("boss");
            int count = system.GetUserCount();

            Check(!system.TryAddSalesperson(boss, "yossi", "Yossi_12", "yossi@cars.com", "0531234567"), "manager who is not logged in is rejected");
            system.LoginWith("dana", "Dana_123");
            Check(!system.TryAddSalesperson(dana, "yossi", "Yossi_12", "yossi@cars.com", "0531234567"), "customer is rejected");
            system.Logout();
            Check(system.GetUserCount() == count, "rejected attempts create no user");

            system.LoginWith("boss", "Boss_123");
            Check(!system.TryAddSalesperson(dana, "yossi", "Yossi_12", "yossi@cars.com", "0531234567"), "a user that is not currentUser is rejected");
            Check(system.TryAddSalesperson(boss, "yossi", "Yossi_12", "yossi@cars.com", "0531234567"), "manager adds a valid salesperson");
            User yossi = system.FindUserByUsername("yossi");
            Check(yossi != null && yossi.IsSalesperson() && yossi.GetDealership() == mazda, "salesperson is linked to the manager's dealership");

            count = system.GetUserCount();
            Check(!system.TryAddSalesperson(boss, "yossi", "Yossi_12", "other@cars.com", "0531234567"), "taken username is rejected");
            Check(!system.TryAddSalesperson(boss, "avi", "weakpass", "avi@cars.com", "0531234567"), "weak password is rejected");
            Check(!system.TryAddSalesperson(boss, "avi", "Avi_1234", "avi-cars.com", "0531234567"), "invalid email is rejected");
            Check(!system.TryAddSalesperson(boss, "avi", "Avi_1234", "avi@cars.com", "12345"), "invalid phone is rejected");
            Check(system.GetUserCount() == count, "failed additions did not change the data");

            system.Logout();
            Check(!system.TryAddSalesperson(boss, "avi", "Avi_1234", "avi@cars.com", "0531234567"), "T-22 blocked after logout");
            User logged = system.LoginWith("yossi", "Yossi_12");
            Check(logged == yossi && logged.GetDealership() == mazda, "T-20 new salesperson logs in with the right dealership");
        }

        // VFDN-101: REQ-006 inventory report (design 7.9)
        private static void RunInventoryReportTests()
        {
            Console.WriteLine("--- REQ-006 Inventory Report (VFDN-101) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership kia = system.FindDealershipById(2);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            User boss = system.FindUserByUsername("boss");

            system.LoginWith("boss", "Boss_123");
            Console.WriteLine("(empty inventory report:)");
            system.PrintInventoryReport(boss);
            Check(system.CountDealershipCars(haifa) == 0, "empty inventory - report prints the empty message");

            Car c1 = MakeCar(system, "1111111", 100000, "Sale", haifa);
            Car c2 = MakeCar(system, "2222222", 120000, "Both", haifa);
            Car c3 = MakeCar(system, "3333333", 90000, "Rental", haifa);
            Car other = MakeCar(system, "4444444", 80000, "Sale", kia);
            system.AddCarToInventory(c1);
            system.AddCarToInventory(c2);
            system.AddCarToInventory(c3);
            system.AddCarToInventory(other);
            c2.MarkAsReserved();
            c3.MarkAsReserved();
            c3.MarkAsRented();

            system.PrintInventoryReport(boss);
            Check(system.CountDealershipCars(haifa) == 3, "only the manager's dealership cars are counted");
            Check(system.CountCarsByStatus(haifa, "Available") == 1 && system.CountCarsByStatus(haifa, "Reserved") == 1
                  && system.CountCarsByStatus(haifa, "Rented") == 1 && system.CountCarsByStatus(haifa, "Sold") == 0,
                  "summary by status is correct");
            system.PrintInventoryReport(boss);
            Check(system.CountCarsByStatus(haifa, "Available") == 1, "a second report gives the same numbers");

            system.Logout();
            int carsBefore = system.GetCarCount();
            system.PrintInventoryReport(boss);
            Check(system.GetCarCount() == carsBefore, "not logged in - report is blocked and nothing changes");
        }

        // VFDN-96: REQ-003 add a car (design 7.3)
        private static void RunAddCarTests()
        {
            Console.WriteLine("--- REQ-003 Add car (VFDN-96) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            User boss = system.FindUserByUsername("boss");
            User dana = system.FindUserByUsername("dana");

            Check(system.TryAddNewCar(boss, "Sedan", "Toyota", "Corolla", 2024, 1000, "1234567", 100000, "Sale", "Haifa")
                  && system.GetCarCount() == 1, "manager adds a car");
            Car car = system.FindCarByLicenseNumber("1234567");
            Check(car.IsAvailable() && car.GetDealership() == haifa, "new car is Available and belongs to the manager's dealership");

            Check(!system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 2023, 0, "1234567", 90000, "Rental", "Haifa")
                  && system.GetCarCount() == 1, "T-05 duplicate license number - no car is created");
            Check(!system.TryAddNewCar(dana, "SUV", "Kia", "Sportage", 2023, 0, "7654321", 90000, "Rental", "Haifa")
                  && system.GetCarCount() == 1, "T-07 a customer cannot add a car");
            Check(!system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 2023, 0, "7654321", -1, "Rental", "Haifa")
                  && !system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 2023, 0, "7654321", 0, "Rental", "Haifa"),
                  "price -1 or 0 is rejected");
            Check(!system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 2023, 0, "7654321", 90000, "", "Haifa"),
                  "empty deal type is rejected");
            Check(!system.TryAddNewCar(boss, "", "Kia", "Sportage", 2023, 0, "7654321", 90000, "Rental", "Haifa")
                  && !system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 1800, 0, "7654321", 90000, "Rental", "Haifa")
                  && !system.TryAddNewCar(boss, "SUV", "Kia", "Sportage", 2023, -1, "7654321", 90000, "Rental", "Haifa"),
                  "blank field, invalid year or negative mileage are rejected");
            Check(system.GetCarCount() == 1, "failed attempts changed nothing");

            for (int i = system.CountDealershipCars(haifa); i < CarDealerShipSystem.DEALERSHIP_CARS_LIMIT; i++)
            {
                system.TryAddNewCar(boss, "Sedan", "Toyota", "Yaris", 2024, 0, "L" + i, 50000, "Sale", "Haifa");
            }
            Check(system.CountDealershipCars(haifa) == 1000
                  && !system.TryAddNewCar(boss, "Sedan", "Toyota", "Yaris", 2024, 0, "EXTRA", 50000, "Sale", "Haifa"),
                  "T-06 the 1001st car of a dealership is rejected");
        }

        // VFDN-105: REQ-007 change car price (design 7.10)
        private static void RunChangePriceTests()
        {
            Console.WriteLine("--- REQ-007 Change car price (VFDN-105) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            system.TryCreateUser("boss", "Boss_123", "boss@cars.com", "0501111111", "Manager", haifa);
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            User boss = system.FindUserByUsername("boss");
            User dana = system.FindUserByUsername("dana");

            Car car = MakeCar(system, "1000001", 100000, "Sale", haifa);
            Car reserved = MakeCar(system, "1000002", 80000, "Sale", haifa);
            Car other = MakeCar(system, "2000001", 70000, "Sale", telAviv);
            system.AddCarToInventory(car);
            system.AddCarToInventory(reserved);
            system.AddCarToInventory(other);
            reserved.MarkAsReserved();

            Check(system.TryChangeCarPrice(boss, car.GetId(), 95000) && car.GetPrice() == 95000, "manager changes the price of an Available car");
            Check(!system.TryChangeCarPrice(boss, car.GetId(), 0) && !system.TryChangeCarPrice(boss, car.GetId(), -5)
                  && car.GetPrice() == 95000, "T-09 price 0 or negative is rejected and the old price is kept");
            Check(!system.TryChangeCarPrice(boss, reserved.GetId(), 60000) && reserved.GetPrice() == 80000,
                  "T-26 a Reserved car keeps its price");
            Check(!system.TryChangeCarPrice(boss, other.GetId(), 60000) && other.GetPrice() == 70000,
                  "a car of another dealership cannot be changed");
            Check(!system.TryChangeCarPrice(dana, car.GetId(), 60000) && car.GetPrice() == 95000, "a customer cannot change a price");
            Check(!system.TryChangeCarPrice(boss, 999, 60000), "unknown car id is rejected");
        }

        // VFDN-100: REQ-009 search and filter cars (design 7.12)
        private static void RunSearchTests()
        {
            Console.WriteLine("--- REQ-009 Search cars (VFDN-100) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            Car toyotaSale = new Car(system.GetNextCarId(), "Sedan", "Toyota", "Corolla", 2024, 0, "1000001", 100000, "Sale", "Haifa", haifa);
            Car kiaRent = new Car(system.GetNextCarId(), "SUV", "Kia", "Sportage", 2020, 0, "1000002", 150000, "Rental", "Haifa", haifa);
            Car mazdaBoth = new Car(system.GetNextCarId(), "SUV", "Mazda", "CX-5", 2022, 0, "1000003", 120000, "Both", "Haifa", haifa);
            Car soldToyota = new Car(system.GetNextCarId(), "Sedan", "Toyota", "Yaris", 2024, 0, "1000004", 60000, "Sale", "Haifa", haifa);
            system.AddCarToInventory(toyotaSale);
            system.AddCarToInventory(kiaRent);
            system.AddCarToInventory(mazdaBoth);
            system.AddCarToInventory(soldToyota);
            soldToyota.MarkAsReserved();
            soldToyota.MarkAsSold();

            Car[] results = new Car[system.GetCarCount()];
            Check(system.SearchCars("", "", 0, 0, 0, "", results) == 3, "T-19 all filters empty returns every Available car");
            Check(system.SearchCars("", "toyota ", 0, 0, 0, "", results) == 1 && results[0] == toyotaSale,
                  "manufacturer filter ignores case and spaces; a Sold car is not returned");
            Check(system.SearchCars("SUV", "", 0, 0, 0, "", results) == 2, "category filter");
            Check(system.SearchCars("", "", 110000, 130000, 0, "", results) == 1 && results[0] == mazdaBoth, "price range filter");
            Check(system.SearchCars("", "", 0, 0, 2021, "", results) == 2, "minimum year filter");
            Check(system.SearchCars("", "", 0, 0, 0, "Sale", results) == 2 && system.SearchCars("", "", 0, 0, 0, "Rental", results) == 2,
                  "a Both car is found in both purchase and rental searches");

            Car[] empty = new Car[system.GetCarCount()];
            Check(system.SearchCars("Truck", "", 0, 0, 0, "", empty) == 0 && empty[0] == null,
                  "T-18 no results returns 0 and the results array is unchanged");
        }

        // VFDN-103: REQ-010 view available cars (design 7.13)
        private static void RunAvailableCarsTests()
        {
            Console.WriteLine("--- REQ-010 Available cars (VFDN-103) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            CarDealership jerusalem = system.FindDealershipById(3);

            Check(system.CountAvailableCars(null) == 0, "no cars at start - the empty message is shown");

            Car car1 = MakeCar(system, "1000001", 100000, "Sale", haifa);
            Car car2 = MakeCar(system, "1000002", 90000, "Rental", haifa);
            Car car3 = MakeCar(system, "2000001", 80000, "Both", telAviv);
            system.AddCarToInventory(car1);
            system.AddCarToInventory(car2);
            system.AddCarToInventory(car3);
            car2.MarkAsReserved();

            Check(system.CountAvailableCars(haifa) == 1, "a Reserved car is not counted as available");
            Check(system.CountAvailableCars(telAviv) == 1, "each dealership counts only its own cars");
            Check(system.CountAvailableCars(jerusalem) == 0, "a dealership without available cars is skipped (no empty header)");
            Check(system.CountAvailableCars(null) == 2, "the total summary counts every available car");
        }

        // VFDN-159: REQ-013 dealership inventory for a salesperson (design 7.15)
        private static void RunDealershipInventoryTests()
        {
            Console.WriteLine("--- REQ-013 Dealership inventory (VFDN-159) ---");

            CarDealerShipSystem system = new CarDealerShipSystem();
            CarDealership haifa = system.FindDealershipById(1);
            CarDealership telAviv = system.FindDealershipById(2);
            // Built directly because AddSalesperson (REQ-014) is on another branch
            User seller = new User(900, "seller", "Pass_123", "0521234567", "seller@cars.com", "Salesperson");
            seller.SetDealership(haifa);
            system.TryCreateUser("dana", "Dana_123", "dana@mail.com", "0521234567", "Customer", null);
            User dana = system.FindUserByUsername("dana");

            Car available = MakeCar(system, "1000001", 100000, "Sale", haifa);
            Car sold = MakeCar(system, "1000002", 90000, "Sale", haifa);
            Car other = MakeCar(system, "2000001", 80000, "Sale", telAviv);
            system.AddCarToInventory(available);
            system.AddCarToInventory(sold);
            system.AddCarToInventory(other);
            sold.MarkAsReserved();
            sold.MarkAsSold();

            Check(system.CountDealershipCars(haifa) == 2, "the inventory includes cars in every status (Available and Sold)");
            Check(system.CountDealershipCars(telAviv) == 1, "cars of another dealership are not included");

            system.PrintDealershipInventory(null);
            system.PrintDealershipInventory(dana);
            system.PrintDealershipInventory(seller);
            Check(true, "null user, customer and salesperson all run without an exception");
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
    }
}
