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
4. For each playlist:
 1. Create a folder with the same name as the playlist in the Playlists folder ( if it doesn't already exist) and save the base 64 image string as an image within the new folder. If folder already exists skip making cover art as it should already exist.
 2. For each song in the playlist:
    1. Check if the song already exists in the playlist folder from 4a. If not, create it. If it does skip this song...
    2. Copy song folder files to new location
    3. If nothing went wrong, delete the old song folder. If something went wrong, mark custom levels folder for renamming
 3. Add playlist info to playlists object
5. Save playlist info to the folders.xml file
6. If renamming is true rename the custom levels folder so no duplicates exist.


# Hashes everywhere #

In order to know for certain what songs go with what playlists we need to match on the ids. We can't simply search the folders by names as the names in the playlist files don't at all match the song folder names. Some aren't even close. Because we can't go simply by name we NEED to get all the songs ids. This is easy enough but takes some time. 

So how do we do it? Easy. Every song folder is named a very specfic way. The songs id is the very first set of charectors followed by a space. That id is NOT what's in the playlist though. It's the id on beatsaver.com. With this id we can make an api call to beatsaver and grab the hash id for the song. THIS is what sets every song apart and THIS is how we link the songs to the playlists. 

So for every song folder in the custom levels folder we:
1. Get the id by parsing the id from the folder name
2. Use this id to call the beastsaver.com api and get the songs hash id
3. Save the songs folder name and hash id to a dictionary object for later use

# Show me the playlists! #

Each playlist file is simply a list containg some info. It has the cover art for the playlist in base 64 string form, the playlist name, and each song in the playlist. Each song has 2 things. An id (hash) and song title. If the song hash was the same id as the folders name or the song names matched the folders we could link them here but neither of those are case. The names don't match the folders and the hash is not the same as the ids in the folders names.

But! The hash id IS the same id beatsaver.com gives us back. Because of this the only way we have to be absolutly certain the song in the playlist matches a song we downloaded is to use the hash ids in the playlists to match the hash ids we know about from above. This id links the playlists song to a song folder on our computer.

We also use the base 64 string here as the cover art for the playlist. We can do this by converting the string into an image file and saving it. The playlist name is used as the playlist folder name and the playlist name in beat saber.

To get the playlists we always look in the Playlists folder. We simply grab all files (not folders) in the folder and loop thorugh them 1 by 1.


# Playlists are stored where? #

Song Core uses a folders.xml file located at UserData\SongCore\folders.xml. This file ususally just says a couple examples and is never used. BUT it does have a function that allows us to specify playlists as folders. JACKPOT! This is how we get around the playlist issue. We force tell SongCore to treat folders as playlists.

In the folders.xml there is a list of folders. Each folder has some things that tells SongCore all it needs to know. It has the name that will be displayed in beat saber, the image location to be used as the background art in beat saber, the folder location that contains all the songs, the wip, and most importantly,the folder type id. With this we can create playlists.

The folder type id is very important here. This is what tells beat saber to create a new playlist and not shove the songs under the default "Custom songs" or "WIP" playists. By specifuing  2 as the type we are telling SongCore that this folder is a playlist and should be treated as such. 

The WIP isn't explained well in the file examples but this simply says if the folder is full of practice mode songs. If this is true any songs in the folder will only be playable in practice mode, not regular mode. We don't want that! So leave it false.

Every playlist NEEDS cover art. Because of this we need to specify the path to an image file that will be used as the playlist cover. The file must be 256x256 pixals. We get this file though from the playlist file info (the base 64 string).

NOTE - SongCore will skip loading duplicate songs! Because of this we can't leave any songs in the default custom levels folder that we put in a playlist. So if we have song A and we want it in a playlist, we MUST delete the song from the custom levels folder AFTER copying it to the playlist folder. If we don't SongCore will SKIP loading the song in the playlist. If all the playlist songs exist in the custom levels folder (we don't remove any after copyng) the playlist will NOT show in beat saber. This is why we delete the song folders after we copy them.


# You renamed the folder. But why?! #

If we go to delete a folder and it is corrupt (something beat drop likes to do) then it will crash SongCore. We don't want that. To get around the issue we MUST rename the folder so that it can be recreated on the next launch of beat saber. This does mean that you lose any songs not already in a playlist but it's as easy as copying the non corrupt folders into the newly created custom levels folder. The program will not do this part for you but this should be a rare use case.

You will NOT lose any songs as all remaining songs will still be in the renamed folder. You just have to move them to the custom levels folder and skip any corrupt folders.


# MORE NFO!! #

If you want more info just create an issue and specify what you'd like to know. I'll update this with any info you need. :) 
