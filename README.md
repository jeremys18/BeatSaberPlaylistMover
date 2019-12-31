# BeatSaberPlaylistMover
Fixes playlist issue in BeatSaber 1.6 by moving playlists to their own folders for you.

To use, simply download the latest zip folder and extract it. Then run the exe file.

IMPORTANT!!!!!!!!!

You MUST have the SongCore mod installed! You can NOT use this workaround without that mod.

Download Mod Assistant and set it up. Then Install the basic mods, which includes the SongCore mod. 

Once you have the mod you need to run Beat Saber at least 1 time so it will create the folders and files needed.

Now you need playlists. If you don't have any, download some from one of the many options. You need playlists in order for this program to make the folders or it has nothing to go off of. I use beat drop but you can use whatever option you like, say.... beatsaver.com perhaps.

Once you have playlists download the songs! This program moves files but does not download missing songs for you.

Check that you have the below folders and files before running this program to fix everything:

Playlists folder
UserData\SongCore folder
UserData\SongCore\folders.xml file
Beat Saber_Data\CustomLevels folder
folders in the Beat Saber_Data\CustomLevels folder (these are the songs)

If you are missing any of the above yu can NOT run this program and need to go back and make sure you get everything before running this program.




THE LOW DOWN

How does this work?
Well it's actually quit simple, though tedious.

SongCore allows you to load folders as playlists or combine multiple folders and treat them all as 1 big song location. With this we can use SongCore to treat folders as playlists and load all our playlists again.

Here's how the program does this:
1. Get the song hashes (ids).
2. Get the playlists 
3. Load the folders.xml file to track any current folder setup
5. For each playlist:
 a. Create a folder with the same name as the playlist in the Playlists folder ( if it doesn't already exist)
 b. For each song in the playlist:
    1. Check if the song already exists in the playlist folder from 5a. If not, create it. If it does skip this song...
    2. Copy song folder files to new location
    3. If nothing went wrong, delete the old song folder. If something went wrong, mark custom levels folder for renamming
 c. Add playlist info to playlists object
6. Save playlist info to the folders.xml file
7. If renamming is true rename the custom levels folder so no duplicates exist.


# Hashes everywhere #

In order to know for vertain what songs go with what playlists we need to match on the ids. We can't simply search the folders by names as the names in the playlist files don't at all match the song folder names. Some aren't even close. Because we can't go simply by name we NEED to get all the songs ids. This is easy enough but takes some time. 

So how do we do it? Easy. Every song folder is named a very specfic way. The songs id is the very first set of charectors followed by a space. That id is NOT what's in the playlist though. It's the id on beatsaver.com. With this id we can make an api call to beatsaver and grab the hash id for the song. THIS is what sets every song apart and THIS is how we link the songs to the playlists. 

So for every song folder in the custom levels folder we:
1. Get the id by parsing the id from the folder name
2. Use this id to call the beastsaver.com api and get the songs hash id
3. Save the songs folder name and hash id to a dictionary object for later use

#

