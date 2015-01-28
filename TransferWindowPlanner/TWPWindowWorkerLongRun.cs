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
#if DEBUG


        class PlotAutomate
        {
            public Int32 DepMinYear { get; set; }
            public Int32 DepMaxYear { get; set; }
            public String TravelMin { get; set; }
            public String TravelMax { get; set; }
            public Int32 Source { get; set; }
            public Int32 Destination { get; set; }

        }


        List<PlotAutomate> lstPlots = new List<PlotAutomate>()
        {
            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="150",TravelMax="600",Source=2,Destination=2},

            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="150",TravelMax="600",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="150",TravelMax="600",Source=5,Destination=0},

            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="50",TravelMax="250",Source=2,Destination=0},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="50",TravelMax="250",Source=2,Destination=0},

            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="50",TravelMax="250",Source=0,Destination=0},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="50",TravelMax="250",Source=0,Destination=0},

            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="100",TravelMax="400",Source=5,Destination=0},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="100",TravelMax="400",Source=5,Destination=0},

            new PlotAutomate() {DepMinYear=1,DepMaxYear=21,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=21,DepMaxYear=41,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=41,DepMaxYear=61,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=61,DepMaxYear=81,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=81,DepMaxYear=101,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=101,DepMaxYear=121,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=121,DepMaxYear=141,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=141,DepMaxYear=161,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=161,DepMaxYear=181,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=181,DepMaxYear=201,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=201,DepMaxYear=221,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=221,DepMaxYear=241,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=241,DepMaxYear=261,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=261,DepMaxYear=281,TravelMin="100",TravelMax="400",Source=0,Destination=2},
            new PlotAutomate() {DepMinYear=281,DepMaxYear=301,TravelMin="100",TravelMax="400",Source=0,Destination=2},

            //new PlotAutomate() {DepMinYear=1,DepMaxYear=2,TravelMin="150",TravelMax="600",Source=2,Destination=2},
            //new PlotAutomate() {DepMinYear=2,DepMaxYear=3,TravelMin="150",TravelMax="600",Source=3,Destination=2},
            //new PlotAutomate() {DepMinYear=3,DepMaxYear=4,TravelMin="150",TravelMax="600",Source=2,Destination=2}

        };


        Int32 CurrentPlot = 0;
        internal void RunPlots()
        {
            CurrentPlot = 0;

            mbTWP.windowSettings.Visible = false;
            WindowRect.height = 400;
            ShowEjectionDetails = false;

            LogFormatted("Starting new Run of {0} Plots", lstPlots.Count);

            StartLongWorker();
        }

        private void StartLongWorker()
        {
            if (CurrentPlot >= lstPlots.Count)
            {
                LogFormatted("No more plots to run");
                Running = false;
                Done = true;
                return;
            }

            LogFormatted("Setting up plot:{0}", CurrentPlot);
            ddlOrigin.SelectedIndex = lstPlots[CurrentPlot].Source;
            SetupDestinationControls();

            ddlDestination.SelectedIndex = lstPlots[CurrentPlot].Destination;
            SetupTransferParams();

            strArrivalAltitude = "0";
            strDepartureAltitude = "0";
            dateMinDeparture = new KSPDateTime(lstPlots[CurrentPlot].DepMinYear, 1,3,0,0,0);
            dateMaxDeparture = new KSPDateTime(lstPlots[CurrentPlot].DepMaxYear, 1,3,0,0,0).AddDays(-1);
            strTravelMinDays = lstPlots[CurrentPlot].TravelMin;
            strTravelMaxDays = lstPlots[CurrentPlot].TravelMax;


            Double TravelRange = (new KSPTimeSpan(strTravelMaxDays, "0", "0", "0") - new KSPTimeSpan(strTravelMinDays, "0", "0", "0")).UT;
            Double DepartureRange = (dateMaxDeparture - dateMinDeparture).UT;
            PlotWidth = (Int32)(DepartureRange / KSPDateStructure.SecondsPerDay * mbTWP.windowDebug.intPlotDeparturePerDay) + 1;
            PlotHeight = (Int32)(TravelRange / KSPDateStructure.SecondsPerDay * mbTWP.windowDebug.intPlotTravelPointsPerDay) + 1;

            LogFormatted("Starting a LongWorker: {0}->{1}, Depart:Year {2}=>Year {3}, Travel:{4}=>{5}", 
                cbOrigin.bodyName, cbDestination.bodyName, 
                lstPlots[CurrentPlot].DepMinYear, lstPlots[CurrentPlot].DepMaxYear, 
                lstPlots[CurrentPlot].TravelMin, lstPlots[CurrentPlot].TravelMax);
            
            SetWorkerVariables();

            workingpercent = 0;
            Running = true;
            Done = false;
            bw = new BackgroundWorker();
            bw.DoWork += bw_GenerateDataPorkchop;
            bw.RunWorkerCompleted += bw_GenerateDataPorkchopCompleted;

            bw.RunWorkerAsync();
        }


        void bw_GenerateDataPorkchopCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Running = false;
            Done = true;

            LogFormatted("Completed a LongWorker: {0}->{1}, Depart:Year {2}=>Year {3}, Travel:{4}=>{5}", cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear, lstPlots[CurrentPlot].DepMaxYear, lstPlots[CurrentPlot].TravelMin, lstPlots[CurrentPlot].TravelMax);

            CurrentPlot++;
            //loop for the next one
            StartLongWorker();
        }

        void bw_GenerateDataPorkchop(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Loop through getting the DeltaV's and assigning em all to an array
                Int32 iCurrent = 0;

                ////////need to make sure this bombing out cause file is locked doesnt stop process :)
                //String strCSVLine = "", strCSVLine2 = "";
                Boolean blnCSVTransferFirst = true;
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVWorking-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVWorking-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVTravelWorking-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVTravelWorking-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVDaily-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVDaily-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }

                LogFormatted("Generating DeltaV Values");
                for (int x = 0; x < PlotWidth; x++)
                {
                    //strCSVLine = "";

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

                        //strCSVLine += String.Format("{0:0.00},", DeltaVs[iCurrent]);
                        //if (blnCSVTransferFirst)
                        //    strCSVLine2 += String.Format("{0:0.00},", transferTemp.TravelTime);

                        if (transferTemp.DVTotal < transferDailyBestDV)
                        {
                            transferDailyBest = transferTemp;
                            transferDailyBestDV = transferTemp.DVTotal;
                        }

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


                    try
                    {
                        //System.IO.File.AppendAllText(String.Format("{0}/DeltaVWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName), strCSVLine.TrimEnd(',') + "\r\n");
                    }
                    catch (Exception) { }
                    try
                    {
                        if (blnCSVTransferFirst)
                        {
                            //System.IO.File.AppendAllText(String.Format("{0}/DeltaVTravelWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName), strCSVLine2.TrimEnd(',') + "\r\n");
                            blnCSVTransferFirst = false;
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        System.IO.File.AppendAllText(String.Format("{0}/DeltaVDaily-{1}-{2}-{3}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName, lstPlots[CurrentPlot].DepMinYear),
                            //String.Format("{0:0.00},{1:0.00},{2:0.00}\r\n", transferDailyBest.DepartureTime, transferDailyBest.DVTotal, transferDailyBest.TravelTime));
                            String.Format("{0:0.00},{1:0.00},{2:0.00},\"{3}\",\"{4}\"\r\n", transferDailyBest.DepartureTime, transferDailyBest.DVTotal, transferDailyBest.TravelTime, new KSPDateTime(transferDailyBest.DepartureTime).ToStringStandard(DateStringFormatsEnum.KSPFormat), new KSPTimeSpan(transferDailyBest.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLong)));
                    }
                    catch (Exception) { }
                }
               
            }
            catch (Exception ex)
            {
                LogFormatted("ERROR: Background Worker Failed\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }



#endif

    }
}
