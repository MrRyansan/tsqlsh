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
    internal sealed class SqlEngine : IDisposable
    {
        private SqlConnection connection;
        private readonly bool isReadonly;

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

        /// <summary>
        /// Connects to the database.
        /// </summary>
        /// <param name="cstr">The connection string.</param>
        internal void Connect(string cstr)
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
        internal ExecutionResults ExecuteCommand(string sql)
        {
            var cmd = this.connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;

            if (requiresWrite() && this.isReadonly)
            {
                return new ExecutionResults()
                {
                    ErrorMessage = "This session can only execute SELECT queries." + 
                                   "  Start another session using the '-rw' flag" + 
                                   " if DDL statements are to be executed."
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
                results.ErrorMessage = ex.Message;
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
            var results = new ExecutionResults() { CommandText = cmd.CommandText };

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                results.ErrorMessage = ex.Message;
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
                ColumnCount = reader.FieldCount,
                Rows = new List<IList<string>>()
            };

            try
            {
                if (!reader.HasRows)
                {
                    return rawData;
                }

                var header = new List<string>();
                for (int i = 0; i < rawData.ColumnCount; i++)
                {
                    header.Add(reader.GetName(i));
                }

                rawData.Rows.Add(header);

                while (reader.Read())
                {
                    var row = new List<string>();

                    for (int i = 0; i < rawData.ColumnCount; i++)
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
                ColumnCount = rawResults.ColumnCount,
                Rows = new List<IList<string>>(),
                Padding = rawResults.Padding
            };

            for (var r = 0; r < rawResults.Rows.Count; r++)
            {
                var rawRow = rawResults.Rows[r];
                var newRow = new List<string>();

                for (var f = 0; f < rawResults.ColumnCount; f++)
                {
                    newRow.Add(rawRow[f].PadRight(rawResults.Padding[f], ' '));
                }

                formattedResult.Rows.Add(newRow);
            }

            return formattedResult;

            void calculatePadding()
            {
                rawResults.Padding = new Dictionary<int, int>();

                for (int i = 0; i < rawResults.ColumnCount; i++)
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
