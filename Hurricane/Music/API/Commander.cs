using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.API
{
    class Commander
    {
        protected const string AllRightText = "ar";

        public Music.MusicManager MusicManager { get; protected set; }
        public List<Command> Commands { get; protected set; }


        public Commander(Music.MusicManager manager)
        {
            this.MusicManager = manager;
            InitalizeCommands();
        }

        protected void InitalizeCommands()
        {
            Commands = new List<Command>();

            Commands.Add(new Command("volume", new List<CommandBase> { new CommandFunction("get", (s) => { return MusicManager.CSCoreEngine.Volume.ToString(); }), new CommandAction("set", (s) => { MusicManager.CSCoreEngine.Volume = float.Parse(s); }) }));
            Commands.Add(new Command("move", new List<CommandBase> { new CommandAction("backward", (s) => MusicManager.GoBackward()), new CommandAction("forward", (s) => MusicManager.GoForward()) }));
            Commands.Add(new Command("isplaying", new List<CommandBase> { new CommandFunction("get", (s) => { return MusicManager.CSCoreEngine.IsPlaying.ToString().ToLower(); }), new CommandAction("toggle", (s) => MusicManager.CSCoreEngine.TogglePlayPause()) }));
            Commands.Add(new Command("track", new List<CommandBase> { new CommandFunction("get", (s) => { return TrackToString(MusicManager.CSCoreEngine.CurrentTrack); }) }));
            Commands.Add(new Command("shuffle", new List<CommandBase> { new CommandFunction("get", (s) => { return MusicManager.RandomTrack.ToString().ToLower(); }), new CommandAction("set", (s) => MusicManager.RandomTrack = bool.Parse(s)) }));
            Commands.Add(new Command("repeat", new List<CommandBase> { new CommandFunction("get", (s) => { return MusicManager.RepeatTrack.ToString().ToLower(); }), new CommandAction("set", (s) => MusicManager.RepeatTrack = bool.Parse(s)) }));
            Commands.Add(new Command("position", new List<CommandBase> { new CommandFunction("get", (s) => { return string.Format("{0} {1}", (int)MusicManager.CSCoreEngine.CurrentTrackPosition.TotalSeconds, (int)MusicManager.CSCoreEngine.CurrentTrackLength.TotalSeconds); }) }));
        }

        protected string TrackToString(Music.Track track)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new ShortTrack() { Title = track.Title, Artist = track.Artist, Album = track.Album, Duration = track.Duration, kbps = track.kbps.ToString(), kHz = track.kHz.ToString() });
        }

        [Newtonsoft.Json.JsonObject("Track")]
        public class ShortTrack
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string Duration { get; set; }
            public string kHz { get; set; }
            public string kbps { get; set; }
        }

        public string ExecuteCommand(string commandstr)
        {
            if (string.IsNullOrEmpty(commandstr)) return null;
            string[] commandlineArguments = commandstr.ToLower().Split(new char[] { ' ' });
            foreach (var command in this.Commands)
            {
                if (command.Token == commandlineArguments[0])
                {
                    foreach (var action in command.CommandActions)
                    {
                        if (action.Token == commandlineArguments[1])
                        {
                            string parameter = string.Empty;
                            if (commandlineArguments.Length > 2) parameter = commandlineArguments[2];
                            try
                            {
                                if (action.GetType() == typeof(CommandAction))
                                { ((CommandAction)action).Action.Invoke(parameter); }
                                else { return command.Token + " " + ((CommandFunction)action).Function.Invoke(parameter); }
                            }
                            catch (Exception ex)
                            {
                                return "Error: " + ex.Message;
                            }

                            return AllRightText;
                        }
                    }
                    break;
                }
            }

            return string.Format("Error: can't identify '{0}.'", commandstr);
        }
    }
}
