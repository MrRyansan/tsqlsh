#pragma warning disable RECS0020

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace tsqlsh
{
    /// <summary>
    /// SqlEngine is what connects to and exectutes the statement input in the 
    /// cli.
    /// </summary>
    internal class SqlEngine : ISqlEngine
    {
        private SqlConnection connection;
        private readonly bool isReadonly;

        private EventHandler<string> dbChanged;

        private void OnDbChanged(string dbName){
            if (this.dbChanged == null){ return; }
            this.dbChanged(this, dbName);
        }

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="cbuilder">The <see cref="SqlConnectionStringBuilder"/>
        /// that is used to connect to the database.</param>
        internal SqlEngine(SqlConnectionStringBuilder cbuilder)
        {
            this.isReadonly = cbuilder.ApplicationIntent == ApplicationIntent.ReadOnly;
            this.Connect(cbuilder.ToString());
        }

        public event EventHandler<string> DbChanged
        {
            add{this.dbChanged += value;}
            remove{this.dbChanged -= value;}
        }

        /// <summary>
        /// Connects to the database.
        /// </summary>
        /// <param name="cstr">The connection string.</param>
        public void Connect(string cstr)
        {
            this.connection = new SqlConnection(cstr);
            if (this.connection.State != ConnectionState.Open)
            {
                this.connection.Open();
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>The command.</returns>
        /// <param name="sql">The statement to be exectuted.</param>
        public ExecutionResults ExecuteCommand(string sql)
        {
            var cmd = this.connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;

            if (requiresWrite() && this.isReadonly)
            {
                return new ExecutionResults()
                {
                    ErrorMsg = "This session can only execute SELECT queries. " + 
                               "Start another session using the '-rw' flag " + 
                               "if DDL statements are to be executed."
                };
            }

            return sql.ToUpper().Contains("SELECT ") ?
                         this.ExecuteReader(cmd) :
                         this.ExecuteNonQuery(cmd);

            bool requiresWrite()
            {
                var cmdTxt = sql.ToUpper();
                return cmdTxt.Contains("INSERT ")
                    || cmdTxt.Contains("UPDATE ")
                    || cmdTxt.Contains("DELETE ")
                    || cmdTxt.Contains("CREATE ")
                    || cmdTxt.Contains("ALTER ")
                    || cmdTxt.Contains("DROP ");
            }
        }

        /// <summary>
        /// Executes a statement that is expected to return a rows.
        /// </summary>
        /// <returns>The results of the query.</returns>
        /// <param name="cmd">The IDbCommand that will execute the query.</param>
        internal ExecutionResults ExecuteReader(IDbCommand cmd)
        {
            var results = new ExecutionResults() { CommandText = cmd.CommandText };

            try
            {
                var reader = cmd.ExecuteReader() as SqlDataReader;
                var rawData = this.ReadRawResults(reader);
                results = this.FormatQueryResult(rawData);
            }
            catch (Exception ex)
            {
                results.ErrorMsg = ex.Message;
            }

            return results;
        }

        /// <summary>
        /// Executes statement that does not return a set of rows.
        /// </summary>
        /// <returns>An empty <see cref="ExecutionResults"/> object.</returns>
        /// <param name="cmd">The IDbCommand that will execute the statement.</param>
        internal ExecutionResults ExecuteNonQuery(IDbCommand cmd)
        {
            bool hasUsingStmt = cmd.CommandText.Substring(0, 3).ToUpper() == "USE";



            var results = new ExecutionResults() { CommandText = cmd.CommandText };

            try
            {
                cmd.ExecuteNonQuery();
                if (hasUsingStmt)
                {
                    var dbName = cmd.CommandText.Substring(4).Replace(";", "");
                    this.OnDbChanged(dbName);
                }
            }
            catch (Exception ex)
            {
                results.ErrorMsg = ex.Message;
            }

            return results;
        }

        /// <summary>
        /// Reads the raw results out of the data reader.
        /// </summary>
        /// <returns>The raw results.</returns>
        /// <param name="reader">Reader.</param>
        internal ExecutionResults ReadRawResults(SqlDataReader reader)
        {
            var rawData = new ExecutionResults()
            {
                ColCount = reader.FieldCount,
                Rows = new List<IList<string>>()
            };

            try
            {
                if (!reader.HasRows)
                {
                    return rawData;
                }

                var header = new List<string>();
                for (int i = 0; i < rawData.ColCount; i++)
                {
                    header.Add(reader.GetName(i));
                }

                rawData.Rows.Add(header);

                while (reader.Read())
                {
                    var row = new List<string>();

                    for (int i = 0; i < rawData.ColCount; i++)
                    {
                        var value = reader[i];
                        row.Add(value != null ? value.ToString() : "<null>");
                    }

                    rawData.Rows.Add(row);
                }
            }
            finally
            {
                reader.Close();
            }

            return rawData;
        }

        internal ExecutionResults FormatQueryResult(ExecutionResults rawResults)
        {
            calculatePadding();

            var formattedResult = new ExecutionResults()
            {
                ColCount = rawResults.ColCount,
                Rows = new List<IList<string>>(),
                Padding = rawResults.Padding
            };

            for (var r = 0; r < rawResults.Rows.Count; r++)
            {
                var rawRow = rawResults.Rows[r];
                var newRow = new List<string>();

                for (var f = 0; f < rawResults.ColCount; f++)
                {
                    newRow.Add(rawRow[f].PadRight(rawResults.Padding[f], ' '));
                }

                formattedResult.Rows.Add(newRow);
            }

            return formattedResult;

            void calculatePadding()
            {
                rawResults.Padding = new Dictionary<int, int>();

                for (int i = 0; i < rawResults.ColCount; i++)
                {
                    var maxFieldSize = rawResults.Rows
                        .Select(r => r[i] ?? "")
                        .Max(f => f.Count());

                    rawResults.Padding.Add(i, maxFieldSize + 2);
                }
            }
        }

        #region IDisposable Support
        private bool isDisposed = false;

        private void Dispose(bool disposing)
        {
            if (this.isDisposed || !disposing)
            {
                return;
            }

            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }

            this.connection.Dispose();
            this.connection = null;
            this.isDisposed = true;
        }

        ~SqlEngine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
