using System;
using System.Linq;

namespace tsqlsh
{
    /// <summary>
    /// Terminal is responsible for writing to the...terminal.
    /// </summary>
    internal static class Terminal
    {
        /// <summary>
        /// Prints the specified result
        /// 
        /// </summary>
        /// <param name="result">Result.</param>
        internal static void Print(ExecutionResults result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{result.ErrorMessage}\n");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            if (!result.Rows.Any())
            {
                return;
            }

            PrintHeader(result);
            PrintRows(result);
        }

        private static void PrintHeader(ExecutionResults result)
        {
            for (int h = 0; h < result.ColumnCount; h++)
            {
                if (h == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write($"\n{result.Rows[0][h]}");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("|");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(result.Rows[0][h]);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n" + "-".PadRight(result.Padding.Values.Sum(), '-'));
        }

        private static void PrintHorizontalLine(ExecutionResults result)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var hl = result.Padding.Values.Sum();

            Console.WriteLine("-".PadRight(hl, '-'));
        }

        private static void PrintRows(ExecutionResults result)
        {
            var rowCount = result.Rows.Count;

            for (int r = 1; r < rowCount; r++)
            {
                for (int c = 0; c < result.ColumnCount; c++)
                {
                    if (c > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("|");
                    }

                    var value = result.Rows[r][c];

                    Console.ForegroundColor = GetDataTypeForecolor(value);

                    Console.Write(value);
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-".PadRight(result.Padding.Values.Sum(), '-'));
        }

        private static ConsoleColor GetDataTypeForecolor(string val)
        {
            if (double.TryParse(val, out double f))
            {
                return ConsoleColor.DarkRed;
            }

            if (bool.TryParse(val, out bool b))
            {
                return ConsoleColor.Green;
            }

            if (DateTime.TryParse(val, out DateTime dt))
            {
                return ConsoleColor.Cyan;
            }

            return ConsoleColor.DarkYellow;
        }

        internal static void PrintUsage()
        {
            Console.WriteLine("usage: tsqlsh server=<servername> database=<database name> [user=<user name> pwd=<password>] [-rw]\n");
            Console.WriteLine("These are the command line options available:");
            Console.WriteLine("   server               (required) the FQDN or IP address of the server hosting the database");
            Console.WriteLine("   database             (required) the name of the database");
            Console.WriteLine("   user                 (optional) the user that will be authenticated");
            Console.WriteLine("                        Note: If the user parameter is provided then pwd parameter will required");
            Console.WriteLine("   pwd                  (optional) the password of the user that will be authenticated");
            Console.WriteLine("                        Note: If the pwd parameter is provided then user parameter will required");
            Console.WriteLine("   -rw                  (optional) a flag to indicate that the connection should allow writes (DDL and CRUD statements)\n");
            Console.WriteLine("                        Note: If the -rw flag is not provided then session will only allow readonly operations\n");
            Console.WriteLine("   -h | --help          (optional) a flag to indicate that the connection should allow writes (DDL and CRUD statements)\n");
        }

        internal static void PrintConnectionInfo(string dataSource, string initialCatalog, string intent)
        {
            var bgc = Console.BackgroundColor;

            Console.Write("Connected to ");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{dataSource} ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"at ");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{ initialCatalog}");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(", mode=");

            Console.BackgroundColor = intent == "ReadWrite" ?
                ConsoleColor.Red :
                ConsoleColor.Black;

            Console.ForegroundColor = intent == "ReadWrite" ?
                ConsoleColor.Yellow :
                ConsoleColor.Green;

            Console.WriteLine($"{intent}");

            Console.BackgroundColor = bgc;
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal static void PrintErrors(string[] errors)
        {
            var output = string.Join("\n", errors);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(output);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
