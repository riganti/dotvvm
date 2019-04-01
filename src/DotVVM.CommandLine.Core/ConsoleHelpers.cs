using System;

namespace DotVVM.CommandLine.Core
{
    public static class ConsoleHelpers
    {
        public static string AskForValue(string question, string defaultValue = null)
        {
            Console.WriteLine(question);
            var result =  Console.ReadLine();
            if (defaultValue != null && string.IsNullOrEmpty(result))
            {
                return defaultValue;
            }

            return result;
        }

        public static bool AskForYesNo(string question)
        {
            Console.WriteLine(question + " [Y|N]");

            while (true)
            {
                var answer = Console.ReadKey().Key;
                if (answer == ConsoleKey.Y)
                {
                    return true;
                }
                else if (answer == ConsoleKey.N)
                {
                    return false;
                }
                Console.WriteLine("Please use Y or N: ");
            }
        }
    }
}
