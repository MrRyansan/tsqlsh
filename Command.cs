using System;
using System.Text;

namespace tsqlsh
{
    /// <summary>
    /// Command is used to capture a statement to be executed from the terminal.
    /// </summary>
    internal static class Command
    {
        /// <summary>
        /// Captures the terminal input to create an executable DDL or DML 
        /// statement.
        /// </summary>
        /// <returns>The build.</returns>
        internal static string Build()
        {
            var commandBuilder = new StringBuilder();

            Console.Write("tsqlsh> ");
            while(true)
            {
                var cmdtext = Console.ReadLine();
                commandBuilder.Append(cmdtext + " ");

                switch(cmdtext.ToLower())
                {
                    case "exit":
                    case "clear":
                        return cmdtext;
                    case "test":
                        return "SELECT NAME, DATABASE_ID, CREATE_DATE FROM SYS.DATABASES;";
                    default:
                        break;
                }

                if(string.IsNullOrWhiteSpace(cmdtext) || cmdtext.Trim()[cmdtext.Length - 1] == ';')
                {
                    break;
                }
                else
                {
                    Console.Write("    ... ");
                }
            }
            var sqlcmd = commandBuilder.ToString().Trim();
            return sqlcmd;
        }
    }
}
