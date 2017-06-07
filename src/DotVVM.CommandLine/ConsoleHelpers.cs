using System;

namespace DotVVM.CommandLine
{
    public static class ConsoleHelpers
    {
        public static string AskForValue(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
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