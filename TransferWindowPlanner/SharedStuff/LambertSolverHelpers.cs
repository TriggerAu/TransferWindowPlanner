using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

using KSPPluginFramework;

namespace TransferWindowPlanner
{
    public class TransferDetails
    {
        public TransferDetails(CelestialBody origin, CelestialBody destination, Double ut, Double dt)
            : this()
        {
            this.Origin = origin;
            this.Destination = destination;
            this.DepartureTime = ut;
            this.TravelTime = dt;
        }
        public TransferDetails() { }

        /// <summary>
        /// Travelling from
        /// </summary>
        public CelestialBody Origin { set; get; }
        /// <summary>
        /// Travelling To
        /// </summary>
        public CelestialBody Destination { get; set; }
        /// <summary>
        /// UT that we are departing at - seconds since Epoch
        /// </summary>
        public Double DepartureTime { get; set; }
        /// <summary>
        /// Seconds of travel time
        /// </summary>
        public Double TravelTime { get; set; }

        /// <summary>
        /// Velocity of the Celestial Origin at Entry Point from Lambert Orbit
        /// </summary>
        public Vector3d OriginVelocity { get; set; }
        /// <summary>
        /// Velocity at Entry point of the Orbit from the Lambert Solver
        /// </summary>
        public Vector3d TransferInitalVelocity { get; set; }
        /// <summary>
        /// Velocity at Exit point of the Orbit from the Lambert Solver
        /// </summary>
        public Vector3d TransferFinalVelocity { get; set; }
        /// <summary>
        /// Velocity of the Celestial Destination at Exit Point from Lambert Orbit
        /// </summary>
        public Vector3d DestinationVelocity { get; set; }

        /// <summary>
        /// velocity in m/s of the vessel in its original orbit before Ejection
        /// </summary>
        public Double OriginVesselOrbitalSpeed { get; set; }
        /// <summary>
        /// velocity in m/s of the vessel in its destination orbit After Injection
        /// </summary>
        public Double DestinationVesselOrbitalSpeed { get; set; }

        /// <summary>
        /// Velocity of the Ejection Burn
        /// </summary>
        public Vector3d EjectionDeltaVector { get; set; }
        /// <summary>
        /// Velocity of the Injection Burn
        /// </summary>
        public Vector3d InjectionDeltaVector { get; set; }

        /// <summary>
        /// Magnitude of the Ejection Burn
        /// </summary>
        public double DVEjection { get { return EjectionDeltaVector.magnitude; } }
        /// <summary>
        /// Magnitude of the Injection Burn
        /// </summary>
        public double DVInjection { get { return InjectionDeltaVector.magnitude; } }
        /// <summary>
        /// Magnitude of all burns
        /// </summary>
        public double DVTotal { get { return DVEjection + DVInjection; } }


        public Double PhaseAngle { get { return PhaseAngleCalc(Origin.orbit, Destination.orbit, DepartureTime); } }
        /// <summary>
        /// How far around the Transfer Orbit will we travel in radians
        /// </summary>
        public Double TransferAngle { get; set; }

        /// <summary>
        /// Angle above Origin orbit plane we are doing the Ejection burn at
        /// </summary>
        public Double EjectionInclination { get; set; }
        /// <summary>
        /// Angle above Transfer orbit plane we are doing the injection burn at
        /// </summary>
        public Double InsertionInclination { get; set; }


        /// <summary>
        /// Velocity Vector for Ejection - Basically Diff between Transfer Orbit and Planet Orbit velocities
        /// </summary>
        public Vector3d EjectionVector { get { return TransferInitalVelocity - OriginVelocity; } }

        /// <summary>
        /// m/s of velocity required in the Normal direction
        /// </summary>
        public Double EjectionDVNormal { get; set; }
        /// <summary>
        /// m/s of velocity required in the Prograde direction
        /// </summary>
        public Double EjectionDVPrograde { get; set; }
        /// <summary>
        /// Heading of the craft in radians
        /// </summary>
        public Double EjectionHeading { get; set; }
        /// <summary>
        /// Ejection angle of the burn in radians - angle from orbit velocity vector
        /// </summary>
        public Double EjectionAngle { get; set; }

        public Boolean EjectionAngleIsRetrograde { get; set; }
        public String EjectionAngleText
        {
            get
            {
                String strret = String.Format("{0:0.00}°", EjectionAngle * LambertSolver.Rad2Deg);
                if (EjectionAngleIsRetrograde)
                    strret += " to retrograde";
                else
                    strret += " to prograde";

                return strret;
            }
        }

        /// <summary>
        /// This calculates the details of the Ejection Angles for the Eject burn
        /// </summary>
        public void CalcEjectionValues()
        {
            Double mu = Origin.gravParameter;
            Double rsoi = Origin.sphereOfInfluence;
            Double vsoi = EjectionVector.magnitude;
            Double v1 = Math.Sqrt(vsoi * vsoi + 2 * OriginVesselOrbitalSpeed * OriginVesselOrbitalSpeed - 2 * mu / rsoi);
            EjectionDVNormal = v1 * Math.Sin(EjectionInclination);
            EjectionDVPrograde = v1 * Math.Cos(EjectionInclination) - OriginVesselOrbitalSpeed;
            EjectionHeading = Math.Atan2(EjectionDVPrograde, EjectionDVNormal);

            Double initialOrbitRadius = mu / (OriginVesselOrbitalSpeed * OriginVesselOrbitalSpeed);
            Double e = initialOrbitRadius * v1 * v1 / mu - 1;
            Double a = initialOrbitRadius / (1 - e);
            Double theta = Math.Acos((a * (1 - e * e) - rsoi) / (e * rsoi));
            theta += Math.Asin(v1 * initialOrbitRadius / (vsoi * rsoi));
            EjectionAngle = EjectionAngleCalc(EjectionDeltaVector, theta, OriginVelocity.normalized);

            MonoBehaviourExtended.LogFormatted("{0}",EjectionAngle);

            if (Destination.orbit.semiMajorAxis < Origin.orbit.semiMajorAxis)
            {
                EjectionAngleIsRetrograde = true;
                EjectionAngle -= Math.PI;
                if (EjectionAngle < 0)
                    EjectionAngle += 2 * Math.PI;
            }
            else
            {
                EjectionAngleIsRetrograde = false;
            }

        }

        /// <summary>
        /// Conversion of ejectionAngle from https://github.com/alexmoon/ksp/blob/gh-pages/javascripts/orbit.js
        /// </summary>
        /// <param name="vsoi">Velocity of the Ejection Vector in the Planets SOI</param>
        /// <param name="theta">???</param>
        /// <param name="prograde">What direction is prograde</param>
        /// <returns>Ejection Angle in Radians</returns>
        private Double EjectionAngleCalc(Vector3d vsoi, Double theta, Vector3d prograde)
        {
            Double a, ax, ay, az, b, c, cosTheta, g, q, vx, vy;

            Vector3d _ref = vsoi.normalized;
            ax = _ref.x; ay = _ref.y; az = _ref.z;
            cosTheta = Math.Cos(theta);
            g = -ax / ay;
            a = 1 + g * g;
            b = 2 * g * cosTheta / ay;
            c = cosTheta * cosTheta / (ay * ay) - 1;
            if (b < 0)
            {
                q = -0.5 * (b - Math.Sqrt(b * b - 4 * a * c));
            }
            else
            {
                q = -0.5 * (b + Math.Sqrt(b * b - 4 * a * c));
            }
            vx = q / a;
            vy = g * vx + cosTheta / ay;
            if (Math.Sign(Vector3d.Cross(new Vector3d(vx, vy, 0), new Vector3d(ax, ay, az))[2]) != Math.Sign(Math.PI - theta))
            {
                vx = c / q;
                vy = g * vx + cosTheta / ay;
            }
            prograde = new Vector3d(prograde.x, prograde.y, 0);
            if (Vector3d.Cross(new Vector3d(vx, vy, 0), prograde).z < 0)
            {
                return (LambertSolver.TwoPi) - Math.Acos(Vector3d.Dot(new Vector3d(vx, vy, 0), prograde));
            }
            else
            {
                return Math.Acos(Vector3d.Dot(new Vector3d(vx, vy, 0), prograde));
            }
        }


        private Double PhaseAngleCalc(Orbit o1, Orbit o2, Double UT)
        {
            Vector3d n = o1.GetOrbitNormal();

            Vector3d p1 = o1.getRelativePositionAtUT(UT);
            Vector3d p2 = o2.getRelativePositionAtUT(UT);
            double phaseAngle = Vector3d.Angle(p1, p2);
            if (Vector3d.Angle(Quaternion.AngleAxis(90, Vector3d.forward) * p1, p2) > 90)
            {
                phaseAngle = 360 - phaseAngle;
            }

            if (o2.semiMajorAxis < o1.semiMajorAxis)
            {
                phaseAngle = phaseAngle - 360;
            }

            return LambertSolver.Deg2Rad * phaseAngle;
            //return LambertSolver.Deg2Rad * ((phaseAngle + 360) % 360);
        }



        internal string TransferDetailsText
        {
            get
            {

                String Message = ""; //String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", this.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000);
                //Message = Message.AppendLine("Depart at:      {0}", KSPTime.PrintDate(new KSPTime(this.DepartureTime), KSPTime.PrintTimeFormat.DateTimeString));
                Message = Message.AppendLine("Depart at:      {0}", new KSPDateTime(this.DepartureTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
                Message = Message.AppendLine("       UT:      {0:0}", this.DepartureTime);
                //Message = Message.AppendLine("   Travel:      {0}", new KSPTime(this.TravelTime).IntervalStringLongTrimYears());
                Message = Message.AppendLine("   Travel:      {0}", new KSPTimeSpan(this.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears));
                Message = Message.AppendLine("       UT:      {0:0}", this.TravelTime);
                //Message = Message.AppendLine("Arrive at:      {0}", KSPTime.PrintDate(new KSPTime(this.DepartureTime + this.TravelTime), KSPTime.PrintTimeFormat.DateTimeString));
                Message = Message.AppendLine("Arrive at:      {0}", new KSPDateTime(this.DepartureTime + this.TravelTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
                Message = Message.AppendLine("       UT:      {0:0}", this.DepartureTime + this.TravelTime);
                Message = Message.AppendLine("Phase Angle:    {0:0.00}°", this.PhaseAngle * LambertSolver.Rad2Deg);
                //Message = Message.AppendLine("Ejection Angle: {0:0.00}°", this.EjectionAngle * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Ejection Angle: {0}", this.EjectionAngleText);
                Message = Message.AppendLine("Ejection Inc.:  {0:0.00}°", this.EjectionInclination * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Ejection Δv:    {0:0} m/s", this.DVEjection);
                Message = Message.AppendLine("Prograde Δv:    {0:0.0} m/s", this.EjectionDVPrograde);
                Message = Message.AppendLine("Normal Δv:      {0:0.0} m/s", this.EjectionDVNormal);
                Message = Message.AppendLine("Heading:        {0:0.00}°", this.EjectionHeading * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Insertion Inc.: {0:0.00}°", this.InsertionInclination * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Insertion Δv:   {0:0} m/s", this.DVInjection);
                Message = Message.AppendLine("Total Δv:       {0:0} m/s", this.DVTotal);
                return Message;
            }
        }

    }
}
