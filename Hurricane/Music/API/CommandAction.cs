using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.API
{
    class CommandAction : CommandBase
    {
        public Action<string> Action { get; protected set; }

        public CommandAction(string token, Action<string> action)
        {
            this.Token = token;
            this.Action = action;
        }
    }
}
