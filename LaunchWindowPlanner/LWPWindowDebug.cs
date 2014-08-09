using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace LaunchWindowPlanner
{
    class LWPWindowDebug : MonoBehaviourWindowPlus
    {
        internal LaunchWindowPlanner mbLWP;
        internal Settings settings;

        public Int32 intTest1 = 0;
        public Int32 intTest2 = 0;
        public Int32 intTest3 = 0;
        public Int32 intTest4 = 0;
        public Int32 intTest5 = 0;

        internal override void DrawWindow(int id)
        {
            try
            {

            DrawTextBox(ref intTest1);
            DrawTextBox(ref intTest2);
            DrawTextBox(ref intTest3);
            DrawTextBox(ref intTest4);
            DrawTextBox(ref intTest5);

            DrawLabel("Hello");

            if (GUILayout.Button("KSP")) SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
            if (GUILayout.Button("UnityDef")) SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.Unity);
            if (GUILayout.Button("Default")) SkinsLibrary.SetCurrent("Default");
            if (GUILayout.Button("Unity")) SkinsLibrary.SetCurrent("Unity");
            if (GUILayout.Button("UnityWKSPButtons")) SkinsLibrary.SetCurrent("UnityWKSPButtons");

            DrawLabel("Padding:{0}", SkinsLibrary.CurrentSkin.window.padding);
            DrawLabel("Margin:{0}", SkinsLibrary.CurrentSkin.window.margin);
            DrawLabel("Border:{0}", SkinsLibrary.CurrentSkin.window.border);

            DrawLabel("Padding:{0}", SkinsLibrary.DefKSPSkin.window.padding);
            DrawLabel("Margin:{0}", SkinsLibrary.DefKSPSkin.window.margin);
            DrawLabel("Border:{0}", SkinsLibrary.DefKSPSkin.window.border);
            }
            catch (Exception)
            {

            }

        }


    }
}
