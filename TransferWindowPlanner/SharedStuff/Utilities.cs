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

        /// <summary>
        /// Return the ejection angle of the vessel/body on this orbit for a given UT
        /// </summary>
        /// <param name="o"></param>
        /// <param name="UT">Te UT at which to calculate the angle</param>
        /// <returns></returns>
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

        /// <summary>
        /// Find the point on the orbit that includes the initial time where the ejection angle is closest to the suplied one
        /// </summary>
        /// <param name="oObject">Orbit of the vessel/body</param>
        /// <param name="timeInitial">The UT you want to search around for the angle - will search 1/2 an orbit back and 1/2 forward</param>
        /// <param name="numDivisions">Higher thisnumber the more precise the answer - and the longer it will take</param>
        /// <param name="closestAngle">The output of the closest angle the method could find</param>
        /// <param name="targetAngle">The ejection angle we are looking for</param>
        /// <returns></returns>
        internal static double timeOfEjectionAngle(Orbit oObject, double timeInitial, double targetAngle, double numDivisions, out double closestAngle)
        {
            double timeStart = timeInitial;
            double periodtoscan = oObject.period;
            
            double closestAngleTime = timeStart;
            double closestAngleValue = 361;
            double minTime = timeStart;
            double maxTime = timeStart + periodtoscan;

            //work out iterations for precision - we only really need to within a second - so how many iterations do we actually need
            //Each iteration gets us 1/10th of the period to scan

            for (int iter = 0; iter < 8; iter++) {
                double dt = (maxTime - minTime) / numDivisions;
                for (int i = 0; i < numDivisions; i++) {
                    double t = minTime + i * dt;
                    double angle = oObject.getEjectionAngleAtUT(t);
                    if (Math.Abs(angle - targetAngle) < closestAngleValue) {
                        closestAngleValue = Math.Abs(angle - targetAngle);
                        closestAngleTime = t;
                    }
                }
                minTime = (closestAngleTime - dt).Clamp(timeStart, timeStart + periodtoscan);
                maxTime = (closestAngleTime + dt).Clamp(timeStart, timeStart + periodtoscan); 
            }

            closestAngle = closestAngleValue + targetAngle;
            return closestAngleTime;
        }

    }
}
