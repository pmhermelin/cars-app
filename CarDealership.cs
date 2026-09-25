namespace CarsApp
{
    // Design 6.2: a car or motorcycle dealership. Created only in the system constructor.
    // Holds a reference to its manager (owner). Does not hold an array of cars.
    public class CarDealership
    {
        private int id;
        private string name;
        private string dealershipType; // Cars or Motorcycles - descriptive only
        private string address;
        private string phone;
        private User owner;

        public CarDealership(int id, string name, string dealershipType, string address, string phone)
        {
            this.id = id;
            this.name = name;
            this.dealershipType = dealershipType;
            this.address = address;
            this.phone = phone;
            this.owner = null;
        }

        public int GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public string GetDealershipType()
        {
            return dealershipType;
        }

        public string GetAddress()
        {
            return address;
        }

        public string GetPhone()
        {
            return phone;
        }

        public User GetOwner()
        {
            return owner;
        }

        // One manager per dealership: returns false if a manager already exists
        public bool SetOwner(User user)
        {
            if (owner != null || user == null)
            {
                return false;
            }
            owner = user;
            return true;
        }

        public bool HasOwner()
        {
            return owner != null;
        }

        public bool IsOwner(User user)
        {
            return owner != null && owner == user;
        }

        // Does not print the manager
        public override string ToString()
        {
            return name + " (" + dealershipType + ") | " + phone + " | " + address;
        }
    }
}
