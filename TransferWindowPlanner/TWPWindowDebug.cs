using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    public static class OrbitExtensions
    {
        //can probably be replaced with Vector3d.xzy?
        public static Vector3d SwapYZ(Vector3d v)
        {
            return v.Reorder(132);
        }

        //
        // These "Swapped" functions translate preexisting Orbit class functions into world
        // space. For some reason, Orbit class functions seem to use a coordinate system
        // in which the Y and Z coordinates are swapped.
        //
        public static Vector3d SwappedOrbitalVelocityAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getOrbitalVelocityAtUT(UT));
        }

        //position relative to the primary
        public static Vector3d SwappedRelativePositionAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getRelativePositionAtUT(UT));
        }

        //position in world space
        public static Vector3d SwappedAbsolutePositionAtUT(this Orbit o, double UT)
        {
            return o.referenceBody.position + o.SwappedRelativePositionAtUT(UT);
        }

    }
    public static class MathExtensions
    {

        public static Vector3d Reorder(this Vector3d vector, int order)
        {
            switch (order)
            {
                case 123:
                    return new Vector3d(vector.x, vector.y, vector.z);
                case 132:
                    return new Vector3d(vector.x, vector.z, vector.y);
                case 213:
                    return new Vector3d(vector.y, vector.x, vector.z);
                case 231:
                    return new Vector3d(vector.y, vector.z, vector.x);
                case 312:
                    return new Vector3d(vector.z, vector.x, vector.y);
                case 321:
                    return new Vector3d(vector.z, vector.y, vector.x);
            }
            throw new ArgumentException("Invalid order", "order");
        }
    }
    class TWPWindowDebug : MonoBehaviourWindowPlus
    {
        internal TransferWindowPlanner mbTWP;
        internal Settings settings;

        public Int32 intTest1 = 455;
        public Int32 intTest2 = 185;
        public Int32 intTest3 = 0;
        public Int32 intTest4 = 0;
        public Int32 intTest5 = 0;

        internal override void DrawWindow(int id)
        {
            try
            {
                DrawTextBox(ref intTest1);
                DrawTextBox(ref intTest2);
                DrawTextBox(ref intTest3);
                DrawTextBox(ref intTest4);
                DrawTextBox(ref intTest5);

                DrawLabel("Hello");

                if (GUILayout.Button("KSP")) SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.KSP);
                if (GUILayout.Button("UnityDef")) SkinsLibrary.SetCurrent(SkinsLibrary.DefSkinType.Unity);
                if (GUILayout.Button("Default")) SkinsLibrary.SetCurrent("Default");
                if (GUILayout.Button("Unity")) SkinsLibrary.SetCurrent("Unity");
                if (GUILayout.Button("UnityWKSPButtons")) SkinsLibrary.SetCurrent("UnityWKSPButtons");

                DrawLabel("Mouse:{0}", mbTWP.windowMain.vectMouse);
                DrawLabel("Plot:{0}", new Rect(mbTWP.windowMain.PlotPosition.x, mbTWP.windowMain.PlotPosition.y, mbTWP.windowMain.PlotWidth, mbTWP.windowMain.PlotHeight));
                DrawLabel("Selected:{0}", mbTWP.windowMain.vectSelected);
                DrawLabel("Departure:{0:0}, Travel:{1:0}", mbTWP.windowMain.DepartureSelected/KSPTime.SecondsPerDay,mbTWP.windowMain.TravelSelected/KSPTime.SecondsPerDay);

                if (mbTWP.windowMain.TransferSelected != null)
                {
                    DrawLabel("D:{0:0} T:{0:0}", mbTWP.windowMain.TransferSelected.DepartureTime, mbTWP.windowMain.TransferSelected.TravelTime);
                    DrawLabel("Origin:{0}", mbTWP.windowMain.TransferSelected.OriginVelocity);
                    DrawLabel("Initial:{0}", mbTWP.windowMain.TransferSelected.TransferInitalVelocity);
                    DrawLabel("Final:{0}", mbTWP.windowMain.TransferSelected.TransferFinalVelocity);
                    DrawLabel("Destination:{0}", mbTWP.windowMain.TransferSelected.DestinationVelocity);
                    DrawLabel("Eject:{0}", mbTWP.windowMain.TransferSelected.EjectionDeltaVector);
                    DrawLabel("Eject-Mag:{0}", mbTWP.windowMain.TransferSelected.EjectionDeltaVector.magnitude);
                    DrawLabel("Insert:{0}", mbTWP.windowMain.TransferSelected.InjectionDeltaVector);
                    DrawLabel("Insert-Mag:{0}", mbTWP.windowMain.TransferSelected.InjectionDeltaVector.magnitude);
                    DrawLabel("TransferAngle:{0}", mbTWP.windowMain.TransferSelected.TransferAngle);
                    DrawLabel("EjectionInclination:{0}", mbTWP.windowMain.TransferSelected.EjectionInclination);
                    DrawLabel("InsertionInclination:{0}", mbTWP.windowMain.TransferSelected.InsertionInclination);

                    DrawLabel("Origin VesselSpeed:{0}", mbTWP.windowMain.TransferSelected.OriginVesselOrbitalSpeed);

                    DrawLabel("EjectionDVNormal:{0}", mbTWP.windowMain.TransferSelected.EjectionDVNormal);
                    DrawLabel("EjectionDVPrograde:{0}", mbTWP.windowMain.TransferSelected.EjectionDVPrograde);
                    DrawLabel("EjectionHeading:{0}", mbTWP.windowMain.TransferSelected.EjectionHeading);
                    DrawLabel("EjectionVector:{0}", mbTWP.windowMain.TransferSelected.EjectionVector);
                    DrawLabel("EjectionAngle:{0}", mbTWP.windowMain.TransferSelected.EjectionAngle);

                }

                //DrawLabel("Padding:{0}", SkinsLibrary.CurrentSkin.window.padding);
                //DrawLabel("Margin:{0}", SkinsLibrary.CurrentSkin.window.margin);
                //DrawLabel("Border:{0}", SkinsLibrary.CurrentSkin.window.border);

                //DrawLabel("Padding:{0}", SkinsLibrary.DefKSPSkin.window.padding);
                //DrawLabel("Margin:{0}", SkinsLibrary.DefKSPSkin.window.margin);
                //DrawLabel("Border:{0}", SkinsLibrary.DefKSPSkin.window.border);

                //CelestialBody cbK = FlightGlobals.Bodies[0];
                //CelestialBody cbO = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "kerbin");
                //CelestialBody cbD = FlightGlobals.Bodies.FirstOrDefault(x => x.bodyName.ToLower() == "duna");

                ////This is the frame of Reference rotation that is occuring
                //QuaternionD Rot = Planetarium.ZupRotation;
                //DrawLabel("Rotation:{0}", Rot);

                //DrawLabel("Kerbin");
                //DrawLabel("True Anomaly:{0}", cbO.orbit.trueAnomaly);
                //DrawLabel("True Anomaly at 0:{0}", cbO.orbit.TrueAnomalyAtUT(0));
                //DrawLabel("True Anomaly at first:{0}", cbO.orbit.TrueAnomalyAtUT(intTest1));
                //DrawLabel("Velocity at first:{0}", cbO.orbit.getOrbitalVelocityAtUT(intTest1).magnitude);
                //DrawLabel("Velocity at first:{0}", cbO.orbit.getOrbitalVelocityAtUT(intTest1));
                //DrawLabel("Pos at firstT:{0}", cbO.orbit.getRelativePositionAtUT(intTest1).magnitude);

                ////We have to remove the frame of ref rotation to get static values to plot
                //DrawLabel("Pos at firstT:{0}", Quaternion.Inverse(Rot) * cbO.orbit.getRelativePositionAtUT(intTest1));
                ////DrawLabel("Pos at firstUT:{0}", cbO.orbit.getRelativePositionAtT(intTest1));

                //DrawLabel("AbsPos at firstUT:{0}", cbO.orbit.getPositionAtUT(intTest1));

                //DrawLabel("Duna");
                //DrawLabel("True Anomaly:{0}", cbD.orbit.trueAnomaly);
                //DrawLabel("True Anomaly at 0:{0}", cbD.orbit.TrueAnomalyAtUT(0));
                //DrawLabel("True Anomaly at first:{0}", cbD.orbit.TrueAnomalyAtUT(intTest1));
                //DrawLabel("Velocity at first:{0}", cbD.orbit.getOrbitalVelocityAtUT(intTest1).magnitude);
                //DrawLabel("Velocity at first:{0}", cbD.orbit.getOrbitalVelocityAtUT(intTest1));
                //DrawLabel("Pos at firstT:{0}", cbD.orbit.getRelativePositionAtUT(intTest1).magnitude);

                ////We have to remove the frame of ref rotation to get static values to plot
                //DrawLabel("Pos at firstT:{0}", Quaternion.Inverse(Rot) * cbD.orbit.getRelativePositionAtUT(intTest1));
                ////DrawLabel("Pos at firstUT:{0}", cbD.orbit.getRelativePositionAtT(intTest1));

                //DrawLabel("AbsPos at firstUT:{0}", cbD.orbit.getPositionAtUT(intTest1));


                
            }
            catch (Exception)
            {

            }

        }


    }
}
