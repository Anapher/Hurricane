using System.Collections.Generic;
using Hurricane.Music.Track;

namespace Hurricane.Music.Data
{
    class TrackComparer : IEqualityComparer<PlayableBase>
    {
        public Dictionary<string, string> FileHashes { get; set; }
        public bool Equals(PlayableBase x, PlayableBase y)
        {
            if (x == null || y == null) return false; //would crash if it needs to compute the hash
            return x.Equals(y);
        }

        public int GetHashCode(PlayableBase track)
        {
            //Check whether the object is null 
            if (ReferenceEquals(track, null)) return 0;

            //Get hash code for the Title field if it is not null. 
            int hashProductName = track.Title == null ? 0 : track.Title.GetHashCode();

            //Get hash code for the Duration field. 
            int hashProductCode = track.Duration.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }
}
