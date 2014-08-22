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
    internal partial class TWPWindow
    {
        List<cbItem> lstPlanets = new List<cbItem>();
        List<cbItem> lstDestinations = new List<cbItem>();

        CelestialBody cbOrigin, cbDestination;
        CelestialBody cbReference;

        Double hohmannTransferTime,synodicPeriod;
        Double DepartureMin,DepartureMax,DepartureRange;
        Double TravelMin, TravelMax, TravelRange;

        Double InitialOrbitAltitude = 100000, FinalOrbitAltitide = 100000;

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

            DepartureMin = 0;
            DepartureMax = DepartureMin + DepartureRange;

            //Work out the time necessary for a hohmann transfer between the two orbits
            hohmannTransferTime = LambertSolver.HohmannTimeOfFlight(cbOrigin.orbit, cbDestination.orbit);
            //Set some reasonable defaults for the travel time range - ie y-axis
            TravelMin = Math.Max(hohmannTransferTime - cbDestination.orbit.period, hohmannTransferTime / 2);
            TravelMax = TravelMin + Math.Min(2 * cbDestination.orbit.period, hohmannTransferTime);

            SetWindowStrings();
        }

        void bw_GeneratePorkchopCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Running = false;
            Done = true;
        }

        Texture2D texPlotArea=null, texDeltaVPalette=null;
        List<Color> DeltaVColorPalette = null;

        Double logMinDeltaV, logMaxDeltaV;
        void bw_GeneratePorkchop(object sender, DoWorkEventArgs e)
        {
            Double logDeltaV, sumlogDeltaV = 0, sumSqLogDeltaV = 0;
            Double maxDeltaV = 0, minDeltaV = Double.MaxValue;

            //Loop through getting the DeltaV's and assigning em all to an array
            Int32 iCurrent = 0;
            LogFormatted("Generating DeltaV Values");
            for (int y = 0; y < PlotHeight; y++)
            {
                //LogFormatted("{0:0.000}-{1}", workingpercent, iCurrent);
                for (int x = 0; x < PlotWidth; x++)
                {
                    iCurrent = (int)(y * PlotHeight + x);

                    //Set the Value for this position to be the DeltaV of this Transfer
                    DeltaVs[iCurrent] = LambertSolver.TransferDeltaV(cbOrigin, cbDestination, 
                        DepartureMin + ((Double)x * xResolution), TravelMin + ((Double)y * yResolution), 
                        InitialOrbitAltitude, FinalOrbitAltitide);

                    if (DeltaVs[iCurrent] > maxDeltaV)
                        maxDeltaV = DeltaVs[iCurrent];
                    if (DeltaVs[iCurrent] < minDeltaV)
                        minDeltaV = DeltaVs[iCurrent];

                    logDeltaV = Math.Log(DeltaVs[iCurrent]);
                    sumlogDeltaV += logDeltaV;
                    sumSqLogDeltaV += logDeltaV * logDeltaV;

                    workingpercent = (Double)iCurrent / (Double)(PlotHeight * PlotWidth);
                }
            }

            String File = "";
            for (int y = 0; y < PlotHeight; y++)
            {
                String strline = "";
                for (int x = 0; x < PlotWidth; x++)
                {
                    iCurrent = (int)(y * PlotHeight + x);
                    strline += String.Format("{0:0},", DeltaVs[iCurrent]);
                }
                File += strline + "\r\n";
            }

#if DEBUG
            System.IO.File.WriteAllText(String.Format("{0}/DeltaVWorking.csv",Resources.PathPlugin), File);
#endif

            //Ensure we have a palette of colors to draw the porkchop
            if (texDeltaVPalette == null || DeltaVColorPalette==null)
                GenerateDeltaVPalette();
            
            //Now Draw the texture
            LogFormatted("Working out Log Values to determine DeltaV->Color Mapping");
            logMinDeltaV = Math.Log(DeltaVs.Min());
            Double mean = sumlogDeltaV / DeltaVs.Length;
            Double stddev = Math.Sqrt(sumSqLogDeltaV / DeltaVs.Length - mean * mean);
            logMaxDeltaV = Math.Min(Math.Log(maxDeltaV), mean + 2 * stddev);

            LogFormatted("Placing Colors on texture");
            texPlotArea = new Texture2D(PlotWidth, PlotHeight, TextureFormat.ARGB32, false);
            for (int y = 0; y < PlotHeight; y++)
            {
                for (int x = 0; x < PlotWidth; x++)
                {
                    iCurrent = (int)(y * PlotHeight + x);
                    logDeltaV = Math.Log(DeltaVs[iCurrent]);
                    double relativeDeltaV = (logDeltaV - logMinDeltaV) / (logMaxDeltaV - logMinDeltaV);
                    Int32 ColorIndex = Math.Min((Int32)(Math.Floor(relativeDeltaV * DeltaVColorPalette.Count)), DeltaVColorPalette.Count - 1);
                    texPlotArea.SetPixel(x, y, DeltaVColorPalette[ColorIndex]);
                }
            }
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
