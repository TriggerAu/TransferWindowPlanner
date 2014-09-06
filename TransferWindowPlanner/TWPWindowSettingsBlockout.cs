using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    /// <summary>
    /// This is just for a window to increase the opacoity of the settings window
    /// </summary>
    class TWPWindowSettingsBlockout:MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;

        internal override void DrawWindow(int id)
        {
            WindowRect = new Rect(mbTWP.windowSettings.WindowRect.x, mbTWP.windowSettings.WindowRect.y, mbTWP.windowSettings.WindowWidth, mbTWP.windowSettings.WindowHeight);
            GUILayout.Box("", new GUIStyle(), GUILayout.Width(100), GUILayout.Height(100));
        }
        internal override void Start()
        {
            WindowRect = new Rect(0, 0, mbTWP.windowSettings.WindowWidth, mbTWP.windowSettings.WindowHeight);
        }
    }
}
