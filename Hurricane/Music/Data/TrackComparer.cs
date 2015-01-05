using System.Collections.Generic;
using Hurricane.Utilities;

namespace Hurricane.Music
{
    class TrackComparer : IEqualityComparer<Track>
    {
        public Dictionary<string, string> FileHashes { get; set; }
        public bool Equals(Track x, Track y)
        {
            if (x == null || y == null || !x.TrackExists || !y.TrackExists) return false; //would crash if it needs to compute the hash
            return x.TrackInformation.Length == y.TrackInformation.Length && GetHash(x.TrackInformation.FullName) == GetHash(y.TrackInformation.FullName);
        }

        protected string GetHash(string path)
        {
            if (FileHashes.ContainsKey(path))
            {
                return FileHashes[path];
            }
            else
            {
                string hash = GeneralHelper.FileToMD5Hash(path);
                FileHashes.Add(path, hash);
                return hash;
            }
        }

        public int GetHashCode(Track track)
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

        public TrackComparer()
        {
            FileHashes = new Dictionary<string, string>();
        }
    }
}
