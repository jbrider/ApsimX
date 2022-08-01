using APSIM.Server.IO;
using Models.Core.Run;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static void HandleCommandRelay(this ICommand command, IEnumerable<WorkerPod> workers)
        {
            if (command is WGPCommand)
            {
                (command as WGPCommand).HandleCommandRelay(workers);
                return;
            }
            
            List<Task> tasks = new List<Task>();
            foreach (var pod in workers)
                tasks.Add(RelayCommand(pod, command));

            Parallel.ForEach(tasks, task =>
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{command} failed", task.Exception);
            });
        }

        public static void HandleCommandRelay(this WGPCommand command, IEnumerable<WorkerPod> workers)
        {
            Console.WriteLine($"Handling WGP Command Relay to see if it goes where it should");
            List<Task> tasks = new List<Task>();
            var workerCommands = command.VariablesToUpdate.Zip(workers, (cmd, worker) => (cmd, worker));
            foreach (var wc in workerCommands)
            {
                var newCommand = new RunCommand(wc.cmd);
                tasks.Add(RelayCommand(wc.worker, newCommand));
            }

            Parallel.ForEach(tasks, task =>
            {
                task.Wait();
                if (task.Status == TaskStatus.Faulted || task.Exception != null)
                    throw new Exception($"{command} failed", task.Exception);
            });
        }

        private static Task RelayCommand(WorkerPod pod, ICommand command)
        {
            return Task.Run(() =>
            {
                if(pod.SocketConnection != null)
                {
                    pod.SocketConnection.SendCommand(command);
                }
                else
                {
                    // Create a new socket connection to the pod.
                    string ip = pod.IPAddress;
                    ushort port = pod.Port;
                    Console.WriteLine($"Attempting connection to pod {pod.Name} on {ip}:{port}");
                    using (NetworkSocketClient conn = new NetworkSocketClient(pod.Options.Verbose, ip, port, Protocol.Managed))
                    {
                        Console.WriteLine($"Connection to {pod.Name} established. Sending command...");

                        // Relay the command to the pod.
                        conn.SendCommand(command);

                        Console.WriteLine($"Closing connection to {pod.Name}...");
                    }
                }
            });
        }

    }
}
