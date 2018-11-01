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
            if(!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{result.ErrorMessage}\n");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            if(!result.Rows.Any())
            {
                return;
            }


            PrintHeader(result);
            PrintRows(result);
            PrintFooterBorder(result);
        }

        private static void PrintHeader(ExecutionResults result)
        {
            PrintHeaderTopBorder(result);
            foreach(var hdr in result.Rows[0])
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\u2502");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(hdr);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\u2502");
            PrintHeaderBottomBorder(result);
        }

        private static void PrintHeaderTopBorder(ExecutionResults result)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\u2552");
            for(int h = 0; h < result.ColumnCount; h++)
            {
                if(h == result.ColumnCount - 1)
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550'));
                }
                else
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550') + "\u2564");
                }
            }
            Console.WriteLine("\u2555");
        }

        private static void PrintHeaderBottomBorder(ExecutionResults result)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\u255e");
            for(int h = 0; h < result.ColumnCount; h++)
            {
                if(h == result.ColumnCount - 1)
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550'));
                }
                else
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550') + "\u256a");
                }
            }
            Console.WriteLine("\u2561");
        }

        private static void PrintRows(ExecutionResults result)
        {
            var rowCount = result.Rows.Count;

            for(int r = 1; r < rowCount; r++)
            {
                for(int c = 0; c < result.ColumnCount; c++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("\u2502");

                    var value = result.Rows[r][c];

                    Console.ForegroundColor = GetDataTypeForecolor(value);

                    Console.Write(value);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\u2502");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static ConsoleColor GetDataTypeForecolor(string val)
        {
            double f;
            bool b;
            DateTime dt;

            if(double.TryParse(val, out f))
            {
                return ConsoleColor.DarkRed;
            }

            if(bool.TryParse(val, out b))
            {
                return ConsoleColor.Green;
            }

            if(DateTime.TryParse(val, out dt))
            {
                return ConsoleColor.Cyan;
            }

            return ConsoleColor.DarkYellow;
        }


        private static void PrintFooterBorder(ExecutionResults result)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\u2558");
            for(int h = 0; h < result.ColumnCount; h++)
            {
                if(h == result.ColumnCount - 1)
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550'));
                }
                else
                {
                    Console.Write("\u2550".PadRight(result.Padding[h], '\u2550') + "\u2567");
                }
            }
            Console.WriteLine("\u255b\n");
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

        internal static void PrintErrors(string[] errors){
            var output = string.Join("\n", errors);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(output);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
