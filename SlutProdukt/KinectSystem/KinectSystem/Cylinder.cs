using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Create3DWorld
{
    class Cylinder
    {
        #region Private Member variabels
        private Circle uppSide;
        private Circle downSide;
        private int nSides;
        private double radiusUpp;
        private double radiusDown;
        private double length;
        private Point3D centerUpp;
        private Point3D centerDown;
        #endregion Private Member Variabels

        #region Constructor
        public Cylinder(Point3D InputCenterUpp, int Nsides, double RadiusUpp, double RadiusDown, double Length)
        {
            centerUpp = InputCenterUpp;
            nSides = Nsides;
            radiusUpp = RadiusUpp;
            radiusDown = RadiusDown;
            length = Length;

            uppSide = new Circle(nSides, centerUpp, radiusUpp);

            centerDown = new Point3D(centerUpp.X, centerUpp.Y, centerUpp.Z - length);

            downSide = new Circle(nSides, centerDown, radiusDown);
        }

        public Cylinder(Point3D InputCenterUpp, int Nsides, double InputRadiusUpp, Double InputRadiusDown, double Lengt,
            Point3D rotationPoint, double RotationInRadians)
        {
            centerUpp = InputCenterUpp;
            nSides = Nsides;
            radiusUpp = InputRadiusUpp;
            radiusDown = InputRadiusDown;
            length = Lengt;

            uppSide = new Circle(nSides, centerUpp, radiusUpp);
            centerDown = new Point3D(centerUpp.X, centerUpp.Y, centerUpp.Z - length);
            downSide = new Circle(nSides, centerDown, radiusDown);

            RotateYZ(rotationPoint, RotationInRadians);
        }
        #endregion Constructor

        #region Methods
        public void RotateYZ(Point3D rotationPoint, double rotationInRadians)
        {
            uppSide.RotateYZ(rotationPoint, rotationInRadians);
            downSide.RotateYZ(rotationPoint, rotationInRadians);
            centerDown = AngularHelpingTools.RotatPointYZ(centerDown, rotationPoint, rotationInRadians);
        }

        public void RotateXZ(Point3D rotationPoint, double rotationInRadians)
        {
            uppSide.RotateXZ(rotationPoint, rotationInRadians);
            downSide.RotateXZ(rotationPoint, rotationInRadians);
            centerDown = AngularHelpingTools.RotatePointXZ(centerDown, rotationPoint, rotationInRadians);
        }

        public void RotateXY(Point3D rotationPoint, double rotationInRadians)
        {
            uppSide.RotateXY(rotationPoint, rotationInRadians);
            downSide.RotateXY(rotationPoint, rotationInRadians);
            centerDown = AngularHelpingTools.RotatePointXY(centerDown, rotationPoint, rotationInRadians);
        }

        public void addToMesh(MeshGeometry3D mesh)
        {
            addToMesh(mesh, false, false);
        }

        public void addToMesh(MeshGeometry3D mesh, bool encloseLoop, bool combineVertices)
        {
            if (uppSide.Points.Count > 2)
            {
                List<Point3D> uppSidePoints = new List<Point3D>();
                foreach (Point3D p in uppSide.Points)
                {
                    uppSidePoints.Add(p);
                }
                uppSidePoints.Add(uppSide.Points[0]);
                List<Point3D> downSidePoints = new List<Point3D>();
                foreach (Point3D p in downSide.Points)
                {
                    downSidePoints.Add(p);
                }
                downSidePoints.Add(downSide.Points[0]);

                for (int i = 1; i < uppSidePoints.Count; i++)
                {
                    Triangle.addTriangleToMesh(uppSidePoints[i - 1], downSidePoints[i - 1], uppSidePoints[i], mesh, combineVertices);
                    Triangle.addTriangleToMesh(uppSidePoints[i], downSidePoints[i - 1], downSidePoints[i], mesh, combineVertices);
                }

                if (encloseLoop)
                {
                    uppSide.addToMesh(mesh, false);
                    downSide.addToMesh(mesh, false);
                }
            }
        }

        public GeometryModel3D createModel(Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D returnModel = new GeometryModel3D(mesh, material);

            return returnModel;
        }

        public GeometryModel3D createModel(Color color, bool encloseLoop, bool combineVector)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, encloseLoop, combineVector);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D returnModel = new GeometryModel3D(mesh, material);

            returnModel.BackMaterial = material;

            return returnModel;
        }
        #endregion Methods

        #region Properties
        public Point3D CenterUpp
        {
            get
            {
                return centerUpp;
            }
        }
        #endregion Properties
    }
}
