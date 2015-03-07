using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hurricane.Music;
using Hurricane.Music.Playlist;

namespace Hurricane.AppCommunication.Commands
{
    class PlaylistCommand : CommandBase
    {
        public override string RegexPattern
        {
            get { return "^getPlaylists$"; }
        }

        public override void Execute(string line, StreamProvider streams, MusicManager musicManager)
        {
            var xmls = new XmlSerializer(typeof(List<NormalPlaylist>));
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
            {
                var playlists = musicManager.Playlists.ToList();
                foreach (var playlist in playlists)
                {
                    foreach (var track in playlist.Tracks.Where(track => !track.TrackExists))
                    {
                        playlist.Tracks.Remove(track);
                    }
                }

                xmls.Serialize(xmlWriter, playlists);
                var binaryWriter = streams.BinaryWriter;
                var bytesToSend = Encoding.UTF8.GetBytes(stringWriter.ToString());
                binaryWriter.Write(bytesToSend.Length);
                binaryWriter.Write(bytesToSend);
                binaryWriter.Flush();
            }
        }
    }
}