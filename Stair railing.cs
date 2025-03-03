using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms.VisualStyles;
using System.Xml.Schema;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.UI.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace Steel_goods
{
    public class Stair_railing : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Steel_stairs class.
        /// </summary>
        public Stair_railing()
          : base(   "Stair railing", 
                    "StRail",
                    "Creates a stairs railing",
                    "Steel goods", 
                    "Stairs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {


            pManager.AddPointParameter("Stair start point", "StrCStart", "Add stair start point", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Stair count", "StrCnt", "Add stair count", GH_ParamAccess.item, 12);
            pManager.AddNumberParameter("Stair rise", "StrRise", "Add stair rise", GH_ParamAccess.item, 180);
            pManager.AddNumberParameter("Stair run", "StrRun", "Add stair run", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("Railing Height", "RailH", "Add railign height", GH_ParamAccess.item, 1005);
            pManager.AddNumberParameter("Max pillar spacing", "MaxSpacing", "Add maximum pillar spacig", GH_ParamAccess.item, 1200);
            pManager.AddNumberParameter("Offset", "Offset", "Offset", GH_ParamAccess.item, 90);

            Param_Point param1 = (Param_Point)pManager[0];
            param1.PersistentData.Append(new GH_Point(new Point3d(0, 0, 0)));

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Stair Curve", "StrCrv", "Stair curve", GH_ParamAccess.item);
            pManager.AddCurveParameter("Railing Curve", "RailCrv", "Rail curve", GH_ParamAccess.list);
            pManager.AddPointParameter("Pts", "Pts", "Pts", GH_ParamAccess.list);
            pManager.AddNumberParameter("dist", "dist", "dist", GH_ParamAccess.list);
            pManager.AddCurveParameter("Test", "Test", "Test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //sets a stair start point
            Point3d start = new Point3d();
            DA.GetData(0, ref start);

            //stair count
            int strCount = new int();
            DA.GetData(1, ref strCount);

            //stair rise
            double strRise = new double();  
            DA.GetData(2, ref strRise);

            //stair run
            double strRun = new double();
            DA.GetData(3, ref strRun);

            //rail height
            double railH = new double();
            DA.GetData(4, ref railH);

            //max spacing
            double maxSpacing = new double();
            DA.GetData(5, ref maxSpacing);

            //offset
            double offset = new double();
            DA.GetData(6, ref offset);

            //Computes one step with given rise and run
            //Creates a point list for the one step curve

            List<Point3d> strPts = new List<Point3d>();
            strPts.Add(start);


            for (int i = 0; i < strCount; i++)

            {
                Point3d edgePt = new Point3d(start.X, start.Y + strRise, 0.0);
                Point3d runEndPt = new Point3d(edgePt.X + strRun, edgePt.Y, 0.0);
               
                strPts.Add(edgePt);
                strPts.Add(runEndPt);

                start = runEndPt;

            }

            //Creates stair crv
            Curve stairCrv = new PolylineCurve(strPts);
            DA.SetData("Stair Curve", stairCrv);


            //Creates list of edges point

            List<Point3d> edgesPts = new List<Point3d>();

            for (int i = 0; i < strPts.Count; i++)

              if (i % 2 ==1)

              {
                   edgesPts.Add(strPts[i]);
              }

            //Create a railing
            //Firstly, create a midpoint list for railing pillars

            List<Point3d> strRailing = new List<Point3d>();
           
            //Adds first point to the list
            strRailing.Add(new Point3d(edgesPts[0].X + (strRun / 2), edgesPts[0].Y, strPts[0].Z));

            //Adds last point to  the list
            Point3d last = edgesPts.Last();   
            
            //Add mid poits to the list
            for (int i = 0; i < edgesPts.Count; i++)
            {
                Point3d p = edgesPts[i];

                if (p.X - edgesPts[0].X >= maxSpacing) 
                {
                    strRailing.Add(new Point3d(p.X + (strRun / 2), p.Y, p.Z));                   
                    edgesPts[0] = p;
                }

            }

            //Removes the second last middpoint if the spacing between last two points is to close
            Point3d lastR = strRailing[strRailing.Count - 1];
            if (last.X - lastR.X <= strRun)
            {
                strRailing.Remove(lastR);
            }

            strRailing.Add(new Point3d(last.X + (strRun / 2), last.Y, last.Z));

            //DA.SetDataList("Pts", strRailing);



            //Creates pillars and top diagonal

            List<Curve>railCrv = new List<Curve>();
            List<Curve> pillars = new List<Curve>();

            List<Point3d> ptsTop = new List<Point3d>();

            foreach (Point3d p in strRailing)
            {
                ptsTop.Add(new Point3d(new Point3d (p.X, p.Y + railH, p.Z)));
                pillars.Add(new LineCurve(p, new Point3d(p.X, p.Y + railH, p.Z)));
                railCrv.Add(new LineCurve(p, new Point3d(p.X, p.Y + railH, p.Z)));

            }

            //New parametars

            Plane plane = new Plane();
            Curve topD = new LineCurve(ptsTop[0], ptsTop.Last());
            Curve tempBottD = new LineCurve(edgesPts[0], edgesPts[1]); //temporary bottom diagonal line
            tempBottD.Offset(plane, offset, 0.0, 0);

            Point3d newPt = new Point3d();

            Rhino.Geometry.Intersect.CurveIntersections intersection = Intersection.CurveCurve(tempBottD, railCrv[0], 0.001, 0.001);
            foreach (Rhino.Geometry.Intersect.IntersectionEvent s in intersection)
            {
                if (s.IsPoint)
                {
                    newPt = s.PointA;
                }
            }

            double newDist = strRailing[0].DistanceTo(newPt);

            //Cretaes bottom diagonal
            List<Point3d> ptsBott = new List<Point3d>();

            foreach (Point3d p in strRailing)
            {
                ptsBott.Add(new Point3d(new Point3d(p.X, p.Y + newDist, p.Z)));               
            }

            Curve bottD = new LineCurve(ptsBott[0], ptsBott.Last());

            railCrv.Add(bottD);

            railCrv.Add(topD);

            DA.SetDataList("Railing Curve", railCrv);

            //Creates vertical infill
         
            List<double> infillSpacing = new List<double>();
   
            for (int i = 0; i < strRailing.Count-1; i++)

            {
                Convert.ToDouble(i);

                double a = strRailing[i].X;
                double b = strRailing[i + 1].X;

                infillSpacing.Add(b - a);

            }

            DA.SetDataList("dist", infillSpacing);

            List<Point3d> ptsTopD = new List<Point3d>();
            List<Point3d> ptsBotD = new List<Point3d>();

            foreach (Curve pillar in pillars)
            {
                Rhino.Geometry.Intersect.CurveIntersections botSec = Intersection.CurveCurve(bottD, pillar, 0.0, 0.0);
                foreach (Rhino.Geometry.Intersect.IntersectionEvent s in botSec)
                {
                    if (s.IsPoint)
                    {
                        ptsBotD.Add(s.PointA);
                    }
                }
                Rhino.Geometry.Intersect.CurveIntersections topSec = Intersection.CurveCurve(topD, pillar, 0.0, 0.0);
                foreach (Rhino.Geometry.Intersect.IntersectionEvent s in topSec)
                {
                    if (s.IsPoint)
                    {
                        ptsTopD.Add(s.PointA);
                    }
                }
            }

            DA.SetDataList("Pts", ptsTopD);

            List<Curve> newDs1 = new List<Curve>();

            for (int i = 0; i <ptsBotD.Count-1; i++)
            {
                newDs1.Add(new LineCurve(ptsBotD[i], ptsBotD[i+1]));
            }

            
            List<Curve> newDs2 = new List<Curve>();

            for (int i = 0; i < ptsTopD.Count - 1; i++)
            {
                newDs2.Add(new LineCurve(ptsTopD[i], ptsTopD[i + 1]));
            }

            DA.SetDataList("Test", newDs2);

            Point3d[] pointsA; 

            for (int i = 0; i < newDs1.Count - 1; i++)

            {
                double interval = infillSpacing[i];
                Curve curve = newDs1[i];

                pointsA = curve.DivideEquidistant(100);
                
            }


            List<Point3d> points = new List<Point3d>();

            for (int i = 0;i < pointsA.Length; i++)
            






        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("27D26DC5-5FA7-42B6-8164-D8D09E7F660C"); }
        }
    }
}