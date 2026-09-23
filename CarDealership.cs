namespace CarsApp
{
    // A car dealership
    public class CarDealership
    {
        public const int MAX_CARS_PER_DEALERSHIP = 1000;

        // VFDN-115: dealership fields
        private int dealershipId;
        private string name;
        private string address;
        private string managerUsername;
        private int maxCars;

        // VFDN-116: constructor
        public CarDealership(int dealershipId, string name, string address, string managerUsername)
        {
            this.dealershipId = dealershipId;
            this.name = name;
            this.address = address;
            this.managerUsername = managerUsername;
            this.maxCars = MAX_CARS_PER_DEALERSHIP;
        }

        // VFDN-117: Get methods
        public int GetDealershipId()
        {
            return dealershipId;
        }

        public string GetName()
        {
            return name;
        }

        public string GetAddress()
        {
            return address;
        }

        public string GetManagerUsername()
        {
            return managerUsername;
        }

        public int GetMaxCars()
        {
            return maxCars;
        }

        // VFDN-117: Set methods
        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetAddress(string address)
        {
            this.address = address;
        }

        public override string ToString()
        {
            return dealershipId + ". " + name + " - " + address;
        }
    }
}
