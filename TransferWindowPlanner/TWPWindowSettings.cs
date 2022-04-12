using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

using TWP_KACWrapper;

namespace TransferWindowPlanner
{
    [WindowInitials(TooltipsEnabled=true,Visible=false,DragEnabled=false,Caption="TWP Settings")]
    class TWPWindowSettings:MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;
        internal Settings settings;

        internal DropDownList ddlSettingsTab;
        private DropDownList ddlSettingsSkin;
        private DropDownList ddlSettingsButtonStyle;
        internal DropDownList ddlSettingsCalendar;

        internal Int32 WindowWidth = 360;
        internal Int32 WindowHeight = 200;

        internal enum SettingsTabs
        {
            [Description("General Properties")] General,
            [Description("Alarm Clock Integration")] AlarmIntegration,
            [Description("Calendar Control")] Calendar,
            //[Description("Styling/Visuals")]    Styling,
            [Description("About...")]   About,
        }
        
        internal override void OnAwake()
        {
            base.OnAwake();

            //WindowRect = new Rect(mbTWP.windowMain.WindowRect.x + mbTWP.windowMain.WindowRect.width, mbTWP.windowMain.WindowRect.y, 300, 200);
            WindowRect = new Rect(0, 0, WindowWidth, WindowHeight);
            settings = TransferWindowPlanner.settings;

            TooltipMouseOffset = new Vector2d(-10, 10);

            ddlSettingsTab = new DropDownList(KSPPluginFramework.EnumExtensions.ToEnumDescriptions<SettingsTabs>(), this);

            ddlSettingsSkin = new DropDownList(KSPPluginFramework.EnumExtensions.ToEnumDescriptions<Settings.DisplaySkin>(), (Int32)settings.SelectedSkin, this);
            ddlSettingsSkin.OnSelectionChanged += ddlSettingsSkin_SelectionChanged;
            ddlSettingsButtonStyle = new DropDownList(KSPPluginFramework.EnumExtensions.ToEnumDescriptions<Settings.ButtonStyleEnum>(), (Int32)settings.ButtonStyleChosen, this);
            ddlSettingsButtonStyle.OnSelectionChanged += ddlSettingsButtonStyle_OnSelectionChanged;
            ddlSettingsCalendar = new DropDownList(KSPPluginFramework.EnumExtensions.ToEnumDescriptions<CalendarTypeEnum>(), this);
            //NOTE:Pull out the custom option for now
            ddlSettingsCalendar.Items.Remove(CalendarTypeEnum.Custom.Description());
            ddlSettingsCalendar.OnSelectionChanged += ddlSettingsCalendar_OnSelectionChanged;
            
            ddlManager.AddDDL(ddlSettingsCalendar);
            ddlManager.AddDDL(ddlSettingsButtonStyle);
            ddlManager.AddDDL(ddlSettingsSkin);
            ddlManager.AddDDL(ddlSettingsTab);

            onWindowVisibleChanged += TWPWindowSettings_onWindowVisibleChanged;
        }


        void TWPWindowSettings_onWindowVisibleChanged(MonoBehaviourWindow sender, bool NewVisibleState)
        {
            if (NewVisibleState)
            {
                if (settings.VersionAttentionFlag)
                    ddlSettingsTab.SelectedIndex = (Int32)SettingsTabs.About;
                else
                    ddlSettingsTab.SelectedIndex = (Int32)SettingsTabs.General;

                //reset the flag
                settings.VersionAttentionFlag = false;
            }
        }

        internal override void OnGUIOnceOnly()
        {
            ddlManager.DropDownGlyphs = new GUIContentWithStyle(Resources.btnDropDown, Styles.styleDropDownGlyph);
            ddlManager.DropDownSeparators = new GUIContentWithStyle("", Styles.styleSeparatorV);
        }

        void ddlSettingsSkin_SelectionChanged(DropDownList sender, int OldIndex, int NewIndex)
        {
            settings.SelectedSkin = (Settings.DisplaySkin)NewIndex;
            SkinsLibrary.SetCurrent(settings.SelectedSkin.ToString());
            settings.Save();
        }

        void ddlSettingsCalendar_OnSelectionChanged(DropDownList sender, int OldIndex, int NewIndex)
        {
            settings.SelectedCalendar = (CalendarTypeEnum)NewIndex;
            settings.Save();
            switch (settings.SelectedCalendar)
            {
                case CalendarTypeEnum.KSPStock: KSPDateStructure.SetKSPStockCalendar(); break;
                case CalendarTypeEnum.Earth:
                    KSPDateStructure.SetEarthCalendar(settings.EarthEpoch);
                    break;
                case CalendarTypeEnum.Custom:   
                    KSPDateStructure.SetCustomCalendar();
                    break;
                default: KSPDateStructure.SetKSPStockCalendar(); break;
            }
            mbTWP.windowMain.ResetWindow();
        }


        void ddlSettingsButtonStyle_OnSelectionChanged(MonoBehaviourWindowPlus.DropDownList sender, int OldIndex, int NewIndex)
        {
            settings.ButtonStyleChosen = (Settings.ButtonStyleEnum)NewIndex;
            settings.Save();

            //destroy Old Objects
            switch ((Settings.ButtonStyleEnum)OldIndex)
            {
                case Settings.ButtonStyleEnum.Toolbar:
                    mbTWP.DestroyToolbarButton(mbTWP.btnToolbar);
                    break;
                case Settings.ButtonStyleEnum.Launcher:
                    mbTWP.DestroyAppLauncherButton();
                    break;
            }

            //Create New ones
            switch ((Settings.ButtonStyleEnum)NewIndex)
            {
                case Settings.ButtonStyleEnum.Toolbar:
                    mbTWP.btnToolbar = mbTWP.InitToolbarButton();
                    break;
                case Settings.ButtonStyleEnum.Launcher:
                    mbTWP.btnAppLauncher = mbTWP.InitAppLauncherButton();
                    break;
            }
        }
        internal override void DrawWindow(int id)
        {
            if (GUI.Button(new Rect(WindowRect.width - 32, 2, 30, 20), "X", "ButtonSettings"))
            {
                Visible = false;
            }

            GUILayout.BeginVertical();

            if (SkinsLibrary.CurrentSkin.name != "Default")
                GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Settings Section", Styles.styleTextHeading, GUILayout.Width(120));
            GUILayout.Space(5);
            ddlSettingsTab.DrawButton();
            GUILayout.Space(4);
            GUILayout.EndHorizontal();

            switch ((SettingsTabs)ddlSettingsTab.SelectedIndex)
            {
                case SettingsTabs.General:
                    DrawWindow_General();
                    WindowHeight = 206;
                    break;
                case SettingsTabs.AlarmIntegration:
                    DrawWindow_Alarm();
                    WindowHeight = 206;
                    break;
                case SettingsTabs.Calendar:
                    DrawWindow_Calendar();
                    WindowHeight = 206;
                    break;
                case SettingsTabs.About:
                    DrawWindow_About();
                    WindowHeight = 285;
                    break;
            }
            GUILayout.EndVertical();

            WindowRect.width = WindowWidth;
            WindowRect.height = WindowHeight;
        }

        //Int32 intBlizzyToolbarMissingHeight = 0;
        private void DrawWindow_General()
        {
            //Styling
            GUILayout.BeginHorizontal(Styles.styleSettingsArea, GUILayout.Height(54));

            GUILayout.BeginVertical(GUILayout.Width(60));
            GUILayout.Space(2); //to even up the text
            GUILayout.Label("Styling:", Styles.styleTextHeading);
            GUILayout.Label("Button:", Styles.styleTextHeading);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            ddlSettingsSkin.DrawButton();

            ddlSettingsButtonStyle.DrawButton();

            //intBlizzyToolbarMissingHeight = 0;
            if (!settings.BlizzyToolbarIsAvailable)
            {
                if (settings.ButtonStyleChosen == Settings.ButtonStyleEnum.Toolbar)
                {
                    if (GUILayout.Button(new GUIContent("Not Installed. Click for Toolbar Info", "Click to open your browser and find out more about the Common Toolbar"), Styles.styleTextCenterGreen))
                        Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/60863");
                    //intBlizzyToolbarMissingHeight = 18;
                }
                //if (DrawToggle(ref settings.UseBlizzyToolbarIfAvailable, new GUIContent("Use Common Toolbar", "Choose to use the Common  Toolbar or the native KSP ARP button"), Styles.styleToggle))
                //{
                //    if (settings.BlizzyToolbarIsAvailable)
                //    {
                //        if (settings.UseBlizzyToolbarIfAvailable)
                //            mbARP.btnToolbar = mbARP.InitToolbarButton();
                //        else
                //            mbARP.DestroyToolbarButton(mbARP.btnToolbar);
                //    }
                //    settings.Save();
                //}
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Label("Click Through Protection", Styles.styleTextHeading);
            GUILayout.BeginVertical(Styles.styleSettingsArea);
            if (DrawToggle(ref settings.ClickThroughProtect_KSC, "Prevent in Space Center", Styles.styleToggle)) {
                if (!settings.ClickThroughProtect_KSC && HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    mbTWP.RemoveInputLock();
                settings.Save();
            }
            if (DrawToggle(ref settings.ClickThroughProtect_Editor, "Prevent in Editors", Styles.styleToggle)) {
                if (!settings.ClickThroughProtect_Editor && (HighLogic.LoadedScene == GameScenes.EDITOR))
                    mbTWP.RemoveInputLock();
                settings.Save();
            }
            if (DrawToggle(ref settings.ClickThroughProtect_Flight, "Prevent in Flight", Styles.styleToggle)) {
                if (!settings.ClickThroughProtect_Flight && HighLogic.LoadedScene == GameScenes.FLIGHT)
                    mbTWP.RemoveInputLock();
                settings.Save();
            }
            if (DrawToggle(ref settings.ClickThroughProtect_Tracking, "Prevent in Tracking Station", Styles.styleToggle)) {
                if (!settings.ClickThroughProtect_Tracking && HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                    mbTWP.RemoveInputLock();
                settings.Save();
            }
            GUILayout.EndVertical();
        }

        private void DrawWindow_Alarm()
        {
            GUILayout.BeginVertical(Styles.styleSettingsArea);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Margin", Styles.styleText, GUILayout.Width(85));
            if (DrawTextBox(ref settings.AlarmMargin))
                settings.Save();
            GUILayout.Label("(hours)", Styles.styleTextYellow, GUILayout.Width(50));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Alarm Clock App (ACA)", Styles.styleTextHeading);

            if (!TWP_KACWrapper.KACWrapper.AssemblyExists || !TWP_KACWrapper.KACWrapper.InstanceExists)
            {
                GUILayout.Label("Stock Alarm Clock App is used.", Styles.styleTextCenterGreen);
            }
            else
            {
                if (DrawToggle(ref settings.OverrideKAC, "use Stock ACA instead of KAC", Styles.styleToggle))
                {
                    settings.Save();
                }
            }
            GUILayout.Space(10);
            GUILayout.Label("Kerbal Alarm Clock (KAC)", Styles.styleTextHeading);

            if (!TWP_KACWrapper.KACWrapper.AssemblyExists)
            {
                //draw something with a link for the KAC
                if (GUILayout.Button(new GUIContent("Not Installed. Click for Alarm Clock Info", "Click to open your browser and find out more about the Kerbal Alarm Clock"), Styles.styleTextCenterGreen))
                    Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/24786");

            }
            else if (TWP_KACWrapper.KACWrapper.NeedUpgrade)
            {
                if (GUILayout.Button(new GUIContent("You need a newer version of KAC", "Click to open your browser and download a newer Kerbal Alarm Clock"), Styles.styleTextCenterGreen))
                    Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/24786");

            }
            else if (!TWP_KACWrapper.KACWrapper.InstanceExists)
            {
                GUILayout.Label("KAC is not loaded in this scene, so we can't configure", Styles.styleTextGreen);
                GUILayout.Label("the integration options", Styles.styleTextGreen);
                GUILayout.Space(10);
                GUILayout.Label("You can access these in scenes where KAC is visible", Styles.styleTextGreen);
                GUILayout.Space(10);
                GUILayout.Label("Go on... Move along... Nothing to see...", Styles.styleTextGreen);
            }
            else
            {
                //Alarm Area
                //if (KACWrapper.KAC.DrawAlarmActionChoice(ref KACAlarmAction, "On Alarm:", 108, 61))
                if (KACWrapper.KAC.DrawAlarmActionChoice(ref settings.KACAlarmAction, "Action:", 90 , 38))
                    {
                    settings.Save();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawWindow_Calendar()
        {
            //Update Check Area
            GUILayout.Label("General Settings", Styles.styleTextHeading);

            GUILayout.BeginVertical(Styles.styleSettingsArea);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(60));
            GUILayout.Space(2); //to even up the text
            GUILayout.Label("Calendar:", Styles.styleTextHeading);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            ddlSettingsCalendar.DrawButton();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (DrawToggle(ref settings.ShowCalendarToggle, "Show Calendar Toggle in Main Window", Styles.styleToggle))
                settings.Save();
            GUILayout.EndVertical();

            if (settings.SelectedCalendar == CalendarTypeEnum.Earth)
            {
                GUILayout.Label("Earth Settings", Styles.styleTextHeading);
                GUILayout.BeginVertical(Styles.styleSettingsArea);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Earth Epoch:");

                String strYear, strMonth, strDay;
                strYear = KSPDateStructure.CustomEpochEarth.Year.ToString();
                strMonth = KSPDateStructure.CustomEpochEarth.Month.ToString();
                strDay = KSPDateStructure.CustomEpochEarth.Day.ToString();
                if (TWPWindow.DrawYearMonthDay(ref strYear, ref strMonth, ref strDay))
                {
                    try
                    {
                        KSPDateStructure.SetEarthCalendar(strYear.ToInt32(), strMonth.ToInt32(), strDay.ToInt32());
                        settings.EarthEpoch = KSPDateStructure.CustomEpochEarth.ToString("yyyy-MM-dd");
                        settings.Save();
                        mbTWP.windowMain.ResetWindow();
                    }
                    catch (Exception)
                    {
                        LogFormatted("Unable to set the Epoch date using the values provided-{0}-{1}-{2}", strYear, strMonth, strDay);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset Earth Epoch"))
                {
                    KSPDateStructure.SetEarthCalendar();
                    settings.EarthEpoch = KSPDateStructure.CustomEpochEarth.ToString("1951-01-01");
                    settings.Save();
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            //if RSS not installed and RSS chosen...

            ///section for custom stuff
        }
        private void DrawWindow_About()
        {
            //Update Check Area
            GUILayout.Label("Version Check", Styles.styleTextHeading);

            GUILayout.BeginVertical(Styles.styleSettingsArea);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Space(3);
            if (DrawToggle(ref settings.DailyVersionCheck, "Check Version Daily", Styles.styleToggle))
                settings.Save();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Check Version Now"))
            {
                settings.VersionCheck(mbTWP, true);
                //Hide the flag as we already have the window open;
                settings.VersionAttentionFlag = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Space(4);
            GUILayout.Label("Last Check Attempt:");
            GUILayout.Label("Current Version:");
            GUILayout.Label("Last Version from Web:");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(settings.VersionCheckDate_AttemptString, Styles.styleTextGreen);
            GUILayout.Label(settings.Version, Styles.styleTextGreen);

            if (settings.VersionCheckRunning)
            {
                Int32 intDots = Convert.ToInt32(Math.Truncate(DateTime.Now.Millisecond / 250d)) + 1;
                GUILayout.Label(String.Format("{0} Checking", new String('.', intDots)), Styles.styleTextYellowBold);
            }
            else
            {
                if (settings.VersionAvailable)
                    GUILayout.Label(String.Format("{0} @ {1}", settings.VersionWeb, settings.VersionCheckDate_SuccessString), Styles.styleTextYellowBold);
                else
                    GUILayout.Label(String.Format("{0} @ {1}", settings.VersionWeb, settings.VersionCheckDate_SuccessString), Styles.styleTextGreen);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (settings.VersionAvailable)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(80);
                if (GUILayout.Button("Updated Version Available - Click Here", Styles.styleTextYellowBold))
                    Application.OpenURL("https://github.com/TriggerAu/TransferWindowPlanner/releases");
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();


            //About Area
            GUILayout.BeginVertical(Styles.styleSettingsArea);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            //GUILayout.Label("Written by:", Styles.styleStageTextHead);
            GUILayout.Label("Documentation and Links:", Styles.styleTextHeading);
            GUILayout.Label("GitHub Page:", Styles.styleTextHeading);
            GUILayout.Label("Forum Page:", Styles.styleTextHeading);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            //GUILayout.Label("Trigger Au",KACResources.styleContent);
            if (GUILayout.Button("Click Here", Styles.styleTextCenterGreen))
                Application.OpenURL("http://triggerau.github.io/TransferWindowPlanner/");
            if (GUILayout.Button("Click Here", Styles.styleTextCenterGreen))
                Application.OpenURL("http://github.com/TriggerAu/TransferWindowPlanner/");
            if (GUILayout.Button("Click Here", Styles.styleTextCenterGreen))
                Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/93115-Transfer-Window-Planner");

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
