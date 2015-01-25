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

#if DEBUG
        internal void RunPlot()
        {
            ddlOrigin.SelectedIndex = 2;
            SetupDestinationControls();

            ddlDestination.SelectedIndex = 2;
            SetupTransferParams();

            strArrivalAltitude = "0";
            strDepartureAltitude = "0";
            dateMinDeparture = new KSPDateTime(1, 1);
            dateMaxDeparture = new KSPDateTime(mbTWP.windowDebug.intTest4, 1);
            strTravelMinDays = "150";
            strTravelMaxDays = "450";


            mbTWP.windowSettings.Visible = false;
            StartLongWorker();
            WindowRect.height = 400;
            ShowEjectionDetails = false;

        }
#endif

        private void StartLongWorker()
        {

            Double TravelRange = (new KSPTimeSpan(strTravelMaxDays, "0", "0", "0") - new KSPTimeSpan(strTravelMinDays, "0", "0", "0")).UT;
            Double DepartureRange = (dateMaxDeparture - dateMinDeparture).UT;
            PlotWidth = (Int32)(DepartureRange / KSPDateStructure.SecondsPerDay * mbTWP.windowDebug.intPlotDeparturePerDay) + 1;
            PlotHeight = (Int32)(TravelRange / KSPDateStructure.SecondsPerDay * mbTWP.windowDebug.intPlotTravelPointsPerDay) + 1;

            LogFormatted("Starting a LongWorker: {0}->{1}: Res={2}x{3}", cbOrigin.bodyName, cbDestination.bodyName, PlotWidth, PlotHeight);
            
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

            LogFormatted("Completed a LongWorker: {0}->{1}: Res={2}x{3}", cbOrigin.bodyName, cbDestination.bodyName, PlotWidth, PlotHeight);
        }

        void bw_GenerateDataPorkchop(object sender, DoWorkEventArgs e)
        {
            try
            {
                //Loop through getting the DeltaV's and assigning em all to an array
                Int32 iCurrent = 0;

                ////////need to make sure this bombing out cause file is locked doesnt stop process :)
                String strCSVLine = "", strCSVLine2 = "";
                Boolean blnCSVTransferFirst = true;
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVTravelWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVTravelWorking-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName));
                }
                catch (Exception ex) { LogFormatted("Unable to delete file:{0}", ex.Message); }
                try
                {
                    if (System.IO.File.Exists(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName)))
                        System.IO.File.Delete(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName));
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
                        System.IO.File.AppendAllText(String.Format("{0}/DeltaVDaily-{1}-{2}.csv", Resources.PathPlugin, cbOrigin.bodyName, cbDestination.bodyName),
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
