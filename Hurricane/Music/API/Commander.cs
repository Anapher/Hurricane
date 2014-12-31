using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hurricane.Music.API
{
    class Commander
    {
        protected const string AllRightText = "ar";

        public MusicManager MusicManager { get; protected set; }
        public List<Command> Commands { get; protected set; }


        public Commander(MusicManager manager)
        {
            this.MusicManager = manager;
            InitalizeCommands();
        }

        protected void InitalizeCommands()
        {
            Commands = new List<Command>
            {
                new Command("volume",
                    new List<CommandBase>
                    {
                        new CommandFunction("get", s => MusicManager.CSCoreEngine.Volume.ToString()),
                        new CommandAction("set", s => { MusicManager.CSCoreEngine.Volume = float.Parse(s); })
                    }),
                new Command("move",
                    new List<CommandBase>
                    {
                        new CommandAction("backward", s => MusicManager.GoBackward()),
                        new CommandAction("forward", s => MusicManager.GoForward())
                    }),
                new Command("isplaying",
                    new List<CommandBase>
                    {
                        new CommandFunction("get", s => MusicManager.CSCoreEngine.IsPlaying.ToString().ToLower()),
                        new CommandAction("toggle", s => MusicManager.CSCoreEngine.TogglePlayPause())
                    }),
                new Command("track",
                    new List<CommandBase>
                    {
                        new CommandFunction("get", s => TrackToString(MusicManager.CSCoreEngine.CurrentTrack))
                    }),
                new Command("shuffle",
                    new List<CommandBase>
                    {
                        new CommandFunction("get", s => MusicManager.IsShuffleEnabled.ToString().ToLower()),
                        new CommandAction("set", s => MusicManager.IsShuffleEnabled = bool.Parse(s))
                    }),
                new Command("loop",
                    new List<CommandBase>
                    {
                        new CommandFunction("get", s => MusicManager.IsLoopEnabled.ToString().ToLower()),
                        new CommandAction("set", s => MusicManager.IsLoopEnabled = bool.Parse(s))
                    }),
                new Command("position",
                    new List<CommandBase>
                    {
                        new CommandFunction("get",  s => string.Format("{0} {1}",(int) MusicManager.CSCoreEngine.CurrentTrackPosition.TotalSeconds,(int) MusicManager.CSCoreEngine.CurrentTrackLength.TotalSeconds))
                    })
            };
        }

        protected string TrackToString(Track track)
        {
            return JsonConvert.SerializeObject(new ShortTrack() { Title = track.Title, Artist = track.Artist, Album = track.Album, Duration = track.Duration, kbps = track.kbps.ToString(), kHz = track.kHz.ToString() });
        }

        [JsonObject("Track")]
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

                            return string.Empty;
                        }
                    }
                    break;
                }
            }

            return string.Format("Error: can't identify '{0}.'", commandstr);
        }
    }
}
