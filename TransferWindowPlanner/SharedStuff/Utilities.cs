using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using KSPPluginFramework;

namespace TransferWindowPlanner
{
    internal static class Utilities
    {
        internal static void CopyTextToClipboard(String CopyText)
        {
            TextEditor t = new TextEditor();
            t.content = new GUIContent(CopyText);
            t.SelectAll();
            t.Copy();
        }

        public static String AppendLine(this String s, String LineToAdd, params object[] args)
        {
            if (!s.EndsWith("\r\n"))
                s += "\r\n";
            s += String.Format(LineToAdd,args);
            return s;
        }


        internal static double getEjectionAngleAtUT(this Orbit o, double UT)
        {
            //What planet is the orbit around
            CelestialBody body = o.referenceBody;

            
            //get the bodies prograde vector
            Vector3d bodyPrograde = body.orbit.getOrbitalVelocityAtUT(UT);
            //get the vessels position vector relative to the body
            Vector3d vesselPosition = o.getRelativePositionAtUT(UT);
            //now get the angle between em
            double returnEjectAngle = ((Math.Atan2(bodyPrograde.y, bodyPrograde.x) - Math.Atan2(vesselPosition.y, vesselPosition.x)) * 180.0 / Math.PI);
            
            //clamp to 360

            return returnEjectAngle.NormalizeAngle360();
        }

        internal static double timeOfEjectionAngle(Orbit orbitVessel, double timeStart, double angleEjection, double numDivisions)
        {
            //Min and max is back and forward half an orbit
            double minTime = timeStart - orbitVessel.period / 2;
            double maxTime = timeStart + orbitVessel.period;

            double returnTime = minTime;

            //iterate it 8 times
            for (int iter = 0; iter < 8; iter++) {
                double dt = (maxTime - minTime) / numDivisions;
                for (int i = 0; i < numDivisions; i++) {
                    //for each division work out the max and min ejection angles
                    double t1 = minTime + i * dt;
                    double t2 = minTime + (i + 1) * dt;

                    double ejectT1 =  orbitVessel.getEjectionAngleAtUT(t1);
                    double ejectT2 = orbitVessel.getEjectionAngleAtUT(t2);

                    //what to do if the ejection angle wraps the 0 degree line
                    dasdas

                    if ( (ejectT1 < angleEjection && angleEjection <= ejectT2) ||
                        (ejectT1 > angleEjection && angleEjection >= ejectT2)) {

                        minTime = t1;
                        maxTime = t2;
                        break;
                    }
                }
                returnTime = minTime;
            }

            return returnTime;
        }
    }
}
