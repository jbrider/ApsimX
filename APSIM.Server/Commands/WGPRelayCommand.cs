using Models.Core.Replace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace APSIM.Server.Commands
{
    [Serializable]
    public class WGPRelayCommand : IQuery<IEnumerable<IEnumerable<double>>>
    {
        public IEnumerable<IEnumerable<IReplacement> > VariablesToUpdate { get; set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public IEnumerable<string> OutputVariableNames { get; set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public IEnumerable<IEnumerable<double>> Result { get; set; }

        public WGPRelayCommand(IEnumerable<IEnumerable<IReplacement> > variables, string tableName, IEnumerable<string> outputVariables)
        {
            VariablesToUpdate = variables;
            TableName = tableName;
            OutputVariableNames = outputVariables;
        }

    }

    public class WGPCommand : IQuery<IEnumerable<double>>
    {
        public IEnumerable<IReplacement> VariablesToUpdate { get; private set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public IEnumerable<string> OutputVariableNames { get; private set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public IEnumerable<double> Result { get; set; }

        public WGPCommand(IEnumerable<IReplacement> variables, string tableName, IEnumerable<string> outputVariables)
        {
            VariablesToUpdate = variables;
            TableName = tableName;
            OutputVariableNames = outputVariables;
        }

    }
}
