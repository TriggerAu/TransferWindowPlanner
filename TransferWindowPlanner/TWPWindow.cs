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

        internal Boolean DrawTextField(ref String Value, String RegexValidator, String LabelText="", Int32 FieldWidth=0, Int32 LabelWidth=0)
        {
            GUIStyle styleTextBox = Styles.styleText;
            if (!System.Text.RegularExpressions.Regex.IsMatch(Value, RegexValidator, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                styleTextBox = Styles.styleTextYellow;

            if(LabelText!="") {
                if (LabelWidth==0)
                    GUILayout.Label(LabelText, Styles.styleTextTitle);
                else
                    GUILayout.Label(LabelText, Styles.styleTextTitle, GUILayout.Width(LabelWidth));
            }

            Boolean blnReturn = false;
            if (LabelWidth == 0)
                blnReturn = DrawTextBox(ref Value, styleTextBox);
            else
                blnReturn = DrawTextBox(ref Value, styleTextBox, GUILayout.Width(FieldWidth));
            return blnReturn;
        }

        internal Boolean DrawYearDay(ref String strYear,ref String strDay)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            blnReturn = blnReturn || DrawTextField(ref strYear, "[\\d\\.]+", "Year:", 70, 70);
            blnReturn = blnReturn || DrawTextField(ref strDay, "[\\d\\.]+", "Day:", 70, 70);
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
            DrawTextField(ref strDepartureAltitude, "[\\d\\.]+");
            DrawLabel("km");
            GUILayout.EndHorizontal();
            
            ddlDestination.DrawButton();

            GUILayout.BeginHorizontal();
            DrawTextField(ref strArrivalAltitude, "[\\d\\.]+");
            DrawLabel("km");
            GUILayout.EndHorizontal();

            DrawYearDay(ref strDepartureMinYear,ref strDepartureMinDay);
            
            DrawYearDay(ref strDepartureMaxYear,ref strDepartureMaxDay);

            GUILayout.BeginHorizontal();
            DrawTextField(ref strTravelMinDays, "[\\d\\.]+");
            DrawLabel("to");
            DrawTextField(ref strTravelMaxDays, "[\\d\\.]+");
            DrawLabel("days");
            GUILayout.EndHorizontal();
            //ddlXferType.DrawButton();


            DrawLabel("DepartureMin:{0}", DepartureMin);
            DrawLabel("DepartureRange:{0}", DepartureRange);
            DrawLabel("DepartureMax:{0}", DepartureMax);
            DrawLabel("TravelMin:{0}", TravelMin);
            DrawLabel("TravelMax:{0}", TravelMax);

            DrawLabel("Hohmann:{0}", hohmannTransferTime);
            DrawLabel("synodic:{0}", synodicPeriod);

            


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

        private void SetWorkerVariables()
        {
            DepartureMin = KSPTime.BuildUTFromRaw(strDepartureMinYear, strDepartureMinDay, "0", "0", "0") - KSPTime.SecondsPerYear - KSPTime.SecondsPerDay;
            DepartureMax = KSPTime.BuildUTFromRaw(strDepartureMaxYear, strDepartureMaxDay, "0", "0", "0") - KSPTime.SecondsPerYear - KSPTime.SecondsPerDay;
            TravelMin = KSPTime.BuildUTFromRaw("0", strTravelMinDays, "0", "0", "0");
            TravelMax = KSPTime.BuildUTFromRaw("0", strTravelMaxDays, "0", "0", "0");
            dblOrbitDestinationAltitude = Convert.ToDouble(strArrivalAltitude);
            dblOrbitOriginAltitude = Convert.ToDouble(strDepartureAltitude);
        }
        private void SetWindowStrings(){
            KSPTime kTime = new KSPTime(DepartureMin);
            strDepartureMinYear = (kTime.Year+1).ToString();
            strDepartureMinDay = (kTime.Day+1).ToString();

            kTime.UT = DepartureMax;
            strDepartureMaxYear = (kTime.Year+1).ToString();
            strDepartureMaxDay = (kTime.Day+1).ToString();

            kTime.UT = TravelMin;
            strTravelMinDays = kTime.Day.ToString();

            kTime.UT = TravelMax;
            strTravelMaxDays = kTime.Day.ToString();

            strArrivalAltitude = dblOrbitOriginAltitude.ToString();
            strDepartureAltitude = dblOrbitDestinationAltitude.ToString();
        }

        private void StartWorker()
        {
            SetWorkerVariables();




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
            //for (int y = 0; y < 300; y++)
            //{
            //    for (int x = 0; x < 300; x++)
            //    {
            //        LogFormatted("{0:0.000}", workingpercent);


            //    }
            //}

            do
            {
                System.Threading.Thread.Sleep(30);
                LogFormatted("{0:0.000}", workingpercent);
                workingpercent += (Single)0.01;
            } while (workingpercent < 1);
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
