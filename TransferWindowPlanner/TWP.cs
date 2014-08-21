using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    [KSPAddon(KSPAddon.Startup.EveryScene,false)]
    public class TransferWindowPlanner:MonoBehaviourExtended
    {
        internal static Settings settings;

        internal TWPWindow windowMain;

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


            //do the daily version check if required
            //if (settings.DailyVersionCheck)
                //settings.VersionCheck(false);

        }

        internal override void OnDestroy()
        {
            LogFormatted("Destroying the TransferWindowPlanner (TWP)");

            windowMain.bw.CancelAsync();

            RenderingManager.RemoveFromPostDrawQueue(1, DrawGUI);

        }
        private void InitWindows()
        {
            windowMain = AddComponent<TWPWindow>();
            windowMain.WindowRect = new Rect(100, 200, 750, 400);
            windowMain.mbTWP = this;
            InitDebugWindow();
        }

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
            //Draw the button - for basic stuff
            if (settings.ButtonStyleToDisplay == Settings.ButtonStyleEnum.Basic)
            {

            }
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
