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
    }
}
