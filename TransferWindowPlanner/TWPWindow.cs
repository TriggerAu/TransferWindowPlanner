using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

using KSP;
using UnityEngine;
using KSPPluginFramework;

using TWP_KACWrapper;

namespace TransferWindowPlanner
{
    [WindowInitials(Caption="Transfer Window Planner",
        Visible=false,
        DragEnabled=true,
        TooltipsEnabled=true,
        WindowMoveEventsEnabled=true)]
    public partial class TWPWindow:MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;
        internal Settings settings;

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
                //RuRo!!
                LogFormatted("Error: Couldn't detect a Star (ref body is itself)");
            }
            else
            {
                BodyParseChildren(cbStar);
            }

            ddlOrigin = new DropDownList(lstPlanets.Select(x => x.NameFormatted), 2, this);
            ddlDestination = new DropDownList(lstPlanets.Select(x => x.NameFormatted), 0, this);
            SetDepartureMinToYesterday();
            SetupDestinationControls();
            SetupTransferParams();

            ddlOrigin.OnSelectionChanged += ddlOrigin_OnSelectionChanged;
            ddlDestination.OnSelectionChanged += ddlDestination_OnSelectionChanged;

            ddlManager.AddDDL(ddlOrigin);
            ddlManager.AddDDL(ddlDestination);

            //ddlXferType = new DropDownList(lstXFerTypes, this);
            //ddlManager.AddDDL(ddlXferType);

            onWindowVisibleChanged += TWPWindow_WindowVisibleChanged;

            //Set the defaults
        }

        void TWPWindow_WindowVisibleChanged(MonoBehaviourWindow sender, bool NewVisibleState)
        {
            //if its toggling on make sure the window is in scene
            if (NewVisibleState)
            {
                this.ClampToScreenNow();
                if (!Running && !Done) {
                    SetDepartureMinToYesterday();
                    SetupDestinationControls();
                }
            }
        }

        private void SetDepartureMinToYesterday()
        {
            //Set the Departure min to be yesterday
            dateMinDeparture = new KSPDateTime(Planetarium.GetUniversalTime()).Date;
            DepartureMin = dateMinDeparture.UT;
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

        internal static Boolean DrawTextField(ref String Value, String RegexValidator, Boolean RegexFailOnMatch, String LabelText = "", Int32 FieldWidth = 0, Int32 LabelWidth = 0, Boolean Locked = false)
        {
            GUIStyle styleTextBox = Styles.styleTextField;
            if (Locked)
                styleTextBox = Styles.styleTextFieldLocked;
            else if ((RegexFailOnMatch && System.Text.RegularExpressions.Regex.IsMatch(Value, RegexValidator, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) ||
                (!RegexFailOnMatch && !System.Text.RegularExpressions.Regex.IsMatch(Value, RegexValidator, System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
                styleTextBox = Styles.styleTextFieldError;


            if(LabelText!="") {
                if (LabelWidth==0)
                    GUILayout.Label(LabelText, Styles.styleTextFieldLabel);
                else
                    GUILayout.Label(LabelText, Styles.styleTextFieldLabel, GUILayout.Width(LabelWidth));
            }


            String textValue = Value;
            Boolean blnReturn = false;
            if (FieldWidth == 0)
                blnReturn = DrawTextBox(ref textValue, styleTextBox);
            else
                blnReturn = DrawTextBox(ref textValue, styleTextBox, GUILayout.Width(FieldWidth));

            if (!Locked) Value = textValue;
            return blnReturn;
        }

        internal static Boolean DrawYearDay(ref KSPDateTime dateToDraw)
        {
            String strYear = dateToDraw.Year.ToString();
            String strMonth = dateToDraw.Month.ToString();
            String strDay = dateToDraw.Day.ToString();

            //If the value changed
            Boolean blnReturn = false;

            if (KSPDateStructure.CalendarType==CalendarTypeEnum.Earth)
            {
                blnReturn = DrawYearMonthDay(ref strYear, ref strMonth, ref strDay);
                if (blnReturn) {
                    dateToDraw = KSPDateTime.FromEarthValues(strYear, strMonth, strDay);
                }
            }
            else { 
                blnReturn =  DrawYearDay(ref strYear, ref strDay);
                if (blnReturn) {
                    dateToDraw = new KSPDateTime(strYear, strDay);
                }
            }
            return blnReturn;
        }

        internal static Boolean DrawYearDay(ref String strYear, ref String strDay)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            blnReturn = blnReturn || DrawTextField(ref strYear, "[^\\d\\.]+", true, "Year:", 50,40);
            blnReturn = blnReturn || DrawTextField(ref strDay, "[^\\d\\.]+", true, "Day:",  50,40);
            GUILayout.EndHorizontal();
            return blnReturn;
        }

        internal static Boolean DrawYearMonthDay(ref String strYear, ref String strMonth, ref String strDay)
        {
            Boolean blnReturn = false;
            GUILayout.BeginHorizontal();
            blnReturn = blnReturn || DrawTextField(ref strYear, "[^\\d\\.]+", true, "Y:", 40, 20);
            blnReturn = blnReturn || DrawTextField(ref strMonth, "[^\\d\\.]+", true, "M:", 30, 20);
            blnReturn = blnReturn || DrawTextField(ref strDay, "[^\\d\\.]+", true, "D:", 30, 20);
            GUILayout.EndHorizontal();
            return blnReturn;
        }



        //String strDepartureMinYear, strDepartureMinDay, strDepartureMaxYear, strDepartureMaxDay;
        internal KSPDateTime dateMinDeparture, dateMaxDeparture;
        String strDepartureAltitude, strArrivalAltitude;
        internal String strTravelMinDays, strTravelMaxDays;

        internal Vector2 vectMouse;
        internal Vector2 vectSelected;
        internal Double DepartureSelected, TravelSelected;
        internal TransferDetails TransferSelected;

        internal Boolean ShowInstructions = true;
        internal Boolean ShowMinimized = false;
        internal Boolean ShowEjectionDetails = false;

        internal override void DrawWindow(int id)
        {
            //Calendar toggle
            if (settings.ShowCalendarToggle)
            {
                if (GUI.Button(new Rect(WindowRect.width - 122, 2, 30, 20), new GUIContent(Resources.btnCalendar, "Toggle Calendar"), "ButtonSettings"))
                {
                    if (settings.SelectedCalendar == CalendarTypeEnum.Earth)
                    {
                        settings.SelectedCalendar = CalendarTypeEnum.KSPStock;
                        KSPDateStructure.SetKSPStockCalendar();
                    }
                    else
                    {
                        settings.SelectedCalendar = CalendarTypeEnum.Earth;
                        KSPDateStructure.SetEarthCalendar(settings.EarthEpoch);
                    }
                    settings.Save();
                }
            }

            //Settings toggle
            GUIContent contSettings = new GUIContent(Resources.GetSettingsButtonIcon(TransferWindowPlanner.settings.VersionAttentionFlag), "Settings...");
            if (TransferWindowPlanner.settings.VersionAvailable) contSettings.tooltip = "Updated Version Available - Settings...";
            mbTWP.windowSettings.Visible = GUI.Toggle(new Rect(WindowRect.width - 92, 2, 30, 20), mbTWP.windowSettings.Visible, contSettings, "ButtonSettings");

            //Set a default for the MinMax button
            GUIContent contMaxMin = new GUIContent(Resources.btnChevronUp, "Minimize");
            if (ShowMinimized)
            {
                contMaxMin.image = Resources.btnChevronDown;
                contMaxMin.tooltip = "Expand";
            }
            if (GUI.Button(new Rect(WindowRect.width - 62, 2, 30, 20), contMaxMin, "ButtonSettings"))
            {
                ShowMinimized = !ShowMinimized;
                //if its changed then affect the window size
                if (ShowMinimized)
                {
                    WindowRect.x = WindowRect.x + WindowRect.width - 320;

                    WindowRect.width = 350;
                    WindowRect.height = 0;
                }
                else
                {
                    WindowRect.x = WindowRect.x + 320 - 750;

                    WindowRect.width = 750;
                    WindowRect.height = 400;
                }
            }

            //Close button
            if (GUI.Button(new Rect(WindowRect.width - 32, 2, 30, 20), "X", "ButtonSettings"))
            {
                //Visible = false;
                if(TransferWindowPlanner.settings.ButtonStyleToDisplay== Settings.ButtonStyleEnum.Launcher)
                {
                    mbTWP.btnAppLauncher.SetFalse();
                }
                else
                    Visible = false;
            }

            //Set the settings window pos
            if (mbTWP.windowSettings.Visible)
            {
                mbTWP.windowSettings.WindowRect.y = WindowRect.y;
                if (ShowMinimized)
                {
                    mbTWP.windowSettings.WindowRect.x = WindowRect.x + WindowRect.width;
                }
                else
                {
                    mbTWP.windowSettings.WindowRect.x = WindowRect.x + WindowRect.width - mbTWP.windowSettings.WindowRect.width;
                }
            }
            mbTWP.windowSettingsBlockout.Visible = mbTWP.windowSettingsBlockoutExtra.Visible = mbTWP.windowSettings.Visible && !ShowMinimized;

            //Now draw the window
            if (ShowMinimized) 
            {
                if (!Done) {
                    GUILayout.Label("No Selected Transfer to be able to display info",Styles.styleTextInstruction);
                    GUILayout.Space(10);
                    GUILayout.Label("Go back to restored mode and plot a transfer first", Styles.styleTextInstruction);
                    //GUILayout.Label("You need to have run a plot and selected a transfer to get this");
                }
                else
                {
                    DrawTransferDetailsMinimal();
                }
            } 
            else 
            { 
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(300));
                DrawTransferEntry();
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(10));
                GUILayout.Box(Resources.texSeparatorV, Styles.styleSeparatorV, GUILayout.Height(335));
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                DrawTransferPlot();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                if (DepartureSelected >= 0 && TransferSelected != null)
                {
                    DrawTransferDetails();
                }
            }

            //close the settings window if we click elsewhere
            if (!ShowMinimized && Event.current.type == EventType.mouseDown)
            {
                if (!mbTWP.windowSettings.WindowRect.Contains(Event.current.mousePosition))
                    mbTWP.windowSettings.Visible = false;
            }

        }

        private void DrawTransferDetailsMinimal()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Origin:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Destination:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Departure:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Travel Time:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Ejection Δv:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Insertion Δv:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Total Δv:", Styles.styleTextDetailsLabel);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(String.Format("{0} (@{1:0}km)", TransferSpecs.OriginName, TransferSpecs.InitialOrbitAltitude / 1000), Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0} (@{1:0}km)", TransferSpecs.DestinationName, TransferSpecs.FinalOrbitAltitude / 1000), Styles.styleTextYellow);
            //GUILayout.Label(String.Format("{0:0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime), KSPTime.PrintTimeFormat.DateTimeString)), Styles.styleTextYellow);
            GUILayout.Label(new KSPDateTime(TransferSelected.DepartureTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat), Styles.styleTextYellow);
            //GUILayout.Label(String.Format("{0:0}", new KSPTime(TransferSelected.TravelTime).IntervalStringLongTrimYears()), Styles.styleTextYellow);
            GUILayout.Label(new KSPTimeSpan(TransferSelected.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears), Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVEjection), Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVInjection), Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVTotal), Styles.styleTextYellow);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private Single EjectionDetailsYOffset;
        private Rect DVEjectionRect;
        private void DrawTransferDetails()
        {
            if (ShowEjectionDetails) {
                GUI.Box(new Rect(10,EjectionDetailsYOffset,WindowRect.width - 20, 23),"");
                //Styles.styleSettingsArea if needed
            }
            ////Draw the selected position indicators
            //GUILayout.Space(mbTWP.windowDebug.intTest1);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected Transfer Details", GUILayout.Width(150));
            GUILayout.Label(String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000), Styles.styleTextYellow);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Departure:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Phase Angle:", Styles.styleTextDetailsLabel);
            if (ShowEjectionDetails) {
                GUILayout.Label(""); GUILayout.Label("    Ejection Heading:", Styles.styleTextDetailsLabel);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            //GUILayout.Label(String.Format("{0:0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime), KSPTime.PrintTimeFormat.DateTimeString)), Styles.styleTextYellow);
            GUILayout.Label(new KSPDateTime(TransferSelected.DepartureTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat), Styles.styleTextYellow);
            GUIStyle styleCopyButton = new GUIStyle(SkinsLibrary.CurrentSkin.button);
            styleCopyButton.fixedHeight = 18;
            styleCopyButton.padding.top = styleCopyButton.padding.bottom = 0;
            if(GUILayout.Button(new GUIContent(Resources.btnCopy, "Copy Departure UT"),styleCopyButton))
            {
                Utilities.CopyTextToClipboard(String.Format("{0:0}", TransferSelected.DepartureTime));
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.PhaseAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
            //GUILayout.Label("Phase Angle:", Styles.styleTextDetailsLabel);
            if (ShowEjectionDetails) {
                GUILayout.Label(""); GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionHeading * LambertSolver.Rad2Deg), Styles.styleTextYellow);
            }

            //Action Buttons
            if (KACWrapper.APIReady)
            {
                if (GUI.Button(new Rect(10, WindowRect.height - 30, 132, 20), new GUIContent("  Add KAC Alarm", Resources.btnKAC)))
                {
                    String tmpID = KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.TransferModelled,
                        String.Format("{0} -> {1}", mbTWP.windowMain.TransferSelected.Origin.bodyName, mbTWP.windowMain.TransferSelected.Destination.bodyName),
                        mbTWP.windowMain.TransferSelected.DepartureTime);


                    KACWrapper.KACAPI.KACAlarm alarmNew = KACWrapper.KAC.Alarms.First(a => a.ID == tmpID);
                    alarmNew.Notes = mbTWP.windowMain.GenerateTransferDetailsText();
                    alarmNew.AlarmMargin = settings.KACMargin * 60 * 60;
                    alarmNew.AlarmAction = settings.KACAlarmAction;
                    alarmNew.XferOriginBodyName = mbTWP.windowMain.TransferSelected.Origin.bodyName;
                    alarmNew.XferTargetBodyName = mbTWP.windowMain.TransferSelected.Destination.bodyName;

                }

                if (GUI.Button(new Rect(132 + 15, WindowRect.height - 30, 120, 20), new GUIContent("  Copy Details", Resources.btnCopy)))
                {
                    CopyAllDetailsToClipboard();
                }
            }
            else
            {
                if (GUI.Button(new Rect(10, WindowRect.height - 30, 250, 20), new GUIContent("  Copy Transfer Details", Resources.btnCopy)))
                {
                    CopyAllDetailsToClipboard();
                }
            }
            
            
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Arrival:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Ejection Angle:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Ejection Inclination:", Styles.styleTextDetailsLabel);
            if (ShowEjectionDetails) {
                GUILayout.Label("Ejection Normal Δv:", Styles.styleTextDetailsLabel);
            }
            if (TransferSpecs.FinalOrbitAltitude > 0) {
                GUILayout.Label("Insertion Inclination:", Styles.styleTextDetailsLabel);
            }else {
                GUILayout.Label("", Styles.styleTextDetailsLabel); //empty label to maintain window height
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            //GUILayout.Label(String.Format("{0:0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime + TransferSelected.TravelTime), KSPTime.PrintTimeFormat.DateTimeString)), Styles.styleTextYellow);
            GUILayout.Label(new KSPDateTime(TransferSelected.DepartureTime + TransferSelected.TravelTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat), Styles.styleTextYellow);
            //GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionAngle * LambertSolver.Rad2Deg), Styles.styleTextYellow);
            GUILayout.Label(TransferSelected.EjectionAngleText, Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.EjectionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
            if (ShowEjectionDetails) {
                GUILayout.Label(String.Format("{0:0.0} m/s", TransferSelected.EjectionDVNormal), Styles.styleTextYellow);
            }
            if (TransferSpecs.FinalOrbitAltitude > 0) {
                GUILayout.Label(String.Format("{0:0.00}°", TransferSelected.InsertionInclination * LambertSolver.Rad2Deg), Styles.styleTextYellow);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Travel Time:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Total Δv:", Styles.styleTextDetailsLabel);
            GUILayout.Label("Ejection Δv:", Styles.styleTextDetailsLabel);
            if (ShowEjectionDetails) {
                GUILayout.Label("Ejection Prograde Δv:", Styles.styleTextDetailsLabel);
            }

            if (TransferSpecs.FinalOrbitAltitude > 0) {
                GUILayout.Label("Insertion Δv:", Styles.styleTextDetailsLabel);
            }
            GUILayout.EndVertical();
            
            
            GUILayout.BeginVertical();
            GUILayout.Label(String.Format("{0:0}", new KSPTimeSpan(TransferSelected.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears)), Styles.styleTextYellow);
            GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVTotal), Styles.styleTextYellow);
            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVEjection), Styles.styleTextYellow);
            if (Event.current.type == EventType.Repaint){
                DVEjectionRect = GUILayoutUtility.GetLastRect();
                EjectionDetailsYOffset = DVEjectionRect.y + 20;
            }
            if (DVEjectionRect!=null){
                if (GUI.Button(new Rect(DVEjectionRect.x + DVEjectionRect.width-20, DVEjectionRect.y, 16, 16), new GUIContent(Resources.btnInfo, "Toggle Details..."), new GUIStyle()))
                {
                    ShowEjectionDetails = !ShowEjectionDetails;
                    if (!ShowEjectionDetails) WindowRect.height = 400;
                }
            }
            GUILayout.EndHorizontal();
            if (ShowEjectionDetails) {
                GUILayout.Label(String.Format("{0:0.0} m/s", TransferSelected.EjectionDVPrograde), Styles.styleTextYellow);
            }
            if (TransferSpecs.FinalOrbitAltitude > 0) {
                GUILayout.Label(String.Format("{0:0} m/s", TransferSelected.DVInjection), Styles.styleTextYellow);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void CopyAllDetailsToClipboard()
        {
            String Message = GenerateTransferDetailsText();
            Utilities.CopyTextToClipboard(Message);
        }

        internal string GenerateTransferDetailsText()
        {
            String Message = String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000);
            //Message = Message.AppendLine("Depart at:      {0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime), KSPTime.PrintTimeFormat.DateTimeString));
            Message = Message.AppendLine("Depart at:      {0}", new KSPDateTime(TransferSelected.DepartureTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
            Message = Message.AppendLine("       UT:      {0:0}", TransferSelected.DepartureTime);
            //Message = Message.AppendLine("   Travel:      {0}", new KSPTime(TransferSelected.TravelTime).IntervalStringLongTrimYears());
            Message = Message.AppendLine("   Travel:      {0}", new KSPTimeSpan(TransferSelected.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears));
            Message = Message.AppendLine("       UT:      {0:0}", TransferSelected.TravelTime);
            //Message = Message.AppendLine("Arrive at:      {0}", KSPTime.PrintDate(new KSPTime(TransferSelected.DepartureTime + TransferSelected.TravelTime), KSPTime.PrintTimeFormat.DateTimeString));
            Message = Message.AppendLine("Arrive at:      {0}", new KSPDateTime(TransferSelected.DepartureTime + TransferSelected.TravelTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
            Message = Message.AppendLine("       UT:      {0:0}", TransferSelected.DepartureTime + TransferSelected.TravelTime);
            Message = Message.AppendLine("Phase Angle:    {0:0.00}°", TransferSelected.PhaseAngle * LambertSolver.Rad2Deg);
            //Message = Message.AppendLine("Ejection Angle: {0:0.00}°", TransferSelected.EjectionAngle * LambertSolver.Rad2Deg);
            Message = Message.AppendLine("Ejection Angle: {0}", TransferSelected.EjectionAngleText);
            Message = Message.AppendLine("Ejection Inc.:  {0:0.00}°", TransferSelected.EjectionInclination * LambertSolver.Rad2Deg);
            Message = Message.AppendLine("Ejection Δv:    {0:0} m/s", TransferSelected.DVEjection);
            Message = Message.AppendLine("Prograde Δv:    {0:0.0} m/s", TransferSelected.EjectionDVPrograde);
            Message = Message.AppendLine("Normal Δv:      {0:0.0} m/s", TransferSelected.EjectionDVNormal);
            Message = Message.AppendLine("Heading:        {0:0.00}°", TransferSelected.EjectionHeading * LambertSolver.Rad2Deg);
            Message = Message.AppendLine("Insertion Inc.: {0:0.00}°", TransferSelected.InsertionInclination * LambertSolver.Rad2Deg);
            Message = Message.AppendLine("Insertion Δv:   {0:0} m/s", TransferSelected.DVInjection);
            Message = Message.AppendLine("Total Δv:       {0:0} m/s", TransferSelected.DVTotal);
            return Message;
        }

        private void DrawTransferPlot()
        {
            if (!Running && !Done)
            {
                DrawInstructions();
            }
            if (Running) {
                GUI.Label(new Rect(PlotPosition.x, PlotPosition.y + PlotHeight / 2 - 30, PlotWidth + 45, 20),
                    String.Format("Calculating: {0} (@{2:0}km) -> {1} (@{3:0}km)...", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000),
                    Styles.styleTextYellowBold);
                //DrawResourceBar(new Rect(PlotPosition.x, PlotPosition.y + PlotHeight / 2 - 10, PlotWidth + 45, 20), (Single)workingpercent);
                DrawResourceBar(new Rect(PlotPosition.x, PlotPosition.y + 292 / 2 - 10, 292 + 45, 20), (Single)workingpercent);
            }
            if (Done) {
                GUILayout.Label(String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", TransferSpecs.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000), Styles.styleTextYellowBold);

                if (TextureReadyToDraw)
                {
                    TextureReadyToDraw = false;
                    //Need to move this texure stuff back on to the main thread - set a flag so we know whats done
                    DrawPlotTexture(sumlogDeltaV, sumSqLogDeltaV, maxDeltaV);
                }

                //GUI.Box(new Rect(340, 50, 306, 305), Resources.texPorkChopAxis);
                //GUI.Box(new Rect(346, 50, 300, 300), texPlotArea);
                GUI.Box(new Rect(PlotPosition.x - 6, PlotPosition.y, PlotWidth + 6, PlotHeight + 6), Resources.texPorkChopAxis, new GUIStyle());
                GUI.Box(new Rect(PlotPosition.x, PlotPosition.y, PlotWidth, PlotHeight), texPlotArea, new GUIStyle());


                //Draw the axis labels

                //have to rotate the GUI for the y labels
                Matrix4x4 matrixBackup = GUI.matrix;
                //rotate the GUI Frame of reference
                GUIUtility.RotateAroundPivot(-90, new Vector2(450, 177));
                //draw the axis label
                GUI.Label(new Rect((Single)(PlotPosition.x - 80), (Single)(PlotPosition.y), PlotHeight, 15), "Travel Days", Styles.stylePlotYLabel);
                //reset rotation
                GUI.matrix = matrixBackup;
                //Y Axis
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x - 50), (Single)(PlotPosition.y + (i * (PlotHeight - 3)) - 5), 40, 15), String.Format("{0:0}", (TransferSpecs.TravelMin + (1 - i) * TransferSpecs.TravelRange) / (KSPDateStructure.SecondsPerDay)), Styles.stylePlotYText);
                }

                //XAxis
                GUI.Label(new Rect((Single)(PlotPosition.x), (Single)(PlotPosition.y + PlotHeight + 20), PlotWidth, 15), "Departure Date", Styles.stylePlotXLabel);
                for (Double i = 0; i <= 1; i += 0.25) {
                    GUI.Label(new Rect((Single)(PlotPosition.x + (i * PlotWidth) - 22), (Single)(PlotPosition.y + PlotHeight + 5), 40, 15), String.Format("{0:0}", (TransferSpecs.DepartureMin + i * TransferSpecs.DepartureRange) / (KSPDateStructure.SecondsPerDay)), Styles.stylePlotXText);
                }

                //Draw the DeltaV Legend
                //Δv
                GUI.Box(new Rect(PlotPosition.x + PlotWidth + 25, PlotPosition.y, 20, PlotHeight), "", Styles.stylePlotLegendImage);
                GUI.Label(new Rect(PlotPosition.x + PlotWidth + 25, PlotPosition.y - 15, 40, 15), "Δv (m/s)", Styles.stylePlotXLabel);
                //m/s values based on min max
                for (Double i = 0; i <= 1; i += 0.25) {
                    Double tmpDeltaV = Math.Exp(i * (logMaxDeltaV - logMinDeltaV) + logMinDeltaV);
                    GUI.Label(new Rect((Single)(PlotPosition.x + PlotWidth + 50), (Single)(PlotPosition.y + (1.0 - i) * (PlotHeight - 5) - 5), 40, 15), String.Format("{0:0}", tmpDeltaV), Styles.stylePlotLegendText);
                }

                vectMouse = Event.current.mousePosition;
                //Draw the hover over cross
                if (new Rect(PlotPosition.x, PlotPosition.y, PlotWidth, PlotHeight).Contains(vectMouse)) {
                    GUI.Box(new Rect(vectMouse.x, PlotPosition.y, 1, PlotHeight), "", Styles.stylePlotCrossHair);
                    GUI.Box(new Rect(PlotPosition.x, vectMouse.y, PlotWidth, 1), "", Styles.stylePlotCrossHair);

                    //GUI.Label(new Rect(vectMouse.x + 5, vectMouse.y - 20, 80, 15), String.Format("{0:0}m/s", 
                    //    DeltaVs[(int)((vectMouse.y - PlotPosition.y) * PlotWidth + (vectMouse.x - PlotPosition.x))]), SkinsLibrary.CurrentTooltip);

                    Int32 iCurrent = (Int32)((vectMouse.y - PlotPosition.y) * PlotWidth + (vectMouse.x - PlotPosition.x));

                    GUI.Label(new Rect(vectMouse.x + 5, vectMouse.y - 20, 80, 15), String.Format("{0:0}m/s", DeltaVs[iCurrent]), SkinsLibrary.CurrentTooltip);

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                        vectSelected = new Vector2(vectMouse.x, vectMouse.y);
                        SetTransferDetails();

                    }

                }

                //Draw the selected position indicators
                if (Done && DepartureSelected >= 0) {
                    GUI.Box(new Rect(vectSelected.x - 8, vectSelected.y - 8, 16, 16), Resources.texSelectedPoint, new GUIStyle());
                    GUI.Box(new Rect(PlotPosition.x - 9, vectSelected.y - 5, 9, 9), Resources.texSelectedYAxis, new GUIStyle());
                    GUI.Box(new Rect(vectSelected.x - 5, PlotPosition.y + PlotHeight, 9, 9), Resources.texSelectedXAxis, new GUIStyle());


                    ColorIndex = DeltaVsColorIndex[(Int32)(((vectSelected.y - PlotPosition.y)) * PlotHeight + (vectSelected.x - PlotPosition.x))];
                    Percent = (Double)ColorIndex / DeltaVColorPalette.Count;
                    GUI.Box(new Rect(PlotPosition.x + PlotWidth + 20, PlotPosition.y + (PlotHeight * (1 - (Single)Percent)) - 5, 30, 9), "", Styles.stylePlotTransferMarkerDV);
                }

            }
        }
        private void DrawInstructions()
        {
            GUILayout.Label("Instructions:", Styles.styleTextYellowBold);
            DrawSingleInstruction("1.","Select the celestial body you will be departing from.");
            DrawSingleInstruction("2.","Enter the altitude of your parking orbit around that body. This is assumed to be a circular, equatorial orbit.");
            DrawSingleInstruction("3.","Select the celestial body you wish to travel to.");
            DrawSingleInstruction("4.", "Enter the altitude of the orbit you wish to establish around your destination body. You can enter 0 if you intend to perform a fly-by or aerobraking maneuver.");
            //DrawSingleInstruction("4.", "Enter the altitude of the orbit you wish to establish around your destination body. You may check the \"No insertion burn\" checkbox instead if you intend to perform a fly-by or aerobraking maneuver.");
            DrawSingleInstruction("5.", "Enter the earliest departure date to include in the plot. Generally this should be your current game time, which you can find in the tracking station in the game.");
            DrawSingleInstruction("6.","Click the \"Plot it!\" button. After a few seconds a plot will appear showing how much Δv is required to reach your destination for different departure dates and times of flight. Click on any point on this plot to see full details of the selected transfer.");
        }
        private void DrawSingleInstruction(String Num, String Text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Num, Styles.styleTextInstructionNum, GUILayout.Width(16));
            GUILayout.Label(Text, Styles.styleTextInstruction);
            GUILayout.EndHorizontal();
        }

        Boolean blnFlyby = false;
        private void DrawTransferEntry()
        {
            GUILayout.Label("Enter Parameters", Styles.styleTextYellowBold);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Space(2);
            GUILayout.Label("Origin:", Styles.styleTextFieldLabel);
            GUILayout.Label("Initial Orbit:", Styles.styleTextFieldLabel);
            GUILayout.Label("Destination:", Styles.styleTextFieldLabel);
            GUILayout.Label("Final Orbit:", Styles.styleTextFieldLabel);
            GUILayout.Label("", Styles.styleTextFieldLabel);

            //Checkbox re insertion burn
            GUILayout.Label("Earliest Departure:", Styles.styleTextFieldLabel);
            GUILayout.Label("Latest Departure:", Styles.styleTextFieldLabel);
            GUILayout.Label("Time of Flight:", Styles.styleTextFieldLabel);

            //GUILayout.Label("Transfer Type:", Styles.styleTextTitle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            ddlOrigin.DrawButton();

            GUILayout.BeginHorizontal();
            DrawTextField(ref strDepartureAltitude, "[^\\d\\.]+", true, FieldWidth: 172);
            GUILayout.Label("km", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            ddlDestination.DrawButton();

            GUILayout.BeginHorizontal();
            DrawTextField(ref strArrivalAltitude, "[^\\d\\.]+", true, FieldWidth: 172, Locked:blnFlyby);
            GUILayout.Label("km", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            DrawToggle(ref blnFlyby, new GUIContent("No Insertion Burn (eg. fly-by)"), Styles.styleToggle);

            GUILayout.Space(8);

            DrawYearDay(ref dateMinDeparture);

            DrawYearDay(ref dateMaxDeparture);

            GUILayout.BeginHorizontal();
            DrawTextField(ref strTravelMinDays, "[^\\d\\.]+", true, FieldWidth: 60);
            GUILayout.Label("to", GUILayout.Width(15));
            DrawTextField(ref strTravelMaxDays, "[^\\d\\.]+", true, FieldWidth: 60);
            GUILayout.Label("days", GUILayout.Width(30));
            GUILayout.EndHorizontal();
            //ddlXferType.DrawButton();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Reset",Resources.btnReset), "ButtonSettings")) {
                mbTWP.windowSettings.Visible = false;
                ResetWindow();
            }
            if (GUILayout.Button("Plot It!"))
            {
                mbTWP.windowSettings.Visible = false;
                StartWorker();
                WindowRect.height = 400;
                ShowEjectionDetails = false;
            }
            GUILayout.EndHorizontal();
        }

        internal void ResetWindow()
        {
            Done = false;
            if (bw != null && bw.IsBusy)
                bw.CancelAsync();
            Running = false;
            TransferSelected = null;
            WindowRect.height = 400;

            SetDepartureMinToYesterday();
            SetupDestinationControls();
        }
        
        internal override void OnGUIEvery()
        {
            //close the settings window if we click elsewhere
            if (!ShowMinimized && Event.current.type == EventType.mouseDown)
            {
                if (!mbTWP.windowSettings.WindowRect.Contains(Event.current.mousePosition))
                    mbTWP.windowSettings.Visible = false;
            }

            base.OnGUIEvery();
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
