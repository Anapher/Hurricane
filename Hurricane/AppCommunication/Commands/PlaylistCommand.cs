using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Hurricane.Music.Playlist;
using Hurricane.ViewModels;

namespace Hurricane.AppCommunication.Commands
{
    public class PlaylistCommand : CommandBase
    {
        public override string RegexPattern
        {
            get { return "^getPlaylists$"; }
        }

        public override void Execute(string command, StreamProvider streams)
        {
            var xmls = new XmlSerializer(typeof (List<NormalPlaylist>));
            using (var stringWriter = new StringWriter())
            {
                var playlists = MainViewModel.Instance.MusicManager.Playlists.ToList();
                foreach (var playlist in playlists)
                {
                    foreach (var track in playlist.Tracks.Where(track => !track.TrackExists))
                    {
                        playlist.Tracks.Remove(track);
                    }
                }
                xmls.Serialize(stringWriter, playlists);
                var binaryWriter = streams.BinaryWriter;

                var bytesToSend = Encoding.Unicode.GetBytes(stringWriter.ToString());
                binaryWriter.Write(bytesToSend.Length);
                binaryWriter.Write(bytesToSend);
                binaryWriter.Flush();
            }
        }
    }
}
