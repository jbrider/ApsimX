using APSIM.Server.IO;
using APSIM.Shared.Utilities;
using Models.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace APSIM.Server.Commands
{
    public static class HandleQueries
    {
        public static object HandleQuery(this ReadQuery readQuery, IDataStore storage)
        {
            if (readQuery == null) throw new Exception("Uknown query type in HandleQuery");

            if (!storage.Reader.TableNames.Contains(readQuery.TableName))
                throw new Exception($"Table {readQuery.TableName} does not exist in the database.");

            var result = storage.Reader.GetData(readQuery.TableName, fieldNames: readQuery.Parameters);
            if (result == null)
                throw new Exception($"Unable to read table {readQuery.TableName} from datastore (cause unknown - but the table appears to exist)");
            
            foreach (string param in readQuery.Parameters)
                if (result.Columns[param] == null)
                    throw new Exception($"Column {param} does not exist in table {readQuery.TableName}");

            result.TableName = readQuery.TableName;
            return result;
        }

        public static object HandleQueryRelay(this ReadQuery readQuery, IEnumerable<WorkerPod> workers)
        {
            if (readQuery == null) throw new Exception("Uknown query type in HandleQuery");

            List<Task<DataTable>> tasks = new List<Task<DataTable>>();
            foreach (var pod in workers)
                tasks.Add(RelayReadQuery(pod, readQuery));
            List<DataTable> tables = new List<DataTable>();
            foreach (Task<DataTable> task in tasks)
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{readQuery} failed", task.Exception);
                if (task.Result != null)
                    tables.Add(task.Result);
            }
            var result = DataTableUtilities.Merge(tables);
            //result.TableName = readQuery.TableName;
            foreach (string param in readQuery.Parameters)
                if (result.Columns[param] == null)
                    throw new Exception($"Column {param} does not exist in table {readQuery.TableName} (it appears to have disappeared in the merge)");
            return result;
        }

        private static Task<DataTable> RelayReadQuery(WorkerPod pod, ReadQuery query)
        {
            return Task.Run(() =>
            {
                if (pod.SocketConnection != null)
                {
                    return pod.SocketConnection.SendQuery(query);
                }
                else 
                {
                    if (string.IsNullOrEmpty(pod.IPAddress))
                        throw new NotImplementedException("Pod IP not set.");

                    // Create a new socket connection to the pod.
                    string ip = pod.IPAddress;
                    ushort port = pod.Port;
                    Console.WriteLine($"Attempting connection to pod {pod.Name} on {ip}:{port}");
                    using (NetworkSocketClient conn = new NetworkSocketClient(pod.Options.Verbose, ip, port, Protocol.Managed))
                    {
                        Console.WriteLine($"Connection to {pod.Name} established. Sending command...");

                        // Relay the command to the pod.
                        try
                        {
                            return conn.SendQuery(query);
                        }
                        catch (Exception err)
                        {
                            throw new Exception($"Unable to read output from pod {pod.Name}", err);
                        }
                    }
                }
            });
        }
    }
}
