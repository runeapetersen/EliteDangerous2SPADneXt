# EliteDangerous2SPADneXt
An add-on script for SPAD.neXt which will import Elite Dangerous game state data into SPAD local variables.

# Who is this for?
Intended to allow SPAD.neXt owners to create interesting UI automations for physical or virtual devices and let these trigger off Elite Dangerous state changes. It was created from a personal desire to use SPAD.neXt for creating cool Stream Deck setups which can reflect game state by changing colour, updating text and images, etc.

# What does it do?
Elite Dangerous exports certain game state information to the file "%userprofile%\Saved Games\Frontier Developments\Elite Dangerous\Status.json". This file is continuously being updated while the game runs.

The script will attempt to locate the Status.json file continuously being written by Elite Dangerous. Every time the file changes any updated values are written to SPAD.neXt Data variables in the LOCAL Session scope. 

It is possible to point to an alternative Status.json file location by editing the <filename tbd>.json file. No other configurations can be defined in the file.

Note: This add-on will write to the SPAD.neXt application log (normally residing in "%appdata%\SPAD.neXt\logs\". If for some reason the add-on is not working as expected check this log for error messages. <elaboration tbd>

Note that data only flows from Elite Dangerous into SPAD.neXt. This plugin does not allow for instrumenting Elite Dangerous from SPAD.neXt actions directly. To do this efficiently, you might want to use VJoy or some sort of middleware layer which can represent a device understood by both applications.

# Usage
Files are copied to "%userprofile%\Documents\SPAD.neXt\Addons\" (create the Addons folder if missing). SPAD.neXt will load the file at runtime and initialize the script. No further steps need to be taken by the user. It should just work.
