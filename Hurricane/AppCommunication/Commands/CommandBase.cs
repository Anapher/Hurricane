using Hurricane.Music;

namespace Hurricane.AppCommunication.Commands
{
    abstract class CommandBase
    {
        public abstract string RegexPattern { get; }
        public abstract void Execute(string line, StreamProvider streams, MusicManager musicManager);
    }
}
