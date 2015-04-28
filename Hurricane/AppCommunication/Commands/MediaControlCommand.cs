using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSCore;
using Hurricane.Music;
using Hurricane.Utilities;

namespace Hurricane.AppCommunication.Commands
{
    class MediaControlCommand : CommandBase
    {
        public override string RegexPattern
        {
            get { return "^mediaControl;(?<command>(.*?))$"; }
        }

        private static List<MediaCommand> _mediaCommands; 
        static List<MediaCommand> MediaCommands
        {
            get
            {
                return _mediaCommands ?? (_mediaCommands = new List<MediaCommand>
                {
                    new MediaCommand("^nextTrack$", (s, provider, arg3) => arg3.GoForward()),
                    new MediaCommand("^previousTrack$", (s, provider, arg3) => arg3.GoBackward()),
                    new MediaCommand("^getCurrentPosition$", (s, provider, arg3) => provider.SendLine(Utilities.TimeSpanToString(arg3.CSCoreEngine.CurrentTrackPosition))),
                    new MediaCommand("^getCurrentLength$", (s, provider, arg3) => provider.SendLine(Utilities.TimeSpanToString(arg3.CSCoreEngine.CurrentTrackLength))),
                    new MediaCommand("^setCurrentPosition;(?<position>(.*?))$", (s, provider, arg3) =>
                    {
                        if (arg3.CSCoreEngine.SoundSource != null) arg3.CSCoreEngine.SoundSource.SetPosition(Utilities.StringToTimeSpan(Regex.Match(s, "^setCurrentPosition;(?<position>(.*?))$").Groups["position"].Value));
                    }),
                    new MediaCommand("^openTrack;(?<id>([0-9]+))$", (s, provider, arg3) =>
                    {
                        var track =
                            Utilities.GetTrackByAuthenticationCode(
                                long.Parse(Regex.Match(s, "^openTrack;(?<id>([0-9]+))$").Groups["id"].Value), arg3.Playlists);
                        if (track == null) return;
                        arg3.CSCoreEngine.OpenTrack(track).Forget();
                    }),
                    new MediaCommand("^getIsPlaying$", (s, provider, arg3) => provider.SendLine(arg3.CSCoreEngine.IsPlaying.ToString().ToLower())),
                    new MediaCommand("^toggleIsPlaying$", (s, provider, arg3) => arg3.CSCoreEngine.TogglePlayPause())
                });
            }
        }

        public override void Execute(string line, StreamProvider streams, MusicManager musicManager)
        {
            var command = Regex.Match(line, RegexPattern).Groups["command"].Value;
            MediaCommands.First(x => Regex.IsMatch(command, x.RegexPattern)).Execute(line, streams, musicManager);
        }

        class MediaCommand : CommandBase
        {
            private readonly string _regexPattern;
            public override string RegexPattern
            {
                get { return _regexPattern; }
            }

            private readonly Action<string, StreamProvider, MusicManager> _action;
            public override void Execute(string line, StreamProvider streams, MusicManager musicManager)
            {
                _action.Invoke(line, streams, musicManager);
            }
             
            public MediaCommand(string regexPattern, Action<string, StreamProvider, MusicManager> action)
            {
                _regexPattern = regexPattern;
                _action = action;
            }
        }
    }
}