namespace CarsApp
{
    public class Program
    {
        // Run normally:      dotnet run
        // Run the tests:     dotnet run -- test
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "test")
            {
                Tests.RunAll();
                return;
            }

            CarSystem system = new CarSystem();
            system.LoadSampleData();
            system.Run();
        }
    }
}
