using BeatSaber_Playlist_Creater.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace BeatSaber_Playlist_Creater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Status { get; set; }
        private string _basePath { get; set; }
        private bool _forceRename { get; set; }
        private Dictionary<string,string> _songHashDisctionary { get; set; }

        private folders _folders { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _forceRename = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                Start();
            }).Start();
        }

        private async void Start()
        {
            var parser = new Parser();
            if (string.IsNullOrWhiteSpace(_basePath))
            {
                StatusText.Text = "You MUST set select your beat saber folder first!";
                return;
            }
            UpdateStatus("Starting........\nGetting song hash info.....");
            _songHashDisctionary = await parser.GetSongFolderHashesAsync(_basePath);
            UpdateStatus("\nGot hases. Looking for playlists.....");
            _folders = parser.GetFoldersFromXml(_basePath);
            var playlists = FindPlaylists();
            CreatePlaylistFolders(playlists);
            UpdateStatus("Saving folders xml file.....");
            parser.SaveFoldersToXml(_basePath, _folders);
            ForceRename();
            UpdateStatus("\n\n\nDone!");
        }

        private List<Playlist> FindPlaylists()
        {
            var playlists = new Parser().GetPlaylists(_basePath);
            return playlists;
        }

        private void CreatePlaylistFolders(List<Playlist> playlists)
        {
            foreach (var playlist in playlists)
            {
                UpdateStatus($"\nFound playlist {playlist.playlistTitle} with {playlist.songs.Count} songs. Moving songs to new playlist folder now.....");
                CreatePlaylistFolder(playlist);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var path = dialog.SelectedPath;
            BasePathLabel.Content = path;
            _basePath = path;
        }

        private void ForceRename()
        {
            if (_forceRename)
            {
                UpdateStatus($"\nRenaming the Beat Saber_Data\\CustomLevels folder now....");
                try
                {
                    Directory.Move($@"{_basePath}\Beat Saber_Data\CustomLevels", $@"{_basePath}\Beat Saber_Data\CustomLevelsRenamed");
                }
                catch (Exception e)
                {
                    UpdateStatus($"\nRenaming failed! Your folder is corrupt!");
                }
            }
        }

        private void CreatePlaylistFolder(Playlist playlist)
        {
            var playlistPath = _basePath + "\\Playlists\\" + playlist.playlistTitle;
            if (Directory.Exists(playlistPath))
            {
                UpdateStatus($"\nPlaylist folder {playlist.playlistTitle} already exists");
            }
            else
            {
                Directory.CreateDirectory(playlistPath);
                UpdateStatus($"\nCreated Playlist folder {playlist.playlistTitle}\nCreating cover photo....");
                var imageString = playlist.image.Split(',')[playlist.image.Split(',').Length - 1];
                File.WriteAllBytes($@"{playlistPath}\cover.jpg", Convert.FromBase64String(imageString));
            }

            foreach( var song in playlist.songs)
            {
                UpdateStatus($"\nChecking for song {song.songName}......");
                var path = new Parser().GetSongPath(_basePath, song.hash, _songHashDisctionary);
                if (path == "")
                {
                    UpdateStatus($"\nSong {song.songName} not found! Could have been moved already or the song hasn't been downloaded yet.");
                    continue;
                }

                var d = new DirectoryInfo(path);
                var songPath = $@"{_basePath}\Playlists\{playlist.playlistTitle}\{d.Name}";
                
                if (Directory.Exists(songPath))
                {
                    UpdateStatus($"\nSong {song.songName} already exists in playlist folder. Skipping.....");
                }
                else
                {
                    var isSafeToDelete = false;
                    try
                    {
                        Directory.CreateDirectory(songPath);
                        foreach (var file in d.GetFiles())
                        {
                            file.CopyTo($@"{songPath}\{file.Name}");
                        }
                        isSafeToDelete = true;
                    }
                    catch (Exception e)
                    {
                        UpdateStatus($"\nAn error occured while trying to copy files over for song {song.songName}. Be sure the folder exists and is not corrupt.");
                    }
                    try
                    {
                        if (isSafeToDelete)
                        {
                            d.Delete(true);
                            UpdateStatus($"\nDeleted {song.songName} folder from CustomLevels folder");
                        }
                    }
                    catch(Exception e)
                    {
                        UpdateStatus($"\nCould not delete {song.songName} folder from CustomLevels folder. There will be duplicates. Because of this renaming will apply to custom levels folder automatically");
                        _forceRename = true;
                    }
                }
            }
            if (!_folders.folder.Any(x => x.Name == playlist.playlistTitle))
            {
                var folder = new foldersFolder
                {
                    Name = playlist.playlistTitle,
                    Pack = 2,
                    WIP = false,
                    Path = playlistPath,
                    ImagePath = $@"{playlistPath}\cover.jpg"
                };
                _folders.folder.Add(folder);
            }
        }

        private void UpdateStatus(string message)
        {
            this.Dispatcher.Invoke(() => StatusText.Text += message) ;
        }
    }
}
