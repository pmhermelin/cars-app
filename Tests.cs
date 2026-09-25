using System;

namespace CarsApp
{
    // Simple test runner: dotnet run -- test
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

            Console.WriteLine();
            Console.WriteLine("Passed: " + passed + ", Failed: " + failed);
        }

        // VFDN-133: infrastructure tests
        private static void RunInfrastructureTests()
        {
            Console.WriteLine("--- Infrastructure (VFDN-133) ---");

            CarSystem system = new CarSystem();
            Check(system.GetUserCount() == 0 && system.GetCarCount() == 0, "new system starts empty");
            Check(system.GetCurrentUser() == null, "no user is logged in at start");

            system.LoadSampleData();
            Check(system.GetDealershipCount() == 2, "sample dealerships loaded");
            Check(system.FindUser("manager1") != null, "find user by username");
            Check(system.FindUser("nobody") == null, "unknown user returns null");

            User manager = system.FindUser("manager1");
            Check(manager.CheckPassword("Manager123"), "CheckPassword accepts the right password");
            Check(!manager.CheckPassword("manager123"), "CheckPassword is case sensitive");

            Car car = system.FindCar("1111111");
            Check(car != null && car.GetStatus() == "Available", "new car starts as Available");
            Check(!car.SetStatus("Broken"), "car rejects an invalid status");
            Check(car.SetStatus("Reserved") && car.GetStatus() == "Reserved", "car accepts a valid status");

            Order order = new Order(1, "manager1", 1, "Purchase");
            Check(order.GetStatus() == "Pending", "new order starts as Pending");
            order.AddCar(system.FindCar("1111111"));
            order.AddCar(system.FindCar("2222222"));
            Check(order.GetTotalPrice() == 255000, "order total price");
            Check(system.GetOrderCustomer(order) == manager, "order -> customer relation");
            Check(system.GetOrderDealership(order).GetName() == "Netanya Cars", "order -> dealership relation");
            Check(system.CountCarsInDealership(1) == 3, "count cars in dealership");
        }

        // VFDN-160: REQ-001 registration tests
        private static void RunRegisterTests()
        {
            Console.WriteLine("--- REQ-001 Register (VFDN-93) ---");

            CarSystem system = new CarSystem();
            system.LoadSampleData();
            int before = system.GetUserCount();

            Check(system.RegisterUser("dana", "Dana1234", "dana@mail.com", "0521234567", 1, "Customer") == "",
                  "valid registration succeeds");
            Check(system.GetUserCount() == before + 1, "user was added to the array");
            User dana = system.FindUser("dana");
            Check(dana != null && dana.GetDealershipId() == 1, "user is linked to the chosen dealership");
            Check(dana.GetAgencyUserId() == 2, "agency user id is the next number in the dealership");

            int count = system.GetUserCount();
            Check(system.RegisterUser("a1", "dana1234", "a1@mail.com", "0521111111", 1, "Customer") != "", "no uppercase letter is rejected");
            Check(system.RegisterUser("a2", "Dana12", "a2@mail.com", "0521111111", 1, "Customer") != "", "short password is rejected");
            Check(system.RegisterUser("a3", "Danaaaaa", "a3@mail.com", "0521111111", 1, "Customer") != "", "password without a digit is rejected");
            Check(system.RegisterUser("a4", "Dana1234", "a4mail.com", "0521111111", 1, "Customer") != "", "email without @ is rejected");
            Check(system.RegisterUser("a5", "Dana1234", "a5@mail", "0521111111", 1, "Customer") != "", "email without a dot is rejected");
            Check(system.RegisterUser("a6", "Dana1234", "a6@mail.com", "052111", 1, "Customer") != "", "short phone is rejected");
            Check(system.RegisterUser("a7", "Dana1234", "a7@mail.com", "0421111111", 1, "Customer") != "", "phone not starting with 05 is rejected");
            Check(system.RegisterUser("dana", "Dana1234", "other@mail.com", "0521111111", 1, "Customer") != "", "duplicate username is rejected");
            Check(system.RegisterUser("a8", "Dana1234", "DANA@mail.com", "0521111111", 1, "Customer") != "", "duplicate email is rejected (any case)");
            Check(system.RegisterUser("a9", "Dana1234", "a9@mail.com", "0521111111", 99, "Customer") != "", "unknown dealership is rejected");
            Check(system.RegisterUser("a10", "Dana1234", "a10@mail.com", "0521111111", 1, "Pilot") != "", "unknown user type is rejected");
            Check(system.RegisterUser("a11", "Dana1234", "a11@mail.com", "0521111111", 1, "Salesperson") != "", "salesperson cannot self-register");
            Check(system.GetUserCount() == count, "failed registrations did not change the data");

            CarSystem full = new CarSystem();
            full.LoadSampleData();
            int i = 0;
            while (full.HasUserCapacity())
            {
                full.AddUserToArray(new User("u" + i, "Pass1234", "u" + i + "@m.com", "0500000000", 1, "Customer", 0));
                i++;
            }
            Check(full.RegisterUser("last", "Dana1234", "last@mail.com", "0521111111", 1, "Customer") != "", "registration fails when the array is full");
        }
    }
}
