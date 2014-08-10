using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    [WindowInitials(Caption="Transfer Window Planner",
        Visible=true,
        DragEnabled=true,
        TooltipsEnabled=true,
        WindowMoveEventsEnabled=true)]
    class TWPWindow:MonoBehaviourWindowPlus
    {
        List<String> lstPlanets = new List<String>() { "Moho", "Eve", "Kerbin", "Duna", "Dres", "Jool", "Eeloo" };
        List<String> lstXFerTypes = new List<String>() { "Ballistic","Mid Course","Optimal"};
        DropDownList ddlOrigin;
        DropDownList ddlDestination;
        DropDownList ddlXferType;

        int intTest = 100;

        internal override void Awake()
        {
            ddlOrigin = new DropDownList(lstPlanets, this);
            ddlDestination = new DropDownList(lstPlanets, this);
            ddlXferType = new DropDownList(lstXFerTypes, this);

            ddlManager.AddDDL(ddlOrigin);
            ddlManager.AddDDL(ddlDestination);
            ddlManager.AddDDL(ddlXferType);
        }
        
        internal override void OnGUIOnceOnly()
        {
            ddlManager.DropDownGlyphs = new GUIContentWithStyle(Resources.btnDropDown, Styles.styleDropDownGlyph);
            ddlManager.DropDownSeparators = new GUIContentWithStyle("", Styles.styleSeparatorV);
        }

        internal override void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label("Enter Parameters");

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Label("Origin:");
            GUILayout.Label("Initial Orbit (km):");
            GUILayout.Label("Destination:");
            GUILayout.Label("Final Orbit (km):");
            GUILayout.Label("Transfer Type:");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            ddlOrigin.DrawButton();
            DrawTextBox(ref intTest);
            ddlDestination.DrawButton();
            DrawTextBox(ref intTest);
            ddlXferType.DrawButton();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Plot It!"))
                StartWorker();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(10));
            GUILayout.Box(Resources.texSeparatorV,Styles.styleSeparatorV,GUILayout.Height(200));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Plot");
            GUILayout.EndVertical();

            if (Running)
                DrawResourceBar(new Rect(350, 180, 280, 20), workingpercent);
            if (Done)
            {
                GUI.Box(new Rect(340, 50, 306, 305), Resources.texPorkChopAxis);
                GUI.Box(new Rect(346, 50, 300, 300), Resources.texPorkChopExample);
            }

            GUILayout.EndHorizontal();

        }

        internal Boolean Running = false;
        internal Boolean Done = false;
        internal Single workingpercent = 0;


        BackgroundWorker bw;

        private void StartWorker()
        {
            workingpercent = 0;
            Running = true;
            Done = false;
            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            bw.RunWorkerAsync();
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Running = false;
            Done = true;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                System.Threading.Thread.Sleep(30);
                LogFormatted("{0:0.000}", workingpercent);
                workingpercent += (Single)0.01;
            } while (workingpercent<1);
        }


        internal static Boolean DrawResourceBar(Rect rectBar, Single percentage)
        {
            Boolean blnReturn = false;
            Single fltBarRemainRatio = percentage;

            //blnReturn = Drawing.DrawBar(styleBack, out rectBar, Width);
            blnReturn = GUI.Button(rectBar, "", Styles.styleBarBlue_Back);

            if ((rectBar.width * fltBarRemainRatio) > 1)
                DrawBarScaled(rectBar, Styles.styleBarBlue, Styles.styleBarBlue_Thin, fltBarRemainRatio);

            return blnReturn;
        }
        internal static void DrawBarScaled(Rect rectStart, GUIStyle Style, GUIStyle StyleNarrow, float Scale)
        {
            Rect rectTemp = new Rect(rectStart);
            rectTemp.width = (float)Math.Ceiling(rectTemp.width = rectTemp.width * Scale);
            if (rectTemp.width <= 2) Style = StyleNarrow;
            GUI.Label(rectTemp, "", Style);
        }

    }
}
