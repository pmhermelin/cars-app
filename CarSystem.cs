using System;

namespace CarsApp
{
    // VFDN-84: the central control class of the system.
    // Named CarSystem because "System" is a reserved namespace in C#.
    public class CarSystem
    {
        public const int MAX_USERS = 1000;
        public const int MAX_CARS = 5000;
        public const int MAX_ORDERS = 5000;
        public const int MAX_DEALERSHIPS = 10;

        // VFDN-87: data arrays, counters and the logged in user
        private User[] users;
        private int userCount;
        private Car[] cars;
        private int carCount;
        private Order[] orders;
        private int orderCount;
        private CarDealership[] dealerships;
        private int dealershipCount;
        private User currentUser;

        // VFDN-86: main constructor
        public CarSystem()
        {
            users = new User[MAX_USERS];
            cars = new Car[MAX_CARS];
            orders = new Order[MAX_ORDERS];
            dealerships = new CarDealership[MAX_DEALERSHIPS];
            userCount = 0;
            carCount = 0;
            orderCount = 0;
            dealershipCount = 0;
            currentUser = null;
        }

        // Sample data so the system can be used right away
        public void LoadSampleData()
        {
            AddDealership(new CarDealership(1, "Netanya Cars", "Herzl 10, Netanya", "manager1"));
            AddDealership(new CarDealership(2, "Tel Aviv Motors", "Dizengoff 50, Tel Aviv", "manager2"));

            AddUserToArray(new User("manager1", "Manager123", "manager1@cars.com", "0501111111", 1, "Manager", 1));
            AddUserToArray(new User("manager2", "Manager123", "manager2@cars.com", "0502222222", 2, "Manager", 1));

            AddCarToArray(new Car("1111111", "Toyota", "Corolla", 120000, "Both", 1));
            AddCarToArray(new Car("2222222", "Mazda", "3", 135000, "Sale", 1));
            AddCarToArray(new Car("3333333", "Kia", "Picanto", 80000, "Rent", 1));
            AddCarToArray(new Car("4444444", "Hyundai", "Tucson", 170000, "Both", 2));
        }

        // ===== Counters and current user =====

        public int GetUserCount()
        {
            return userCount;
        }

        public int GetCarCount()
        {
            return carCount;
        }

        public int GetOrderCount()
        {
            return orderCount;
        }

        public int GetDealershipCount()
        {
            return dealershipCount;
        }

        public User GetCurrentUser()
        {
            return currentUser;
        }

        public bool IsLoggedIn()
        {
            return currentUser != null;
        }

        // ===== Adding to the arrays (return false when the array is full) =====

        public bool AddUserToArray(User user)
        {
            if (userCount >= MAX_USERS)
            {
                return false;
            }
            users[userCount] = user;
            userCount++;
            return true;
        }

        public bool AddCarToArray(Car car)
        {
            if (carCount >= MAX_CARS)
            {
                return false;
            }
            cars[carCount] = car;
            carCount++;
            return true;
        }

        public bool AddOrderToArray(Order order)
        {
            if (orderCount >= MAX_ORDERS)
            {
                return false;
            }
            orders[orderCount] = order;
            orderCount++;
            return true;
        }

        public bool AddDealership(CarDealership dealership)
        {
            if (dealershipCount >= MAX_DEALERSHIPS)
            {
                return false;
            }
            dealerships[dealershipCount] = dealership;
            dealershipCount++;
            return true;
        }

        // ===== VFDN-88: relations between the classes =====

        public User FindUser(string username)
        {
            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetUsername() == username)
                {
                    return users[i];
                }
            }
            return null;
        }

        public Car FindCar(string licenseNumber)
        {
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetLicenseNumber() == licenseNumber)
                {
                    return cars[i];
                }
            }
            return null;
        }

        public CarDealership FindDealership(int dealershipId)
        {
            for (int i = 0; i < dealershipCount; i++)
            {
                if (dealerships[i].GetDealershipId() == dealershipId)
                {
                    return dealerships[i];
                }
            }
            return null;
        }

        // The customer of an order
        public User GetOrderCustomer(Order order)
        {
            return FindUser(order.GetCustomerUsername());
        }

        // The dealership of an order
        public CarDealership GetOrderDealership(Order order)
        {
            return FindDealership(order.GetDealershipId());
        }

        // Number of users that belong to a dealership
        public int CountUsersInDealership(int dealershipId)
        {
            int count = 0;
            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetDealershipId() == dealershipId)
                {
                    count++;
                }
            }
            return count;
        }

        // Number of cars that belong to a dealership
        public int CountCarsInDealership(int dealershipId)
        {
            int count = 0;
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetDealershipId() == dealershipId)
                {
                    count++;
                }
            }
            return count;
        }

        public void PrintDealerships()
        {
            for (int i = 0; i < dealershipCount; i++)
            {
                Console.WriteLine(dealerships[i].ToString());
            }
        }

        // ===== REQ-001: registration (VFDN-93) =====

        // VFDN-121: is there room for another user
        public bool HasUserCapacity()
        {
            return userCount < MAX_USERS;
        }

        // VFDN-122: at least 8 characters, with an uppercase letter, a lowercase letter and a digit
        public bool IsStrongPassword(string password)
        {
            if (password == null || password.Length < 8)
            {
                return false;
            }

            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];
                if (char.IsUpper(c))
                {
                    hasUpper = true;
                }
                else if (char.IsLower(c))
                {
                    hasLower = true;
                }
                else if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
            }
            return hasUpper && hasLower && hasDigit;
        }

        // VFDN-122: one '@' that is not first, a '.' after it that is not last, and no spaces
        public bool IsValidEmail(string email)
        {
            if (email == null || email.Length == 0 || email.IndexOf(' ') != -1)
            {
                return false;
            }

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
            {
                return false;
            }

            int dotIndex = email.LastIndexOf('.');
            return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
        }

        // VFDN-122: Israeli mobile number - 10 digits starting with 05
        public bool IsValidPhone(string phone)
        {
            if (phone == null || phone.Length != 10 || phone[0] != '0' || phone[1] != '5')
            {
                return false;
            }
            for (int i = 0; i < phone.Length; i++)
            {
                if (!char.IsDigit(phone[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // VFDN-123: the user type must be one of the types the system knows
        public bool IsValidUserType(string userType)
        {
            return userType == "Customer" || userType == "Salesperson" || userType == "Manager";
        }

        // VFDN-124: is the username already taken
        public bool UsernameExists(string username)
        {
            return FindUser(username) != null;
        }

        // VFDN-124: is the email already taken (not case sensitive)
        public bool EmailExists(string email)
        {
            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetEmail().ToLower() == email.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        // VFDN-125: creates a new user object; its id inside the dealership is the next free number
        public User CreateUser(string username, string password, string email, string phone,
                               int dealershipId, string userType)
        {
            int agencyUserId = CountUsersInDealership(dealershipId) + 1;
            return new User(username, password, email, phone, dealershipId, userType, agencyUserId);
        }

        // Checks all the rules of a new user. Returns "" when everything is valid, otherwise the error message.
        public string ValidateNewUser(string username, string password, string email, string phone, int dealershipId)
        {
            if (!HasUserCapacity())
            {
                return "The system is full, no more users can be added.";
            }
            if (username == null || username.Length == 0)
            {
                return "Username cannot be empty.";
            }
            if (!IsStrongPassword(password))
            {
                return "Password must be at least 8 characters and include an uppercase letter, a lowercase letter and a digit.";
            }
            if (!IsValidEmail(email))
            {
                return "Invalid email address.";
            }
            if (!IsValidPhone(phone))
            {
                return "Invalid phone number (10 digits, starting with 05).";
            }
            if (UsernameExists(username))
            {
                return "Username already exists.";
            }
            if (EmailExists(email))
            {
                return "Email already exists.";
            }
            if (FindDealership(dealershipId) == null)
            {
                return "Dealership not found.";
            }
            return "";
        }

        // REQ-001: registers a new user. Returns "" on success, otherwise the error message.
        // Only customers can register themselves - salespeople are added by a manager (REQ-014).
        public string RegisterUser(string username, string password, string email, string phone,
                                   int dealershipId, string userType)
        {
            if (!IsValidUserType(userType))
            {
                return "Invalid user type.";
            }
            if (userType != "Customer")
            {
                return "Only customers can register. Salespeople are added by the dealership manager.";
            }

            string error = ValidateNewUser(username, password, email, phone, dealershipId);
            if (error != "")
            {
                return error;
            }

            // VFDN-126: save the user in the array (it is linked to the dealership by its dealership id)
            User user = CreateUser(username, password, email, phone, dealershipId, userType);
            AddUserToArray(user);
            return "";
        }

        // VFDN-160: registration screen
        private void RegisterMenu()
        {
            Console.WriteLine();
            Console.WriteLine("----- Register -----");
            if (!HasUserCapacity())
            {
                Console.WriteLine("The system is full, no more users can be added.");
                return;
            }

            Console.WriteLine("User type: 1. Customer  2. Salesperson  3. Manager");
            int typeChoice = ReadInt("Choose user type: ");
            string userType = "";
            if (typeChoice == 1)
            {
                userType = "Customer";
            }
            else if (typeChoice == 2)
            {
                userType = "Salesperson";
            }
            else if (typeChoice == 3)
            {
                userType = "Manager";
            }

            string username = ReadText("Username: ");
            string password = ReadText("Password: ");
            string email = ReadText("Email: ");
            string phone = ReadText("Phone: ");
            Console.WriteLine("Dealerships:");
            PrintDealerships();
            int dealershipId = ReadInt("Dealership number: ");

            string result = RegisterUser(username, password, email, phone, dealershipId, userType);
            if (result == "")
            {
                Console.WriteLine("Registration completed successfully. You can log in now.");
            }
            else
            {
                Console.WriteLine("Registration failed: " + result);
            }
        }

        // ===== Menu =====

        public void Run()
        {
            bool running = true;
            while (running)
            {
                if (currentUser == null)
                {
                    running = ShowMainMenu();
                }
                else
                {
                    running = ShowUserMenu();
                }
            }
            Console.WriteLine("Goodbye!");
        }

        // Menu for a user who is not logged in. Returns false to exit the program.
        private bool ShowMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Car Dealership System =====");
            Console.WriteLine("1. Register");
            Console.WriteLine("0. Exit");
            string choice = ReadText("Choose: ");

            if (choice == "1")
            {
                RegisterMenu();
            }
            else if (choice == "0")
            {
                return false;
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
            return true;
        }

        // Menu for a logged in user. Returns false to exit the program.
        private bool ShowUserMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Logged in as " + currentUser.GetUsername() + " (" + currentUser.GetUserType() + ") =====");
            Console.WriteLine("0. Exit");
            string choice = ReadText("Choose: ");

            if (choice == "0")
            {
                return false;
            }
            Console.WriteLine("Invalid choice.");
            return true;
        }

        // ===== Input helpers =====

        public static string ReadText(string prompt)
        {
            Console.Write(prompt);
            string text = Console.ReadLine();
            if (text == null)
            {
                return "";
            }
            return text.Trim();
        }

        // Returns -1 when the input is not a number
        public static int ReadInt(string prompt)
        {
            string text = ReadText(prompt);
            int number;
            if (int.TryParse(text, out number))
            {
                return number;
            }
            return -1;
        }
    }
}
