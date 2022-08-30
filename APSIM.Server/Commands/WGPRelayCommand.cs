using Models.Core;
using Models.Core.Replace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace APSIM.Server.Commands
{
    [Serializable]
    public class VariableReference 
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    [Serializable]
    public class WGPRelayCommand : IQuery<List<List<double>>>
    {
        public List<List<VariableReference>> VariablesToUpdate { get; set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public List<string> OutputVariableNames { get; set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public IEnumerable<IEnumerable<double>> Result { get; set; }
        //public WGPRelayCommand()
        //{
        //    //VariablesToUpdate = new List<List<VariableReference>>();
        //}

        public WGPRelayCommand(List<List<VariableReference>> variables, string tableName, List<string> outputVariables)
        {
            VariablesToUpdate = variables;
            TableName = tableName;
            OutputVariableNames = outputVariables;
            Result = new List<List<double>>();
        }
    }

    [Serializable]
    public class WGPCommand : IQuery<IEnumerable<double>>
    {
        public List<VariableReference> VariablesToUpdate { get; set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public List<string> OutputVariableNames { get; private set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public List<double> Result { get; set; }
        
        public WGPCommand(List<VariableReference> variables, string tableName, List<string> outputVariables)
        {
            VariablesToUpdate = variables;
            TableName = tableName;
            OutputVariableNames = outputVariables;
            Result = new List<double>();
        }

    }
}
