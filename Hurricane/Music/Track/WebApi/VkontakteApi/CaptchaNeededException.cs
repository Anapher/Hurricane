using System;

namespace Hurricane.Music.Track.WebApi.VkontakteApi
{
    [Serializable]
    public class CaptchaNeededException : Exception
    {
        public long Sid { get; private set; }
        public Uri Img { get; private set; }

        public CaptchaNeededException(long sid, string img)
            : this(sid, string.IsNullOrEmpty(img) ? null : new Uri(img))
        {
        }

        public CaptchaNeededException(long sid, Uri img)
        {
            Sid = sid;
            Img = img;
        }
    }
}
