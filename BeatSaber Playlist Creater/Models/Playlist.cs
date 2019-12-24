using System.Collections.Generic;

namespace BeatSaber_Playlist_Creater.Models
{
    public class Playlist
    {
        public string playlistTitle { get; set; }
        public string playlistAuthor { get; set; }
        public string playlistDescription { get; set; }
        public string image { get; set; }
        public List<Song> songs { get; set; }
    }
}
