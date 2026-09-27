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

        // ===== REQ-005: Approve / reject orders (design 7.7-7.8) - Yehuda =====

        private bool IsManagerWithDealership(User user)
        {
            return user != null && user.IsManager() && user.GetDealership() != null;
        }

        // Steps 1-3 shared by approve and reject: the order exists and belongs to the manager's dealership.
        // Returns null otherwise.
        private Order FindOrderOfManager(User manager, int orderNumber)
        {
            if (!IsManagerWithDealership(manager))
            {
                return null;
            }
            Order order = FindOrderByNumber(orderNumber);
            if (order == null || order.GetDealership() == null || !order.GetDealership().IsOwner(manager))
            {
                return null;
            }
            return order;
        }

        // Prints the Pending orders of the manager's dealership and returns how many were printed.
        // Orders of other dealerships are never shown.
        public int PrintPendingOrdersOfDealership(User manager)
        {
            if (!IsManagerWithDealership(manager))
            {
                return 0;
            }
            int printed = 0;
            for (int i = 0; i < orderCount; i++)
            {
                if (orders[i].IsPending() && orders[i].GetDealership() == manager.GetDealership())
                {
                    PrintOrderDetails(orders[i]);
                    printed++;
                }
            }
            return printed;
        }

        // Reads an order number of the manager's dealership. Returns null on 0 or an unknown order.
        private Order ReadOrderOfManager(User manager)
        {
            int orderNumber = Input.ReadInt("מספר הזמנה (0 לביטול): ");
            if (orderNumber == 0)
            {
                return null;
            }
            Order order = FindOrderOfManager(manager, orderNumber);
            if (order == null)
            {
                Console.WriteLine("✗ הזמנה לא נמצאה בסוכנות שלך");
            }
            return order;
        }

        // REQ-005 approve without keyboard input (used by the tests).
        // The order moves to Approved first; only then Sale cars become Sold and Rental cars become Rented.
        public bool TryApproveOrder(User manager, int orderNumber)
        {
            Order order = FindOrderOfManager(manager, orderNumber);
            if (order == null || !order.Approve())
            {
                return false;
            }
            for (int i = 0; i < order.GetCarCount(); i++)
            {
                if (order.GetOrderType() == DEAL_SALE)
                {
                    order.GetCar(i).MarkAsSold();
                }
                else
                {
                    order.GetCar(i).MarkAsRented();
                }
            }
            return true;
        }

        // REQ-005 approve interactive (design 7.7): manager menu 4
        public bool ApproveOrder(User manager)
        {
            if (!IsManagerWithDealership(manager))
            {
                Console.WriteLine("✗ רק מנהל סוכנות יכול לאשר עסקאות");
                return false;
            }
            if (PrintPendingOrdersOfDealership(manager) == 0)
            {
                Console.WriteLine("✗ אין הזמנות ממתינות בסוכנות שלך");
                return false;
            }
            Order order = ReadOrderOfManager(manager);
            if (order == null)
            {
                return false;
            }
            if (!order.IsPending())
            {
                Console.WriteLine("✗ ההזמנה אינה ממתינה לאישור");
                return false;
            }
            return TryApproveOrder(manager, order.GetOrderNumber());
        }

        // REQ-005 reject without keyboard input (used by the tests).
        // The order moves to Rejected first; only then its cars are released to Available.
        public bool TryRejectOrder(User manager, int orderNumber)
        {
            Order order = FindOrderOfManager(manager, orderNumber);
            if (order == null || !order.Reject())
            {
                return false;
            }
            for (int i = 0; i < order.GetCarCount(); i++)
            {
                order.GetCar(i).MakeAvailable();
            }
            return true;
        }

        // REQ-005 reject interactive (design 7.8): manager menu 4
        public bool RejectOrder(User manager)
        {
            if (!IsManagerWithDealership(manager))
            {
                Console.WriteLine("✗ רק מנהל סוכנות יכול לדחות עסקאות");
                return false;
            }
            if (PrintPendingOrdersOfDealership(manager) == 0)
            {
                Console.WriteLine("✗ אין הזמנות ממתינות בסוכנות שלך");
                return false;
            }
            Order order = ReadOrderOfManager(manager);
            if (order == null)
            {
                return false;
            }
            if (!order.IsPending())
            {
                Console.WriteLine("✗ ההזמנה אינה ממתינה לאישור");
                return false;
            }
            return TryRejectOrder(manager, order.GetOrderNumber());
        }

        // ===== REQ-008: Sold or rented cars (design 7.11) - Yehuda =====

        // True when the car belongs to the dealership and was really sold or rented
        private bool IsClosedDealCar(Car car, CarDealership dealership)
        {
            bool closed = car.GetStatus() == STATUS_SOLD || car.GetStatus() == STATUS_RENTED;
            return closed && car.GetDealership() == dealership;
        }

        // Number of the dealership's cars in Approved orders that are Sold or Rented (used by the tests)
        public int CountSoldOrRentedCars(CarDealership dealership)
        {
            int count = 0;
            for (int i = 0; i < orderCount; i++)
            {
                if (orders[i].GetStatus() != ORDER_APPROVED)
                {
                    continue;
                }
                for (int j = 0; j < orders[i].GetCarCount(); j++)
                {
                    if (IsClosedDealCar(orders[i].GetCar(j), dealership))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        // REQ-008 (design 7.11): salesperson menu 5. Only Approved orders of the salesperson's dealership.
        public void PrintSoldOrRentedCars(User salesperson)
        {
            if (salesperson == null || !salesperson.IsSalesperson() || salesperson.GetDealership() == null)
            {
                Console.WriteLine("✗ רק איש מכירות יכול לצפות ברכבים שנמכרו או הושכרו");
                return;
            }
            CarDealership dealership = salesperson.GetDealership();
            int printed = 0;
            for (int i = 0; i < orderCount; i++)
            {
                Order order = orders[i];
                if (order.GetStatus() != ORDER_APPROVED)
                {
                    continue;
                }
                for (int j = 0; j < order.GetCarCount(); j++)
                {
                    Car car = order.GetCar(j);
                    if (IsClosedDealCar(car, dealership))
                    {
                        Console.WriteLine(car.ToString());
                        printed++;
                    }
                }
            }
            if (printed == 0)
            {
                Console.WriteLine("אין רכבים שנמכרו או הושכרו!");
            }
        }
    }
}
