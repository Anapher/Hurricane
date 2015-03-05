using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hurricane.AppCommunication.Commands;

namespace Hurricane.AppCommunication
{
    public class Commander
    {
        private static List<CommandBase> _commands;
 
        public static void ExecuteCommand(string line, StreamProvider streams)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (_commands == null)
            {
                _commands = new List<CommandBase> {new FileTransferCommand(), new PlaylistCommand()};
            }

            var command = _commands.First(x => Regex.IsMatch(line, x.RegexPattern));
            if (command != null) command.Execute(line, streams);
        }
    }
}
