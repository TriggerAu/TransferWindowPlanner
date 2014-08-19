using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        Double dblOrbitOriginAltitude = 100000, dblOrbitDestinationAltitude = 100000;

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
