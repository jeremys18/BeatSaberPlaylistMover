using BeatSaber_Playlist_Creater.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BeatSaber_Playlist_Creater
{
    public class Parser
    {
        public List<Playlist> GetPlaylists(string path)
        {
            var playlists = new List<Playlist>();
            var fileNames = GetPlayistFileNames(path);

            foreach(var fileName in fileNames)
            {
                var playlist = GetPlaylist(path + "\\playlists\\" + fileName);
                playlists.Add(playlist);
            }

            return playlists;
        }

        private List<string> GetPlayistFileNames(string path)
        {
            var fileNames = new List<string>();
            var folder = new DirectoryInfo(path + @"\Playlists");
            FileInfo[] files = folder.GetFiles(); 

            foreach (FileInfo file in files)
            {
               fileNames.Add(file.Name);
            }
            return fileNames;
        }

        private Playlist GetPlaylist(string path)
        {
            var playlist = new Playlist();
            using (StreamReader r = new StreamReader(path))
            {
                var json = r.ReadToEnd();
                playlist = JsonConvert.DeserializeObject<Playlist>(json);
            }
            return playlist;
        }

        public string GetSongPath(string path, string songHash, Dictionary<string,string> hashDisctionary)
        {
            var basePath = path + @"\Beat Saber_Data\CustomLevels";

            var foldername = hashDisctionary.FirstOrDefault(x => x.Value == songHash).Key;

            return string.IsNullOrWhiteSpace(foldername) ? "" :  $@"{basePath}\{foldername}";
        }

        public folders GetFoldersFromXml(string path)
        {
            var result = new folders();
            var filePath = $@"{path}\UserData\SongCore\folders.xml";

            XmlSerializer ser = new XmlSerializer(typeof(folders));

            try
            {
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    result = (folders)ser.Deserialize(reader);
                }
            }
            catch(Exception e)
            {
                result = new folders
                {
                    folder = new List<foldersFolder>()
                };
            }
            
            return result;
        }

        public void SaveFoldersToXml(string path, folders folders)
        {
            // removes version
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            XmlSerializer xsSubmit = new XmlSerializer(typeof(folders));
            using (StringWriter sw = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sw, settings))
            {
                // removes namespace
                var xmlns = new XmlSerializerNamespaces();
                xmlns.Add(string.Empty, string.Empty);

                xsSubmit.Serialize(writer, folders, xmlns);
                var content = sw.ToString();
                var xml_formatted = XDocument.Parse(content).ToString();
                File.WriteAllText($@"{path}\UserData\SongCore\folders.xml", xml_formatted);
            }
        }

        public async Task<Dictionary<string, string>> GetSongFolderHashesAsync(string path)
        {
            var result = new Dictionary<string, string>();
            var d = new DirectoryInfo($@"{path}\Beat Saber_Data\CustomLevels");

            foreach (var folder in d.GetDirectories())
            {
                try
                {
                    var client = new HttpClient();
                    var id = folder.Name.Split(' ')[0].Trim();

                    var response = await client.GetAsync("https://beatsaver.com/api/stats/key/" + id);
                    var json = await response.Content.ReadAsStringAsync();
                    var info = JsonConvert.DeserializeObject<SongInformation>(json);

                    result.Add(folder.Name, info.hash);
                }
                catch(Exception e)
                {

                }
                
            }

            return result;
        }
    }
}
