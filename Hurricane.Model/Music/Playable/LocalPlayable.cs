using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public class LocalPlayable : PlayableBase
    {
        public string TrackPath { get; set; }
        public override bool IsAvailable => File.Exists(TrackPath);

        [XmlAttribute]
        public string Extension { get; set; }

        [XmlAttribute]
        public double SampleRate { get; set; }

        [XmlAttribute, DefaultValue(0)]
        public int Bitrate { get; set; }

        public override Task<IPlaySource> GetSoundSource()
        {
            return Task.Run(() => (IPlaySource) (new LocalFilePlaySource(TrackPath)));
        }

        public async override Task LoadImage()
        {
            
        }
    }
}