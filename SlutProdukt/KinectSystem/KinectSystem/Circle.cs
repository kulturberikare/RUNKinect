using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Windows.Forms;


namespace Create3DWorld
{
    class Circle
    {
        #region Private Variable Member
        private int nSides = 6;
        private double angle;
        private Point3D center;
        private Point3D top;
        private List<Point3D> points;
        public double radiusX;
        private double radiusY;
        #endregion Private Variable Member

        #region Constructor
        public Circle(int Nsides, Point3D InputCenter, double InpuRadius)
        {
            nSides = Nsides;
            angle = (double)360.0 / (double)nSides;

            center = new Point3D(InputCenter.X, InputCenter.Y, InputCenter.Z);

            radiusX = InpuRadius;
            radiusY = InpuRadius;

            makeCircle();
        }

        public Circle(int Nsides, Point3D InputCenter, double InputRadiusX, double InputRadiusY)
        {
            nSides = Nsides;
            angle = (double)360.0 / (double)nSides;

            center = new Point3D(InputCenter.X, InputCenter.Y, InputCenter.Z);

            radiusX = InputRadiusX;
            radiusY = InputRadiusY;

            makeCircle();
        }
        #endregion Constructor

        #region Methods
        private void makeCircle()
        {
            points = new List<Point3D>();
            top = new Point3D(center.X, center.Y + radiusY, center.Z);
            points.Add(top);

            for (int i = 1; i < nSides; i++)
            {
                Point3D workPoint = AngularHelpingTools.RotatePointXY(top, Center, AngularHelpingTools.DegreesToRadians(angle * i));

                if (radiusX != radiusY)
                {
                    double diff = workPoint.X - center.X;
                    diff *= radiusX;
                    diff /= radiusY;
                    workPoint = new Point3D(center.X + diff, workPoint.Y, workPoint.Z);
                }
                points.Add(workPoint);
            }
        }

        public void RotateYZ(Point3D rotationPoint, double radians)
        {
            List<Point3D> newList = new List<Point3D>();

            foreach(Point3D p in Points)
            {
                newList.Add(AngularHelpingTools.RotatPointYZ(p, rotationPoint, radians));
            }
            center = AngularHelpingTools.RotatPointYZ(Center, rotationPoint, radians);
            points = newList;
        }

        public void RotateXY(Point3D rotationPoint, double radians)
        {
            List<Point3D> newList = new List<Point3D>();

            foreach (Point3D p in Points)
            {
                newList.Add(AngularHelpingTools.RotatePointXY(p, rotationPoint, radians));
            }
            center = AngularHelpingTools.RotatePointXY(Center, rotationPoint, radians);
            points = newList;
        }

        public void RotateXZ(Point3D rotationPoint, double radians)
        {
            List<Point3D> newList = new List<Point3D>();

            foreach (Point3D p in points)
            {
                newList.Add(AngularHelpingTools.RotatePointXZ(p, rotationPoint, radians));
            }
            center = AngularHelpingTools.RotatePointXZ(Center, rotationPoint, radians);
            points = newList;
        }

        public void addToMesh(MeshGeometry3D mesh, bool combineVertices)
        {
            if (points.Count > 2)
            {
                List<Point3D> temp = new List<Point3D>();

                foreach (Point3D p in Points)
                {
                    temp.Add(p);
                }

                temp.Add(points[0]);

                for (int i = 1; i < temp.Count; i++)
                {
                    Triangle.addTriangleToMesh(temp[i], Center, temp[i - 1], mesh, combineVertices);
                }
            }
        }

        public GeometryModel3D createModel(Color color, bool combineVertices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, combineVertices);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D returnModel = new GeometryModel3D(mesh, material);

            MessageBox.Show("Jag är här");

            return returnModel;
        }

        public GeometryModel3D createTwoSidedModel(Color color, bool combineVertices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, combineVertices);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D returnModel = new GeometryModel3D(mesh, material);

            returnModel.BackMaterial = material;

            return returnModel;
        }

        public static GeometryModel3D creatCircleModel(int Nsides, Point3D InputCenter, double InputRadius)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            Circle circle = new Circle(Nsides, InputCenter, InputRadius);

            circle.addToMesh(mesh, false);

            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.White));

            GeometryModel3D productModel = new GeometryModel3D(mesh, material);

            return productModel;
        }
        #endregion Methods

        #region Properties
        public Point3D Center
        {
            get
            {
                return center;
            }
        }

        public List<Point3D> Points
        {
            get
            {
                return points;
            }
        }
        #endregion Properties
    }
}
