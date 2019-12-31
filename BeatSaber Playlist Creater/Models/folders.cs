using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BeatSaber_Playlist_Mover.Models
{
    [Serializable, XmlRoot(Namespace = "", IsNullable = false)]
    public class folders
    {
        [XmlElement("folder")]
        public List<foldersFolder> folder { get; set; }
    }
}


