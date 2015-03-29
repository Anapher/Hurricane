using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
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
                MessageBox.Show(bytesToSend.ToString());
                MessageBox.Show(Encoding.UTF8.GetString(bytesToSend));

                const int bufferSize = 1024;
                int noOfPackets = (int)Math.Ceiling((double)bytesToSend.Length / bufferSize);
                int totalLength = bytesToSend.Length;

                for (int i = 0; i < noOfPackets; i++)
                {
                    int currentPacketLength;
                    if (totalLength > bufferSize)
                    {
                        currentPacketLength = bufferSize;
                        totalLength -= currentPacketLength;
                    }
                    else
                    {
                        currentPacketLength = totalLength;
                    }

                    var sendingBuffer = new byte[currentPacketLength];
                    binaryWriter.Write(sendingBuffer, 0, sendingBuffer.Length);
                    binaryWriter.Flush();
                }
            }
        }
    }
}