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
Version 1.7.2.0		-	KSP Version: 1.7.3
- Fix issue with camera being destroyed when leaving TS/map mode

Version 1.7.1.0		-	KSP Version: 1.7.3
- Recompile for 1.7.x
- Fixed display issues when transfer was impossible - result was NaN (Issue #53)
- Fixed up some Date Formatting (Thanks to Aelfhe1m - Issue #51)
- Fixed up some altitude swapping (Thanks to nanathan - Issue #50)
- Fixed up some issues with the anglerendering (Thanks to codesquid - Issue #49)
- Fixed up some issues with drawing angles when not in the right scene
- Fixed issues with clamptoscreen for window
- Updated KACWrapper
- Added some ClickThrough locks for TS scene
- Fixed updated version check

Version 1.6.3.0		-	KSP Version: 1.4.1
- Recompile for 1.4.1

Version 1.6.2.0		-	KSP Version: 1.3.0
- Recompile for 1.3.0
- Added guard clause to always create the PluginData folder if its missing

Version 1.6.1.0		-	KSP Version: 1.2.2
- Moved settings to PluginData to help out for MM peeps
- Tidied up some minor obselesence

Version 1.6.0.0		-	KSP Version: 1.2.0
- Recompile for 1.2

Version 1.5.1.0		-	KSP Version: 1.1.2
- Adjusted code to handle UIToggle and flight Pause menu

Version 1.5.0.0		-	KSP Version: 1.1.2
- Compiled against 1.1.2
- tweaked angle stuff
- integrated nanathan pull for ongui code

Version 1.4.0.0		-	KSP Version: 1.0.5
- adjusted angle names, etc to make em consistent - to retrograde, to prograde, etc
- fixed issues with log spam and map view (Issue #35)
- fixed issue with KACAlarms and margins (Issue #38)
- Added new Angle Renderer to display phase and ejection angles

Version 1.3.1.0		-	KSP Version: 1.0.4
- Fix added for long drop down lists when theres lots of planets (Issue #33)

Version 1.3.0.1		-	KSP Version: 1.0.2
- changed version file to handle patches for CKAN

Version 1.3.0.0		-	KSP Version: 1.0
- Recompiled for 1.0
- Code changes for launcher
- Updated KAC Wrapper
- Adjusted Editor Lock type

Version 1.2.3.0		-	KSP Version: 0.90
- Fixed missing icon (Issue #29)
- Changed input lock in flight mode (Issue #30)

Version 1.2.2.0		-	KSP Version: 0.90
- All backend stuff...
- Added AVC version files (Issue #25)
- Updated Toolbar wrapper
- Updated DateTime Library

Version 1.2.1.0		-	KSP Version: 0.90
- Reworked the storage of input values to remove issues (Issue #25)
- Redid Input Locks to resolve interface lockups (Issue #27)
- Added Calendar options so RSS can show Earth Dates for things
- Added Fly-By checkbox
- Restructured Zip File
- Made ToolbarIcon relative (Issue #28)
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
