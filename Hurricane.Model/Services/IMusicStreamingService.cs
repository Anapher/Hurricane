using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.Model.Services
{
    public interface IMusicStreamingService
    {
        string Name { get; }
        string Url { get; }
        Geometry Icon { get; }
        Type StreamableType { get; }

        string PluginAuthor { get; }
        string PluginAuthorWebsite { get; }
        string PluginDescription { get; }

        /// <summary>
        /// Fast search. Fast is all that counts
        /// </summary>
        /// <param name="query">The search query</param>
        /// <returns>Return ~5 results</returns>
        Task<IEnumerable<ISearchResult>> FastSearch(string query);

        /// <summary>
        /// Search.
        /// </summary>
        /// <param name="query">The search query</param>
        /// <returns>Return ~50 results</returns>
        Task<IEnumerable<ISearchResult>> Search(string query);

        /// <summary>
        /// Search the track name and return the best result (most views, best match, whatever)
        /// </summary>
        /// <param name="trackName">The name of the track</param>
        /// <returns>Returns the track</returns>
        Task<ISearchResult> GetTrack(string trackName);
    }
}