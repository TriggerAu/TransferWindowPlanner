using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

using TWPToolbarWrapper;
using TWP_KACWrapper;

namespace TransferWindowPlanner
{

    #region Starter Classes
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TransferWindowPlannerFlight : TransferWindowPlanner { }
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class TransferWindowPlannerEditor : TransferWindowPlanner { }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class TransferWindowPlannerSpaceCenter : TransferWindowPlanner { }
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TransferWindowPlannerTrackingStation : TransferWindowPlanner { }
    #endregion    

    public partial class TransferWindowPlanner:MonoBehaviourExtended
    {
        internal static Settings settings;
        internal IButton btnToolbar = null;

        internal TWPWindow windowMain;
        internal TWPWindowSettings windowSettings;
        internal TWPWindowSettingsBlockout windowSettingsBlockout;
        // need two for unity skin
        internal TWPWindowSettingsBlockout windowSettingsBlockoutExtra;

        internal AngleRenderPhase PhaseAngle;
        internal AngleRenderEject EjectAngle;

        internal static List<GameScenes> lstScenesForAngles = new List<GameScenes>() { GameScenes.TRACKSTATION, GameScenes.FLIGHT };

        internal override void OnAwake()
        {
            LogFormatted("Awakening the TransferWindowPlanner (TWP)");

            LogFormatted("Checking for PluginData folder");
            if (!System.IO.Directory.Exists(Resources.PathPlugin + "/PluginData"))
                System.IO.Directory.CreateDirectory(Resources.PathPlugin + "/PluginData");

            LogFormatted("Loading Settings");
            settings = new Settings("PluginData/settings.cfg");
			Boolean blnSettingsLoaded = settings.Load();
			if (!blnSettingsLoaded)
			{
				settings = new Settings("settings.cfg");
				blnSettingsLoaded = settings.Load();
				if (blnSettingsLoaded)
				{
					settings.FilePath = "PluginData/settings.cfg";
					System.IO.File.Move(Resources.PathPlugin + "/settings.cfg", Resources.PathPlugin + "/PluginData/settings.cfg");
				}
			}

			if (!blnSettingsLoaded)
			{
				settings.FilePath = "PluginData/settings.cfg";
				LogFormatted("Settings Load Failed");
			}

            InitWindows();

            if(settings.SelectedCalendar==CalendarTypeEnum.Earth) {
                KSPDateStructure.SetEarthCalendar(settings.EarthEpoch);
                windowSettings.ddlSettingsCalendar.SelectedIndex = (Int32)settings.SelectedCalendar;
            }

            KSPDateStructure.UseStockDateFormatters = settings.UseStockDateFormatters;

            //plug us in to the draw queue and start the worker
            //Rem out for unity5
            //RenderingManager.AddToPostDrawQueue(1, DrawGUI);

            //Get whether the toolbar is there
            settings.BlizzyToolbarIsAvailable = ToolbarManager.ToolbarAvailable;

            //setup the Toolbar button if necessary
            if (settings.ButtonStyleToDisplay == Settings.ButtonStyleEnum.Toolbar)
            {
                btnToolbar = InitToolbarButton();
            }

            //Hook the App Launcher
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(DestroyAppLauncherButton);

            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequestedForAppLauncher);

            // Need this one to handle the hideUI cancelling via pause menu
            GameEvents.onGameUnpause.Add(OnUnpause);

            //Hook the Angle renderers
            if (lstScenesForAngles.Contains(HighLogic.LoadedScene))
            {
                PhaseAngle = MapView.MapCamera.gameObject.AddComponent<AngleRenderPhase>();
                EjectAngle = MapView.MapCamera.gameObject.AddComponent<AngleRenderEject>();
            }

            //do the daily version check if required
            if (settings.DailyVersionCheck)
                settings.VersionCheck(false);
        }

        internal override void OnDestroy()
        {
            LogFormatted("Destroying the TransferWindowPlanner (TWP)");

            if (windowMain.bw!=null && windowMain.bw.IsBusy)
                windowMain.bw.CancelAsync();

            //Rem out for unity5
            //RenderingManager.RemoveFromPostDrawQueue(1, DrawGUI);

            if (PhaseAngle != null)
            {
                Destroy(PhaseAngle.gameObject);
            }
            if (EjectAngle != null)
            {
                Destroy(EjectAngle.gameObject);
            }

            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(DestroyAppLauncherButton);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequestedForAppLauncher);

            // Need this one to handle the hideUI cancelling via pause menu
            GameEvents.onGameUnpause.Remove(OnUnpause);

            DestroyAppLauncherButton();

            DestroyToolbarButton(btnToolbar);

            DestroyAPIHooks();
        }


        private void OnUnpause()
        {
            LogFormatted_DebugOnly("OnUnpause");
            windowMain.blnFlightUIVisible = true;
            windowSettings.blnFlightUIVisible = true;
            windowSettingsBlockout.blnFlightUIVisible = true;
            windowSettingsBlockoutExtra.blnFlightUIVisible = true;
#if DEBUG
            windowDebug.blnFlightUIVisible = true;
#endif
        }

        internal override void Start()
        {
            LogFormatted("Searching for RSS");
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if(t.FullName == ("RealSolarSystem.RSSWatchDog")) {
                    settings.RSSActive = true;
                    if(!settings.RSSShowCalendarToggled) {
                        settings.ShowCalendarToggle = true;
                        settings.RSSShowCalendarToggled = true;
                    }
                }
            });

            //Init the KAC Integration
            KACWrapper.InitKACWrapper();
            if (KACWrapper.APIReady)
            {
                LogFormatted("Successfully Hooked the KAC");

                KACWrapper.KAC.onAlarmStateChanged += KAC_onAlarmStateChanged;
            }

            //Ensure no lagging locks
            RemoveInputLock();
        }

        private void DestroyAPIHooks()
        {
            //clean up the integration events
            if (KACWrapper.APIReady)
            {
                KACWrapper.KAC.onAlarmStateChanged -= KAC_onAlarmStateChanged;
            }
        }

        void KAC_onAlarmStateChanged(KACWrapper.KACAPI.AlarmStateChangedEventArgs e)
        {
            //LogFormatted("AlarmStateChanged:{0}-{1}", e.alarm.Name, e.eventType);
        }

        private void InitWindows()
        {
            windowMain = AddComponent<TWPWindow>();
            windowMain.WindowRect = new Rect(100, 200, 750, 400);
            windowMain.mbTWP = this;
            windowMain.settings = settings;

            windowSettings = AddComponent<TWPWindowSettings>();
            windowSettings.mbTWP = this;

            windowSettingsBlockout = AddComponent<TWPWindowSettingsBlockout>();
            windowSettingsBlockout.mbTWP = this;
            windowSettingsBlockoutExtra = AddComponent<TWPWindowSettingsBlockout>();
            windowSettingsBlockoutExtra.mbTWP = this;

            InitDebugWindow();
        }

#region Toolbar Stuff
        /// <summary>
        /// initialises a Toolbar Button for this mod
        /// </summary>
        /// <returns>The ToolbarButtonWrapper that was created</returns>
        internal IButton InitToolbarButton()
        {
            IButton btnReturn;
            try
            {
                LogFormatted("Initialising the Toolbar Icon");
                btnReturn = ToolbarManager.Instance.add(_ClassName, "btnToolbarIcon");
                SetToolbarIcon(btnReturn);
                btnReturn.ToolTip = "Transfer Window Planner";
                btnReturn.OnClick += (e) =>
                {
                    windowMain.Visible = !windowMain.Visible;
                    SetToolbarIcon(e.Button);
                    MouseOverToolbarBtn = true;
                };
                btnReturn.OnMouseEnter += btnReturn_OnMouseEnter;
                btnReturn.OnMouseLeave += btnReturn_OnMouseLeave;
            }
            catch (Exception ex)
            {
                btnReturn = null;
                LogFormatted("Error Initialising Toolbar Button: {0}", ex.Message);
            }
            return btnReturn;
        }

        private static void SetToolbarIcon(IButton btnReturn)
        {
            //if (settings.ToggleOn) 
            //btnReturn.TexturePath = "TriggerTech/KSPAlternateResourcePanel/ToolbarIcons/KSPARPa_On";
            //else
            String strToolbarIcon = Resources.PathPluginToolbarIcons.Substring(Resources.PathPluginToolbarIcons.ToLower().IndexOf("/gamedata/")+10) + "/TWPIcon";
            btnReturn.TexturePath = strToolbarIcon;// "TriggerTech/TransferWindowPlanner/ToolbarIcons/TWPIcon";
        }

        void btnReturn_OnMouseLeave(MouseLeaveEvent e)
        {
            MouseOverToolbarBtn = false;
        }

        internal Boolean MouseOverToolbarBtn = false;
        void btnReturn_OnMouseEnter(MouseEnterEvent e)
        {
            MouseOverToolbarBtn = true;
        }

        /// <summary>
        /// Destroys theToolbarButtonWrapper object
        /// </summary>
        /// <param name="btnToDestroy">Object to Destroy</param>
        internal void DestroyToolbarButton(IButton btnToDestroy)
        {
            if (btnToDestroy != null)
            {
                LogFormatted("Destroying Toolbar Button");
                btnToDestroy.Destroy();
            }
            btnToDestroy = null;
        }
#endregion

#if DEBUG
        internal TWPWindowDebug windowDebug;
#endif
        [System.Diagnostics.Conditional("DEBUG")]
        private void InitDebugWindow()
        {
#if DEBUG
            windowDebug = AddComponent<TWPWindowDebug>();
            windowDebug.mbTWP = this;
            windowDebug.Visible = true;
            windowDebug.WindowRect = new Rect(Screen.width-300, 50, 300, 200);
            windowDebug.DragEnabled = true;
            windowDebug.ClampToScreen = false;
            windowDebug.settings = settings;
#endif
        }





        internal override void OnGUIOnceOnly()
        {
            //Get the textures we need into Textures
            Resources.LoadTextures();

            //Set up the Styles
            Styles.InitStyles();

            //Set up the Skins
            Styles.InitSkins();

            //Set the current Skin
            SkinsLibrary.SetCurrent(settings.SelectedSkin.ToString());

        }

        //internal override void OnGUIEvery()
        //{
        //    DrawGUI();
        //}

        internal Boolean MouseOverAnyWindow = false;
        internal Boolean InputLockExists = false;
        void DrawGUI()
        {
            ////Draw the button - for basic stuff - Not using a button
            //if (settings.ButtonStyleToDisplay == Settings.ButtonStyleEnum.Basic)
            //{

            //}

            //Do this for control Locks
            if (settings.ClickThroughProtect_KSC || settings.ClickThroughProtect_Editor || settings.ClickThroughProtect_Flight) {
                MouseOverAnyWindow = false;
                MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(windowMain.WindowRect, windowMain.Visible);
                MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(windowSettings.WindowRect, windowSettings.Visible);
#if DEBUG
                MouseOverAnyWindow = MouseOverAnyWindow || MouseOverWindow(windowDebug.WindowRect, windowDebug.Visible);
#endif
                //MonoBehaviourWindow[] TWPwindows = FindObjectsOfType<MonoBehaviourWindow>();
                //foreach (MonoBehaviourWindow item in TWPwindows) {
                //    if (MouseOverWindow(item.WindowRect, item.Visible)) {
                //        MouseOverAnyWindow = true;
                //        break;
                //    }
                //}

                //If the setting is on and the mouse is over any window then lock it
                if (MouseOverAnyWindow && !InputLockExists) {
                    Boolean AddLock = false;
                    switch (HighLogic.LoadedScene) {
                        case GameScenes.SPACECENTER: AddLock = settings.ClickThroughProtect_KSC && !(InputLockManager.GetControlLock("TWPControlLock") != ControlTypes.None); break;
                        case GameScenes.EDITOR: AddLock = settings.ClickThroughProtect_Editor && !(InputLockManager.GetControlLock("TWPControlLock") != ControlTypes.None); break;
                        case GameScenes.FLIGHT: AddLock = settings.ClickThroughProtect_Flight && !(InputLockManager.GetControlLock("TWPControlLock") != ControlTypes.None); break;
                        case GameScenes.TRACKSTATION:
                            break;
                        default:
                            break;
                    }
                    if (AddLock) {
                        //LogFormatted_DebugOnly("AddingLock-{0}", "TWPControlLock");

                        switch (HighLogic.LoadedScene) {
                            case GameScenes.SPACECENTER: InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, "TWPControlLock"); break;
                            case GameScenes.EDITOR: InputLockManager.SetControlLock((ControlTypes.EDITOR_LOCK | ControlTypes.EDITOR_GIZMO_TOOLS), "TWPControlLock"); break;
                            case GameScenes.FLIGHT: InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "TWPControlLock"); break;
                            case GameScenes.TRACKSTATION:
                                break;
                            default:
                                break;
                        }
                    InputLockExists = true;
                    }
                }
                //Otherwise make sure the lock is removed
                else if (!MouseOverAnyWindow && InputLockExists) {
                    RemoveInputLock();
                }
            }
        }

        internal void RemoveInputLock()
        {
            if (InputLockManager.GetControlLock("TWPControlLock") != ControlTypes.None)
            {
                //LogFormatted_DebugOnly("Removing-{0}", "TWPControlLock");
                InputLockManager.RemoveControlLock("TWPControlLock");
            }
            InputLockExists = false;
        }

        private Boolean MouseOverWindow(Rect WindowRect, Boolean WindowVisible)
        {
            return WindowVisible && WindowRect.Contains(Event.current.mousePosition);
        }

     }



//#if DEBUG
//    //This will kick us into the save called default and set the first vessel active
//    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
//    public class Debug_AutoLoadPersistentSaveOnStartup : MonoBehaviour
//    {
//        //use this variable for first run to avoid the issue with when this is true and multiple addons use it
//        public static bool first = true;
//        public void Start()
//        {
//            //only do it on the first entry to the menu
//            if (first)
//            {
//                first = false;
//                HighLogic.SaveFolder = "default";
//                Game game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);

//                if (game != null && game.flightState != null && game.compatible)
//                {
//                    HighLogic.CurrentGame = game;
//                    HighLogic.LoadScene(GameScenes.SPACECENTER);
//                    return;
//                    Int32 FirstVessel;
//                    Boolean blnFoundVessel = false;
//                    for (FirstVessel = 0; FirstVessel < game.flightState.protoVessels.Count; FirstVessel++)
//                    {
//                        if (game.flightState.protoVessels[FirstVessel].vesselType != VesselType.SpaceObject &&
//                            game.flightState.protoVessels[FirstVessel].vesselType != VesselType.Unknown)
//                        {
//                            blnFoundVessel = true;
//                            break;
//                        }
//                    }
//                    if (!blnFoundVessel)
//                        FirstVessel = 0;
//                    FlightDriver.StartAndFocusVessel(game, FirstVessel);
//                }

//                //CheatOptions.InfiniteFuel = true;
//            }
//        }
//    }
//#endif
}
