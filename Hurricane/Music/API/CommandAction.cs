using System;

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
