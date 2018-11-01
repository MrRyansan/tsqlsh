using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace tsqlsh
{
    /// <summary>
    /// Used to parse and verify the cmd line arguments needed to connect to
    /// a database.
    /// </summary>
    internal sealed class CmdLineArgs
    {
        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="args">Arguments.</param>
        internal CmdLineArgs(string[] args)
        {
            this.Init(args);
        }

        /// <summary>
        /// Initializes the instance using the <paramref name="args"/> passed in.
        /// </summary>
        /// <param name="args">The command line arguments passed in.</param>
        private void Init(string[] args)
        {
            var svr = args.FirstOrDefault(a => a.Contains("server="));
            var dbs = args.FirstOrDefault(a => a.Contains("database="));
            var usr = args.FirstOrDefault(a => a.Contains( "user="));
            var pwd = args.FirstOrDefault(a => a.Contains ("pwd="));

            this.ShowHelp =
                args == null
                || !args.Any()
                || args.Any(a => a.Contains("-h") 
                || a.Contains("--help"));

            if (this.ShowHelp)
            {
                return;
            }

            this.Server = svr?.Replace("server=", "");
            this.Database = dbs?.Replace("database=", "");

            this.User = usr?.Replace("user=", "");
            this.Password = pwd?.Replace("pwd=", "");

            this.Intent = args.FirstOrDefault(a => a.Contains("-rw")) == null ?
                ApplicationIntent.ReadOnly :
                ApplicationIntent.ReadWrite;

            this.Validate();
        }

        /// <summary>
        /// The name of the database that will be connected to.
        /// </summary>
        /// <value>The database.</value>
        public string Database { get; private set; }

        /// <summary>
        /// The FQDN or IP address of the server hosting the datatbase.
        /// </summary>
        /// <value>The server.</value>
        public string Server { get; private set; }

        /// <summary>
        /// The name of the user that will authenticate against the database.
        /// </summary>
        /// <value>The user.</value>
        public string User { get; private set; }

        /// <summary>
        /// The password for the <see cref="User"/>.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; private set; }

        /// <summary>
        /// Indicates whether or not the help should be displayed.
        /// </summary>
        /// <remarks>
        /// If this is set to 'true' then all other arugments will be ignored.
        /// </remarks>
        /// <value><c>true</c> if show help; otherwise, <c>false</c>.</value>
        public bool ShowHelp { get; private set; }

        /// <summary>
        /// Indicates if the connection to the database permits CRUD and DDL
        /// statements to be executed.
        /// </summary>
        /// <remarks>
        /// NOTE: Security settings in the database will take precedence.
        /// </remarks>
        /// <value>The intent.</value>
        public ApplicationIntent Intent { get; private set; } = ApplicationIntent.ReadOnly;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:tsqlsh.CmdLineArgs"/> has errors.
        /// </summary>
        /// <value><c>true</c> if has errors; otherwise, <c>false</c>.</value>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Returns a <see cref="SqlConnectionStringBuilder"/> using the arguments
        /// that were passed in.
        /// </summary>
        /// <value>The sql connection.</value>
        public SqlConnectionStringBuilder SqlConnection { get; private set; }

        /// <summary>
        /// Returns any validation errors of the command line arguments.
        /// </summary>
        /// <value>The errors.</value>
        public List<string> Errors => new List<string>();

        /// <summary>
        /// Validate this command line args passed in.
        /// </summary>
        internal void Validate()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Server))
            {
                this.Errors.Add("missing server name");
            }

            if (string.IsNullOrWhiteSpace(this.Database))
            {
                this.Errors.Add("missing database name");
            }

            if (!string.IsNullOrWhiteSpace(this.User) && string.IsNullOrWhiteSpace(this.Password))
            {
                this.Errors.Add("a password is required if supplying a user name");
            }

            if (string.IsNullOrWhiteSpace(this.User) && !string.IsNullOrWhiteSpace(this.Password))
            {
                this.Errors.Add("a user name is required if supplying a password");
            }

            if (this.HasErrors)
            {
                return;
            }
            this.SqlConnection = new SqlConnectionStringBuilder();

            this.SqlConnection.DataSource = this.Server;
            this.SqlConnection.InitialCatalog = this.Database;
            this.SqlConnection.UserID = this.User ?? "";
            this.SqlConnection.Password = this.Password ?? "";
            this.SqlConnection.ApplicationIntent = this.Intent;

            this.SqlConnection.IntegratedSecurity = string.IsNullOrWhiteSpace(this.User);
        }
    }
}
