using System;

namespace CarsApp
{
    // Design 6.6: the entry point. Responsible only for menus, input and messages.
    // Contains no business logic and never touches the arrays.
    public class Program
    {
        private static CarDealerShipSystem system;

        // Run normally:  dotnet run
        // Run the tests: dotnet run -- test
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "test")
            {
                Tests.RunAll();
                return;
            }

            system = new CarDealerShipSystem();
            RunMainMenu();
        }

        // Design 9.1: main menu
        private static void RunMainMenu()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== מערכת סוכנות רכבים =====");
                Console.WriteLine("1. הרשמה");
                Console.WriteLine("2. התחברות");
                Console.WriteLine("3. יציאה");
                int choice = Input.ReadInt("בחירה: ");
                if (Input.IsEndOfInput())
                {
                    break;
                }

                if (choice == 1)
                {
                    if (system.CreateUser())
                    {
                        Console.WriteLine("✓ ההרשמה הושלמה בהצלחה");
                    }
                    else
                    {
                        Console.WriteLine("✗ ההרשמה לא הושלמה");
                    }
                }
                else if (choice == 2)
                {
                    NotReady("REQ-002");
                }
                else if (choice == 3)
                {
                    running = false;
                }
                else
                {
                    Console.WriteLine("✗ בחירה לא חוקית");
                }
            }
            Console.WriteLine("להתראות!");
        }

        // Design 9.2: dealership manager menu
        private static void RunManagerMenu(User user)
        {
            bool inMenu = true;
            while (inMenu)
            {
                Console.WriteLine();
                Console.WriteLine("===== תפריט מנהל - " + user.GetDealership().GetName() + " =====");
                Console.WriteLine("1. הוספת רכב");
                Console.WriteLine("2. עדכון פרטי רכב");
                Console.WriteLine("3. שינוי מחיר רכב");
                Console.WriteLine("4. אישור או דחיית עסקאות");
                Console.WriteLine("5. הפקת דוח מלאי");
                Console.WriteLine("6. הוספת איש מכירות");
                Console.WriteLine("7. התנתקות");
                int choice = Input.ReadInt("בחירה: ");
                if (Input.IsEndOfInput())
                {
                    return;
                }

                if (choice == 1 || choice == 2)
                {
                    NotReady("REQ-003");
                }
                else if (choice == 3)
                {
                    NotReady("REQ-007");
                }
                else if (choice == 4)
                {
                    NotReady("REQ-005");
                }
                else if (choice == 5)
                {
                    NotReady("REQ-006");
                }
                else if (choice == 6)
                {
                    NotReady("REQ-014");
                }
                else if (choice == 7)
                {
                    inMenu = false;
                }
                else
                {
                    Console.WriteLine("✗ בחירה לא חוקית");
                }
            }
        }

        // Design 9.3: salesperson menu
        private static void RunSalespersonMenu(User user)
        {
            bool inMenu = true;
            while (inMenu)
            {
                Console.WriteLine();
                Console.WriteLine("===== תפריט איש מכירות - " + user.GetDealership().GetName() + " =====");
                Console.WriteLine("1. צפייה במלאי הסוכנות");
                Console.WriteLine("2. הוספת רכב");
                Console.WriteLine("3. עדכון פרטי רכב");
                Console.WriteLine("4. יצירת עסקה עבור לקוח");
                Console.WriteLine("5. רכבים שנמכרו או הושכרו");
                Console.WriteLine("6. התנתקות");
                int choice = Input.ReadInt("בחירה: ");
                if (Input.IsEndOfInput())
                {
                    return;
                }

                if (choice == 1)
                {
                    NotReady("REQ-013");
                }
                else if (choice == 2 || choice == 3)
                {
                    NotReady("REQ-003");
                }
                else if (choice == 4)
                {
                    NotReady("REQ-011");
                }
                else if (choice == 5)
                {
                    NotReady("REQ-008");
                }
                else if (choice == 6)
                {
                    inMenu = false;
                }
                else
                {
                    Console.WriteLine("✗ בחירה לא חוקית");
                }
            }
        }

        // Design 9.4: customer menu
        private static void RunCustomerMenu(User user)
        {
            bool inMenu = true;
            while (inMenu)
            {
                Console.WriteLine();
                Console.WriteLine("===== תפריט לקוח - " + user.GetUsername() + " =====");
                Console.WriteLine("1. צפייה ברכבים זמינים");
                Console.WriteLine("2. חיפוש וסינון רכבים");
                Console.WriteLine("3. ביצוע הזמנה");
                Console.WriteLine("4. צפייה בהזמנות שלי");
                Console.WriteLine("5. ביטול הזמנה");
                Console.WriteLine("6. התנתקות");
                int choice = Input.ReadInt("בחירה: ");
                if (Input.IsEndOfInput())
                {
                    return;
                }

                if (choice == 1)
                {
                    NotReady("REQ-010");
                }
                else if (choice == 2)
                {
                    NotReady("REQ-009");
                }
                else if (choice == 3)
                {
                    NotReady("REQ-011");
                }
                else if (choice == 4)
                {
                    NotReady("REQ-012");
                }
                else if (choice == 5)
                {
                    NotReady("REQ-004");
                }
                else if (choice == 6)
                {
                    inMenu = false;
                }
                else
                {
                    Console.WriteLine("✗ בחירה לא חוקית");
                }
            }
        }

        // Placeholder for menu items whose story is not implemented yet
        private static void NotReady(string req)
        {
            Console.WriteLine("הפעולה עדיין בפיתוח (" + req + ")");
        }
    }
}
