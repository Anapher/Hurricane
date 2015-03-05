using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hurricane.Music.Track;
using Hurricane.ViewModels;

namespace Hurricane.AppCommunication.Commands
{
    public class FileTransferCommand : CommandBase
    {
        private const int BufferSize = 4096;

        public override string RegexPattern
        {
            get { return "^getFile:(?<trackId>(.*?))$"; }
        }

        public override void Execute(string command, StreamProvider streams)
        {
            long id;
            if (!long.TryParse(Regex.Match(command, RegexPattern).Groups["trackId"].Value, out id))
            {
                streams.SendLine("getFile:invalidparameter");
                return;
            }

            var musicManager = MainViewModel.Instance.MusicManager;


            PlayableBase track = null;

            foreach (var playlist in musicManager.Playlists)
            {
                track = playlist.Tracks.FirstOrDefault(x => x.AuthenticationCode == id);
                if (track != null) break;
            }

            if (track == null)
            {
                streams.SendLine("getFile:tracknotfound");
                return;
            }

            var localTrack = track as LocalTrack;
            if (localTrack == null)
            {
                streams.SendLine("getFile:trackisstream");
                return;
            }

            if (!localTrack.TrackExists)
            {
                streams.SendLine("getFile:tracknotexists");
                return;
            }

            streams.SendLine("getFile:ok");

            using (var fs = new FileStream(localTrack.TrackInformation.FullName, FileMode.Open, FileAccess.Read))
            {
                var binaryWriter = streams.BinaryWriter;

                var totalLength = (int)fs.Length;
                binaryWriter.Write(totalLength);

                int noOfPackets = (int)Math.Ceiling((double)fs.Length / BufferSize);

                for (int i = 0; i < noOfPackets; i++)
                {
                    int currentPacketLength;
                    if (totalLength > BufferSize)
                    {
                        currentPacketLength = BufferSize;
                        totalLength -= currentPacketLength;
                    }
                    else
                    {
                        currentPacketLength = totalLength;
                    }

                    var sendingBuffer = new byte[currentPacketLength];
                    fs.Read(sendingBuffer, 0, currentPacketLength);
                    binaryWriter.Write(sendingBuffer, 0, sendingBuffer.Length);
                }
            }
        }
    }
}