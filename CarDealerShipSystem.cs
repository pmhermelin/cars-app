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

        // ===== REQ-002: login (design 7.2) =====

        // Finds the user and checks the password. On success saves currentUser and returns the user,
        // otherwise returns null and currentUser stays as it was.
        public User LoginWith(string username, string password)
        {
            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetUsername() == username && users[i].CheckPassword(password))
                {
                    currentUser = users[i];
                    return currentUser;
                }
            }
            return null;
        }

        // Interactive login. The error message does not tell which field was wrong.
        public User Login()
        {
            Console.WriteLine("----- התחברות -----");
            string username = Input.ReadText("שם משתמש: ");
            string password = Input.ReadText("סיסמה: ");

            User user = LoginWith(username, password);
            if (user == null)
            {
                Console.WriteLine("✗ שם משתמש או סיסמה שגויים");
            }
            return user;
        }

        // ===== REQ-014: add salesperson (design 7.16) =====

        // The user must be the logged in user (design decision 15.12), a manager, and linked to a dealership
        private bool IsLoggedInManager(User user)
        {
            return user != null && user == currentUser && user.IsManager() && user.GetDealership() != null;
        }

        // Creates a salesperson in the manager's dealership after every rule was checked.
        // Returns false and changes nothing when any rule fails.
        public bool TryAddSalesperson(User manager, string username, string password, string email, string phone)
        {
            if (!IsLoggedInManager(manager))
            {
                return false;
            }
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

            User salesperson = new User(nextUserId, username, password, phone, email, ROLE_SALESPERSON);
            salesperson.SetDealership(manager.GetDealership());
            users[index] = salesperson;
            userCount++;
            nextUserId++;
            return true;
        }

        // Interactive: the same validation loops as registration (REQ-001); 0 cancels.
        public bool AddSalesperson(User manager)
        {
            if (!IsLoggedInManager(manager))
            {
                Console.WriteLine("✗ רק מנהל סוכנות מחובר יכול להוסיף איש מכירות");
                return false;
            }
            if (FindFreeUserIndex() == -1)
            {
                Console.WriteLine("✗ אין מקום להוספת משתמשים נוספים");
                return false;
            }

            Console.WriteLine("----- הוספת איש מכירות ל-" + manager.GetDealership().GetName() + " (0 לביטול) -----");

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

            return TryAddSalesperson(manager, username, password, email, phone);
        }

        // ===== REQ-006: inventory report (design 7.9) =====

        // Number of cars of a dealership with a given status
        public int CountCarsByStatus(CarDealership dealership, string status)
        {
            int count = 0;
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetDealership() == dealership && cars[i].GetStatus() == status)
                {
                    count++;
                }
            }
            return count;
        }

        // Prints the cars of the manager's dealership and a summary line by status.
        // The counters are local, so a repeated report never adds up on the previous one.
        public void PrintInventoryReport(User manager)
        {
            if (!IsLoggedInManager(manager))
            {
                Console.WriteLine("✗ רק מנהל סוכנות מחובר יכול להפיק דוח מלאי");
                return;
            }

            CarDealership dealership = manager.GetDealership();
            int total = 0;
            int available = 0;
            int reserved = 0;
            int sold = 0;
            int rented = 0;

            Console.WriteLine("===== דוח מלאי - " + dealership.GetName() + " =====");
            for (int i = 0; i < carCount; i++)
            {
                Car car = cars[i];
                if (car.GetDealership() == dealership)
                {
                    Console.WriteLine("Car #" + car.GetId() + " | " + car.GetCategory() + " | " + car.GetManufacturer() + " "
                                      + car.GetModel() + " (" + car.GetYear() + ") | " + car.GetPrice() + " NIS | " + car.GetStatus());
                    total++;
                    if (car.GetStatus() == STATUS_AVAILABLE)
                    {
                        available++;
                    }
                    else if (car.GetStatus() == STATUS_RESERVED)
                    {
                        reserved++;
                    }
                    else if (car.GetStatus() == STATUS_SOLD)
                    {
                        sold++;
                    }
                    else if (car.GetStatus() == STATUS_RENTED)
                    {
                        rented++;
                    }
                }
            }

            if (total == 0)
            {
                Console.WriteLine("אין רכבים במלאי!");
                return;
            }
            Console.WriteLine("סה\"כ: " + total + " | זמינים: " + available + " | שמורים: " + reserved
                              + " | נמכרו: " + sold + " | מושכרים: " + rented);
        }

        // ===== REQ-015: logout (design 7.17) =====

        // Resets currentUser. The data in the arrays is kept. Program then returns to the main menu.
        public void Logout()
        {
            currentUser = null;
            Console.WriteLine("✓ התנתקת בהצלחה");
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

        // REQ-003 (7.3) without keyboard input: checks every rule and only then creates the car.
        // Returns false and changes nothing when any rule fails.
        public bool TryAddNewCar(User user, string category, string manufacturer, string model, int year,
                                 int mileage, string licenseNumber, double price, string dealType, string location)
        {
            if (user == null || !(user.IsManager() || user.IsSalesperson()) || user.GetDealership() == null)
            {
                return false;
            }
            int index = FindFreeCarIndex();
            if (index == -1 || CountDealershipCars(user.GetDealership()) >= DEALERSHIP_CARS_LIMIT)
            {
                return false;
            }
            if (IsBlank(category) || IsBlank(manufacturer) || IsBlank(model) || IsBlank(location) || IsBlank(licenseNumber))
            {
                return false;
            }
            if (!IsValidYear(year) || mileage < 0 || price <= 0)
            {
                return false;
            }
            if (dealType != DEAL_SALE && dealType != DEAL_RENTAL && dealType != DEAL_BOTH)
            {
                return false;
            }
            if (FindCarByLicenseNumber(licenseNumber) != null)
            {
                return false;
            }

            cars[index] = new Car(GetNextCarId(), category, manufacturer, model, year, mileage,
                                  licenseNumber, price, dealType, location, user.GetDealership());
            carCount++;
            return true;
        }

        // Reads a text field until it is not blank. Returns "0" when the user cancels.
        private string ReadRequiredText(string prompt)
        {
            string text = Input.ReadText(prompt);
            while (!Input.IsCancel(text) && IsBlank(text))
            {
                Console.WriteLine("✗ השדה לא יכול להיות ריק");
                text = Input.ReadText(prompt);
            }
            return text;
        }

        // REQ-003 (7.3) interactive: every field is asked again until it is valid; 0 cancels.
        public bool AddNewCar(User user)
        {
            if (user == null || !(user.IsManager() || user.IsSalesperson()) || user.GetDealership() == null)
            {
                Console.WriteLine("✗ אין לך הרשאה להוסיף רכב");
                return false;
            }
            if (FindFreeCarIndex() == -1)
            {
                Console.WriteLine("✗ אין מקום להוספת רכבים נוספים במערכת");
                return false;
            }
            if (CountDealershipCars(user.GetDealership()) >= DEALERSHIP_CARS_LIMIT)
            {
                Console.WriteLine("✗ הסוכנות הגיעה למגבלת " + DEALERSHIP_CARS_LIMIT + " הרכבים");
                return false;
            }

            Console.WriteLine("----- הוספת רכב (0 לביטול) -----");
            string category = ReadRequiredText("קטגוריה: ");
            if (Input.IsCancel(category)) return false;
            string manufacturer = ReadRequiredText("יצרן: ");
            if (Input.IsCancel(manufacturer)) return false;
            string model = ReadRequiredText("דגם: ");
            if (Input.IsCancel(model)) return false;

            int year = Input.ReadInt("שנת ייצור: ");
            while (year != 0 && !IsValidYear(year))
            {
                Console.WriteLine("✗ שנה לא תקינה");
                year = Input.ReadInt("שנת ייצור: ");
            }
            if (year == 0) return false;

            int mileage = Input.ReadInt("קילומטראז': ");
            while (mileage < 0)
            {
                Console.WriteLine("✗ קילומטראז' חייב להיות מספר 0 ומעלה");
                mileage = Input.ReadInt("קילומטראז': ");
            }

            string licenseNumber = ReadRequiredText("מספר רישוי: ");
            if (Input.IsCancel(licenseNumber)) return false;
            if (FindCarByLicenseNumber(licenseNumber) != null)
            {
                Console.WriteLine("✗ מספר הרישוי כבר קיים במערכת");
                return false;
            }

            double price = Input.ReadDouble("מחיר: ");
            while (price < 0)
            {
                Console.WriteLine("✗ המחיר חייב להיות מספר גדול מאפס");
                price = Input.ReadDouble("מחיר: ");
            }
            if (price == 0) return false;

            Console.WriteLine("סוג עסקה: 1. " + DEAL_SALE + "  2. " + DEAL_RENTAL + "  3. " + DEAL_BOTH);
            int typeChoice = Input.ReadInt("בחר: ");
            while (typeChoice < 0 || typeChoice > 3)
            {
                Console.WriteLine("✗ בחירה לא חוקית");
                typeChoice = Input.ReadInt("בחר: ");
            }
            if (typeChoice == 0) return false;
            string dealType = DEAL_SALE;
            if (typeChoice == 2) dealType = DEAL_RENTAL;
            else if (typeChoice == 3) dealType = DEAL_BOTH;

            string location = ReadRequiredText("מיקום: ");
            if (Input.IsCancel(location)) return false;

            if (!TryAddNewCar(user, category, manufacturer, model, year, mileage, licenseNumber, price, dealType, location))
            {
                Console.WriteLine("✗ הרכב לא נוסף");
                return false;
            }
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

        // ===== REQ-007: change car price (design 7.10) =====

        // REQ-007 (7.10) without keyboard input: manager of the car's dealership, car Available, price > 0.
        // Returns false and keeps the old price when any rule fails.
        public bool TryChangeCarPrice(User manager, int carId, double newPrice)
        {
            if (manager == null || !manager.IsManager() || manager.GetDealership() == null)
            {
                return false;
            }
            Car car = FindCarById(carId);
            if (car == null || car.GetDealership() != manager.GetDealership() || !car.IsAvailable())
            {
                return false;
            }
            return car.SetPrice(newPrice); // SetPrice rejects price <= 0
        }

        // Prints the Available cars of a dealership and returns how many were printed
        private int PrintAvailableCarsOf(CarDealership dealership)
        {
            int printed = 0;
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetDealership() == dealership && cars[i].IsAvailable())
                {
                    Console.WriteLine(cars[i].ToString());
                    printed++;
                }
            }
            return printed;
        }

        // REQ-007 (7.10) interactive: shows the cars first, then asks for the id and the price; 0 cancels.
        public bool ChangeCarPrice(User manager)
        {
            if (manager == null || !manager.IsManager() || manager.GetDealership() == null)
            {
                Console.WriteLine("✗ רק מנהל סוכנות רשאי לשנות מחיר");
                return false;
            }
            if (PrintAvailableCarsOf(manager.GetDealership()) == 0)
            {
                Console.WriteLine("✗ אין בסוכנות רכבים זמינים לשינוי מחיר");
                return false;
            }

            int carId = Input.ReadInt("מזהה רכב (0 לביטול): ");
            if (carId == 0)
            {
                return false;
            }
            Car car = FindCarById(carId);
            if (car == null)
            {
                Console.WriteLine("✗ רכב לא נמצא");
                return false;
            }
            if (car.GetDealership() != manager.GetDealership())
            {
                Console.WriteLine("✗ הרכב אינו שייך לסוכנות שלך");
                return false;
            }
            if (!car.IsAvailable())
            {
                Console.WriteLine("✗ ניתן לשנות מחיר רק לרכב בסטטוס " + STATUS_AVAILABLE + " (סטטוס נוכחי: " + car.GetStatus() + ")");
                return false;
            }

            double newPrice = Input.ReadDouble("מחיר חדש (0 לביטול): ");
            while (newPrice < 0)
            {
                Console.WriteLine("✗ המחיר חייב להיות מספר גדול מאפס");
                newPrice = Input.ReadDouble("מחיר חדש (0 לביטול): ");
            }
            if (newPrice == 0)
            {
                return false;
            }

            if (!TryChangeCarPrice(manager, carId, newPrice))
            {
                Console.WriteLine("✗ המחיר לא עודכן");
                return false;
            }
            Console.WriteLine("✓ המחיר עודכן בהצלחה");
            return true;
        }

        // ===== REQ-009: search and filter cars (design 7.12) =====

        // A blank filter matches everything; otherwise compares without case and surrounding spaces
        private bool TextMatches(string value, string filter)
        {
            if (IsBlank(filter))
            {
                return true;
            }
            return value != null && value.Trim().ToLower() == filter.Trim().ToLower();
        }

        public int SearchCars(string category, string manufacturer, double minPrice, double maxPrice,
                              int minYear, string dealType, Car[] results)
        {
            int count = 0;
            for (int i = 0; i < carCount && count < results.Length; i++)
            {
                Car car = cars[i];
                if (!car.IsAvailable()) continue;
                if (!TextMatches(car.GetCategory(), category)) continue;
                if (!TextMatches(car.GetManufacturer(), manufacturer)) continue;
                if (minPrice > 0 && car.GetPrice() < minPrice) continue;
                if (maxPrice > 0 && car.GetPrice() > maxPrice) continue;
                if (minYear > 0 && car.GetYear() < minYear) continue;
                if (!IsBlank(dealType) && !car.SupportsDealType(dealType)) continue;

                results[count] = car;
                count++;
            }
            return count;
        }

        // ===== REQ-010: view available cars (design 7.13) =====

        // Number of Available cars in a dealership, or in the whole system when dealership is null
        public int CountAvailableCars(CarDealership dealership)
        {
            int count = 0;
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].IsAvailable() && (dealership == null || cars[i].GetDealership() == dealership))
                {
                    count++;
                }
            }
            return count;
        }

        public void PrintAvailableCars()
        {
            int totalShown = 0;
            for (int d = 0; d < dealershipCount; d++)
            {
                CarDealership dealership = dealerships[d];
                if (CountAvailableCars(dealership) == 0)
                {
                    continue; // a dealership without available cars gets no empty header
                }

                Console.WriteLine("--- " + dealership.GetName() + " (" + dealership.GetDealershipType() + ") ---");
                for (int i = 0; i < carCount; i++)
                {
                    Car car = cars[i];
                    if (car.GetDealership() == dealership && car.IsAvailable())
                    {
                        Console.WriteLine(car.ToString());
                        totalShown++;
                    }
                }
            }

            if (totalShown == 0)
            {
                Console.WriteLine("אין רכבים זמינים במערכת!");
            }
            else
            {
                Console.WriteLine("סה\"כ רכבים זמינים: " + totalShown); // design 7.13 step 4
            }
        }

        // ===== REQ-013: dealership inventory for salesperson (design 7.15) =====

        public void PrintDealershipInventory(User user)
        {
            if (user == null || !user.IsSalesperson() || user.GetDealership() == null)
            {
                Console.WriteLine("✗ פעולה זו זמינה רק לאיש מכירות עם סוכנות משויכת");
                return;
            }

            CarDealership dealership = user.GetDealership();
            if (CountDealershipCars(dealership) == 0)
            {
                Console.WriteLine("אין רכבים במלאי הסוכנות!");
                return;
            }

            Console.WriteLine("===== מלאי " + dealership.GetName() + " =====");
            for (int i = 0; i < carCount; i++)
            {
                if (cars[i].GetDealership() == dealership)
                {
                    Console.WriteLine(cars[i].ToString()); // every status: Available, Reserved, Sold, Rented
                }
            }
        }

        // ===== REQ-011: Orders (design 7.5) - Yehuda =====

        // Design 8: cars in the customer's active (Pending or Approved) purchase orders.
        // Counts cars, not orders.
        public int CountActivePurchaseCars(User customer)
        {
            return CountActiveCars(customer, DEAL_SALE);
        }

        // Design 8: cars in the customer's active (Pending or Approved) rental orders
        public int CountActiveRentalCars(User customer)
        {
            return CountActiveCars(customer, DEAL_RENTAL);
        }

        private int CountActiveCars(User customer, string orderType)
        {
            int count = 0;
            for (int i = 0; i < orderCount; i++)
            {
                Order order = orders[i];
                bool active = order.GetStatus() == ORDER_PENDING || order.GetStatus() == ORDER_APPROVED;
                if (active && order.BelongsTo(customer) && order.GetOrderType() == orderType)
                {
                    count = count + order.GetCarCount();
                }
            }
            return count;
        }

        // Step 1: who the order is for. A customer orders for himself; a salesperson orders for a customer.
        // Returns false for any other role.
        private bool CanOrderFor(User user, User customer)
        {
            if (user == null || customer == null || !customer.IsCustomer())
            {
                return false;
            }
            if (user.IsCustomer())
            {
                return user == customer;
            }
            return user.IsSalesperson() && user.GetDealership() != null;
        }

        // Steps 3-4: Sale is always 1 car, Rental is 1 to MAX_CARS_PER_ORDER
        private bool IsValidOrderCount(string orderType, int count)
        {
            if (orderType == DEAL_SALE)
            {
                return count == 1;
            }
            if (orderType == DEAL_RENTAL)
            {
                return count >= 1 && count <= Order.MAX_CARS_PER_ORDER;
            }
            return false;
        }

        // Step 5: the customer limits, checked before any car is chosen.
        // Returns "" when the order is allowed, otherwise the message to show.
        private string CheckCustomerLimits(User customer, string orderType, int count)
        {
            if (orderType == DEAL_SALE && CountActivePurchaseCars(customer) >= 1)
            {
                return "✗ ללקוח כבר יש רכב ברכישה פעילה";
            }
            if (orderType == DEAL_RENTAL && count + CountActiveRentalCars(customer) > Order.MAX_CARS_PER_ORDER)
            {
                return "✗ חריגה ממגבלת ההשכרה - עד " + Order.MAX_CARS_PER_ORDER + " רכבים בהשכרה פעילה";
            }
            return "";
        }

        // Steps 6-8: can this car join the order being built?
        // Returns "" when it can, otherwise the reason it was not added.
        private string CheckCarForOrder(User user, Car car, string orderType, Car[] chosen, int chosenCount)
        {
            if (car == null)
            {
                return "✗ רכב לא נמצא";
            }
            if (!car.IsAvailable())
            {
                return "✗ הרכב אינו זמין";
            }
            if (!car.SupportsDealType(orderType))
            {
                return "✗ הרכב אינו מוצע לסוג העסקה הזה";
            }
            for (int i = 0; i < chosenCount; i++)
            {
                if (chosen[i] == car)
                {
                    return "✗ הרכב כבר נבחר להזמנה";
                }
            }
            if (chosenCount > 0 && car.GetDealership() != chosen[0].GetDealership())
            {
                return "✗ כל הרכבים בהזמנה חייבים להיות מאותה סוכנות";
            }
            if (user.IsSalesperson() && car.GetDealership() != user.GetDealership())
            {
                return "✗ איש מכירות יוצר הזמנות רק על מלאי הסוכנות שלו";
            }
            return "";
        }

        // Steps 10-12: creates the Pending order and only then reserves its cars
        private bool SaveOrder(User customer, string orderType, Car[] chosen, int chosenCount)
        {
            int index = FindFreeOrderIndex();
            if (index == -1 || chosenCount == 0)
            {
                return false;
            }
            Order order = new Order(nextOrderNumber, customer, orderType);
            for (int i = 0; i < chosenCount; i++)
            {
                order.AddCar(chosen[i]);
            }
            for (int i = 0; i < chosenCount; i++)
            {
                chosen[i].MarkAsReserved();
            }
            orders[index] = order;
            orderCount++;
            nextOrderNumber++;
            return true;
        }

        // REQ-011 without keyboard input (used by the tests). carIds holds count car ids.
        // Every rule is checked before any car changes status; false changes nothing.
        public bool TryConfirmOrder(User user, User customer, string orderType, int[] carIds, int count)
        {
            if (!CanOrderFor(user, customer))
            {
                return false;
            }
            if (FindFreeOrderIndex() == -1)
            {
                return false;
            }
            if (carIds == null || count > carIds.Length || !IsValidOrderCount(orderType, count))
            {
                return false;
            }
            if (CheckCustomerLimits(customer, orderType, count) != "")
            {
                return false;
            }

            Car[] chosen = new Car[Order.MAX_CARS_PER_ORDER];
            int chosenCount = 0;
            for (int i = 0; i < count; i++)
            {
                Car car = FindCarById(carIds[i]);
                if (CheckCarForOrder(user, car, orderType, chosen, chosenCount) == "")
                {
                    chosen[chosenCount] = car;
                    chosenCount++;
                }
            }
            return SaveOrder(customer, orderType, chosen, chosenCount);
        }

        // REQ-011 interactive (design 7.5): customer menu 3, salesperson menu 4. 0 cancels.
        public bool ConfirmOrder(User user)
        {
            User customer = null;
            if (user != null && user.IsCustomer())
            {
                customer = user;
            }
            else if (user != null && user.IsSalesperson() && user.GetDealership() != null)
            {
                string username = Input.ReadText("שם המשתמש של הלקוח (0 לביטול): ");
                if (Input.IsCancel(username))
                {
                    return false;
                }
                customer = FindUserByUsername(username);
                if (customer == null || !customer.IsCustomer())
                {
                    Console.WriteLine("✗ לקוח לא נמצא");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("✗ אין הרשאה לבצע הזמנה");
                return false;
            }

            if (FindFreeOrderIndex() == -1)
            {
                Console.WriteLine("✗ אין מקום להזמנות נוספות");
                return false;
            }

            int typeChoice = Input.ReadInt("סוג עסקה: 1 = רכישה, 2 = השכרה (0 לביטול): ");
            while (typeChoice != 0 && typeChoice != 1 && typeChoice != 2)
            {
                Console.WriteLine("✗ בחירה לא חוקית");
                typeChoice = Input.ReadInt("סוג עסקה: 1 = רכישה, 2 = השכרה (0 לביטול): ");
            }
            if (typeChoice == 0)
            {
                return false;
            }
            string orderType = DEAL_SALE;
            if (typeChoice == 2)
            {
                orderType = DEAL_RENTAL;
            }

            int count = 1;
            if (orderType == DEAL_RENTAL)
            {
                count = Input.ReadInt("כמה רכבים (1-" + Order.MAX_CARS_PER_ORDER + "): ");
                while (count != 0 && !IsValidOrderCount(orderType, count))
                {
                    Console.WriteLine("✗ כמות לא חוקית");
                    count = Input.ReadInt("כמה רכבים (1-" + Order.MAX_CARS_PER_ORDER + "): ");
                }
                if (count == 0)
                {
                    return false;
                }
            }

            string limitError = CheckCustomerLimits(customer, orderType, count);
            if (limitError != "")
            {
                Console.WriteLine(limitError);
                return false;
            }

            if (PrintCarsForOrder(user, orderType) == 0)
            {
                Console.WriteLine("✗ אין רכבים זמינים לסוג העסקה הזה");
                return false;
            }

            Car[] chosen = new Car[Order.MAX_CARS_PER_ORDER];
            int chosenCount = 0;
            for (int i = 0; i < count; i++)
            {
                int carId = Input.ReadInt("מזהה רכב " + (i + 1) + " (0 לביטול): ");
                if (carId == 0)
                {
                    return false; // nothing was reserved yet
                }
                Car car = FindCarById(carId);
                string carError = CheckCarForOrder(user, car, orderType, chosen, chosenCount);
                if (carError == "")
                {
                    chosen[chosenCount] = car;
                    chosenCount++;
                }
                else
                {
                    Console.WriteLine(carError);
                }
            }

            if (chosenCount == 0)
            {
                Console.WriteLine("✗ לא נבחר אף רכב תקין");
                return false;
            }
            return SaveOrder(customer, orderType, chosen, chosenCount);
        }

        // Prints the available cars for the deal type (a salesperson sees only his dealership).
        // Returns how many were printed.
        private int PrintCarsForOrder(User user, string orderType)
        {
            int printed = 0;
            for (int i = 0; i < carCount; i++)
            {
                Car car = cars[i];
                bool ownStock = !user.IsSalesperson() || car.GetDealership() == user.GetDealership();
                if (car.IsAvailable() && car.SupportsDealType(orderType) && ownStock)
                {
                    Console.WriteLine(car.ToString() + " | " + car.GetDealership().GetName());
                    printed++;
                }
            }
            return printed;
        }

        // ===== REQ-004: Cancel order (design 7.6) - Yehuda =====

        // Prints an order the way the requirements show it: the order line, the customer, the total and its cars.
        // Built here from getters - Order.ToString() prints only its own fields (design 6.7).
        private void PrintOrderDetails(Order order)
        {
            Console.WriteLine(order.ToString() + " | " + order.GetCustomer().GetUsername()
                              + " | " + order.GetTotalPrice() + " NIS");
            for (int i = 0; i < order.GetCarCount(); i++)
            {
                Console.WriteLine("    " + order.GetCar(i).ToString());
            }
        }

        // Prints the customer's Pending orders and returns how many were printed
        private int PrintPendingOrdersOf(User customer)
        {
            int printed = 0;
            for (int i = 0; i < orderCount; i++)
            {
                if (orders[i].BelongsTo(customer) && orders[i].IsPending())
                {
                    PrintOrderDetails(orders[i]);
                    printed++;
                }
            }
            return printed;
        }

        // REQ-004 without keyboard input (used by the tests).
        // The cars are released only after order.Cancel() succeeded; false changes nothing.
        public bool TryCancelOrder(User customer, int orderNumber)
        {
            if (customer == null || !customer.IsCustomer())
            {
                return false;
            }
            Order order = FindOrderByNumber(orderNumber);
            if (order == null || !order.BelongsTo(customer))
            {
                return false;
            }
            if (!order.Cancel())
            {
                return false;
            }
            for (int i = 0; i < order.GetCarCount(); i++)
            {
                order.GetCar(i).MakeAvailable();
            }
            return true;
        }

        // REQ-004 interactive (design 7.6): customer menu 5. 0 cancels.
        public bool CancelOrder(User customer)
        {
            if (customer == null || !customer.IsCustomer())
            {
                Console.WriteLine("✗ רק לקוח יכול לבטל הזמנה");
                return false;
            }
            if (PrintPendingOrdersOf(customer) == 0)
            {
                Console.WriteLine("✗ אין לך הזמנות ממתינות לביטול");
                return false;
            }

            int orderNumber = Input.ReadInt("מספר הזמנה לביטול (0 לביטול): ");
            if (orderNumber == 0)
            {
                return false;
            }
            Order order = FindOrderByNumber(orderNumber);
            if (order == null || !order.BelongsTo(customer))
            {
                Console.WriteLine("✗ הזמנה לא נמצאה");
                return false;
            }
            if (!order.IsPending())
            {
                Console.WriteLine("✗ אפשר לבטל רק הזמנה שממתינה לאישור");
                return false;
            }
            return TryCancelOrder(customer, orderNumber);
        }
    }
}
