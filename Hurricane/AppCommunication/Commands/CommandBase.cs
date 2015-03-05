namespace Hurricane.AppCommunication.Commands
{
    public abstract class CommandBase
    {
        public abstract string RegexPattern { get; }
        public abstract void Execute(string command, StreamProvider streams);
    }
}
