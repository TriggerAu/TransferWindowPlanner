using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

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

        internal enum SettingsTabs
        {
            [Description("General Properties")] General,
            //[Description("Styling/Visuals")]    Styling,
            [Description("About...")]   About,
        }

        internal override void Awake()
        {
            //WindowRect = new Rect(mbTWP.windowMain.WindowRect.x + mbTWP.windowMain.WindowRect.width, mbTWP.windowMain.WindowRect.y, 300, 200);
            WindowRect = new Rect(0 ,0, 310, 200);
            settings = TransferWindowPlanner.settings;

            TooltipMouseOffset = new Vector2d(-10, 10);

            ddlSettingsTab = new DropDownList(EnumExtensions.ToEnumDescriptions<SettingsTabs>(), this);

            ddlSettingsSkin = new DropDownList(EnumExtensions.ToEnumDescriptions<Settings.DisplaySkin>(), (Int32)settings.SelectedSkin, this);
            ddlSettingsSkin.OnSelectionChanged += ddlSettingsSkin_SelectionChanged;
            ddlSettingsButtonStyle = new DropDownList(EnumExtensions.ToEnumDescriptions<Settings.ButtonStyleEnum>(), (Int32)settings.ButtonStyleChosen, this);
            ddlSettingsButtonStyle.OnSelectionChanged += ddlSettingsButtonStyle_OnSelectionChanged;

            ddlManager.AddDDL(ddlSettingsButtonStyle);
            ddlManager.AddDDL(ddlSettingsSkin);
            ddlManager.AddDDL(ddlSettingsTab);
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
            GUILayout.Label("Settings Section", Styles.styleTextHeading, GUILayout.Width(140));
            GUILayout.Space(5);
            ddlSettingsTab.DrawButton();
            GUILayout.Space(4);
            GUILayout.EndHorizontal();

            switch ((SettingsTabs)ddlSettingsTab.SelectedIndex)
            {
                case SettingsTabs.General:
                    DrawWindow_General();
                    break;
                case SettingsTabs.About:
                    DrawWindow_About();
                    break;
            }
            GUILayout.EndVertical();

            WindowRect.width = 310;
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
                settings.VersionCheck(true);
                //Hide the flag as we already have the window open;
                settings.VersionAttentionFlag = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(160));
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
                    Application.OpenURL("http://kerbalspaceport.com/kspalternateresourcepanel/");
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
                Application.OpenURL("https://sites.google.com/site/kspalternateresourcepanel/");
            if (GUILayout.Button("Click Here", Styles.styleTextCenterGreen))
                Application.OpenURL("https://github.com/TriggerAu/AlternateResourcePanel/");
            if (GUILayout.Button("Click Here", Styles.styleTextCenterGreen))
                Application.OpenURL("http://forum.kerbalspaceprogram.com/threads/60227-KSP-Alternate-Resource-Panel");

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
