using BeatSaber_Playlist_Mover.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace BeatSaber_Playlist_Mover
{
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
            UpdateStatus(string.Empty);
            if (string.IsNullOrWhiteSpace(_basePath))
            {
                UpdateStatus("You MUST set select your beat saber folder first!\n");
                return;
            }
            if (!CheckRequiredFoldersAndFiles())
            {
                UpdateStatus("\nCan not continue until all files and folders required are in place.....");
                return;
            }
            UpdateStatus("\n\nStarting........\nGetting song hash info.....");
            _songHashDisctionary = await parser.GetSongFolderHashesAsync(_basePath);
            UpdateStatus("\nGot hashes. Looking for playlists.....");
            _folders = parser.GetFoldersFromXml(_basePath);
            if (!_folders.folder.Any())
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Oh no! It looks like you either have an empty folders.xml file OR you have capitalized WIP values. If you have not manually changed the file continue. If you have changed the file, go back and make the WIP values all lower case. Do you want to continue anyways and overwrite the folders.xml file?", "Overwrite Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    UpdateStatus("\n\nCould not continue!\nIf you have True or False values in your folders.xml file you NEED to change them to be all lower case. Make any \"False\" values false and any \"True\" values true. Then hit start to try again.");
                    return;
                }
                else
                {
                    UpdateStatus("\n\nContinuing. This will overwrite your existing xml file with a valid one.");
                }
            }
            _folders.folder.RemoveAll(x => x.Path.StartsWith("Example")); // We don't want to include examples here
            var playlists = FindPlaylists();
            CreatePlaylistFolders(playlists);
            UpdateStatus("\n\nSaving folders xml file.....");
            parser.SaveFoldersToXml(_basePath, _folders);
            ForceRename();
            UpdateStatus("\n\n\nDone!"); 
        }

        private bool CheckRequiredFoldersAndFiles()
        {
            var result = true;
            if (!Directory.Exists($@"{_basePath}\UserData\SongCore"))
            {
                UpdateStatus("\n Oh no! You don't have a required folder. Please ensure you have the SongCore mod installed and have already run beat saber at least once with the SongCore mod. Missing folder UserData\\SongCore.");
                result = false;
            } 

            if (!Directory.Exists($@"{_basePath}\Playlists"))
            {
                UpdateStatus("\nOh no! You don't have a required folder. Please ensure you have already installed your mods and have already downloaded playlists. Missing folder Playlists.");
                result = false;
            } else if (!Directory.EnumerateFiles($@"{_basePath}\Playlists").Any())
            {
                UpdateStatus("\nOh no! You don't have any playlists yet. Please ensure you have downloaded at least 1 playlist. Playlists folder has no files.");
                result = false;
            }

            if (!Directory.Exists($@"{_basePath}\Beat Saber_Data\CustomLevels"))
            {
                UpdateStatus("\nOh no! You don't have a required folder. Please ensure you have the SongCore mod installed and have already run beat saber at least once with the SongCore mod. Missing folder Beat Saber_Data\\CustomLevels.");
                result = false;
            }

            if (!File.Exists($@"{_basePath}\UserData\SongCore\folders.xml"))
            {
                UpdateStatus("\nOh no! You don't have a required file. Please ensure you have the SongCore mod installed and have already run beat saber at least once with the SongCore mod. Missing file UserData\\SongCore\\folders.xml.");
                result = false;
            }
            

            return result;
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
                if(playlist.songs.Any())
                {
                    UpdateStatus($"\n\nFound playlist {playlist.playlistTitle} with {playlist.songs.Count} songs. Moving songs to new playlist folder now.....");
                }
                else
                {
                    UpdateStatus($"\n\nFound playlist {playlist.playlistTitle} But it has no songs. Skipping playlist because it's empty....");
                    continue;
                }

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
                UpdateStatus($"\\nnRenaming the Beat Saber_Data\\CustomLevels folder now....");
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
                UpdateStatus($"\n\nChecking for song {song.songName}......");
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
                        UpdateStatus($"\nCopied {song.songName} to Playlist folder...");
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
            if (string.IsNullOrWhiteSpace(message))
            {
                Dispatcher.Invoke(() => StatusText.Text = string.Empty);
            }
            else
            {
                Dispatcher.Invoke(() => StatusText.Text += message);
            }
           
        }
    }
}
