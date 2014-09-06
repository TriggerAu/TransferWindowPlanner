using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

using TWPToolbarWrapper;

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

        internal override void Awake()
        {
            LogFormatted("Awakening the TransferWindowPlanner (TWP)");

            LogFormatted("Loading Settings");
            settings = new Settings("settings.cfg");
            if (!settings.Load())
                LogFormatted("Settings Load Failed");

            InitWindows();

            //plug us in to the draw queue and start the worker
            RenderingManager.AddToPostDrawQueue(1, DrawGUI);


            //Get whether the toolbar is there
            settings.BlizzyToolbarIsAvailable = ToolbarManager.ToolbarAvailable;

            //setup the Toolbar button if necessary
            if (settings.ButtonStyleToDisplay == Settings.ButtonStyleEnum.Toolbar)
            {
                btnToolbar = InitToolbarButton();
            }

            //Hook the App Launcher
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequestedForAppLauncher);

            //do the daily version check if required
            //if (settings.DailyVersionCheck)
                //settings.VersionCheck(false);

            //APIAwake();
        }

        internal override void OnDestroy()
        {
            LogFormatted("Destroying the TransferWindowPlanner (TWP)");

            if (windowMain.bw!=null && windowMain.bw.IsBusy)
                windowMain.bw.CancelAsync();

            RenderingManager.RemoveFromPostDrawQueue(1, DrawGUI);

            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            DestroyAppLauncherButton();

            DestroyToolbarButton(btnToolbar);

            //APIDestroy();
        }
        private void InitWindows()
        {
            windowMain = AddComponent<TWPWindow>();
            windowMain.WindowRect = new Rect(100, 200, 750, 400);
            windowMain.mbTWP = this;

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
            btnReturn.TexturePath = "TriggerTech/TransferWindowPlanner/ToolbarIcons/TWPIcon";
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

        void DrawGUI()
        {
            ////Draw the button - for basic stuff - Not using a button
            //if (settings.ButtonStyleToDisplay == Settings.ButtonStyleEnum.Basic)
            //{

            //}
        }

    }



#if DEBUG
    //This will kick us into the save called default and set the first vessel active
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Debug_AutoLoadPersistentSaveOnStartup : MonoBehaviour
    {
        //use this variable for first run to avoid the issue with when this is true and multiple addons use it
        public static bool first = true;
        public void Start()
        {
            //only do it on the first entry to the menu
            if (first)
            {
                first = false;
                HighLogic.SaveFolder = "default";
                Game game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);

                if (game != null && game.flightState != null && game.compatible)
                {
                    HighLogic.CurrentGame = game;
                    HighLogic.LoadScene(GameScenes.SPACECENTER);

                    //Int32 FirstVessel;
                    //Boolean blnFoundVessel = false;
                    //for (FirstVessel = 0; FirstVessel < game.flightState.protoVessels.Count; FirstVessel++)
                    //{
                    //    if (game.flightState.protoVessels[FirstVessel].vesselType != VesselType.SpaceObject &&
                    //        game.flightState.protoVessels[FirstVessel].vesselType != VesselType.Unknown)
                    //    {
                    //        blnFoundVessel = true;
                    //        break;
                    //    }
                    //}
                    //if (!blnFoundVessel)
                    //    FirstVessel = 0;
                    //FlightDriver.StartAndFocusVessel(game, FirstVessel);
                }

                //CheatOptions.InfiniteFuel = true;
            }
        }
    }
#endif
}
