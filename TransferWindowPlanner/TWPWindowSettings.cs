using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    [WindowInitials(TooltipsEnabled=true,Visible=false,DragEnabled=false)]
    class TWPWindowSettings:MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;

        internal override void Awake()
        {
            WindowRect = new Rect(mbTWP.windowMain.WindowRect.x + mbTWP.windowMain.WindowRect.width, mbTWP.windowMain.WindowRect.y, 300, 200);
        }

        internal override void OnGUIEvery()
        {
            WindowRect.x = mbTWP.windowMain.WindowRect.x + mbTWP.windowMain.WindowRect.width;
            WindowRect.y = mbTWP.windowMain.WindowRect.y;
            base.OnGUIEvery();
        }

        internal override void DrawWindow(int id)
        {
            







        }

    }
}
