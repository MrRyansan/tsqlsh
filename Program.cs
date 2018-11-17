#pragma warning disable IDE1006
using System;

namespace tsqlsh
{
    /// <summary>
    /// The place where it all starts: the 'Program'.
    /// </summary>
    /// <remarks>
    /// This class should only be used to get things rolling.
    /// </remarks>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            var p = new Program(args);
        }

        internal ISqlEngine engine { get; set; }
        private readonly cmdLineArgs settings;
        internal Command cmdBuilder { get; set; }

        private Program(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(this.CtrlCHandlerHandler);
            this.settings = new cmdLineArgs(args);

            if (this.settings.ShowHelp)
            {
                Terminal.PrintUsage();
                return;
            }

            if (this.settings.HasErrors)
            {
                Terminal.PrintErrors(this.settings.Errors.ToArray());
                Terminal.PrintUsage();
                return;
            }

            this.Run();
        }

        /// <summary>
        /// Helps ensure that any live connection to the database is gracefully
        /// closed and disposed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void CtrlCHandlerHandler(object sender, ConsoleCancelEventArgs e)
        {
            this.engine.Dispose();
            System.Diagnostics.Debug.WriteLine("^C intercepted");
        }

        private void Run()
        {
            if (this.engine == null)
            {
                this.engine = new SqlEngine(this.settings.SqlConnection);
            }

            if (this.cmdBuilder == null)
            {
                this.cmdBuilder = new Command();
            }

            this.engine.DbChanged += (s,e) => { this.cmdBuilder.SetDbName(e); };

            Terminal.PrintConnectionInfo(this.settings.SqlConnection.DataSource, this.settings.SqlConnection.InitialCatalog, this.settings.SqlConnection.ApplicationIntent.ToString());

            while (true)
            {
                var cmd = this.cmdBuilder.Build();

                switch (cmd)
                {
                    case "exit":
                        this.engine.Dispose();
                        System.Diagnostics.Debug.WriteLine("exit invoked");
                        Environment.Exit(0);
                        break;
                    case "":
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case null:
                        continue;
                    default:
                        var result = this.engine.ExecuteCommand(cmd);
                        Terminal.Print(result);
                        break;
                }
            }
        }
    }
}
