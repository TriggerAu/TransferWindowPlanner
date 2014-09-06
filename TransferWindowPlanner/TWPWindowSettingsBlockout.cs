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
            WindowRect = new Rect(mbTWP.windowSettings.WindowRect.x, mbTWP.windowSettings.WindowRect.y, 310, 200);
            GUILayout.Box("", new GUIStyle(), GUILayout.Width(100), GUILayout.Height(100));
        }
        internal override void Awake()
        {
            WindowRect = new Rect(0, 0, 310, 200);
        }
    }
}
