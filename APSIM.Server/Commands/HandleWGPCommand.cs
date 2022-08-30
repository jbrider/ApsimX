using APSIM.Shared.Utilities;
using Models.Core.Replace;
using Models.Core.Run;
using Models.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.Server.Commands
{
    public static class HandleWGPCommand
    {
        public static CombinedResultsClass HandleQueryRelay(this WGPRelayCommand wgpQuery, IEnumerable<WorkerPod> workers)
        {
            if (wgpQuery == null) throw new Exception("Uknown query type in HandleQueryRelay");
            Console.Write(ReflectionUtilities.JsonSerialise(wgpQuery, false));

            var tasks = workers.Zip(wgpQuery.ValuesToUpdate, (WorkerPod pod, List<double> values) => 
                RelayReadQuery(pod, new WGPCommand(wgpQuery.VariablesToUpdate, values, wgpQuery.TableName, wgpQuery.OutputVariableNames)));

            var results = new CombinedResultsClass();
            Parallel.ForEach(tasks, task =>
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{wgpQuery} failed", task.Exception);

                if (task.Result != null)
                    results.Results.Add(task.Result.Results);
            });
            return results;
        }

        private static Task<ResultsClass> RelayReadQuery(WorkerPod pod, WGPCommand query)
        {
            return Task.Run(() =>
            {
                if(pod.SocketConnection == null) throw new NotImplementedException("SocketConnnection not created.");
                return pod.SocketConnection.SendQuery(query);
            });
        }

        public static ResultsClass HandleQuery(this WGPCommand wgpQuery, Runner runner, ServerJobRunner jobRunner, IDataStore storage)
        {
            if (wgpQuery == null) throw new Exception("Uknown query type in HandleQuery");

            var timer = Stopwatch.StartNew();
            jobRunner.Replacements = wgpQuery.VariablesToUpdate.Zip(wgpQuery.ValuesToUpdate, (path, value) => new PropertyReplacement(path, value));

            List<Exception> errors = runner.Run();

            timer.Stop();
            Console.WriteLine($"Raw job took {timer.ElapsedMilliseconds}ms");

            if (errors != null && errors.Count > 0)
                throw new AggregateException("File ran with errors", errors);

            if (!storage.Reader.TableNames.Contains(wgpQuery.TableName))
                throw new Exception($"Table {wgpQuery.TableName} does not exist in the database.");

            var result = storage.Reader.GetData(wgpQuery.TableName, fieldNames: wgpQuery.OutputVariableNames);
            if (result == null)
                throw new Exception($"Unable to read table {wgpQuery.TableName} from datastore (cause unknown - but the table appears to exist)");

            foreach (string param in wgpQuery.OutputVariableNames)
                if (result.Columns[param] == null)
                    throw new Exception($"Column {param} does not exist in table {wgpQuery.TableName}");

            if(result.Rows.Count == 0)
                throw new Exception($"Report was empty");

            if (result.Rows.Count > 1)
                throw new Exception($"Report had more than 1 result"); //debug for checking if table is rest
            var lstVars = wgpQuery.OutputVariableNames.ToList();
            var lstValues = new ResultsClass();

            for (int i = 0; i < lstVars.Count;++i)
            {
                lstValues.Results.Add(Convert.ToDouble(result.Rows[0][lstVars[i]]));
            }
            return lstValues;
        }

    }
}
