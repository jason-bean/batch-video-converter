Batch video converter for Windows to convert videos for iPhone or iPod profiles for the htpc/pvr crowd using HandBrakeCLI and mp4box. Can automatically cut out commercials if an EDL file is present and/or auto crop the source video.

v1.0

Initial version
equires HandBrakeCLI for minimum functionality and mp4box for full functionality.
v1.5

Adds thetvdb.com for some metadata and poster retrieval and themoviedb.org for poster retrieval.
Added requirement of AtomicParsley? for adding metadata to resulting M4V file.
SageTV v7 recordings required for best results, otherwise filename must be in "Series Name - SxxExx? - Episode Name" format
v1.5.1

Fixes metadata retrieval from SageTV recordings. I had to make sure erroneous "characters" were removed. Probably a result of reading a binary file byte for byte while converting each byte to an ASCII character. This resulted in some strange and obsolete control characters being inserted and preventing things from working correctly.
v1.5.5

Includes many bug fixes for metadata retrieval.
Moves metadata retrieval before encoding so the user is provided with a list of possible titles if no exact match is found.
v1.5.6

Adds ability to import converted videos directly into iTunes.
Optionally allows the deletion of converted video once iTunes import complete.
v1.5.7

Moved the selection of the encoding profile to the Settings dialog.
Added encoding profiles for the iPhone 4 (960x640), iPad (1024x768), iPad 2/AppleTV 2 (1280x720). The iPad profile can also be used for the iPad 2 if you desire to have a 1:1 pixel match to the LCD. I specified the 1280x720 profile as iPad 2 since the device is capable of 720p playback.
v1.6

Updated EDL code so that Comskip files are now supported. Thank you to Alex for the code.
Changed the way settings are saved so that they don't get lost between version.
Lot's of bug fixes.
To compile this project requires the Windows API Code Pack available here:

http://archive.msdn.microsoft.com/WindowsAPICodePack