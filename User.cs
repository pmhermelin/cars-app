namespace CarsApp
{
    // Design 6.1: a single user of the system - Customer, Manager or Salesperson.
    // Holds a reference to its dealership (null for a customer). Has no getter for the password.
    public class User
    {
        private int id;
        private string username;
        private string password;
        private string phone;
        private string email;
        private string role;
        private CarDealership dealership;

        public User(int id, string username, string password, string phone, string email, string role)
        {
            this.id = id;
            this.username = username;
            this.password = password;
            this.phone = phone;
            this.email = email;
            this.role = role;
            this.dealership = null;
        }

        public int GetId()
        {
            return id;
        }

        public string GetUsername()
        {
            return username;
        }

        public string GetPhone()
        {
            return phone;
        }

        public string GetEmail()
        {
            return email;
        }

        public string GetRole()
        {
            return role;
        }

        public CarDealership GetDealership()
        {
            return dealership;
        }

        // Links the user to a dealership. Returns false if the user is already linked.
        public bool SetDealership(CarDealership dealership)
        {
            if (this.dealership != null || dealership == null)
            {
                return false;
            }
            this.dealership = dealership;
            return true;
        }

        public bool IsManager()
        {
            return role == CarDealerShipSystem.ROLE_MANAGER;
        }

        public bool IsSalesperson()
        {
            return role == CarDealerShipSystem.ROLE_SALESPERSON;
        }

        public bool IsCustomer()
        {
            return role == CarDealerShipSystem.ROLE_CUSTOMER;
        }

        // The only way to verify the password - it never leaves the object
        public bool CheckPassword(string pass)
        {
            return password == pass;
        }

        // Never includes the password
        public override string ToString()
        {
            return username + " | " + role + " | " + email + " | " + phone;
        }
    }
}
