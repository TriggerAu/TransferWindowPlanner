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
    public partial class TWPWindow
    {
        List<cbItem> lstPlanets = new List<cbItem>();
        List<cbItem> lstDestinations = new List<cbItem>();

        CelestialBody cbOrigin, cbDestination;
        CelestialBody cbReference;

        Double hohmannTransferTime,synodicPeriod;
        Double DepartureMin,DepartureMax,DepartureRange;
        Double TravelMin, TravelMax, TravelRange;

        Double InitialOrbitAltitude = 100000, FinalOrbitAltitude = 100000;

        void SetupDestinationControls()
        {
            LogFormatted_DebugOnly("Setting Destination Controls");
            cbOrigin = lstBodies.First(x => x.Name == ddlOrigin.SelectedValue.Trim(' ')).CB;
            LogFormatted_DebugOnly("Origin:{0}",cbOrigin.bodyName);
            cbReference = cbOrigin.referenceBody;
            LogFormatted_DebugOnly("Reference:{0}", cbReference.bodyName);

            BuildListOfDestinations();

            LogFormatted_DebugOnly("Updating DropDown List");
            ddlDestination.SelectedIndex = 0;
            ddlDestination.Items = lstDestinations.Select(x => x.Name).ToList();

            SetupTransferParams();
        }

        void SetupTransferParams()
        {
            LogFormatted_DebugOnly("Running Maths for Default values for this transfer");
            cbDestination = lstBodies.First(x => x.Name == ddlDestination.SelectedValue.Trim(' ')).CB;
            LogFormatted_DebugOnly("Destination:{0}", cbOrigin.bodyName);

            //work out the synodic period and a reasonable range from the min to max - ie x axis
            synodicPeriod = Math.Abs(1 / (1 / cbDestination.orbit.period - 1 / cbOrigin.orbit.period));
            DepartureRange = Math.Min(2 * synodicPeriod, 2 * cbOrigin.orbit.period);

            //Set this to yesterday
            //DepartureMin = 0;
            DepartureMax = DepartureMin + DepartureRange;

            //Work out the time necessary for a hohmann transfer between the two orbits
            hohmannTransferTime = LambertSolver.HohmannTimeOfFlight(cbOrigin.orbit, cbDestination.orbit);
            //Set some reasonable defaults for the travel time range - ie y-axis
            TravelMin = Math.Max(hohmannTransferTime - cbDestination.orbit.period, hohmannTransferTime / 2);
            TravelMax = TravelMin + Math.Min(2 * cbDestination.orbit.period, hohmannTransferTime);

            SetWindowStrings();
        }

        private void SetWindowStrings()
        {
            //KSPDateTime kTime = new KSPDateTime(DepartureMin);
            //strDepartureMinYear = (kTime.Year ).ToString();
            //strDepartureMinDay = (kTime.Day).ToString();

            //kTime.UT = DepartureMax;
            //strDepartureMaxYear = (kTime.Year).ToString();
            //strDepartureMaxDay = (kTime.Day).ToString();

            dateMinDeparture = new KSPDateTime(DepartureMin);
            dateMaxDeparture = new KSPDateTime(DepartureMax);

            KSPTimeSpan kSpan = new KSPTimeSpan(0);
            kSpan.UT = TravelMin;
            strTravelMinDays = ((Int32)kSpan.TotalDays).ToString();

            kSpan.UT = TravelMax;
            strTravelMaxDays = ((Int32)kSpan.TotalDays).ToString();

            strArrivalAltitude = (InitialOrbitAltitude / 1000).ToString();
            strDepartureAltitude = (FinalOrbitAltitude / 1000).ToString();
        }

        internal Boolean Running = false;
        internal Boolean Done = false;
        internal Boolean TextureReadyToDraw = false;
        internal Double workingpercent = 0;

        internal BackgroundWorker bw;

        public class TransferWorkerDetails
        {
            public Double DepartureMin { get; set; }
            public Double DepartureMax { get; set; }
            public Double DepartureRange { get; set; }
            public Double TravelMin { get; set; }
            public Double TravelMax { get; set; }
            public Double TravelRange { get; set; }
            public Double InitialOrbitAltitude { get; set; }
            public Double FinalOrbitAltitude { get; set; }
            public String OriginName { get; set; }
            public String DestinationName { get; set; }
        }


        internal TransferWorkerDetails TransferSpecs;

        private void SetWorkerVariables()
        {
            DepartureMin = dateMinDeparture.UT; // new KSPDateTime(strDepartureMinYear, strDepartureMinDay).UT; // new KSPDateTime(strDepartureMinYear, strDepartureMinDay, "0", "0", "0").UT - KSPDateStructure.SecondsPerYear - KSPDateStructure.SecondsPerDay;
            DepartureMax = dateMaxDeparture.UT; // new KSPDateTime(strDepartureMaxYear, strDepartureMaxDay).UT; // new KSPDateTime(strDepartureMaxYear, strDepartureMaxDay, "0", "0", "0").UT - KSPDateStructure.SecondsPerYear - KSPDateStructure.SecondsPerDay;
            DepartureRange = DepartureMax - DepartureMin;
            DepartureSelected = -1;
            TravelMin = new KSPTimeSpan(strTravelMinDays, "0", "0", "0").UT;
            TravelMax = new KSPTimeSpan(strTravelMaxDays, "0", "0", "0").UT;
            TravelRange = TravelMax - TravelMin;
            TravelSelected = -1;
            InitialOrbitAltitude = Convert.ToDouble(strDepartureAltitude) * 1000;
            if (blnFlyby)
                FinalOrbitAltitude = 0;
            else
                FinalOrbitAltitude = Convert.ToDouble(strArrivalAltitude) * 1000;

            //Store the transfer Specs for display purposes
            TransferSpecs = new TransferWorkerDetails();
            TransferSpecs.DepartureMin = DepartureMin;
            TransferSpecs.DepartureMax = DepartureMax;
            TransferSpecs.DepartureRange = DepartureRange;
            TransferSpecs.TravelMin = TravelMin;
            TransferSpecs.TravelMax = TravelMax;
            TransferSpecs.TravelRange = TravelRange;
            TransferSpecs.InitialOrbitAltitude = InitialOrbitAltitude;
            TransferSpecs.FinalOrbitAltitude = FinalOrbitAltitude;
            TransferSpecs.OriginName = cbOrigin.bodyName;
            TransferSpecs.DestinationName = cbDestination.bodyName;
            
            // minus 1 so when we loop from for PlotX pixels the last pixel is the actual last value
            xResolution = DepartureRange / (PlotWidth - 1);
            yResolution = TravelRange / (PlotHeight - 1);

            DeltaVs = new Double[PlotWidth * PlotHeight];
            DeltaVsColorIndex = new Int32[PlotWidth * PlotHeight];
        }

        internal Boolean dVDataOnly = false;
        private void StartWorker(Boolean NoTextureGen = false)
        {
            SetWorkerVariables();

            dVDataOnly = NoTextureGen;
            if (!dVDataOnly)
            {
                PlotHeight = 292;
                PlotWidth = 292;
            }

            workingpercent = 0;
            Running = true;
            Done = false;
            bw = new BackgroundWorker();
            bw.DoWork += bw_GeneratePorkchop;
            bw.RunWorkerCompleted += bw_GeneratePorkchopCompleted;

            bw.RunWorkerAsync();
        }


        void bw_GeneratePorkchopCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Running = false;
            Done = true;

            if (!dVDataOnly)
                TextureReadyToDraw = true;
        }

        internal Int32 PlotWidth = 292, PlotHeight = 292;
        Double xResolution, yResolution;

        Double[] DeltaVs;
        Int32[] DeltaVsColorIndex;

        Texture2D texPlotArea=null, texDeltaVPalette=null;
        List<Color> DeltaVColorPalette = null;

        internal Double logDeltaV, sumlogDeltaV, sumSqLogDeltaV;
        internal Double maxDeltaV, minDeltaV;

        Double logMinDeltaV, logMaxDeltaV;

        Vector2 minDeltaVPoint;

        void bw_GeneratePorkchop(object sender, DoWorkEventArgs e)
        {
            try
            {
                sumlogDeltaV = 0; sumSqLogDeltaV = 0;
                maxDeltaV = 0; minDeltaV = Double.MaxValue;

                //Loop through getting the DeltaV's and assigning em all to an array
                Int32 iCurrent = 0;

#if DEBUG
                ////////need to make sure this bombing out cause file is locked doesnt stop process :)
                String strCSVLine = "", strCSVLine2 = "";
                Boolean blnCSVTransferFirst = true;
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVWorking.csv", Resources.PathPlugin)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVWorking.csv", Resources.PathPlugin));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVTravelWorking.csv", Resources.PathPlugin)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVTravelWorking.csv", Resources.PathPlugin));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
#endif
                LogFormatted("Generating DeltaV Values");
                for (int x = 0; x < PlotWidth; x++)
                {
#if DEBUG
                    strCSVLine = "";
#endif
                    TransferDetails transferDailyBest = new TransferDetails();
                    Double transferDailyBestDV = Double.MaxValue;
                    //LogFormatted("{0:0.000}-{1}", workingpercent, iCurrent);
                    for (int y = 0; y < PlotHeight; y++)
                    {
                        //have to keep this this way so the texture draws the right way around
                        iCurrent = (int)(y * PlotWidth + x);

                        TransferDetails transferTemp;

                        //Set the Value for this position to be the DeltaV of this Transfer
                        DeltaVs[iCurrent] = LambertSolver.TransferDeltaV(cbOrigin, cbDestination,
                            DepartureMin + ((Double)x * xResolution), TravelMax - ((Double)y * yResolution),
                            InitialOrbitAltitude, FinalOrbitAltitude, out transferTemp);

                        //LogFormatted("dt: {0}  TT:{1}", TravelMax - ((Double)y * yResolution), transferTemp.TravelTime);
#if DEBUG
                        strCSVLine += String.Format("{0:0.00},", DeltaVs[iCurrent]);
                        if (blnCSVTransferFirst)
                            strCSVLine2 += String.Format("{0:0.00},", transferTemp.TravelTime);

                        if (transferTemp.DVTotal < transferDailyBestDV)
                        {
                            transferDailyBest = transferTemp;
                            transferDailyBestDV = transferTemp.DVTotal;
                        }
#endif
                        /////////////// Long Running ////////////////////////////
                        //LogFormatted("{0}x{1} ({3}) = {2:0}", x, y, DeltaVs[iCurrent],iCurrent);

                        if (DeltaVs[iCurrent] > maxDeltaV)
                            maxDeltaV = DeltaVs[iCurrent];
                        if (DeltaVs[iCurrent] < minDeltaV)
                        {
                            minDeltaV = DeltaVs[iCurrent];
                            minDeltaVPoint = new Vector2(x, y);
                        }

                        logDeltaV = Math.Log(DeltaVs[iCurrent]);
                        sumlogDeltaV += logDeltaV;
                        sumSqLogDeltaV += logDeltaV * logDeltaV;

                        workingpercent = (x * PlotHeight + y) / (Double)(PlotHeight * PlotWidth);
                    }

#if DEBUG
                    try
                    {
                        System.IO.File.AppendAllText(String.Format("{0}/DeltaVWorking.csv", Resources.PathPlugin), strCSVLine.TrimEnd(',') + "\r\n");
                    }
                    catch (Exception) { }
                    try
                    {
                        if (blnCSVTransferFirst)
                        {
                            System.IO.File.AppendAllText(String.Format("{0}/DeltaVTravelWorking.csv", Resources.PathPlugin), strCSVLine2.TrimEnd(',') + "\r\n");
                            blnCSVTransferFirst = false;
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        System.IO.File.AppendAllText(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName),
                            String.Format("{0:0.00},{1:0.00},{2:0.00},\"{3}\",\"{4}\"\r\n", transferDailyBest.DepartureTime, transferDailyBest.DVTotal, transferDailyBest.TravelTime, new KSPDateTime(transferDailyBest.DepartureTime).ToStringStandard(DateStringFormatsEnum.KSPFormat), new KSPTimeSpan(transferDailyBest.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLong)));
                    }
                    catch (Exception) { }
#endif
                }

                if (dVDataOnly)
                {
                    return;
                }

                Double mean, stddev;
                //Calculate the ColorIndex for the plot - BUT DONT AFFECT TEXTURES ON THE BW THREAD
                LogFormatted("Working out Log Values to determine DeltaV->Color Mapping");

                /////////////// Long Running ////////////////////////////
                //LogFormatted("DVMin: {0} - DVLength:{1}", DeltaVs.Min(), DeltaVs.Length);
                //LogFormatted(String.Join(",", DeltaVs.Select(v=>v.ToString()).ToArray()));

                logMinDeltaV = Math.Log(DeltaVs.Min());
                mean = sumlogDeltaV / DeltaVs.Length;
                stddev = Math.Sqrt(sumSqLogDeltaV / DeltaVs.Length - mean * mean);
                logMaxDeltaV = Math.Min(Math.Log(maxDeltaV), mean + 2 * stddev);

                /////////////// Long Running ////////////////////////////
                //LogFormatted("logmin:{0}, LogMax:{1}", logMinDeltaV, logMaxDeltaV);

                if (DeltaVColorPalette == null)
                    GenerateDeltaVPalette();

                LogFormatted("Placing ColorIndexes in array");
                for (int y = 0; y < PlotHeight; y++)
                {
                    for (int x = 0; x < PlotWidth; x++)
                    {
                        iCurrent = (Int32)(y * PlotWidth + x);
                        logDeltaV = Math.Log(DeltaVs[iCurrent]);
                        double relativeDeltaV = (logDeltaV - logMinDeltaV) / (logMaxDeltaV - logMinDeltaV);
                        Int32 ColorIndex = Math.Min((Int32)(Math.Floor(relativeDeltaV * DeltaVColorPalette.Count)), DeltaVColorPalette.Count - 1);

                        DeltaVsColorIndex[iCurrent] = ColorIndex;
                    }
                }

                //Set the Best Transfer
                vectSelected = new Vector2(PlotPosition.x + minDeltaVPoint.x, PlotPosition.y + minDeltaVPoint.y);
                SetTransferDetails();
            }
            catch (Exception ex)
            {
                LogFormatted("ERROR: Background Worker Failed\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }

        private void SetTransferDetails()
        {
            DepartureSelected = DepartureMin + (vectSelected.x - PlotPosition.x) * xResolution;
            TravelSelected = TravelMax - (vectSelected.y - PlotPosition.y) * yResolution;

            LambertSolver.TransferDeltaV(cbOrigin, cbDestination, DepartureSelected, TravelSelected, InitialOrbitAltitude, FinalOrbitAltitude, out TransferSelected);
            TransferSelected.CalcEjectionValues();
        }

        private void DrawPlotTexture(Double sumlogDeltaV, Double sumSqLogDeltaV, Double maxDeltaV)
        {
            //Ensure we have a palette of colors to draw the porkchop
            if (texDeltaVPalette == null)
                GenerateDeltaVTexture();

            //Now Draw the texture
            LogFormatted("Placing Colors on texture-{0}x{1}", PlotWidth, PlotHeight);
            Int32 iCurrent = 0;
            Int32 ColorIndex = 0;
            /////////////// Long Running ////////////////////////////
            //String Line = "A";
            try
            {

            texPlotArea = new Texture2D(PlotWidth, PlotHeight, TextureFormat.ARGB32, false);
            for (int y = 0; y < PlotHeight; y++) {
                for (int x = 0; x < PlotWidth; x++) {
                    //Line = x.ToString() + "-" + y.ToString() + "-A";
                    iCurrent = (Int32)(y * PlotWidth + x);
                    //Line += "A";
                    ColorIndex = DeltaVsColorIndex[iCurrent];
                    //Line += "A";
                    //Data flows from left->right and top->bottom so need to reverse y (and cater to 0 based) when drawing the texture
                    texPlotArea.SetPixel(x, (PlotHeight - 1 - y), DeltaVColorPalette[ColorIndex]);
                    //Line += "A";
                }
            }
            }
            catch (Exception ex)
            {
                //LogFormatted("Value:{0} - ColIndex:{1} - Length:{2} - Line:{3}", iCurrent, ColorIndex, DeltaVsColorIndex.Length, Line);
                LogFormatted(ex.Message);
                //LogFormatted("DVColPalLength:{0}", DeltaVColorPalette.Count);
            }

#if DEBUG
            Byte[] PNG = texPlotArea.EncodeToPNG();
            System.IO.File.WriteAllBytes(String.Format("{0}/DeltaVWorkingPlot.png", Resources.PathPlugin), PNG);
#endif
            texPlotArea.Apply();
        }

        private void GenerateDeltaVPalette()
        {
            LogFormatted("Generating DeltaV Color Palette");
            DeltaVColorPalette = new List<Color>();
            for (int i = 64; i <= 68; i++)
                DeltaVColorPalette.Add(new Color32(64, (byte)i, 255, 255));
            for (int i = 133; i <= 255; i++)
                DeltaVColorPalette.Add(new Color32(128, (byte)i, 255, 255));
            for (int i = 255; i >= 128; i--)
                DeltaVColorPalette.Add(new Color32(128, 255, (byte)i, 255));
            for (int i = 128; i <= 255; i++)
                DeltaVColorPalette.Add(new Color32((byte)i, 255, 128, 255));
            for (int i = 255; i >= 128; i--)
                DeltaVColorPalette.Add(new Color32(255, (byte)i, 128, 255));

        }

        private void GenerateDeltaVTexture()
        {
            texDeltaVPalette = new Texture2D(1, 512);
            for (int i = 0; i < DeltaVColorPalette.Count; i++)
            {
                texDeltaVPalette.SetPixel(1, i, DeltaVColorPalette[i]);
            }
            texDeltaVPalette.Apply();

            Styles.stylePlotLegendImage.normal.background = texDeltaVPalette;

#if DEBUG
            Byte[] PNG = texDeltaVPalette.EncodeToPNG();
            System.IO.File.WriteAllBytes(String.Format("{0}/DeltaVPalette.png", Resources.PathPlugin), PNG);
#endif
        }

        #region CelestialBody List Stuff
        private void BuildListOfDestinations()
        {
            LogFormatted_DebugOnly("Setting Destination List. Origin:{0}", cbOrigin.bodyName);
            lstDestinations = new List<cbItem>();
            foreach (cbItem item in lstBodies.Where(x => (x.Parent == cbOrigin.referenceBody && x.CB != cbOrigin)))
            {
                if (item.CB != cbStar)
                {
                    LogFormatted_DebugOnly("Adding Dest:{0}", item.Name);
                    lstDestinations.Add(item);
                }
            } 
        }


        private void BodyParseChildren(CelestialBody cbRoot, Int32 Depth = 0)
        {
            foreach (cbItem item in lstBodies.Where(x => x.Parent == cbRoot).OrderBy(x => x.SemiMajorRadius))
            {
                item.Depth = Depth;
                if (item.CB != cbStar)
                {
                    lstPlanets.Add(item);
                    if (lstBodies.Where(x => x.Parent == item.CB).Count() > 1)
                    {
                        BodyParseChildren(item.CB, Depth + 1);
                    }
                }
            }
        }

        internal class cbItem
        {
            internal cbItem(CelestialBody CB)
            {
                this.CB = CB;
                if (CB.referenceBody != CB)
                    this.SemiMajorRadius = CB.orbit.semiMajorAxis;
            }

            internal CelestialBody CB { get; private set; }
            internal Int32 Depth = 0;
            internal String Name { get { return CB.bodyName; } }
            internal String NameFormatted { get { return new String(' ', Depth * 4) + Name; } }
            internal Double SemiMajorRadius { get; private set; }
            internal CelestialBody Parent { get { return CB.referenceBody; } }
        }
        #endregion
    }
}
