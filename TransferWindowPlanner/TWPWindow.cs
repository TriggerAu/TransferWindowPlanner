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
        Visible=false,
        DragEnabled=true,
        TooltipsEnabled=true,
        WindowMoveEventsEnabled=true)]
    internal partial class TWPWindow:MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;

        DropDownList ddlOrigin;
        DropDownList ddlDestination;
        //DropDownList ddlXferType;

        CelestialBody cbStar = null;
        List<cbItem> lstBodies = new List<cbItem>();

        internal Vector2 PlotPosition = new Vector2(372, 50);

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

            WindowVisibleChanged += TWPWindow_WindowVisibleChanged;

            //Set the defaults
        }

        void TWPWindow_WindowVisibleChanged(MonoBehaviourWindow sender, bool NewVisibleState)
        {
            //if its toggling on make sure the window is in scene
            if (NewVisibleState)
            {
                this.ClampToScreenNow();
            }
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

        //internal Boolean DrawYearDay(ref Double UT)
        //{
        //    Double oldUT = UT;
        //    KSPTime kTime = new KSPTime(UT);

        //    String strYear = kTime.Year.ToString("{0:0}");
        //    String strDay = kTime.Day.ToString("{0:0}");
            
        //    GUILayout.BeginHorizontal();
        //    DrawTimeField(ref strYear, "Year:", 100, 100);
        //    DrawTimeField(ref strDay, "Day:", 100, 100);
        //    GUILayout.EndHorizontal();
        //    kTime = kTime.UpdateFromStrings(strYear, strDay);

        //    UT = kTime.UT;
        //    return UT != oldUT;
        //}

        //internal Boolean DrawTimeField(ref String Value, String LabelText, Int32 FieldWidth, Int32 LabelWidth)
        //{
        //    Boolean blnReturn = false;
        //    Int32 intParse;
        //    GUIStyle styleTextBox = Styles.styleAddField;
        //    GUIContent contText = new GUIContent(Value);
        //    Boolean BlnIsNum = Int32.TryParse(Value, out intParse);
        //    if (!BlnIsNum) styleTextBox = Styles.styleAddFieldError;

        //    //styleTextBox.alignment = TextAnchor.MiddleRight;
        //    GUILayout.Label(LabelText, Styles.styleTextTitle, GUILayout.Width(LabelWidth));
        //    blnReturn = DrawTextBox(ref Value, styleTextBox, GUILayout.MaxWidth(FieldWidth));

        //    return blnReturn;
        //}

        internal Boolean DrawTextField(ref String Value, String RegexValidator, Boolean RegexFailOnMatch, String LabelText="", Int32 FieldWidth=0, Int32 LabelWidth=0)
        {
            GUIStyle styleTextBox = Styles.styleTextField;
            if ((RegexFailOnMatch && System.Text.RegularExpressions.Regex.IsMatch(Value, RegexValidator, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) ||
                (!RegexFailOnMatch && !System.Text.RegularExpressions.Regex.IsMatch(Value, RegexValidator, System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
                styleTextBox = Styles.styleTextFieldError;

            if(LabelText!="") {
                if (LabelWidth==0)
                    GUILayout.Label(LabelText, Styles.styleTextFieldLabel);
                else
                    GUILayout.Label(LabelText, Styles.styleTextFieldLabel, GUILayout.Width(LabelWidth));
            }

            Boolean blnReturn = false;
            if (FieldWidth == 0)
                blnReturn = DrawTextBox(ref Value, styleTextBox);
            else
                blnReturn = DrawTextBox(ref Value, styleTextBox, GUILayout.Width(FieldWidth));
            return blnReturn;
        }

        internal Boolean DrawYearDay(ref String strYear,ref String strDay)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            blnReturn = blnReturn || DrawTextField(ref strYear, "[^\\d\\.]+", true, "Year:", 50,40);
            blnReturn = blnReturn || DrawTextField(ref strDay, "[^\\d\\.]+", true, "Day:",  50,40);
            GUILayout.EndHorizontal();
            return blnReturn;
        }

        String strOrigin, strDestination;
        String strDepartureAltitude,strArrivalAltitude;
        String strDepartureMinYear, strDepartureMinDay, strDepartureMaxYear, strDepartureMaxDay;
        String strTravelMinDays, strTravelMaxDays;

        internal Vector2 vectMouse;
        internal Vector2 vectSelected;
        internal Double DepartureSelected, TravelSelected;
        internal TransferDetails TransferSelected;

        internal override void DrawWindow(int id)
        {
            //Settings toggle
            GUIContent contSettings = new GUIContent(Resources.GetSettingsButtonIcon(TransferWindowPlanner.settings.VersionAttentionFlag), "Settings...");
            if (TransferWindowPlanner.settings.VersionAvailable) contSettings.tooltip = "Updated Version Available - Settings...";
            mbTWP.windowSettings.Visible = GUI.Toggle(new Rect(WindowRect.width - 32, 2, 30, 20), mbTWP.windowSettings.Visible, contSettings, "ButtonSettings");
          
            if (mbTWP.windowSettings.Visible)
            {
                mbTWP.windowSettings.WindowRect.x = WindowRect.x + WindowRect.width;
                mbTWP.windowSettings.WindowRect.y = WindowRect.y;
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label("Enter Parameters");

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Space(2);
            GUILayout.Label("Origin:",Styles.styleTextFieldLabel);
            GUILayout.Label("Initial Orbit:", Styles.styleTextFieldLabel);
            GUILayout.Label("Destination:", Styles.styleTextFieldLabel);
            GUILayout.Label("Final Orbit:", Styles.styleTextFieldLabel);

            //Checkbox re insertion burn
            GUILayout.Label("Earliest Departure:", Styles.styleTextFieldLabel);
            GUILayout.Label("Latest Departure:", Styles.styleTextFieldLabel);
            GUILayout.Label("Time of Flight:", Styles.styleTextFieldLabel);
                        
            //GUILayout.Label("Transfer Type:", Styles.styleTextTitle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            ddlOrigin.DrawButton();
            
            GUILayout.BeginHorizontal();
            DrawTextField(ref strDepartureAltitude, "[^\\d\\.]+", true,FieldWidth: 172);
            GUILayout.Label("km",GUILayout.Width(20));
            GUILayout.EndHorizontal();
            
            ddlDestination.DrawButton();

            GUILayout.BeginHorizontal();
            DrawTextField(ref strArrivalAltitude, "[^\\d\\.]+", true, FieldWidth: 172);
            GUILayout.Label("km", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            DrawYearDay(ref strDepartureMinYear,ref strDepartureMinDay);
            
            DrawYearDay(ref strDepartureMaxYear,ref strDepartureMaxDay);

            GUILayout.BeginHorizontal();
            DrawTextField(ref strTravelMinDays, "[^\\d\\.]+", true, FieldWidth: 60);
            GUILayout.Label("to",GUILayout.Width(15));
            DrawTextField(ref strTravelMaxDays, "[^\\d\\.]+", true, FieldWidth: 60);
            GUILayout.Label("days", GUILayout.Width(30));
            GUILayout.EndHorizontal();
            //ddlXferType.DrawButton();

            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Plot It!"))
                StartWorker();





            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(10));
            GUILayout.Box(Resources.texSeparatorV,Styles.styleSeparatorV,GUILayout.Height(200));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();


            if (Running)
            {
                GUI.Label(new Rect(PlotPosition.x, PlotPosition.y + PlotHeight / 2 - 30, PlotWidth + 45, 20),
                    String.Format("Calculating: {0} (@{2:0}km) -> {1} (@{3:0}km)...", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000),
                    Styles.styleTextYellowBold);
                DrawResourceBar(new Rect(PlotPosition.x, PlotPosition.y + PlotHeight / 2 - 10, PlotWidth + 45, 20), (Single)workingpercent);
            }
            if (Done)
            {
                if (TextureReadyToDraw) {
                    TextureReadyToDraw = false;
                    //Need to move this texure stuff back on to the main thread - set a flag so we know whats done
                    DrawPlotTexture(sumlogDeltaV, sumSqLogDeltaV, maxDeltaV);
                }

                GUILayout.Label(String.Format("Calculating: {0} (@{2:0}km) -> {1} (@{3:0}km)...", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000), Styles.styleTextYellowBold);

                //GUI.Box(new Rect(340, 50, 306, 305), Resources.texPorkChopAxis);
                //GUI.Box(new Rect(346, 50, 300, 300), texPlotArea);
                GUI.Box(new Rect(PlotPosition.x - 6, PlotPosition.y, PlotWidth+6, PlotHeight+6), Resources.texPorkChopAxis,new GUIStyle());
                GUI.Box(new Rect(PlotPosition.x, PlotPosition.y, PlotWidth, PlotHeight), texPlotArea, new GUIStyle());


                //Draw the axis labels

                //have to rotate the GUI for the y labels
                Matrix4x4 matrixBackup = GUI.matrix;
                //rotate the GUI Frame of reference
                GUIUtility.RotateAroundPivot(-90, new Vector2(450, 177)); 
                //draw the axis label
                GUI.Label(new Rect((Single)(PlotPosition.x - 80), (Single)(PlotPosition.y), PlotHeight,15), "Travel Days", Styles.stylePlotYLabel);
                //reset rotation
                GUI.matrix = matrixBackup;
                //Y Axis
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x - 50), (Single)(PlotPosition.y + (i * (PlotHeight - 3)) - 5), 40, 15), String.Format("{0:0}", (TransferSpecs.TravelMin + (1 - i) * TransferSpecs.TravelRange) / (KSPTime.SecondsPerDay)), Styles.stylePlotYText);
                }

                //XAxis
                GUI.Label(new Rect((Single)(PlotPosition.x),(Single)(PlotPosition.y + PlotHeight +20),PlotWidth,15),"Departure Date",Styles.stylePlotXLabel);
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x + (i * PlotWidth) - 22), (Single)(PlotPosition.y + PlotHeight + 5), 40, 15), String.Format("{0:0}", (TransferSpecs.DepartureMin + i * TransferSpecs.DepartureRange) / (KSPTime.SecondsPerDay)), Styles.stylePlotXText);
                }

                //Draw the DeltaV Legend
                //Δv
                GUI.Box(new Rect(PlotPosition.x + PlotWidth + 25, PlotPosition.y, 20, PlotHeight), "", Styles.stylePlotLegendImage);
                GUI.Label(new Rect(PlotPosition.x + PlotWidth + 25, PlotPosition.y-15, 40, 15), "Δv (m/s)", Styles.stylePlotXLabel);
                //m/s values based on min max
                for (Double i = 0; i <=1; i+=0.25) {
                    Double tmpDeltaV = Math.Exp(i * (logMaxDeltaV - logMinDeltaV) + logMinDeltaV);
                    GUI.Label(new Rect((Single)(PlotPosition.x + PlotWidth + 50), (Single)(PlotPosition.y + (1.0 - i) * (PlotHeight-5) - 5), 40, 15), String.Format("{0:0}", tmpDeltaV), Styles.stylePlotLegendText);
                }

                vectMouse=Event.current.mousePosition;
                //Draw the hover over cross
                if (new Rect(PlotPosition.x, PlotPosition.y, PlotWidth, PlotHeight).Contains(vectMouse)) {
                    GUI.Box(new Rect(vectMouse.x, PlotPosition.y, 1, PlotHeight), "", Styles.stylePlotCrossHair);
                    GUI.Box(new Rect(PlotPosition.x, vectMouse.y, PlotWidth, 1), "", Styles.stylePlotCrossHair);

                    //GUI.Label(new Rect(vectMouse.x + 5, vectMouse.y - 20, 80, 15), String.Format("{0:0}m/s", 
                    //    DeltaVs[(int)((vectMouse.y - PlotPosition.y) * PlotWidth + (vectMouse.x - PlotPosition.x))]), SkinsLibrary.CurrentTooltip);

                    Int32 iCurrent = (Int32)((vectMouse.y-PlotPosition.y)*PlotWidth + (vectMouse.x - PlotPosition.x));

                    GUI.Label(new Rect(vectMouse.x + 5, vectMouse.y - 20, 80, 15), String.Format("{0:0}m/s", DeltaVs[iCurrent]), SkinsLibrary.CurrentTooltip);

                    if (Event.current.type== EventType.MouseDown && Event.current.button == 0)
                    {
                        vectSelected = new Vector2(vectMouse.x,vectMouse.y);
                        SetTransferDetails();

                    }

                }

                //Draw the selected position indicators
                if (Done && DepartureSelected>=0)
                {
                    GUI.Box(new Rect(vectSelected.x - 8, vectSelected.y - 8, 16, 16), Resources.texSelectedPoint, new GUIStyle());
                    GUI.Box(new Rect(PlotPosition.x - 9, vectSelected.y - 5, 9,9), Resources.texSelectedYAxis, new GUIStyle());
                    GUI.Box(new Rect(vectSelected.x - 5, PlotPosition.y+PlotHeight, 9,9), Resources.texSelectedXAxis, new GUIStyle());


                    ColorIndex = DeltaVsColorIndex[(Int32)(((vectSelected.y - PlotPosition.y)) * PlotHeight + (vectSelected.x - PlotPosition.x))];
                    Percent = (Double)ColorIndex / DeltaVColorPalette.Count;
                    GUI.Box(new Rect(PlotPosition.x + PlotWidth + 20, PlotPosition.y+(PlotHeight*(1-(Single)Percent))-5, 30, 9), "", Styles.stylePlotTransferMarkerDV);
                }

            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            ////Draw the selected position indicators
            if (DepartureSelected >= 0 && TransferSelected!=null)
            {
                GUILayout.Space(105);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Selected Transfer Details",GUILayout.Width(150));
                GUILayout.Label(String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000), Styles.styleTextYellow);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Departure:", Styles.styleTextFieldLabel);
                GUILayout.Label("Phase Angle:", Styles.styleTextFieldLabel);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime),KSPTime.PrintTimeFormat.DateTimeString)), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.PhaseAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Arrival:", Styles.styleTextFieldLabel);
                GUILayout.Label("Ejection Angle:", Styles.styleTextFieldLabel);
                GUILayout.Label("Ejection Inclination:", Styles.styleTextFieldLabel);
                GUILayout.Label("Insertion Inclination:", Styles.styleTextFieldLabel);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime + TransferSelected.TravelTime),KSPTime.PrintTimeFormat.DateTimeString)), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.InsertionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Travel Time:", Styles.styleTextFieldLabel);
                GUILayout.Label("Total Δv:", Styles.styleTextFieldLabel);
                GUILayout.Label("Ejection Δv:", Styles.styleTextFieldLabel);
                GUILayout.Label("Insertion Δv:", Styles.styleTextFieldLabel);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}",new KSPTime(TransferSelected.TravelTime).IntervalStringLongTrimYears()), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVTotal), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVEjection), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVInjection), Styles.styleTextYellow);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal Int32 ColorIndex;
        internal Double Percent;
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
