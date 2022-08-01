using APSIM.Server.Cli;
using APSIM.Server.IO;
using k8s.Models;
using Models.Core.Run;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.Server.Commands
{
    public static class HandleCommands
    {
        public static void Handle(this ICommand runCmd, Runner runner, ServerJobRunner jobRunner)
        {
            if(runCmd is RunCommand)
                HandleRunCommand(runCmd as RunCommand, runner, jobRunner);    

        }
        public static void HandleRunCommand(RunCommand runCmd, Runner runner, ServerJobRunner jobRunner)
        {
            jobRunner.Replacements = runCmd.Changes;
            var timer = Stopwatch.StartNew();

            List<Exception> errors = runner.Run();

            timer.Stop();
            Console.WriteLine($"Raw job took {timer.ElapsedMilliseconds}ms");

            if (errors != null && errors.Count > 0)
                throw new AggregateException("File ran with errors", errors);

        }
        public static void HandleCommandRelay(this ICommand command, IEnumerable<V1Pod> workers, RelayServerOptions relayOptions, string podPortNoLabelName)
        {
            if (command is WGPCommand)
            {
                (command as WGPCommand).HandleWGPCommandRelay(workers, relayOptions, podPortNoLabelName);
                return;
            }
            
            List<Task> tasks = new List<Task>();
            foreach (var pod in workers)
                tasks.Add(RelayCommand(pod, command, relayOptions, podPortNoLabelName));

            Parallel.ForEach(tasks, task =>
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{command} failed", task.Exception);
            });
        }

        public static void HandleWGPCommandRelay(this WGPCommand command, IEnumerable<V1Pod> workers, RelayServerOptions relayOptions, string podPortNoLabelName)
        {
            Console.WriteLine($"Handling WGP Command Relay to see if it goes where it should");
            List<Task> tasks = new List<Task>();
            var workerCommands = command.VariablesToUpdate.Zip(workers, (cmd, worker) => (cmd, worker));
            foreach (var wc in workerCommands)
            {
                var newCommand = new RunCommand(wc.cmd);
                tasks.Add(RelayCommand(wc.worker, newCommand, relayOptions, podPortNoLabelName));
            }

            Parallel.ForEach(tasks, task =>
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{command} failed", task.Exception);
            });
        }

        private static Task RelayCommand(V1Pod pod, ICommand command, RelayServerOptions relayOptions, string podPortNoLabelName)
        {
            return Task.Run(() =>
            {
                // Create a new socket connection to the pod.
                string ip = pod.Status.PodIP;
                ushort port = GetPortNo(pod, podPortNoLabelName);
                Console.WriteLine($"Attempting connection to pod {pod.Name()} on {ip}:{port}");
                using (NetworkSocketClient conn = new NetworkSocketClient(relayOptions.Verbose, ip, port, Protocol.Managed))
                {
                    Console.WriteLine($"Connection to {pod.Name()} established. Sending command...");

                    // Relay the command to the pod.
                    conn.SendCommand(command);

                    Console.WriteLine($"Closing connection to {pod.Name()}...");
                }
            });
        }

        /// <summary>
        /// Get the port number on which a pod is listening.
        /// </summary>
        /// <param name="pod">A worker pod.</param>
        private static ushort GetPortNo(V1Pod pod, string podPortNoLabelName)
        {
            IDictionary<string, string> labels = pod.Metadata.Labels;
            if (labels == null)
                throw new InvalidOperationException($"Pod {pod.Name()} has no labels");
            if (!labels.TryGetValue(podPortNoLabelName, out string portString))
                throw new InvalidOperationException($"Pod {pod.Name()} has no {podPortNoLabelName} label");
            if (!ushort.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort port))
                throw new InvalidOperationException($"Unable to parse port number '{portString} for pod {pod.Name()}");
            return port;
        }
    }
}
