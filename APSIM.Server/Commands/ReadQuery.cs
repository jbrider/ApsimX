using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Models.Core;
using Models.Core.Run;
using Models.Storage;

namespace APSIM.Server.Commands
{
    /// <summary>
    /// A command to run simulations.
    /// </summary>
    [Serializable]
    public class ReadQuery : IQuery<DataTable>
    {
        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Parameter names to be read.
        /// </summary>
        public IEnumerable<string> Parameters { get; private set; }

        /// <summary>
        /// The result of the ReadCommand.
        /// Contains the data 
        /// </summary>
        public DataTable Result { get; set; }

        /// <summary>
        /// Creates a <see cref="RunCommand" /> instance with sensible defaults.
        /// </summary>
        public ReadQuery(string tablename, IEnumerable<string> parameters)
        {
            this.TableName = tablename;
            this.Parameters = parameters;
        }

        public override string ToString()
        {
            return $"{GetType().Name} with {Parameters.Count()} parameters";
        }

        public override bool Equals(object obj)
        {
            if (obj is ReadQuery command)
            {
                if (TableName != command.TableName)
                    return false;
                if (Parameters == null && command.Parameters == null)
                    return true;
                if (Parameters == null || command.Parameters == null)
                    return false;
                if (Parameters.Zip(command.Parameters, (x, y) => x != y).Any(x => x))
                    return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (TableName, Parameters).GetHashCode();
        }
    }
}
