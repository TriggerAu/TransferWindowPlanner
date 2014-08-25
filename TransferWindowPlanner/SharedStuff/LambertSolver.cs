// Copyright (c) 2013-2014, Alex Moon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.
// * Neither the name of the <organization> nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LambertSolver
{
	public const double TwoPi = 2.0d * Math.PI;
	public const double HalfPi = 0.5d * Math.PI;
	public static double InverseSquareGoldenRatio = (3.0d - Math.Sqrt(5.0d)) * 0.5d; // This is 1 - (1 / phi) == 1/phi^2
	public static double MachineEpsilon = CalculateMachineEpsilon();
	public static double SqrtMachineEpsilon = Math.Sqrt(MachineEpsilon);

    public static Vector3d ZAxis = new Vector3d(0, 0, 1);

    public const double Rad2Deg = 180.0d / Math.PI;
    public const double Deg2Rad = Math.PI / 180.0d;


    public static double TransferDeltaV(CelestialBody origin, CelestialBody destination, double ut, double dt, double initialOrbitAltitude, double? finalOrbitAltitude)
    {
        TransferDetails tmp;
        return TransferDeltaV(origin, destination, ut, dt, initialOrbitAltitude, finalOrbitAltitude, out tmp);
    }

    /// <summary>
    /// Find the delta-v required for a ballistic transfer from <paramref name="origin"/> to <paramref name="destination"/>,
    /// departing at <paramref name="ut"/> UT, with a time of flight of <paramref name="dt"/> seconds, starting from a circular
    /// parking orbit <paramref name="initialOrbitAltitude"/> meters above the origin's surface, and optionally inserting into a
    /// final orbit <paramref name="finalOrbitAltitude"/> meters above the destination's surface.
    /// </summary>
    /// <returns>The total delta-v in meters per second of the specified transfer.</returns>
    /// <param name="origin">The origin body.</param>
    /// <param name="destination">The destination body. Must have the same <c>referenceBody</c> as <paramref name="origin"/>.</param>
    /// <param name="ut">The universal time of departure, in seconds. Must be greater than 0.</param>
    /// <param name="dt">The time of flight, in seconds. Must be greater than 0.</param>
    /// <param name="initialOrbitAltitude">The altitude of the initial circular parking orbit. If 0, parking orbit ejection is ignored. Must be greater than or equal to 0.</param>
    /// <param name="finalOrbitAltitude">(Optional) The altitude of the final circular orbit. Must be greater than or equal to 0 if provided.</param>
    public static double TransferDeltaV(CelestialBody origin, CelestialBody destination, double ut, double dt, double initialOrbitAltitude, double? finalOrbitAltitude, out TransferDetails oTransfer)
    {
        double gravParameter = origin.referenceBody.gravParameter;
        double tA = origin.orbit.TrueAnomalyAtUT(ut);
        Vector3d originPositionAtDeparture = OrbitPositionFromTrueAnomaly(origin.orbit, tA);
        Vector3d originVelocity = OrbitVelocityFromTrueAnomaly(origin.orbit, tA);

        tA = destination.orbit.TrueAnomalyAtUT(ut + dt);
        Vector3d destinationPositionAtArrival = OrbitPositionFromTrueAnomaly(destination.orbit, tA);

        bool longWay = Vector3d.Cross(originPositionAtDeparture, destinationPositionAtArrival).z < 0;

        Vector3d velocityBeforeInsertion;
        Vector3d velocityAfterEjection = Solve(gravParameter, originPositionAtDeparture, destinationPositionAtArrival, dt, longWay, out velocityBeforeInsertion);

        Vector3d ejectionDeltaVector = velocityAfterEjection - originVelocity;
        double ejectionDeltaV = ejectionDeltaVector.magnitude;
        if (initialOrbitAltitude > 0) {
            double mu = origin.gravParameter;
            double r0 = initialOrbitAltitude + origin.Radius;
            double rsoi = origin.sphereOfInfluence;
            double v0 = Math.Sqrt(origin.gravParameter / r0); // Initial circular orbit velocity
            double v1 = Math.Sqrt(ejectionDeltaV * ejectionDeltaV + 2 * v0 * v0 - 2 * mu / rsoi); // Velocity at periapsis

            double e = r0 * v1 * v1 / mu - 1; // Ejection orbit eccentricity
            double ap = r0 * (1 + e) / (1 - e); // Ejection orbit apoapsis

            if (ap > 0 && ap <= rsoi) {
                oTransfer = null;
                return Double.NaN; // There is no orbit that leaves the SoI with a velocity of ejectionDeltaV
            }

            if (ejectionDeltaVector.z != 0) {
                double sinEjectionInclination = ejectionDeltaVector.z / ejectionDeltaV;
                ejectionDeltaV = Math.Sqrt(v0 * v0 + v1 * v1 - 2 * v0 * v1 * Math.Sqrt(1 - sinEjectionInclination * sinEjectionInclination));
            } else {
                ejectionDeltaV = v1 - v0;
            }
        }

        oTransfer = new TransferDetails(origin, destination, ut, dt);
        oTransfer.OriginVelocity = originVelocity;
        oTransfer.TransferInitalVelocity = velocityAfterEjection;
        oTransfer.TransferFinalVelocity = velocityBeforeInsertion;

        double insertionDeltaV = 0;
        if (finalOrbitAltitude.HasValue) {
            Vector3d destinationVelocity = OrbitVelocityFromTrueAnomaly(destination.orbit, tA);
            insertionDeltaV = (velocityBeforeInsertion - destinationVelocity).magnitude;

            if (finalOrbitAltitude.Value != 0) {
                double finalOrbitVelocity = Math.Sqrt(destination.gravParameter / (finalOrbitAltitude.Value + destination.Radius));
                insertionDeltaV = Math.Sqrt(insertionDeltaV * insertionDeltaV + 2 * finalOrbitVelocity * finalOrbitVelocity - 2 * destination.gravParameter / destination.sphereOfInfluence) - finalOrbitVelocity;
            }

            oTransfer.DestinationVelocity = destinationVelocity;
            oTransfer.InjectionDeltaVector = (velocityBeforeInsertion - destinationVelocity);

        }


        return ejectionDeltaV + insertionDeltaV;
    }

	/// <summary>
	/// Find the universal time of the next lauch window from <paramref name="origin"/> to <paramref name="destination"/>.
	/// Limitations: Does not take into account ejection inclination change costs. Does not take into acccount insertion delta-v.
	/// </summary>
	/// <returns>The universal time of the next launch window from <paramref name="origin"/> to <paramref name="destination"/>.</returns>
	/// <param name="origin">The origin body.</param>
	/// <param name="destination">The destination body. Must have the same <c>referenceBody</c> as <paramref name="origin"/>.</param>
    public static double NextLaunchWindowUT(CelestialBody origin, CelestialBody destination)
	{
		if (origin.referenceBody != destination.referenceBody) {
			throw new ArgumentException("Origin and destination bodies must be orbiting the same referenceBody.");
		}

        double now = Planetarium.GetUniversalTime();
		double currentPhaseAngle = CurrentPhaseAngle(origin.orbit, destination.orbit);
		double hohmannPhaseAngle = HohmannPhaseAngle(origin.orbit, destination.orbit);
		double deltaPhaseAngle = (360.0 + currentPhaseAngle - hohmannPhaseAngle) % 360.0;
		if (destination.orbit.semiMajorAxis < origin.orbit.semiMajorAxis) {
			deltaPhaseAngle = 360.0 - deltaPhaseAngle;
		}

		double synodicPeriod = Math.Abs(1.0 / (1.0 / origin.orbit.period - 1.0 / destination.orbit.period));
		double estimatedDeparture = now + deltaPhaseAngle / (360.0 / synodicPeriod);

		if (CoplanarOrbits(origin.orbit, destination.orbit) && origin.orbit.eccentricity == 0 && destination.orbit.eccentricity == 0) { // Hohmann transfer
			return estimatedDeparture;
		} else {
			double gravParameter = origin.referenceBody.gravParameter;
			double hohmannTimeOfFlight = HohmannTimeOfFlight(origin.orbit, destination.orbit);
			double xmin = now;
			double xmax = now + synodicPeriod;
			bool longWay = false;

			Func<Vector2d, Vector2d, Range> boundsFunc = (Vector2d point, Vector2d direction) => {
				double t_xmin = 0, t_xmax = 0;
				if (direction.x != 0) {
					if (direction.x > 0) {
						t_xmin = (xmin - point.x) / direction.x;
						t_xmax = (xmax - point.x) / direction.x;
					} else {
						t_xmin = (xmax - point.x) / direction.x;
						t_xmax = (xmin - point.x) / direction.x;
					}

					if (Math.Abs(direction.y / direction.x) < 0.1) { // Direction is > 90% horizontal, don't worry about the y bounds
						return new Range(t_xmin, t_xmax);
					}
				}

				double timeToOpposition = TimeAtOpposition(origin.orbit.getRelativePositionAtUT(point.x), destination.orbit, point.x + 0.5 * hohmannTimeOfFlight) - point.x;

				double ymin = longWay ? 0.95 * timeToOpposition : 0.5 * hohmannTimeOfFlight;
				double ymax = longWay ? 2.0 * hohmannTimeOfFlight : 1.05 * timeToOpposition;
				double t_ymin, t_ymax;
				if (direction.y > 0) {
					t_ymin = (ymin - point.y) / direction.y;
					t_ymax = (ymax - point.y) / direction.y;
				} else {
					t_ymin = (ymax - point.y) / direction.y;
					t_ymax = (ymin - point.y) / direction.y;
				}

				if (Math.Abs(direction.x / direction.y) < 0.1) { // Direction is > 90% vertical, don't worry about the x bounds
					return new Range(t_ymin, t_ymax);
				} else {
					return new Range(Math.Max(t_xmin, t_ymin), Math.Min(t_xmax, t_ymax));
				}
			};

			Func<Vector2d,Vector3d> deltaVFunc = (Vector2d coords) => {
				double t1 = coords.x;
				double dt = coords.y;
				Vector3d pos1 = origin.orbit.getRelativePositionAtUT(t1);
				Vector3d pos2 = destination.orbit.getRelativePositionAtUT(t1 + dt);
				Vector3d ejectionVelocity = Solve(gravParameter, pos1, pos2, dt, longWay);
				return ejectionVelocity - origin.orbit.getOrbitalVelocityAtUT(t1);
			};

			Vector2d shortTransfer = new Vector2d(now + estimatedDeparture, 0.90 * hohmannTimeOfFlight);
			Vector3d shortDeltaVector = MinimizeDeltaV(ref shortTransfer, boundsFunc, 1e-4, deltaVFunc);

			longWay = true;
			Vector2d longTransfer = new Vector2d(shortTransfer.x, 1.10 * hohmannTimeOfFlight);
			Vector3d longDeltaVector = MinimizeDeltaV(ref longTransfer, boundsFunc, 1e-4, deltaVFunc);

			if (shortDeltaVector.sqrMagnitude <= longDeltaVector.sqrMagnitude) {
				return shortTransfer.x;
			} else {
				return longTransfer.x;
			}
		}
	}

	/// <summary>
	/// Calculates the time of flight for a Hohmann transfer between <paramref name="origin"/> and <paramref name="destination"/>, assuming the orbits are circular and coplanar.
	/// </summary>
	/// <returns>The time of flight.</returns>
	/// <param name="origin">The origin orbit.</param>
	/// <param name="destination">The destination orbit.</param>
	public static double HohmannTimeOfFlight(Orbit origin, Orbit destination)
	{
		double a = (origin.semiMajorAxis + destination.semiMajorAxis) * 0.5;
		double mu = origin.referenceBody.gravParameter;
		return Math.PI * Math.Sqrt((a * a * a) / mu);
	}

	/// <summary>
	/// Calculates the phase angle for a Hohmann transfer between <paramref name="origin"/> and <paramref name="destination"/>, assuming the orbits are circular and coplanar.
	/// </summary>
	/// <returns>The phase angle.</returns>
	/// <param name="origin">The origin orbit.</param>
	/// <param name="destination">The destination orbit.</param>
	public static double HohmannPhaseAngle(Orbit origin, Orbit destination)
	{
		return 180.0 - HohmannTimeOfFlight(origin, destination) * 360.0 / destination.period;
	}

	/// <summary>
	/// Calculates the current phase angle between <paramref name="origin"/> and <paramref name="destination"/>.
	/// </summary>
	/// <returns>The phase angle.</returns>
	/// <param name="origin">Origin.</param>
	/// <param name="destination">Destination.</param>
	public static double CurrentPhaseAngle(Orbit origin, Orbit destination)
	{
		Vector3d normal = origin.GetOrbitNormal().normalized;
		Vector3d projected = Vector3d.Exclude(normal, destination.pos);
		double result = Vector3d.Angle(origin.pos, projected);
		if (Vector3d.Dot(Vector3d.Cross(origin.pos, projected), normal) < 0) {
			return 360.0 - result;
		} else {
			return result;
		}
	}

	/// <summary>
	/// Calculates the earliest universal time after <paramref name="after"/> when <paramref name="destination"/> will be 180 degrees from the <paramref name="origin"/> position.
	/// </summary>
	/// <returns>The universal time when <paramref name="destination"/> will be in opposition to <paramref name="origin"/>.</returns>
	/// <param name="origin">Origin position.</param>
	/// <param name="destination">Destination orbit.</param>
	/// <param name="after">Universal time after which to find the opposition.</param>
	public static double TimeAtOpposition(Vector3d origin, Orbit destination, double after = 0)
	{
		Vector3d normal = destination.GetOrbitNormal().normalized;
		double trueAnomaly = TrueAnomaly(destination, Vector3d.Exclude(normal, -origin));
		double ut = Planetarium.GetUniversalTime() + destination.GetDTforTrueAnomaly(trueAnomaly, 0);
        if (ut <= after) {
            ut += destination.period * Math.Floor((after - ut) / destination.period + 1);
        }

		return ut;
	}

    /// <summary>
    /// Solve the Lambert Problem, determining the velocity at <paramref name="pos1"/> of an orbit passing through <paramref name="pos2"/> after <paramref name="timeOfFlight"/>.
    /// </summary>
    /// <returns>The velocity vector of the identified orbit at <paramref name="pos1"/>.</returns>
    /// <param name="gravParameter">Gravitational parameter of the central body.</param>
    /// <param name="pos1">The first point the orbit passes through.</param>
    /// <param name="pos2">The second point the orbit passes through.</param>
    /// <param name="timeOfFlight">The time of flight between <paramref name="pos1"/> and <paramref name="pos2"/>.</param>
    /// <param name="longWay">If set to <c>true</c>, solve for an orbit subtending more than 180 degrees between <paramref name="pos1"/> and <paramref name="pos2"/>.</param>
    public static Vector3d Solve(double gravParameter, Vector3d pos1, Vector3d pos2, double timeOfFlight, bool longWay)
    {
        Vector3d tmp;
        return Solve(gravParameter, pos1, pos2, timeOfFlight, longWay, out tmp);
    }

	/// <summary>
	/// Solve the Lambert Problem, determining the velocity at <paramref name="pos1"/> of an orbit passing through <paramref name="pos2"/> after <paramref name="timeOfFlight"/>.
	/// </summary>
	/// <returns>The velocity vector of the identified orbit at <paramref name="pos1"/>.</returns>
	/// <param name="gravParameter">Gravitational parameter of the central body.</param>
	/// <param name="pos1">The first point the orbit passes through.</param>
	/// <param name="pos2">The second point the orbit passes through.</param>
	/// <param name="timeOfFlight">The time of flight between <paramref name="pos1"/> and <paramref name="pos2"/>.</param>
	/// <param name="longWay">If set to <c>true</c>, solve for an orbit subtending more than 180 degrees between <paramref name="pos1"/> and <paramref name="pos2"/>.</param>
    /// <param name="v2">The velocity vector of the identified orbit at <paramref name="pos2"/>.</paramref>
    public static Vector3d Solve(double gravParameter, Vector3d pos1, Vector3d pos2, double timeOfFlight, bool longWay, out Vector3d v2)
	{
		// Based on Sun, F.T. "On the Minimum Time Trajectory and Multiple Solutions of Lambert's Problem"
		// AAS/AIAA Astrodynamics Conference, Provincetown, Massachusetts, AAS 79-164, June 25-27, 1979
		double r1 = pos1.magnitude;
		double r2 = pos2.magnitude;
		double angleOfFlight = Math.Acos(Vector3d.Dot(pos1, pos2) / (r1 * r2));
		if (longWay) {
			angleOfFlight = TwoPi - angleOfFlight;
		}

		// Intermediate terms
		Vector3d deltaPos = pos2 - pos1;
		double c = deltaPos.magnitude;
		double m = r1 + r2 + c;
		double n = r1 + r2 - c;

        double angleParameter = Math.Sqrt(n / m);
		if (longWay) {
			angleParameter = -angleParameter;
		}

		double normalizedTime = 4.0 * timeOfFlight * Math.Sqrt(gravParameter / (m * m * m));
		double parabolicNormalizedTime = 2.0 / 3.0 * (1.0 - angleParameter * angleParameter * angleParameter);

		double x, y; // Path parameters
		Func<double,double> fy = (xn) => (angleParameter < 0) ? -Math.Sqrt(1.0 - angleParameter * angleParameter * (1.0 - xn * xn)) : Math.Sqrt(1.0 - angleParameter * angleParameter * (1.0 - xn * xn));
        if (RelativeError(normalizedTime, parabolicNormalizedTime) < 1e-6) { // Parabolic solution
			x = 1.0;
			y = (angleParameter < 0) ? -1 : 1;
        } else if (normalizedTime < parabolicNormalizedTime) { // Hyperbolic solution
            // Returns the difference between the normalized time for a path parameter of xn and normalizedTime for a hyperbolic orbit (xn > 1.0)
            Func<double,double> fdt = (xn) => {
                double yn = fy(xn);
                double g = Math.Sqrt(xn * xn - 1.0);
                double h = Math.Sqrt(yn * yn - 1.0);
                return (Acoth(yn / h) - Acoth(xn / g) + xn * g - yn * h) / (g * g * g) - normalizedTime;
            };

            Range bounds = new Range(1.0 + MachineEpsilon, 2.0);
            while (fdt(bounds.upper) > 0.0) {
                bounds.lower = bounds.upper;
                bounds.upper *= 2.0;
            }

            x = FindRoot(bounds, 1e-6, fdt); // Solve for x
            y = fy(x);
		} else {
            double minimumEnergyNormalizedTime = Math.Acos(angleParameter) + angleParameter * Math.Sqrt(1 - angleParameter * angleParameter);

            if (RelativeError(normalizedTime, minimumEnergyNormalizedTime) < 1e-6) { // Minimum energy elliptical solution
                x = 0.0;
                y = fy(x);
            } else {
                // Returns the difference between the normalized time for a path parameter of xn and normalizedTime for an elliptical orbit (-1.0 < xn < 1.0)
                Func<double,double> fdt = (xn) =>
                {
                    double yn = fy(xn);
                    double g = Math.Sqrt(1.0 - xn * xn);
                    double h = Math.Sqrt(1.0 - yn * yn);
                    double result = (Acot(xn / g) - Math.Atan(h / yn) - xn * g + yn * h) / (g * g * g) - normalizedTime;
                    return result;
                };

                // Select our bounds based on the relationship between the known normalized times and normalizedTime
                Range bounds;
                if (normalizedTime > minimumEnergyNormalizedTime) { // Elliptical high path solution
                    bounds.lower = -1.0 + MachineEpsilon;
                    bounds.upper = 0.0;
                } else { // Elliptical low path solution
                    bounds.lower = 0.0;
                    bounds.upper = 1.0 - MachineEpsilon;
                }

                x = FindRoot(bounds, 1e-6, fdt); // Solve for x
                y = fy(x);
            }
		}

		double sqrtMu = Math.Sqrt(gravParameter);
		double invSqrtM = 1.0 / Math.Sqrt(m);
		double invSqrtN = 1.0 / Math.Sqrt(n);

		double vc = sqrtMu * (y * invSqrtN + x * invSqrtM);
		double vr = sqrtMu * (y * invSqrtN - x * invSqrtM);
		Vector3d ec = deltaPos * (vc / c);

        v2 = ec - pos2 * (vr / r2);
		return ec + pos1 * (vr / r1);
	}

	private struct Range
	{
		public double lower, upper;

		public Range(double lwr, double upr)
		{
			lower = lwr;
			upper = upr;
		}
	}

    private delegate Vector3d LinearMinimizationFunc(ref Vector2d x0, Vector2d direction);

	private static Vector3d MinimizeDeltaV(ref Vector2d p0, Func<Vector2d, Vector2d, Range> getBounds, double relativeAccuracy, Func<Vector2d, Vector3d> f)
	{
		// Uses Powell's method to find the local minimum of f(x,y) within the bounds returned by getBounds(point, direction): http://en.wikipedia.org/wiki/Powell's_method
		List<Vector2d> directionVectors = new List<Vector2d>(new Vector2d[] { new Vector2d (1, 0), new Vector2d (0, 1) });

        LinearMinimizationFunc findMinimumAlongDirection = (ref Vector2d p, Vector2d direction) => {
			double u;
            Vector2d x0 = p;
			Vector3d result = MinimizeDeltaV(getBounds(p, direction), out u, relativeAccuracy, (v) => {
				Vector2d point = x0 + v * direction;
				return f(point); });
			p += u * direction;
            return result;
		};

        Vector2d pn = p0;
        Vector3d fpnVector = f(pn);
        double fpn = fpnVector.magnitude;
        for (int i = 1; i < 100; i++) {
            double fp0 = fpn;
            double dfMax = 0;
            Vector2d dfMaxDirection = new Vector2d();

			foreach (Vector2d direction in directionVectors) {
                double fprev = fpn;
				fpnVector = findMinimumAlongDirection(ref pn, direction);
                fpn = fpnVector.magnitude;
                double df = fprev - fpn;
                if (df > dfMax) {
                    dfMax = df;
                    dfMaxDirection = direction;
                }
			}

            if (2 * (fp0 - fpn) <= relativeAccuracy * (Math.Abs(fp0) + Math.Abs(fpn)) + MachineEpsilon) {
                p0 = pn;
                return fpnVector;
            }

            Vector2d newDirection = pn - p0;
            Vector2d pe = pn + newDirection;
            double fpe = f(pe).magnitude;
            if (fpe < fp0) {
                double g = fp0 - fpn - dfMax;
                double h = fp0 - fpe;
                if (2 * (fp0 - 2 * fpn + fpe) * g * g < h * h * dfMax) {
                    fpnVector = findMinimumAlongDirection(ref pn, newDirection);
                    fpn = fpnVector.magnitude;
                    if (directionVectors[0] == dfMaxDirection) {
                        directionVectors[0] = directionVectors[1];
                    }
                    directionVectors[1] = newDirection;
                }
            }

            p0 = pn;
		}

		throw new Exception("LambertSolver 2D delta-v minimization failed to converge!");
	}

	private static Vector3d MinimizeDeltaV(Range bounds, out double x, double relativeAccuracy, Func<double,Vector3d> f)
	{
		// Uses Brent's method of parabolic interpolation to find a local minimum: http://linneus20.ethz.ch:8080/1_5_2.html
		x = bounds.lower + InverseSquareGoldenRatio * (bounds.upper - bounds.lower);
		double w = x;
		double v = w;
		double e = 0.0;
		Vector3d fxVector = f(x);
		double fx = fxVector.sqrMagnitude;
		double fw = fx;
		double fv = fw;
		double delta = 0;

		for (int i = 0;; i++) {
			double midpoint = 0.5d * (bounds.lower + bounds.upper);
			double tol = (SqrtMachineEpsilon + relativeAccuracy) * Math.Abs(x);
			double t2 = 2.0d * tol;

			if (Math.Abs(x - midpoint) <= t2 - 0.5d * (bounds.upper - bounds.lower)) { // Are we close enough?
				return fxVector;
			} else if (i > 100) {
				throw new Exception("LambertSolver 1D delta-v minimization failed to converge!");
			}

			// Fit a parabola between a, x, and b 
			double p = 0, q = 0, r = 0;
			if (tol < Math.Abs(e)) {
				r = (x - w) * (fx - fv);
				q = (x - v) * (fx - fw);
				p = (x - v) * q - (x - w) * r;
				q = 2.0d * (q - r);
				if (q <= 0.0) {
					q = -q;
				} else {
					p = -p;
				}
				r = e;
				e = delta;
			}

			double u;
			if (Math.Abs(p) < Math.Abs(0.5d * q * r) && p > q * (bounds.lower - x) && p < q * (bounds.upper - x)) {
				// Use parabolic interpolation for this step
				delta = p / q;
				u = x + delta;

				// We don't want to evaluate f for x within 2 * tol of a or b
				if ((u - bounds.lower) < t2 || (bounds.upper - u) < t2) {
					if (x < midpoint) {
						delta = tol;
					} else {
						delta = -tol;
					}
				}
			} else {
				// Use the golden section for this step
				if (x < midpoint) {
					e = bounds.upper - x;
				} else {
					e = bounds.lower - x;
				}
				delta = InverseSquareGoldenRatio * e;
					
				if (Math.Abs(delta) >= tol) {
					u = x + delta;
				} else if (delta > 0.0) {
					u = x + tol;
				} else {
					u = x - tol;
				}
			}

			Vector3d fuVector = f(u);
			double fu = fuVector.sqrMagnitude;

			if (fu <= fx) {
				if (u < x) {
					bounds.upper = x;
				} else {
					bounds.lower = x;
				}

				v = w;
				fv = fw;
				w = x;
				fw = fx;
				x = u;
				fxVector = fuVector;
				fx = fu;
			} else {
				if (u < x) {
					bounds.lower = u;
				} else {
					bounds.upper = u;
				}

				if (fu <= fw || w == x) {
					v = w;
					fv = fw;
					w = u;
					fw = fu;
				} else if (fu <= fv || v == x || v == w) {
					v = u;
					fv = fu;
				}
			}
		}
	}

	private static double FindRoot(Range bounds, double tolerance, Func<double,double> f)
	{
		// Uses Brent's root finding method: http://math.fullerton.edu/mathews/n2003/BrentMethodMod.html
		double a = bounds.lower;
		double b = bounds.upper;
		double c = a;
		double fa = f(a);
		double fb = f(b);
		double fc = fa;
		double d = b - a;
		double e = d;

        tolerance *= 0.5d;

		for (int i = 0;; i++) {
			if (Math.Abs(fc) < Math.Abs(fb)) { // If c is closer to the root than b, swap b and c
				a = b;
				b = c;
				c = a;
				fa = fb;
				fb = fc;
				fc = fa;
			}

            double tol = 2.0 * MachineEpsilon * Math.Abs(b) + tolerance;
			double m = 0.5d * (c - b);

			if (fb == 0.0d || Math.Abs(m) <= tol) {
				return b;
			} else if (i > 100) {
				throw new Exception("LambertSolver root failed to converge!");
			}

			if (Math.Abs(e) < tol || Math.Abs(fa) <= Math.Abs(fb)) { // Use a bisection step
				d = e = m;
			} else {
				double p, q, r;
				double s = fb / fa;

				if (a == c) { // Use a linear interpolation step
					p = 2.0d * m * s;
					q = 1.0d - s;
				} else {  // Use inverse quadratic interpolation
					q = fa / fc;
					r = fb / fc;
					p = s * (2.0d * m * q * (q - r) - (b - a) * (r - 1.0d));
					q = (q - 1.0d) * (r - 1.0d) * (s - 1.0d);
				}

				if (p > 0.0d) {
					q = -q;
				} else {
					p = -p;
				}

				if (2.0d * p < Math.Min(3.0d * m * q - Math.Abs(tol * q), Math.Abs(e * q))) { // Verify interpolation
					e = d;
					d = p / q;
				} else { // Fall back to bisection
					d = e = m;
				}
			}

			a = b;
			fa = fb;

			if (Math.Abs(d) > tol) {
				b += d;
			} else {
				b += (m > 0 ? tol : -tol);
			}

			fb = f(b);

			if ((fb < 0 && fc < 0) || (fb > 0 && fc > 0)) { // Ensure fb and fc have different signs
				c = a;
				fc = fa;
				d = e = b - a;
			}
		}
	}
	
	private static Vector3d HyperbolicEjectionAngle(Vector3d vsoi, double cosTrueAnomaly, Vector3d normal)
	{
		vsoi = vsoi.normalized;

		// We have three equations of three unknowns (v.x, v.y, v.z):
		//   dot(v, vsoi) = cosTrueAnomaly
		//   norm(v) = 1  [Unit vector]
		//   dot(v, normal) = 0  [Perpendicular to normal]
		//
		// Solution is defined iff:
		//   normal.z != 0
		//   vinf.y != 0 or (vinf.z != 0 and normal.y != 0) [because we are solving for v.x first]
		//   vinf is not parallel to normal

		// Intermediate terms
		double f = vsoi.y - vsoi.z * normal.y / normal.z;
		double g = (vsoi.z * normal.x - vsoi.x * normal.z) / (vsoi.y * normal.z - vsoi.z * normal.y);
		double h = (normal.x + g * normal.y) / normal.z;
		double m = normal.y * normal.y + normal.z * normal.z;
		double n = f * normal.z * normal.z / cosTrueAnomaly;

		// Quadratic coefficients
		double a = (1 + g * g + h * h);
		double b = 2 * (g * m + normal.x * normal.y) / n;
		double c = m * cosTrueAnomaly / (f * n) - 1;

		// Quadratic formula without loss of significance (Numerical Recipes eq. 5.6.4)
		double q;
		if (b < 0) {
			q = -0.5 * (b - Math.Sqrt(b * b - 4 * a * c));
		} else {
			q = -0.5 * (b + Math.Sqrt(b * b - 4 * a * c));
		}

		Vector3d v;
		v.x = q / a;
		v.y = g * v.x + cosTrueAnomaly / f;
		v.z = -(v.x * normal.x + v.y * normal.y) / normal.z;

		if (Vector3d.Dot(Vector3d.Cross(v, vsoi), normal) < 0) { // Wrong orbital direction
			v.x = c / q;
			v.y = g * v.x + cosTrueAnomaly / f;
			v.z = -(v.x * normal.x + v.y * normal.y) / normal.z;
		}

		return v;
	}

	private static double TrueAnomaly(Orbit orbit, Vector3d direction)
	{
		Vector3d periapsis = orbit.GetEccVector();
        double trueAnomaly = Vector3d.Angle(periapsis, direction) * Deg2Rad;
		if (Vector3d.Dot(Vector3d.Cross(periapsis, direction), orbit.GetOrbitNormal()) < 0) {
			trueAnomaly = TwoPi - trueAnomaly;
		}

		return trueAnomaly;
	}

    private static QuaternionD OrbitRotation(Orbit orbit)
    {
        Vector3d axisOfInclination = new Vector3d(Math.Cos(-orbit.argumentOfPeriapsis * Deg2Rad), Math.Sin(-orbit.argumentOfPeriapsis * Deg2Rad), 0);
        return QuaternionD.AngleAxis(orbit.LAN + orbit.argumentOfPeriapsis, ZAxis) * QuaternionD.AngleAxis(orbit.inclination, axisOfInclination);
    }

    private static Vector3d OrbitPositionFromTrueAnomaly(Orbit orbit, double tA)
    {
        double cos = Math.Cos(tA);
        double sin = Math.Sin(tA);

        double e = orbit.eccentricity;
        double r = orbit.semiMajorAxis * (1 - e * e) / (1 + e * cos);

        return OrbitRotation(orbit) * new Vector3d(r * cos, r * sin, 0);
    }

    private static Vector3d OrbitVelocityFromTrueAnomaly(Orbit orbit, double tA)
    {
        double sin = Math.Sin(tA);
        double cos = Math.Cos(tA);

        double mu = orbit.referenceBody.gravParameter;
        double e = orbit.eccentricity;
        double h = orbit.h.magnitude;
        double r = orbit.semiMajorAxis * (1 - e * e) / (1 + e * cos);

        double vr = mu * e * sin / h;
        double vtA = h / r;

        return OrbitRotation(orbit) * new Vector3d(vr * cos - vtA * sin, vr * sin + vtA * cos, 0);
    }

	private static bool CoplanarOrbits(Orbit o1, Orbit o2)
	{
		return o1.inclination == o2.inclination && (o1.inclination == 0 || o1.LAN == o2.LAN);
	}

	private static double Acot(double x)
	{
		return HalfPi - Math.Atan(x);
	}

	private static double Acoth(double x)
	{
		return 0.5 * Math.Log((x + 1) / (x - 1));
	}

	private static double Acosh(double x)
	{
		return Math.Log(x + Math.Sqrt(x * x - 1));
	}

    private static double RelativeError(double a, double b)
    {
        return Math.Abs(1.0d - a / b);
    }

	private static double CalculateMachineEpsilon()
	{
		double result = 1.0d;

		do {
			result *= 0.5d;
        } while (1.0d + (0.5d * result) != 1.0d);

		return result;
	}



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

        public CelestialBody Origin {set;get;}
        public CelestialBody Destination { get; set; }
        public Double DepartureTime { get; set; }
        public Double TravelTime { get; set; }

        public Vector3d OriginVelocity { get; set; }
        public Vector3d TransferInitalVelocity { get; set; }
        public Vector3d TransferFinalVelocity { get; set; }
        public Vector3d DestinationVelocity { get; set; }

        public Vector3d EjectionDeltaVector { get; set; }
        public Vector3d InjectionDeltaVector { get; set; }

        public double DVEjection { get { return EjectionDeltaVector.magnitude; } }
        public double DVInjection { get { return InjectionDeltaVector.magnitude; } }
        public double DVTotal { get { return DVEjection + DVInjection; } }
        public Double EjectionAngle { get; set; }

    }
}

