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

        internal Vector2 PlotPosition = new Vector2(360, 50);

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
                    GUILayout.Label(LabelText, Styles.styleTextTitle);
                else
                    GUILayout.Label(LabelText, Styles.styleTextTitle, GUILayout.Width(LabelWidth));
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


        String strDepartureAltitude,strArrivalAltitude;
        String strDepartureMinYear, strDepartureMinDay, strDepartureMaxYear, strDepartureMaxDay;
        String strTravelMinDays, strTravelMaxDays;

        internal override void DrawWindow(int id)
        {

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label("Enter Parameters");

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Space(2);
            GUILayout.Label("Origin:",Styles.styleTextTitle);
            GUILayout.Label("Initial Orbit:", Styles.styleTextTitle);
            GUILayout.Label("Destination:", Styles.styleTextTitle);
            GUILayout.Label("Final Orbit:", Styles.styleTextTitle);

            //Checkbox re insertion burn
            GUILayout.Label("Earliest Departure:", Styles.styleTextTitle);
            GUILayout.Label("Latest Departure:", Styles.styleTextTitle);
            GUILayout.Label("Time of Flight:", Styles.styleTextTitle);
                        
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


            //DrawLabel("DepartureMin:{0}", DepartureMin);
            //DrawLabel("DepartureRange:{0}", DepartureRange);
            //DrawLabel("DepartureMax:{0}", DepartureMax);
            //DrawLabel("TravelMin:{0}", TravelMin);
            //DrawLabel("TravelMax:{0}", TravelMax);

            //DrawLabel("Hohmann:{0}", hohmannTransferTime);
            //DrawLabel("synodic:{0}", synodicPeriod);

            //DrawLabel("Ktime:{0}", kTime.DateString());
            


            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Plot It!"))
                StartWorker();

            //if (GUILayout.Button("A number!"))
            //{
            //    CelestialBody cbO = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "kerbin");
            //    CelestialBody cbD = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "duna");
            //    LogFormatted_DebugOnly("Next UT:{0}->{1}: {2}",cbO.bodyName,cbD.bodyName,LambertSolver.NextLaunchWindowUT(cbO,cbD));
            //}

            //if (GUILayout.Button("A DV!"))
            //{
            //    CelestialBody cbO = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "kerbin");
            //    CelestialBody cbD = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "duna");
            //    LogFormatted_DebugOnly("DV:{0}->{1}: {2}", cbO.bodyName, cbD.bodyName, LambertSolver.TransferDeltaV(cbO, cbD, 5030208, 5718672, 100000, 100000));
            //}

            //if (GUILayout.Button("Solve!"))
            //{
            //    CelestialBody cbO = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "kerbin");
            //    CelestialBody cbD = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "duna");
            //    Vector3d originPositionAtDeparture = cbO.orbit.getRelativePositionAtUT(5030208);
            //    Vector3d destinationPositionAtArrival = cbD.orbit.getRelativePositionAtUT(5030208 + 5718672);
            //    bool longWay = Vector3d.Cross(originPositionAtDeparture, destinationPositionAtArrival).y < 0;

            //    LogFormatted_DebugOnly("DV:{0}->{1}: {2}", cbO.bodyName, cbD.bodyName, LambertSolver.Solve(cbO.referenceBody.gravParameter, originPositionAtDeparture, destinationPositionAtArrival, 5718672, longWay));
            //    LogFormatted_DebugOnly("Origin:{0}", originPositionAtDeparture);
            //    LogFormatted_DebugOnly("Dest:{0}", destinationPositionAtArrival);
            //}

            //if (GUILayout.Button("Maths"))
            //{
            //    CelestialBody cbK = FlightGlobals.Bodies[0];
            //    CelestialBody cbO = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "kerbin");
            //    CelestialBody cbD = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "duna");

            //    LogFormatted_DebugOnly("TAat0:{0}: {1}", cbO.bodyName, cbO.orbit.TrueAnomalyAtUT(0));
            //    LogFormatted_DebugOnly("TAat0:{0}: {1}", cbD.bodyName, cbD.orbit.TrueAnomalyAtUT(0));
            //    LogFormatted_DebugOnly("TAat5030208:{0}: {1}", cbO.bodyName, cbO.orbit.TrueAnomalyAtUT(5030208));
            //    LogFormatted_DebugOnly("TAat5030208:{0}: {1}", cbD.bodyName, cbD.orbit.TrueAnomalyAtUT(5030208));
            //    LogFormatted_DebugOnly("OVat5030208:{0}: {1}", cbO.bodyName, cbO.orbit.getOrbitalVelocityAtUT(5030208).magnitude);
            //    LogFormatted_DebugOnly("OVat5030208:{0}: {1}", cbD.bodyName, cbD.orbit.getOrbitalVelocityAtUT(5030208).magnitude);
            //    LogFormatted_DebugOnly("RPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbO.bodyName, cbO.orbit.getRelativePositionAtUT(5030208).x, cbO.orbit.getRelativePositionAtUT(5030208).y, cbO.orbit.getRelativePositionAtUT(5030208).z);
            //    LogFormatted_DebugOnly("RPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbD.bodyName, cbD.orbit.getRelativePositionAtUT(5030208).x, cbD.orbit.getRelativePositionAtUT(5030208).y, cbD.orbit.getRelativePositionAtUT(5030208).z);

            //    LogFormatted_DebugOnly("RPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbO.bodyName, cbO.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).x, cbO.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).y, cbO.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).z);
            //    LogFormatted_DebugOnly("RPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbD.bodyName, cbD.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).x, cbD.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).y, cbD.orbit.getRelativePositionAtUT(5030208 - Planetarium.GetUniversalTime()).z);

            //    //LogFormatted_DebugOnly("SwapRPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbO.bodyName, cbO.orbit.SwappedRelativePositionAtUT(5030208).x, cbO.orbit.SwappedRelativePositionAtUT(5030208).y, cbO.orbit.SwappedRelativePositionAtUT(5030208).z);
            //    //LogFormatted_DebugOnly("SwapRPat5030208:{0}: X:{1},Y:{2},Z:{3}", cbD.bodyName, cbD.orbit.SwappedRelativePositionAtUT(5030208).x, cbD.orbit.SwappedRelativePositionAtUT(5030208).y, cbD.orbit.SwappedRelativePositionAtUT(5030208).z);

            //    ////LogFormatted_DebugOnly("Absat5030208:{0}: {1}", cbK.bodyName, getAbsolutePositionAtUT(cbK.orbit, 5030208));
            //    //LogFormatted_DebugOnly("Absat5030208:{0}: {1}", cbO.bodyName, getAbsolutePositionAtUT(cbO.orbit,5030208));
            //    //LogFormatted_DebugOnly("Absat5030208:{0}: {1}", cbD.bodyName, getAbsolutePositionAtUT(cbD.orbit, 5030208));

            //    //LogFormatted_DebugOnly("Posat5030208:{0}: {1}", cbO.bodyName, cbO.getPositionAtUT(5030208));
            //    //LogFormatted_DebugOnly("TPosat5030208:{0}: {1}", cbO.bodyName, cbO.getTruePositionAtUT(5030208));
            //    //LogFormatted_DebugOnly("Posat5030208:{0}: {1}", cbD.bodyName, cbD.getPositionAtUT(5030208));
            //    //LogFormatted_DebugOnly("TPosat5030208:{0}: {1}", cbD.bodyName, cbD.getTruePositionAtUT(5030208));
            //    //LogFormatted_DebugOnly("Posat5030208:{0}: {1}", cbK.bodyName, cbK.getPositionAtUT(5030208));
            //    //LogFormatted_DebugOnly("TPosat5030208:{0}: {1}", cbK.bodyName, cbK.getTruePositionAtUT(5030208));



            //    Vector3d pos1 = new Vector3d(13028470326,3900591743,0);
            //    Vector3d pos2 = new Vector3d(-19970745720,-1082561296,15466922.92);
            //    double tof = 5718672;

            //    Vector3d vdest;
            //    Vector3d vinit = LambertSolver.Solve(cbK.gravParameter, pos1, pos2, tof, false, out vdest);
            //    LogFormatted_DebugOnly("Init:{0} - {1}", vinit.magnitude, vinit);
            //    LogFormatted_DebugOnly("vdest:{0} - {1}", vdest.magnitude, vdest);


            //    Vector3d vr1 = cbO.orbit.getOrbitalVelocityAtUT(5030208);
            //    Vector3d vr2 = cbD.orbit.getOrbitalVelocityAtUT(5030208 + 5718672);

            //    Vector3d vdest2;
            //    Vector3d vinit2;
            //    LambertSolver2.Solve(pos1, pos2, tof,cbK, true,out vinit2, out vdest2);

            //    LogFormatted_DebugOnly("Origin:{0} - {1}", vr1.magnitude, vr1);
            //    LogFormatted_DebugOnly("Dest:{0} - {1}", vr2.magnitude, vr2);

            //    LogFormatted_DebugOnly("Depart:{0} - {1}", vinit2.magnitude, vinit2);
            //    LogFormatted_DebugOnly("Arrive:{0} - {1}", vdest2.magnitude, vdest2);

            //    LogFormatted_DebugOnly("Eject:{0} - {1}", (vinit2 - vr1).magnitude, vinit2 - vr1);
            //    LogFormatted_DebugOnly("Inject:{0} - {1}", (vdest2 - vr2).magnitude, vdest2 - vr2);

            //}



            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(10));
            GUILayout.Box(Resources.texSeparatorV,Styles.styleSeparatorV,GUILayout.Height(200));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Plot");
            GUILayout.EndVertical();

            if (Running)
                DrawResourceBar(new Rect(350, 180, 280, 20), (Single)workingpercent);
            if (Done)
            {
                if (TextureReadyToDraw) {
                    TextureReadyToDraw = false;
                    //Need to move this texure stuff back on to the main thread - set a flag so we know whats done
                    DrawPlotTexture(sumlogDeltaV, sumSqLogDeltaV, maxDeltaV);
                }

                //GUI.Box(new Rect(340, 50, 306, 305), Resources.texPorkChopAxis);
                //GUI.Box(new Rect(346, 50, 300, 300), texPlotArea);
                GUI.Box(new Rect(PlotPosition.x - 6, PlotPosition.y, PlotWidth+6, PlotHeight+6), Resources.texPorkChopAxis,new GUIStyle());
                GUI.Box(new Rect(PlotPosition.x, PlotPosition.y, PlotWidth, PlotHeight), texPlotArea, new GUIStyle());


                //Draw the axis labels

                //have to rotate the GUI for the y labels
                Matrix4x4 matrixBackup = GUI.matrix;
                //rotate the GUI Frame of reference
                GUIUtility.RotateAroundPivot(-90, new Vector2(mbTWP.windowDebug.intTest1, mbTWP.windowDebug.intTest2)); 
                //draw the axis label
                GUI.Label(new Rect((Single)(PlotPosition.x - 80), (Single)(PlotPosition.y), PlotHeight,15), "Travel Days", Styles.stylePlotYLabel);
                //reset rotation
                GUI.matrix = matrixBackup;
                //Y Axis
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x - 50), (Single)(PlotPosition.y + (i * (PlotHeight-3))-5), 40, 15), String.Format("{0:0}", (TravelMin + (1-i) * TravelRange)/(KSPTime.SecondsPerDay)), Styles.stylePlotYText);
                }

                //XAxis
                GUI.Label(new Rect((Single)(PlotPosition.x),(Single)(PlotPosition.y + PlotHeight +20),PlotWidth,15),"Departure Date",Styles.stylePlotXLabel);
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x + (i * PlotWidth) - 22), (Single)(PlotPosition.y + PlotHeight + 5), 40, 15), String.Format("{0:0}", (DepartureMin + i * DepartureRange)/(KSPTime.SecondsPerDay)), Styles.stylePlotXText);
                }

                //Draw the DeltaV Legend
                //Δv
                GUI.Box(new Rect(PlotPosition.x + PlotWidth + 40, PlotPosition.y, 20, PlotHeight), "", Styles.stylePlotLegendImage);
                GUI.Label(new Rect(PlotPosition.x + PlotWidth + 40, PlotPosition.y-15, 40, 15), "Δv (m/s)", Styles.stylePlotXLabel);
                //m/s values based on min max
                for (Double i = 0; i <=1; i+=0.25) {
                    Double tmpDeltaV = Math.Exp(i * (logMaxDeltaV - logMinDeltaV) + logMinDeltaV);
                    GUI.Label(new Rect((Single)(PlotPosition.x + PlotWidth + 65), (Single)(PlotPosition.y + (1.0 - i) * (PlotHeight-5) - 5), 40, 15), String.Format("{0:0}", tmpDeltaV), Styles.stylePlotLegendText);
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

                    Double logDeltaV = Math.Log(DeltaVs[(Int32)(vectSelected.y * PlotHeight + vectSelected.x)]);
                    double relativeDeltaV = (logDeltaV - logMinDeltaV) / (logMaxDeltaV - logMinDeltaV);
                    Int32 ColorIndex = Math.Min((Int32)(Math.Floor(relativeDeltaV * DeltaVColorPalette.Count)), DeltaVColorPalette.Count - 1);
                    Single SelectedDV = ColorIndex / DeltaVColorPalette.Count;

                    GUI.Box(new Rect(PlotPosition.x + PlotWidth + 35, PlotPosition.y+(PlotHeight-SelectedDV), 30, 9), "", Styles.stylePlotTransferMarkerDV);
                }

            }

            GUILayout.EndHorizontal();

            ////Draw the selected position indicators
            if (DepartureSelected >= 0 && TransferSelected!=null)
            {
                GUILayout.Space(150);
                GUILayout.Label("Selected Transfer Details");
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("Departure:", Styles.styleTextTitle);
                GUILayout.Label("Phase Angle:", Styles.styleTextTitle);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}", TransferSelected.DepartureTime), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.PhaseAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Arrival:", Styles.styleTextTitle);
                GUILayout.Label("Ejection Angle:", Styles.styleTextTitle);
                GUILayout.Label("Ejection Inclination:", Styles.styleTextTitle);
                GUILayout.Label("Insertion Inclination:", Styles.styleTextTitle);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}", TransferSelected.DepartureTime + TransferSelected.TravelTime), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.InsertionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label("Travel Time:", Styles.styleTextTitle);
                GUILayout.Label("Total Δv:", Styles.styleTextTitle);
                GUILayout.Label("Ejection Δv:", Styles.styleTextTitle);
                GUILayout.Label("Insertion Δv:", Styles.styleTextTitle);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(String.Format("{0:0}", TransferSelected.TravelTime), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVTotal), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVEjection), Styles.styleTextYellow);
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVInjection), Styles.styleTextYellow);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        internal Vector2 vectMouse;
        internal Vector2 vectSelected;
        internal Double DepartureSelected,TravelSelected;
        internal TransferDetails TransferSelected;

        internal static Vector3d getAbsolutePositionAtUT(Orbit orbit, double UT)
        {
            Vector3d pos = orbit.getRelativePositionAtUT(UT);
            pos += orbit.referenceBody.position;
            return pos;
        }



        internal Boolean Running = false;
        internal Boolean Done = false;
        internal Boolean TextureReadyToDraw = false;
        internal Double workingpercent = 0;


        internal BackgroundWorker bw;

        private void SetWorkerVariables()
        {
            DepartureMin = KSPTime.BuildUTFromRaw(strDepartureMinYear, strDepartureMinDay, "0", "0", "0") - KSPTime.SecondsPerYear - KSPTime.SecondsPerDay;
            DepartureMax = KSPTime.BuildUTFromRaw(strDepartureMaxYear, strDepartureMaxDay, "0", "0", "0") - KSPTime.SecondsPerYear - KSPTime.SecondsPerDay;
            DepartureRange = DepartureMax - DepartureMin;
            DepartureSelected = -1;
            TravelMin = KSPTime.BuildUTFromRaw("0", strTravelMinDays, "0", "0", "0");
            TravelMax = KSPTime.BuildUTFromRaw("0", strTravelMaxDays, "0", "0", "0");
            TravelRange = TravelMax - TravelMin;
            TravelSelected = -1;
            FinalOrbitAltitide = Convert.ToDouble(strArrivalAltitude)*1000;
            InitialOrbitAltitude = Convert.ToDouble(strDepartureAltitude)*1000;

            // minus 1 so when we loop from for PlotX pixels the last pixel is the actual last value
            xResolution = DepartureRange / (PlotWidth-1);
            yResolution = TravelRange / (PlotHeight-1);

            DeltaVs = new Double[PlotWidth * PlotHeight];

        }
        Double xResolution, yResolution;
        internal Int32 PlotWidth = 308, PlotHeight = 308;
        Double[] DeltaVs;

        private void SetWindowStrings(){
            KSPTime kTime = new KSPTime(DepartureMin);
            strDepartureMinYear = (kTime.Year+1).ToString();
            strDepartureMinDay = (kTime.Day+1).ToString();

            kTime.UT = DepartureMax;
            strDepartureMaxYear = (kTime.Year+1).ToString();
            strDepartureMaxDay = (kTime.Day+1).ToString();

            kTime.UT = TravelMin;
            strTravelMinDays = (kTime.Year * KSPTime.DaysPerYear + kTime.Day).ToString();

            kTime.UT = TravelMax;
            strTravelMaxDays = (kTime.Year * KSPTime.DaysPerYear + kTime.Day).ToString();

            strArrivalAltitude = (InitialOrbitAltitude/1000).ToString();
            strDepartureAltitude = (FinalOrbitAltitide/1000).ToString();
        }

        private void StartWorker()
        {
            SetWorkerVariables();

            workingpercent = 0;
            Running = true;
            Done = false;
            bw = new BackgroundWorker();
            bw.DoWork += bw_GeneratePorkchop;
            bw.RunWorkerCompleted += bw_GeneratePorkchopCompleted;

            bw.RunWorkerAsync();
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
