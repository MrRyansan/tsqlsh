using System.Collections.Generic;
using System.Linq;

namespace tsqlsh
{
    /// <summary>
    /// ExecutionResults is used to capture the results of the command that was 
    /// executed against the database.
    /// </summary>
    internal class ExecutionResults
    {
        /// <summary>
        /// Returns the sql command text that was executed.
        /// </summary>
        internal string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the number of columns returned in the resultset.
        /// </summary>
        /// <value>The column count.</value>
        internal int ColCount { get; set; }

        /// <summary>
        /// Gets the number of rows that were returned.
        /// </summary>
        /// <value>The row count.</value>
        internal int RowCount { get { return this.Rows.Count; } }

        /// <summary>
        /// The actual rows that were returned.
        /// </summary>
        /// <value>The rows returned.</value>
        internal IList<IList<string>> Rows { get; set; }

        /// <summary>
        /// Gets or sets the information used to draw a table around the rows
        /// that may have been returned (unlike sqlcmd whose output is ugly).
        /// </summary>
        /// <value>The padding.</value>
        internal IDictionary<int, int> Padding { get; set; }

        /// <summary>
        /// Any error(s) returned by the database attempting to execute the
        /// command.
        /// </summary>
        /// <value>The error message.</value>
        internal string ErrorMsg { get; set; }

        // Indicates if the execution produced any resultset.
        internal bool HasRows {
            get { return this.Rows != null && this.Rows.Any(); }
        }
    }
}
