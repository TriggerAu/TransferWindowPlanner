using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    internal class Settings : ConfigNodeStorage
    {
        internal Settings(String FilePath)
            : base(FilePath)
        {
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //on each start set the attention flag to the property - should be on each program start
            VersionAttentionFlag = VersionAvailable;

            OnEncodeToConfigNode();
        }



        [Persistent] internal DisplaySkin SelectedSkin = DisplaySkin.Default;

        internal Boolean BlizzyToolbarIsAvailable = false;

        internal ButtonStyleEnum ButtonStyleToDisplay {
            get {
                if (BlizzyToolbarIsAvailable || ButtonStyleChosen != ButtonStyleEnum.Toolbar)
                    return ButtonStyleChosen;
                else
                    return ButtonStyleEnum.Launcher;
            }
        }
        [Persistent] internal ButtonStyleEnum ButtonStyleChosen = ButtonStyleEnum.Launcher;

        [Persistent] internal CalendarTypeEnum SelectedCalendar = CalendarTypeEnum.KSPStock;

        internal enum ButtonStyleEnum
        {
            //[Description("Basic button")]                       Basic,
            [Description("Common Toolbar (by Blizzy78)")]       Toolbar,
            [Description("KSP App Launcher Button")]          Launcher,
        }


        internal enum DisplaySkin
        {
            [Description("KSP Style")]          Default,
            [Description("Unity Style")]        Unity,
            [Description("Unity/KSP Buttons")]  UnityWKSPButtons
        }


        [Persistent] internal TWP_KACWrapper.KACWrapper.KACAPI.AlarmActionEnum KACAlarmAction = TWP_KACWrapper.KACWrapper.KACAPI.AlarmActionEnum.KillWarp;
        [Persistent] internal Double KACMargin = 24;

        [Persistent] internal Boolean ClickThroughProtect_KSC=true;
        [Persistent] internal Boolean ClickThroughProtect_Editor=true;
        [Persistent] internal Boolean ClickThroughProtect_Flight=true;

        //Version Stuff
        [Persistent] internal Boolean DailyVersionCheck = true;
        internal Boolean VersionAttentionFlag = false;
        //When did we last check??
        internal DateTime VersionCheckDate_Attempt = new DateTime();
        [Persistent] internal String VersionCheckDate_AttemptStored;
        public String VersionCheckDate_AttemptString { get { return ConvertVersionCheckDateToString(this.VersionCheckDate_Attempt); } }
        internal DateTime VersionCheckDate_Success = new DateTime();
        [Persistent] internal String VersionCheckDate_SuccessStored;
        public String VersionCheckDate_SuccessString { get { return ConvertVersionCheckDateToString(this.VersionCheckDate_Success); } }

        public override void OnEncodeToConfigNode()
        {
            VersionCheckDate_AttemptStored = VersionCheckDate_AttemptString;
            VersionCheckDate_SuccessStored = VersionCheckDate_SuccessString;
        }
        public override void OnDecodeFromConfigNode()
        {
            DateTime.TryParseExact(VersionCheckDate_AttemptStored, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out VersionCheckDate_Attempt);
            DateTime.TryParseExact(VersionCheckDate_SuccessStored, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out VersionCheckDate_Success);
        }

        #region Version Checks
        private String VersionCheckURL = "http://triggerau.github.io/TransferWindowPlanner/versioncheck.txt";
        //Could use this one to see usage, but need to be very aware of data connectivity if its ever used "http://bit.ly/TWPVersion";

        private String ConvertVersionCheckDateToString(DateTime Date)
        {
            if (Date < DateTime.Now.AddYears(-10))
                return "No Date Recorded";
            else
                return String.Format("{0:yyyy-MM-dd}", Date);
        }

        public String Version = "";
        [Persistent]
        public String VersionWeb = "";
        public Boolean VersionAvailable
        {
            get
            {
                //todo take this out
                if (this.VersionWeb == "")
                    return false;
                else
                    try
                    {
                        //if there was a string and its version is greater than the current running one then alert
                        System.Version vTest = new System.Version(this.VersionWeb);
                        return (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.CompareTo(vTest) < 0);
                    }
                    catch (Exception ex)
                    {
                        MonoBehaviourExtended.LogFormatted("webversion: '{0}'", this.VersionWeb);
                        MonoBehaviourExtended.LogFormatted("Unable to compare versions: {0}", ex.Message);
                        return false;
                    }

                //return ((this.VersionWeb != "") && (this.Version != this.VersionWeb));
            }
        }

        public String VersionCheckResult = "";


        /// <summary>
        /// Does some logic to see if a check is needed, and returns true if there is a different version
        /// </summary>
        /// <param name="ForceCheck">Ignore all logic and simply do a check</param>
        /// <returns></returns>
        public Boolean VersionCheck(Boolean ForceCheck)
        {
            Boolean blnReturn = false;
            Boolean blnDoCheck = false;

            try
            {
                if (ForceCheck)
                {
                    blnDoCheck = true;
                    MonoBehaviourExtended.LogFormatted("Starting Version Check-Forced");
                }
                //else if (this.VersionWeb == "")
                //{
                //    blnDoCheck = true;
                //    MonoBehaviourExtended.LogFormatted("Starting Version Check-No current web version stored");
                //}
                else if (this.VersionCheckDate_Attempt < DateTime.Now.AddYears(-9))
                {
                    blnDoCheck = true;
                    MonoBehaviourExtended.LogFormatted("Starting Version Check-No current date stored");
                }
                else if (this.VersionCheckDate_Attempt.Date != DateTime.Now.Date)
                {
                    blnDoCheck = true;
                    MonoBehaviourExtended.LogFormatted("Starting Version Check-stored date is not today");
                }
                else
                    MonoBehaviourExtended.LogFormatted("Skipping version check");


                if (blnDoCheck)
                {
                    //prep the background thread
                    bwVersionCheck = new BackgroundWorker();
                    bwVersionCheck.DoWork += bwVersionCheck_DoWork;
                    bwVersionCheck.RunWorkerCompleted += bwVersionCheck_RunWorkerCompleted;

                    //fire the worker
                    bwVersionCheck.RunWorkerAsync();
                }
                blnReturn = true;
            }
            catch (Exception ex)
            {
                MonoBehaviourExtended.LogFormatted("Failed to run the update test");
                MonoBehaviourExtended.LogFormatted(ex.Message);
            }
            return blnReturn;
        }

        internal Boolean VersionCheckRunning = false;
        BackgroundWorker bwVersionCheck;
        WWW wwwVersionCheck;

        void bwVersionCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            //set initial stuff and save it
            VersionCheckRunning = true;
            this.VersionCheckResult = "Unknown - check again later";
            this.VersionCheckDate_Attempt = DateTime.Now;
            this.Save();

            //now do the download
            MonoBehaviourExtended.LogFormatted("Reading version from Web");
            wwwVersionCheck = new WWW(VersionCheckURL);
            while (!wwwVersionCheck.isDone) { }
            MonoBehaviourExtended.LogFormatted("Download complete:{0}", wwwVersionCheck.text.Length);
            MonoBehaviourExtended.LogFormatted_DebugOnly("Content:{0}", wwwVersionCheck.text);
            VersionCheckRunning = false;
        }

        void bwVersionCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //get the response from the variable and work with it
                //Parse it for the version String
                String strFile = wwwVersionCheck.text;
                MonoBehaviourExtended.LogFormatted("Response Length:" + strFile.Length);

                Match matchVersion;
                matchVersion = Regex.Match(strFile, "(?<=\\|LATESTVERSION\\|).+(?=\\|LATESTVERSION\\|)", System.Text.RegularExpressions.RegexOptions.Singleline);
                MonoBehaviourExtended.LogFormatted("Got Version '" + matchVersion.ToString() + "'");

                String strVersionWeb = matchVersion.ToString();
                if (strVersionWeb != "")
                {
                    this.VersionCheckResult = "Success";
                    this.VersionCheckDate_Success = DateTime.Now;
                    this.VersionWeb = strVersionWeb;
                }
                else
                {
                    this.VersionCheckResult = "Unable to parse web service";
                }
            }
            catch (Exception ex)
            {
                MonoBehaviourExtended.LogFormatted("Failed to read Version info from web");
                MonoBehaviourExtended.LogFormatted(ex.Message);

            }
            MonoBehaviourExtended.LogFormatted("Version Check result:" + VersionCheckResult);

            this.Save();
            VersionAttentionFlag = VersionAvailable;
        }

        //public Boolean getLatestVersion()
        //{
        //    Boolean blnReturn = false;
        //    try
        //    {
        //        //Get the file from Codeplex
        //        this.VersionCheckResult = "Unknown - check again later";
        //        this.VersionCheckDate_Attempt = DateTime.Now;

        //        MonoBehaviourExtended.LogFormatted("Reading version from Web");
        //        //Page content FormatException is |LATESTVERSION|1.2.0.0|LATESTVERSION|
        //        //                WWW www = new WWW("http://kspalternateresourcepanel.codeplex.com/wikipage?title=LatestVersion");
        //        WWW www = new WWW("https://sites.google.com/site/kspalternateresourcepanel/latestversion");
        //        while (!www.isDone) { }

        //        //Parse it for the version String
        //        String strFile = www.text;
        //        MonoBehaviourExtended.LogFormatted("Response Length:" + strFile.Length);

        //        Match matchVersion;
        //        matchVersion = Regex.Match(strFile, "(?<=\\|LATESTVERSION\\|).+(?=\\|LATESTVERSION\\|)", System.Text.RegularExpressions.RegexOptions.Singleline);
        //        MonoBehaviourExtended.LogFormatted("Got Version '" + matchVersion.ToString() + "'");

        //        String strVersionWeb = matchVersion.ToString();
        //        if (strVersionWeb != "")
        //        {
        //            this.VersionCheckResult = "Success";
        //            this.VersionCheckDate_Success = DateTime.Now;
        //            this.VersionWeb = strVersionWeb;
        //            blnReturn = true;
        //        }
        //        else
        //        {
        //            this.VersionCheckResult = "Unable to parse web service";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MonoBehaviourExtended.LogFormatted("Failed to read Version info from web");
        //        MonoBehaviourExtended.LogFormatted(ex.Message);

        //    }
        //    MonoBehaviourExtended.LogFormatted("Version Check result:" + VersionCheckResult);
        //    return blnReturn;
        //}
        #endregion

    }
}
