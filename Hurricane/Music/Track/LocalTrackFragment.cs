using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CSCore;
using CSCore.Codecs;

namespace Hurricane.Music.Track
{
    public class LocalTrackFragment : LocalTrack
    {
        public LocalTrackFragment()
        { }

        public LocalTrackFragment(TimeSpan start_from, string title)
        {
            _offset = start_from;
            _title = title;
            Title = title;
        }

        protected override async Task<bool> UpdateInformation(FileInfo filename)
        {
            try
            {
                await Task.Run(() => {
                    using (var source = CodecFactory.Instance.GetCodec(filename.FullName))
                    {
                        kHz = source.WaveFormat.SampleRate / 1000;
                        var duration = source.GetLength();
                        SetDuration(duration - _offset);
                    }
                });

                IsChecked = true;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [XmlElement("Offset")]
        TimeSpan _offset;

        string _title;
    }
}
