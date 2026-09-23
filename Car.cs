namespace CarsApp
{
    // VFDN-90: a car in a dealership's inventory
    public class Car
    {
        // VFDN-112: car fields
        private string licenseNumber;
        private string manufacturer;
        private string model;
        private double price;
        private string status;          // Available, Reserved, Sold, Rented
        private string transactionType; // Sale, Rent, Both
        private int dealershipId;

        // VFDN-113: constructor - every new car starts as Available
        public Car(string licenseNumber, string manufacturer, string model, double price,
                   string transactionType, int dealershipId)
        {
            this.licenseNumber = licenseNumber;
            this.manufacturer = manufacturer;
            this.model = model;
            this.price = price;
            this.transactionType = transactionType;
            this.dealershipId = dealershipId;
            this.status = "Available";
        }

        // VFDN-114: Get methods
        public string GetLicenseNumber()
        {
            return licenseNumber;
        }

        public string GetManufacturer()
        {
            return manufacturer;
        }

        public string GetModel()
        {
            return model;
        }

        public double GetPrice()
        {
            return price;
        }

        public string GetStatus()
        {
            return status;
        }

        public string GetTransactionType()
        {
            return transactionType;
        }

        public int GetDealershipId()
        {
            return dealershipId;
        }

        // VFDN-114: Set methods
        public void SetManufacturer(string manufacturer)
        {
            this.manufacturer = manufacturer;
        }

        public void SetModel(string model)
        {
            this.model = model;
        }

        public void SetPrice(double price)
        {
            this.price = price;
        }

        public void SetTransactionType(string transactionType)
        {
            this.transactionType = transactionType;
        }

        // VFDN-114: status update - accepts only the four allowed values
        public bool SetStatus(string newStatus)
        {
            if (newStatus == "Available" || newStatus == "Reserved" ||
                newStatus == "Sold" || newStatus == "Rented")
            {
                status = newStatus;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return licenseNumber + " | " + manufacturer + " " + model + " | " + price + " | " + status + " | " + transactionType;
        }
    }
}
