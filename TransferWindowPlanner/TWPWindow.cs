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
    internal partial class TWPWindow:MonoBehaviourWindowPlus
    {
        DropDownList ddlOrigin;
        DropDownList ddlDestination;
        //DropDownList ddlXferType;

        CelestialBody cbStar = null;
        List<cbItem> lstBodies = new List<cbItem>();

        internal override void Awake()
        {
            foreach (CelestialBody item in FlightGlobals.Bodies)
            {
                //if(item.name!="Sun")
                //    LogFormatted("{0}-{1}", item.bodyName, item.orbit.semiMajorAxis);
                lstBodies.Add(new cbItem(item));
            }
            //The star is the body that has no reference
            cbStar = FlightGlobals.Bodies.FirstOrDefault(x => x.referenceBody == x.referenceBody);
            if (cbStar == null)
            {
                //RuRo
                LogFormatted("Error: Couldn't detect a Star (ref body is itself)");
            }
            else
            {
                BodyParseChildren(cbStar);
            }

            ddlOrigin = new DropDownList(lstPlanets.Select(x => x.NameFormatted), 2, this);
            ddlDestination = new DropDownList(lstPlanets.Select(x => x.NameFormatted), 0, this);
            SetupDestinationControls();
            SetupTransferParams();

            ddlOrigin.OnSelectionChanged += ddlOrigin_OnSelectionChanged;
            ddlDestination.OnSelectionChanged += ddlDestination_OnSelectionChanged;

            ddlManager.AddDDL(ddlOrigin);
            ddlManager.AddDDL(ddlDestination);

            //ddlXferType = new DropDownList(lstXFerTypes, this);
            //ddlManager.AddDDL(ddlXferType);



            //Set the defaults
        }

        void ddlOrigin_OnSelectionChanged(MonoBehaviourWindowPlus.DropDownList sender, int OldIndex, int NewIndex)
        {
            LogFormatted_DebugOnly("New Origin Selected:{0}",ddlOrigin.SelectedValue.Trim(' '));

            SetupDestinationControls();
        }
        void ddlDestination_OnSelectionChanged(MonoBehaviourWindowPlus.DropDownList sender, int OldIndex, int NewIndex)
        {
            LogFormatted_DebugOnly("New Destination Selected:{0}", ddlDestination.SelectedValue.Trim(' '));
            SetupTransferParams();
        }

        internal override void OnGUIOnceOnly()
        {
            ddlManager.DropDownGlyphs = new GUIContentWithStyle(Resources.btnDropDown, Styles.styleDropDownGlyph);
            ddlManager.DropDownSeparators = new GUIContentWithStyle("", Styles.styleSeparatorV);
        }

        Int32 intTest1 = 100, intTest2 = 100;

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

        internal static Vector3d getAbsolutePositionAtUT(Orbit orbit, double UT)
        {
            Vector3d pos = orbit.getRelativePositionAtUT(UT);
            pos += orbit.referenceBody.position;
            return pos;
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
