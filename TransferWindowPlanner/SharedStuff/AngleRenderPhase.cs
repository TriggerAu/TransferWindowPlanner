using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;
using KSPPluginFramework;


namespace TransferWindowPlanner
{
    public class AngleRenderPhase : MonoBehaviourExtended
    {
        public Boolean isDrawing { get; private set; }


        /// <summary>
        /// Is the angle drawn and visible on screen
        /// </summary>
        public Boolean isVisible { get { return isAngleVisible && (!ShowTarget || isTargetVisible); } }


        public Boolean isAngleVisible { get; private set; }
        public Boolean isTargetVisible { get; private set; }

        /// <summary>
        /// Is the angle in the process of becoming visible
        /// </summary>
        public Boolean isBecomingVisible { get; private set; }

        /// <summary>
        /// Is the angle in the process of being hidden
        /// </summary>
        public Boolean IsBecomingInvisible { get; private set; }

        /// <summary>
        /// The Body we are measuring from
        /// </summary>
        public CelestialBody bodyOrigin { get; set; }

        /// <summary>
        /// The Body we are measuring against
        /// </summary>
        public CelestialBody bodyTarget { get; set; }

        /// <summary>
        /// Are we drawing a Target as well
        /// </summary>
        public Boolean ShowTarget { get; set; }

        /// <summary>
        /// The target Angle to Draw - if we have a target
        /// </summary>
        public Single AngleTargetValue { get; set; }

        public Single AngleValue { get; set; }




        private GameObject objLineStart = new GameObject("LineStart");
        private GameObject objLineEnd = new GameObject("LineEnd");
        private GameObject objLineArc = new GameObject("LineArc");
        private GameObject objLineTarget = new GameObject("LineTarget");
        private GameObject objLineTargetArc = new GameObject("LineTargetArc");

        private LineRenderer lineStart = null;
        private LineRenderer lineEnd = null;
        private LineRenderer lineArc = null;
        private LineRenderer lineTarget = null;
        private LineRenderer lineTargetArc = null;


        private PlanetariumCamera cam;

        internal Int32 ArcPoints = 72;
        internal Int32 StartWidth = 10;
        internal Int32 EndWidth = 10;


        internal override void Start()
        {
            base.Start();

            LogFormatted("Initializing Angle Render");

            //Get the orbit lines material so things look similar
            Material orbitLines = ((MapView)GameObject.FindObjectOfType(typeof(MapView))).orbitLinesMaterial;

            //init all the lines
            lineStart = InitLine(objLineStart, Color.blue, 2, 10, orbitLines);
            lineEnd = InitLine(objLineEnd, Color.blue, 2, 10, orbitLines);
            lineArc = InitLine(objLineArc, Color.blue, ArcPoints, 10, orbitLines);
            lineTarget = InitLine(objLineTarget, Color.blue, 2, 10, orbitLines);
            lineTargetArc = InitLine(objLineTargetArc, Color.blue, ArcPoints, 10, orbitLines);

            //get the map camera - well need this for distance/width calcs
            cam = (PlanetariumCamera)GameObject.FindObjectOfType(typeof(PlanetariumCamera));
        }
        
        /// <summary>
        /// Initialise a LineRenderer with some baseic values
        /// </summary>
        /// <param name="objToAttach">GameObject that renderer is attached to - one linerenderer per object</param>
        /// <param name="lineColor">Draw this color</param>
        /// <param name="VertexCount">How many vertices make up the line</param>
        /// <param name="InitialWidth">line width</param>
        /// <param name="linesMaterial">Line material</param>
        /// <returns></returns>
        private LineRenderer InitLine(GameObject objToAttach,Color lineColor,Int32 VertexCount, Int32 InitialWidth,Material linesMaterial)
        {
            objToAttach.layer = 9;
            LineRenderer lineReturn = objToAttach.AddComponent<LineRenderer>();

            lineReturn.material = linesMaterial;
            lineReturn.SetColors(lineColor, lineColor);
            lineReturn.transform.parent = null;
            lineReturn.useWorldSpace = true;
            lineReturn.SetWidth(InitialWidth, InitialWidth);
            lineReturn.SetVertexCount(VertexCount);
            lineReturn.enabled = false;

            return lineReturn;
        }



        internal override void OnDestroy()
        {
            base.OnDestroy();

            //Bin the objects
            lineStart = null;
            lineEnd = null;
            lineArc = null;
            lineTarget = null;
            lineTargetArc = null;

            objLineStart.DestroyGameObject();
            objLineEnd.DestroyGameObject();
            objLineArc.DestroyGameObject();
            objLineTarget.DestroyGameObject();
            objLineTargetArc.DestroyGameObject();
        }


        public void DrawAngle(CelestialBody bodyOrigin, CelestialBody bodyTarget)
        {
            ShowTarget = false;
            this.bodyOrigin = bodyOrigin;
            this.bodyTarget = bodyTarget;

            isDrawing = true;
        }

        public void HideAngle()
        {
            isDrawing = false;
        }

        internal override void LateUpdate()
        {
            base.LateUpdate();

            if (MapView.MapIsEnabled && isDrawing)
            {
                Vector3d vectStart = bodyOrigin.transform.position - bodyOrigin.referenceBody.transform.position;
                Double vectStartMag = vectStart.magnitude;

                Double _PhaseAngleCurrent = LambertSolver.CurrentPhaseAngle(bodyOrigin.orbit,bodyTarget.orbit);

                Vector3d vectEnd = Quaternion.AngleAxis(-(Single)_PhaseAngleCurrent, bodyOrigin.orbit.GetOrbitNormal().xzy) * vectStart;
                vectEnd = vectEnd.normalized * bodyTarget.orbit.ApR * 1.2;
                Double vectEndMag = vectEnd.magnitude;

                Vector3d vectPointEnd = bodyOrigin.referenceBody.transform.position + vectEnd;

                DrawLine(lineStart, bodyOrigin.referenceBody.transform.position, bodyOrigin.transform.position);
                DrawLine(lineEnd, bodyOrigin.referenceBody.transform.position, vectPointEnd);



                ////float scale = (float)(0.004 * cam.Distance);
                ////line.SetWidth(scale, scale);

                //lineAngle.enabled = true;
                //lineAngle.SetPosition(0, ScaledSpace.LocalToScaledSpace(bodyOrigin.referenceBody.transform.position));
                //lineAngle.SetPosition(1, ScaledSpace.LocalToScaledSpace(vectAngle));
                //lineAngle.SetWidth((float)intTest1 / 1000 * cam.Distance, (float)intTest1 / 1000 * cam.Distance);

                ////get the smaller of the two values
                //Double shortest = Math.Min(vectStartMag, vectDestMag) * 0.9;

                //lineArc.enabled = true;
                //lineArc.SetWidth((float)intTest1 / 1000 * cam.Distance, (float)intTest1 / 1000 * cam.Distance);

                ////now we draw an arc from a to b at that distance minus a smidge
                //for (int i = 0; i < ArcVertexCount; i++)
                //{
                //    Vector3d vectArc = Quaternion.AngleAxis(-(float)this.intTest2 / (ArcVertexCount - 1) * i, bodyOrigin.orbit.GetOrbitNormal().xzy) * vectStart;
                //    vectArc = vectArc.normalized * shortest;
                //    vectArc = bodyOrigin.referenceBody.transform.position + vectArc;

                //    lineArc.SetPosition(i, ScaledSpace.LocalToScaledSpace(vectArc));

                //}

                //cam.camera.WorldToScreenPoint(bodyOrigin.transform.position);

            }
            else
            {
                lineStart.enabled = false;
                lineEnd.enabled = false;
                lineArc.enabled = false;
                lineTarget.enabled = false;
                lineTargetArc.enabled = false;
            }

        }

        private void DrawLine(LineRenderer line, Vector3d pointStart, Vector3d pointEnd )
        {
            line.SetPosition(0, ScaledSpace.LocalToScaledSpace(pointStart));
            line.SetPosition(1, ScaledSpace.LocalToScaledSpace(pointEnd));
            line.SetWidth((Single)StartWidth / 1000 * (Single)(cam.transform.position - pointStart).magnitude, (Single)EndWidth / 1000 * (Single)(cam.transform.position - pointEnd).magnitude);
            line.SetWidth((float)10 / 1000 * cam.Distance, (float)10 / 1000 * cam.Distance);
            line.enabled = true;
        }

        internal override void FixedUpdate()
        {
            base.FixedUpdate();


        }
    }
}
