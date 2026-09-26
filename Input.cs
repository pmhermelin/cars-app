using System;

namespace CarsApp
{
    // Design 4.3: every input is read as a string and converted with TryParse.
    // Entering 0 in a form cancels the whole action.
    public class Input
    {
        public const string CANCEL = "0";

        private static bool endOfInput = false;

        public static string ReadText(string prompt)
        {
            Console.Write(prompt);
            string text = Console.ReadLine();
            if (text == null)
            {
                endOfInput = true; // input stream closed - treat as cancel
                return CANCEL;
            }
            return text.Trim();
        }

        // True when the input stream was closed (for example when input is piped from a file)
        public static bool IsEndOfInput()
        {
            return endOfInput;
        }

        // Returns -1 when the input is not a whole number
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
        // Returns -1 when the input is not a valid number
        public static double ReadDouble(string prompt)
        {
            string text = ReadText(prompt);
            double number;
            if (double.TryParse(text, out number))
            {
                return number;
            }
            return -1;
        }
        public static bool IsCancel(string text)
        {
            return text == CANCEL;
        }
    }
}
