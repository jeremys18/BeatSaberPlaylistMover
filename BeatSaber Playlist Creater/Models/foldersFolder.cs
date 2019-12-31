using System.Xml.Serialization;

namespace BeatSaber_Playlist_Mover.Models
{
    [XmlType(AnonymousType = true)]
    public class foldersFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Pack { get; set; }
        public bool WIP { get; set; }
        public string ImagePath { get; set; }
    }
}
