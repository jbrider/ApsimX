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
    public class ResultsClass
    {
        public ResultsClass()
        {
            Results = new List<double>();
        }
        public List<double> Results { get; set; }
    }

    [Serializable]
    public class CombinedResultsClass
    {
        public CombinedResultsClass()
        {
            Results = new List<List<double>>();
        }
        public List<List<double>> Results { get; set; }

    }

    [Serializable]
    public class WGPRelayCommand : IQuery<CombinedResultsClass>
    {
        public List<string> VariablesToUpdate { get; set; }
        public List<List<double>> ValuesToUpdate { get; set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public List<string> OutputVariableNames { get; set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public CombinedResultsClass ResultValues { get; set; }

        public WGPRelayCommand(List<string> variables, List<List<double>> valuesToUpdate, string tableName, List<string> outputVariables)
        {
            VariablesToUpdate = variables;
            ValuesToUpdate = valuesToUpdate;
            TableName = tableName;
            OutputVariableNames = outputVariables;
            ResultValues = new CombinedResultsClass();
        }
    }

    [Serializable]
    public class WGPCommand : IQuery<ResultsClass>
    {
        public List<string> VariablesToUpdate { get; set; }
        public List<double> ValuesToUpdate { get; set; }

        public bool isQuery() => true;
        /// <summary>
        /// Name of the table from which parameters will be read.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>Variables names to be extracted from the report.</summary>
        public List<string> OutputVariableNames { get; set; }

        /// <summary>The result of the ReadCommand.Contains the data /// </summary>
        public ResultsClass ResultValues { get; set; }
        
        public WGPCommand(List<string> variables, List<double> values, string tableName, List<string> outputVariables)
        {
            VariablesToUpdate = variables;
            ValuesToUpdate= values; 
            TableName = tableName;
            OutputVariableNames = outputVariables;
            ResultValues = new ResultsClass();
        }

    }
}
