using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hurricane.Music.API
{
    class TcpConnection : IDisposable
    {
        public TcpClient Client { get; set; }
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }

        public bool VolumeChangedEvent { get; protected set; }
        public bool PositionChangedEvent { get; protected set; }
        public bool TrackChangedEvent { get; protected set; }
        public bool PlayStateChangedEvent { get; protected set; }

        public void RegisterPositionChanged()
        {
            manager.CSCoreEngine.PositionChanged += CSCoreEngine_PositionChanged;
            this.PositionChangedEvent = true;
        }

        public void RegisterTrackChanged()
        {
            manager.CSCoreEngine.TrackChanged += CSCoreEngine_TrackChanged;
            this.TrackChangedEvent = true;
        }

        public void RegisterVolumeChanged()
        {
            manager.CSCoreEngine.VolumeChanged += CSCoreEngine_VolumeChanged;
            this.VolumeChangedEvent = true;
        }

        public void RegisterPlayStateChanged()
        {
            manager.CSCoreEngine.PlaybackStateChanged += CSCoreEngine_PlaybackStateChanged;
            this.PlayStateChangedEvent = true;
        }

        void CSCoreEngine_PlaybackStateChanged(object sender, Music.PlayStateChangedEventArgs e)
        {
            WriteLine("event playstate");
        }

        void CSCoreEngine_VolumeChanged(object sender, EventArgs e)
        {
            WriteLine("event volume");
        }

        void CSCoreEngine_TrackChanged(object sender, Music.TrackChangedEventArgs e)
        {
            WriteLine("event track");
        }

        void CSCoreEngine_PositionChanged(object sender, Music.PositionChangedEventArgs e)
        {
            WriteLine(string.Format("event position {0} {1}", e.NewPosition, e.TrackLength));
        }

        protected Music.MusicManager manager;
        public TcpConnection(TcpClient client, Music.MusicManager manager)
        {
            this.Client = client;
            this.Reader = new StreamReader(client.GetStream());
            this.Writer = new StreamWriter(client.GetStream());
            this.manager = manager;
        }

        public void WriteLine(string line)
        {
            Writer.WriteLine(line);
            Writer.Flush();
        }

        public void Dispose()
        {
            Client.Close();
            Reader.Dispose();
            Writer.Dispose();
            if (PositionChangedEvent) manager.CSCoreEngine.PositionChanged -= CSCoreEngine_PositionChanged;
            if (VolumeChangedEvent) manager.CSCoreEngine.VolumeChanged -= CSCoreEngine_VolumeChanged;
            if (TrackChangedEvent) manager.CSCoreEngine.TrackChanged -= CSCoreEngine_TrackChanged;
            if (PlayStateChangedEvent) manager.CSCoreEngine.PlaybackStateChanged -= CSCoreEngine_PlaybackStateChanged;
        }
    }
}
