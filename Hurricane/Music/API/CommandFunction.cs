using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.API
{
    class CommandFunction : CommandBase
    {
        public Func<string, string> Function { get; protected set; }

        public CommandFunction(string token, Func<string, string> function)
        {
            this.Token = token;
            this.Function = function;
        }
    }
}
