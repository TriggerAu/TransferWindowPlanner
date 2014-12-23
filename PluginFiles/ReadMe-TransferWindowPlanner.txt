TransferWindowPlanner - %VERSIONSTRING%
--------------------------
An in-game implementation of [AluxMun's Launch Window Planner WebApp](http://alexmoon.github.io/ksp/) for [Kerbal Space Program](http://www.kerbalspaceprogram.com/)

By Trigger Au

Forum Thread for latest: http://forum.kerbalspaceprogram.com/threads/93115-Transfer-Window-Planner

INSTALLATION
******************* NOTE  ******************* NOTE ******************* NOTE *******************
IF YOU WANT TO MAINTAIN YOUR SETTINGS DO NOT COPY THE CONFIG.XML FILE OVER
******************* NOTE  ******************* NOTE ******************* NOTE *******************

Installing the plugin involves copying the plugin files into the correct location in the KSP application folder
1. Extract the Zip file you have downloaded to a temporary Location
2. Open the Extracted folder structure and open the TransferWindowPlanner_v%VERSIONSTRING% Folder
3. Inside this you will find a GameData folder which contains all the content you will need
4. Open another window to your KSP application folder - We'll call this <KSP_OS>
5. Copy the Contents of the extracted GameData folder to the <KSP_OS>\GameData Folder
6. Start the Game and enjoy :)

TROUBLESHOOTING
The plugin records troubleshooting data in the "<KSP_OS>\KSP_Data\output_log.txt".
If there are errors in loading the config you can delete the "<KSP_OS>\GameData\TriggerTech\TransferWindowPlanner\settings.cfg" and restart the game

LICENSE
This work is licensed under an MIT license as outlined at the OSI site. Visit the documentation site for more details and Attribution

VERSION HISTORY
Version 1.2.0.0		-	KSP Version: 0.90
- Reworked the storage of input values to remove issues (Issue #25)
- Redid Input Locks to resolve interface lockups (Issue #27)
- Added Calendar options so RSS can show Earth Dates for things
- Added Fly-By checkbox
- Restructured Zip File
- Bunch of other small stuff
- Built and included KSPDateTime Library (Issue #24)

Version 1.1.3.0		-	KSP Version: 0.90
- Recompiled for 0.90 and checked code stuff
- Fixed some issues with KAC Integration (Issue #23)
- Fixed Flyby Transfer adding Insertion Burn value (Issue #22)

Version 1.1.2.0		-	KSP Version: 0.25.0
- Added extra logging and null checks re AppLauncher
- Updated KACWrapper to handle Alarmtime properly and add repeat properties

Version 1.1.1.0		-	KSP Version: 0.25.0
- Redid Start date calcs - defaults to yesterday (Issue #21)
- Redid project references 

Version 1.1.0.0		-	KSP Version: 0.25.0
- Added Ejection Burn Details and fiddle Start Date calcs
- Added KAC Alarm Integration - Requires at least KAC 3.0.0.5 (Issue #16)

Version 1.0.1.0		-	KSP Version: 0.24.2
- Fixed Framerate Issue (Issue #17)
- Updated links and version code
- Added Extra details to Copy

Version 1.0.0.0		-	KSP Version: 0.24.2
- First Public Release
- In-Game Lambert solver to display dV required for a transfer
- Generates a "porkchop" plot to help visualise wthe dV cost at various times
- Copy to Clipboard functionality for details so you can get them out of game
- Minimal version so you can see your selected transfers details in Editor scenes
- Integrates to AppLauncher and Common Toolbar

Version 0.8.0.0		-	KSP Version: 0.24.2
- Alpha Test of the toolset in game for comparing diffn solar systems
