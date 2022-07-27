using System;
using System.Collections.Generic;
using System.Linq;
using Models.Core.Replace;
using Models.Core.Run;
using Models.Storage;

namespace APSIM.Server.Commands
{
    /// <summary>
    /// A command to run simulations.
    /// </summary>
    [Serializable]
    public class RunCommand : ICommand
    {
        private bool runPostSimulationTools;
        private bool runTests;
        private IEnumerable<string> simulationNamesToRun;
        private int numberOfProcessors;
        
        /// <summary>Changes to apply to simulations before running</summary>
        public IEnumerable<IReplacement> Changes { get; set; }

        /// <summary>
        /// Creates a <see cref="RunCommand" /> instance with sensible defaults.
        /// </summary>
        /// <param name="changes">Changes to be applied to the simulations before being run.</param>
        public RunCommand(IEnumerable<IReplacement> changes)
        {
            runPostSimulationTools = true;
            runTests = true;
            numberOfProcessors = -1;
            this.Changes = changes;
            simulationNamesToRun = null;
        }

        /// <summary>
        /// Create a <see cref="RunCommand" /> instance.
        /// </summary>
        /// <param name="runPostSimTools">Should post-simulation tools be run?</param>
        /// <param name="runTests">Should tests be run?</param>
        /// <param name="numProcessors">Max number of processors to use.</param>
        /// <param name="simulationNames">Simulation names to run.</param>
        /// <param name="changes">Changes to be applied to the simulations before being run.</param>
        public RunCommand(bool runPostSimTools, bool runTests, int numProcessors, IEnumerable<IReplacement> changes, IEnumerable<string> simulationNames)
        {
            runPostSimulationTools = runPostSimTools;
            this.runTests = runTests;
            numberOfProcessors = numProcessors;
            this.Changes = changes;
            simulationNamesToRun = simulationNames;
        }

        public override bool Equals(object obj)
        {
            if (obj is RunCommand command)
            {
                if (runPostSimulationTools != command.runPostSimulationTools)
                    return false;
                if (runTests != command.runTests)
                    return false;
                if (numberOfProcessors != command.numberOfProcessors)
                    return false;
                if (simulationNamesToRun.Count() != command.simulationNamesToRun.Count())
                    return false;
                if (simulationNamesToRun.Zip(command.simulationNamesToRun, (x, y) => x != y).Any(x => x))
                    return false;
                if (Changes.Count() != command.Changes.Count())
                    return false;
                if (Changes.Zip(command.Changes, (x, y) => !x.Equals(y)).Any(x => x))
                    return false;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (runPostSimulationTools, runTests, numberOfProcessors, simulationNamesToRun, Changes).GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} with {Changes.Count()} changes";
        }
    }
}
