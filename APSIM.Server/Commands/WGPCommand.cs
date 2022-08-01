using Models.Core.Replace;
using System;
using System.Collections.Generic;
using System.Text;

namespace APSIM.Server.Commands
{
    [Serializable]
    public class WGPCommand : ICommand
    {
        public IEnumerable<IEnumerable<IReplacement> > VariablesToUpdate { get; set; }

        public WGPCommand(IEnumerable<IEnumerable<IReplacement> > variables)
        {
            VariablesToUpdate = variables;
        }

    }
}
