namespace CarsApp
{
    // VFDN-89: a user of the system - customer, salesperson or dealership manager
    public class User
    {
        // VFDN-108: user fields
        private string username;
        private string password;
        private string email;
        private string phone;
        private int dealershipId;
        private string userType;
        private int agencyUserId;

        // VFDN-109: constructor
        public User(string username, string password, string email, string phone,
                    int dealershipId, string userType, int agencyUserId)
        {
            this.username = username;
            this.password = password;
            this.email = email;
            this.phone = phone;
            this.dealershipId = dealershipId;
            this.userType = userType;
            this.agencyUserId = agencyUserId;
        }

        // VFDN-110: Get methods
        public string GetUsername()
        {
            return username;
        }

        public string GetEmail()
        {
            return email;
        }

        public string GetPhone()
        {
            return phone;
        }

        public int GetDealershipId()
        {
            return dealershipId;
        }

        public string GetUserType()
        {
            return userType;
        }

        public int GetAgencyUserId()
        {
            return agencyUserId;
        }

        // VFDN-110: Set methods (only for fields that are allowed to change)
        public void SetPassword(string password)
        {
            this.password = password;
        }

        public void SetEmail(string email)
        {
            this.email = email;
        }

        public void SetPhone(string phone)
        {
            this.phone = phone;
        }

        public void SetDealershipId(int dealershipId)
        {
            this.dealershipId = dealershipId;
        }

        // VFDN-111: returns true only when the input matches the saved password (case sensitive)
        public bool CheckPassword(string input)
        {
            return password == input;
        }

        // The password is never printed
        public override string ToString()
        {
            return username + " (" + userType + "), email: " + email + ", phone: " + phone;
        }
    }
}
