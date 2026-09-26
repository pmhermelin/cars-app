using System;

namespace CarsApp
{
    // Design 6.5: the central service class. Holds the four arrays, the counters and the id generators,
    // and contains all the business logic and permission checks. Program talks only to this class.
    public class CarDealerShipSystem
    {
        // ===== Array sizes (design 3.4) =====
        public const int USERS_MAX = 100;
        public const int DEALERSHIPS_MAX = 5;
        public const int CARS_MAX = 5000;
        public const int ORDERS_MAX = 500;
        public const int DEALERSHIP_CARS_LIMIT = 1000;
        public const int PASSWORD_MIN_LENGTH = 8;

        // ===== Roles, statuses and deal types - defined once (design 2.3, 16.3) =====
        public const string ROLE_CUSTOMER = "Customer";
        public const string ROLE_MANAGER = "Manager";
        public const string ROLE_SALESPERSON = "Salesperson";

        public const string STATUS_AVAILABLE = "Available";
        public const string STATUS_RESERVED = "Reserved";
        public const string STATUS_SOLD = "Sold";
        public const string STATUS_RENTED = "Rented";

        public const string DEAL_SALE = "Sale";
        public const string DEAL_RENTAL = "Rental";
        public const string DEAL_BOTH = "Both";

        public const string ORDER_PENDING = "Pending";
        public const string ORDER_APPROVED = "Approved";
        public const string ORDER_REJECTED = "Rejected";
        public const string ORDER_CANCELLED = "Cancelled";

        public const string TYPE_CARS = "Cars";
        public const string TYPE_MOTORCYCLES = "Motorcycles";

        private User[] users;
        private int userCount;
        private CarDealership[] dealerships;
        private int dealershipCount;
        private Car[] cars;
        private int carCount;
        private Order[] orders;
        private int orderCount;
        private int nextUserId;
        private int nextCarId;
        private int nextOrderNumber;
        private User currentUser;

        // Constructor: arrays, counters, id generators and the 5 seed dealerships (design 6.5).
        // No users and no cars are created here.
        public CarDealerShipSystem()
        {
            users = new User[USERS_MAX];
            dealerships = new CarDealership[DEALERSHIPS_MAX];
            cars = new Car[CARS_MAX];
            orders = new Order[ORDERS_MAX];
            userCount = 0;
            carCount = 0;
            orderCount = 0;
            nextUserId = 1;
            nextCarId = 1;
            nextOrderNumber = 1;

            dealerships[0] = new CarDealership(1, "Toyota Haifa", TYPE_CARS, "חיפה", "04-0000001");
            dealerships[1] = new CarDealership(2, "Kia Tel Aviv", TYPE_CARS, "תל אביב", "03-0000002");
            dealerships[2] = new CarDealership(3, "Mazda Jerusalem", TYPE_CARS, "ירושלים", "02-0000003");
            dealerships[3] = new CarDealership(4, "Moto Center Haifa", TYPE_MOTORCYCLES, "חיפה", "04-0000004");
            dealerships[4] = new CarDealership(5, "Bike Motors Beer Sheva", TYPE_MOTORCYCLES, "באר שבע", "08-0000005");
            dealershipCount = 5;

            currentUser = null;
        }

        // ===== Access methods for Program =====

        public int GetCarCount()
        {
            return carCount;
        }

        public int GetUserCount()
        {
            return userCount;
        }

        public int GetOrderCount()
        {
            return orderCount;
        }

        public User GetCurrentUser()
        {
            return currentUser;
        }

        // The number the next order will get (used by ConfirmOrder, REQ-011)
        public int GetNextOrderNumber()
        {
            return nextOrderNumber;
        }

        // ===== Design 8: free index search (design 4.4) =====

        private int FindFreeUserIndex()
        {
            for (int i = 0; i < users.Length; i++)
            {
                if (users[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindFreeCarIndex()
        {
            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindFreeOrderIndex()
        {
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasFreeUserSlot()
        {
            return FindFreeUserIndex() != -1;
        }

        // ===== Design 8: search helpers =====

        public User FindUserByUsername(string username)
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

        public Car FindCarById(int id)
        {
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetId() == id)
                {
                    return cars[i];
                }
            }
            return null;
        }

        public Car FindCarByLicenseNumber(string licenseNumber)
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

        public Order FindOrderByNumber(int orderNumber)
        {
            for (int i = 0; i < orderCount; i++)
            {
                if (orders[i].GetOrderNumber() == orderNumber)
                {
                    return orders[i];
                }
            }
            return null;
        }

        public CarDealership FindDealershipById(int id)
        {
            for (int i = 0; i < dealershipCount; i++)
            {
                if (dealerships[i].GetId() == id)
                {
                    return dealerships[i];
                }
            }
            return null;
        }

        // Design 8: number of cars of a dealership (for the 1000 cars limit)
        public int CountDealershipCars(CarDealership dealership)
        {
            int count = 0;
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetDealership() == dealership)
                {
                    count++;
                }
            }
            return count;
        }

        // Adds a ready car object to the inventory. Used by the tests to prepare data
        // before AddNewCar (REQ-003) is implemented.
        internal bool AddCarToInventory(Car car)
        {
            int index = FindFreeCarIndex();
            if (index == -1)
            {
                return false;
            }
            cars[index] = car;
            carCount++;
            return true;
        }

        public int GetNextCarId()
        {
            return nextCarId++;
        }

        // ===== Design 8: input validation =====

        public static bool IsBlank(string text)
        {
            return text == null || text.Trim().Length == 0;
        }

        public static bool IsValidYear(int year)
        {
            return year >= 1950 && year <= DateTime.Now.Year + 1;
        }

        public bool UsernameExists(string username)
        {
            return FindUserByUsername(username) != null;
        }

        // At least PASSWORD_MIN_LENGTH characters, a digit and a special character from $, %, _
        public bool IsStrongPassword(string password)
        {
            if (password == null || password.Length < PASSWORD_MIN_LENGTH)
            {
                return false;
            }

            bool hasDigit = false;
            bool hasSpecial = false;
            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                if (c == '$' || c == '%' || c == '_')
                {
                    hasSpecial = true;
                }
            }
            return hasDigit && hasSpecial;
        }

        // Contains '@' (not first) and a '.' after it (not last)
        public bool IsValidEmail(string email)
        {
            if (IsBlank(email) || email.IndexOf(' ') != -1)
            {
                return false;
            }
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0)
            {
                return false;
            }
            int dotIndex = email.IndexOf('.', atIndex);
            return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
        }

        // 10 digits starting with 05
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

        // ===== REQ-001: registration (design 7.1) =====

        // Creates a user after every rule was checked. A manager gets a dealership that has no manager yet.
        // Returns false and changes nothing when any rule fails.
        public bool TryCreateUser(string username, string password, string email, string phone,
                                  string role, CarDealership dealership)
        {
            int index = FindFreeUserIndex();
            if (index == -1)
            {
                return false;
            }
            if (IsBlank(username) || UsernameExists(username))
            {
                return false;
            }
            if (!IsStrongPassword(password) || !IsValidEmail(email) || !IsValidPhone(phone))
            {
                return false;
            }
            if (role != ROLE_CUSTOMER && role != ROLE_MANAGER)
            {
                return false; // salespeople are created only by a manager (REQ-014)
            }
            if (role == ROLE_MANAGER && (dealership == null || dealership.HasOwner()))
            {
                return false;
            }

            User user = new User(nextUserId, username, password, phone, email, role);
            if (role == ROLE_MANAGER)
            {
                dealership.SetOwner(user);
                user.SetDealership(dealership);
            }
            users[index] = user;
            userCount++;
            nextUserId++;
            return true;
        }

        // Interactive registration: every field is asked again until it is valid; 0 cancels.
        public bool CreateUser()
        {
            if (FindFreeUserIndex() == -1)
            {
                Console.WriteLine("✗ אין מקום להוספת משתמשים נוספים");
                return false;
            }

            Console.WriteLine("----- הרשמה (0 לביטול) -----");

            string username = Input.ReadText("שם משתמש: ");
            while (!Input.IsCancel(username) && (IsBlank(username) || UsernameExists(username)))
            {
                Console.WriteLine("✗ שם המשתמש ריק או תפוס, נסה שוב");
                username = Input.ReadText("שם משתמש: ");
            }
            if (Input.IsCancel(username))
            {
                return false;
            }

            string password = Input.ReadText("סיסמה: ");
            while (!Input.IsCancel(password) && !IsStrongPassword(password))
            {
                Console.WriteLine("✗ הסיסמה חייבת להכיל לפחות " + PASSWORD_MIN_LENGTH + " תווים, ספרה ותו מיוחד ($, %, _)");
                password = Input.ReadText("סיסמה: ");
            }
            if (Input.IsCancel(password))
            {
                return false;
            }

            string email = Input.ReadText("דוא\"ל: ");
            while (!Input.IsCancel(email) && !IsValidEmail(email))
            {
                Console.WriteLine("✗ כתובת דוא\"ל לא תקינה");
                email = Input.ReadText("דוא\"ל: ");
            }
            if (Input.IsCancel(email))
            {
                return false;
            }

            string phone = Input.ReadText("טלפון: ");
            while (!Input.IsCancel(phone) && !IsValidPhone(phone))
            {
                Console.WriteLine("✗ מספר טלפון לא תקין (10 ספרות, מתחיל ב-05)");
                phone = Input.ReadText("טלפון: ");
            }
            if (Input.IsCancel(phone))
            {
                return false;
            }

            int roleChoice = Input.ReadInt("תפקיד: 1 = לקוח, 2 = מנהל סוכנות: ");
            while (roleChoice != 0 && roleChoice != 1 && roleChoice != 2)
            {
                Console.WriteLine("✗ בחירה לא חוקית");
                roleChoice = Input.ReadInt("תפקיד: 1 = לקוח, 2 = מנהל סוכנות: ");
            }
            if (roleChoice == 0)
            {
                return false;
            }

            string role = ROLE_CUSTOMER;
            CarDealership dealership = null;
            if (roleChoice == 2)
            {
                role = ROLE_MANAGER;
                int freeCount = PrintDealershipsWithoutOwner();
                if (freeCount == 0)
                {
                    Console.WriteLine("✗ אין סוכנות פנויה - לכל הסוכנויות כבר יש מנהל");
                    return false;
                }
                int dealershipId = Input.ReadInt("מספר סוכנות: ");
                dealership = FindDealershipById(dealershipId);
                while (dealershipId != 0 && (dealership == null || dealership.HasOwner()))
                {
                    Console.WriteLine("✗ יש לבחור סוכנות פנויה מהרשימה");
                    dealershipId = Input.ReadInt("מספר סוכנות: ");
                    dealership = FindDealershipById(dealershipId);
                }
                if (dealershipId == 0)
                {
                    return false;
                }
            }

            return TryCreateUser(username, password, email, phone, role, dealership);
        }

        // Prints the dealerships that have no manager and returns how many were printed
        private int PrintDealershipsWithoutOwner()
        {
            int count = 0;
            for (int i = 0; i < dealershipCount; i++)
            {
                if (!dealerships[i].HasOwner())
                {
                    Console.WriteLine("Agency #" + dealerships[i].GetId() + " | " + dealerships[i].ToString());
                    count++;
                }
            }
            return count;
        }
        
    // ===== REQ-003: add and update cars (design 7.3-7.4) =====

    // REQ-003 (7.3): adds a new car to the dealership of the current user (manager or salesperson).
    public bool AddNewCar(User user)
    {
        if ((user.GetRole() != ROLE_MANAGER && user.GetRole() != ROLE_SALESPERSON) || user.GetDealership() == null)
        {
            Console.WriteLine("✗ אין לך הרשאה להוסיף רכב");
            return false;
        }

        int index = FindFreeCarIndex();
        if (index == -1)
        {
            Console.WriteLine("✗ אין מקום להוספת רכבים נוספים במערכת");
            return false;
        }

        if (CountDealershipCars(user.GetDealership()) >= DEALERSHIP_CARS_LIMIT)
        {
            Console.WriteLine("✗ הסוכנות הגיעה למגבלת " + DEALERSHIP_CARS_LIMIT + " הרכבים");
            return false;
        }

        string category = Input.ReadText("קטגוריה: ");
        string manufacturer = Input.ReadText("יצרן: ");
        string model = Input.ReadText("דגם: ");

        int year = Input.ReadInt("שנת ייצור: ");
        while (!IsValidYear(year))
        {
            Console.WriteLine("✗ שנה לא תקינה");
            year = Input.ReadInt("שנת ייצור: ");
        }

        int mileage = Input.ReadInt("קילומטראז': ");
        string licenseNumber = Input.ReadText("מספר רישוי: ");

        if (FindCarByLicenseNumber(licenseNumber) != null)
        {
            Console.WriteLine("✗ מספר הרישוי כבר קיים במערכת");
            return false;
        }

        double price = Input.ReadDouble("מחיר: ");
        Console.WriteLine("סוג עסקה: 1. " + DEAL_SALE + "  2. " + DEAL_RENTAL + "  3. " + DEAL_BOTH);
        int typeChoice = Input.ReadInt("בחר: ");
        string dealType = "";
        if (typeChoice == 1) dealType = DEAL_SALE;
        else if (typeChoice == 2) dealType = DEAL_RENTAL;
        else if (typeChoice == 3) dealType = DEAL_BOTH;

        string location = Input.ReadText("מיקום: ");

        Car car = new Car(GetNextCarId(), category, manufacturer, model, year, mileage,
                           licenseNumber, price, dealType, location, user.GetDealership());
        cars[index] = car;
        carCount++;

        Console.WriteLine("✓ הרכב נוסף בהצלחה");
        return true;
    }

    // REQ-003 (7.4): updates an existing car (not price - that's REQ-007).
    public bool UpdateCar(User user)
    {
        if ((user.GetRole() != ROLE_MANAGER && user.GetRole() != ROLE_SALESPERSON) || user.GetDealership() == null)
        {
            Console.WriteLine("✗ אין לך הרשאה לעדכן רכב");
            return false;
        }

        string licenseNumber = Input.ReadText("מספר רישוי של הרכב לעדכון (0 לביטול): ");
        if (Input.IsCancel(licenseNumber))
        {
            return false;
        }

        Car car = FindCarByLicenseNumber(licenseNumber);
        if (car == null)
        {
            Console.WriteLine("✗ רכב לא נמצא");
            return false;
        }
        if (car.GetDealership() != user.GetDealership())
        {
            Console.WriteLine("✗ הרכב אינו שייך לסוכנות שלך");
            return false;
        }

        Console.WriteLine("מה לעדכן? 1-יצרן 2-דגם 3-שנה 4-ק\"מ 5-קטגוריה 6-מיקום");
        int choice = Input.ReadInt("בחר: ");
        bool ok = false;

        if (choice == 1) ok = car.SetManufacturer(Input.ReadText("יצרן חדש: "));
        else if (choice == 2) ok = car.SetModel(Input.ReadText("דגם חדש: "));
        else if (choice == 3) ok = car.SetYear(Input.ReadInt("שנה חדשה: "));
        else if (choice == 4) ok = car.SetMileage(Input.ReadInt("ק\"מ חדש: "));
        else if (choice == 5) ok = car.SetCategory(Input.ReadText("קטגוריה חדשה: "));
        else if (choice == 6) ok = car.SetLocation(Input.ReadText("מיקום חדש: "));
        else
        {
            Console.WriteLine("✗ בחירה לא חוקית");
            return false;
        }

        if (!ok)
        {
            Console.WriteLine("✗ העדכון נכשל — הערך שהוזן אינו תקין");
            return false;
        }

        Console.WriteLine("✓ הרכב עודכן בהצלחה");
        return true;
    }
    }
}
