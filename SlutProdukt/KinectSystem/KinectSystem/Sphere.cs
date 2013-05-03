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
    class Sphere
    {
        #region Private Variable Members
        private Circle[] sphereCirces;
        private int nSides;
        private int nStacks;
        private double radius;
        private Point3D centerCenter;
        #endregion Private Variable Members

        #region Constructor
        public Sphere(Point3D InputCenter, int Nsides, int Nstacks, Double InputRadius)
        {
            nSides = Nsides;
            nStacks = Nstacks;
            sphereCirces = new Circle[2 * nStacks - 1];
            centerCenter = InputCenter;
            radius = InputRadius;

            MessageBox.Show(sphereCirces.Length.ToString());

            for(int i = 0; i < nStacks; i++)
            {
                double zHeight = radius * ((double)(nStacks - 1 - i) / (nStacks - 1));
                Point3D centerOfCircle = new Point3D(centerCenter.X, centerCenter.Y, centerCenter.Z + zHeight);
                double radiusOfCircle = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(zHeight, 2));
                sphereCirces[i] = new Circle(nSides, centerOfCircle, radiusOfCircle);
            }

            for (int i = nStacks; i > 1; i--)
            {
                double zHeight = radius * ((double)(i - 1) / (nStacks - 1));
                Point3D centerOfCircle = new Point3D(centerCenter.X, centerCenter.Y, centerCenter.Z - (radius * ((double)(i - 1) / (nStacks - 1))));
                double radiusOfCircle = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(zHeight, 2));
                sphereCirces[(nStacks - 2) + i] = new Circle(nSides, centerOfCircle, radiusOfCircle);
            }
            MessageBox.Show(sphereCirces[0].Center.ToString());
            MessageBox.Show(sphereCirces[0].radiusX.ToString());
            MessageBox.Show(sphereCirces[1].Center.ToString());
            MessageBox.Show(sphereCirces[1].radiusX.ToString());
            MessageBox.Show(sphereCirces[2 * nStacks - 2].Center.ToString());
            MessageBox.Show(sphereCirces[2 * nStacks - 2].radiusX.ToString());
            MessageBox.Show(sphereCirces[nStacks - 1].Center.ToString());
            MessageBox.Show(sphereCirces[nStacks - 1].radiusX.ToString());
            MessageBox.Show(sphereCirces[nStacks - 3].Center.ToString());
            MessageBox.Show(sphereCirces[nStacks - 3].radiusX.ToString());
            MessageBox.Show(sphereCirces[nStacks].Center.ToString());
            MessageBox.Show(sphereCirces[nStacks].radiusX.ToString());
        }
        #endregion Constructor

        #region Methods
        public void addToMesh(MeshGeometry3D mesh, bool combineVertices)
        {
            if (sphereCirces[0].Points.Count > 2)
            {
                List<Point3D> ListOfPoints = new List<Point3D>();
                for (int i = 0; i <= 2*nStacks - 2; i++)
                {
                    foreach (Point3D p in sphereCirces[i].Points)
                    {
                        ListOfPoints.Add(p);
                    }
                    ListOfPoints.Add(sphereCirces[i].Points[0]);
                }

                MessageBox.Show(ListOfPoints.Count.ToString());
                MessageBox.Show(sphereCirces[0].Points.Count.ToString());
                for (int i = 0; i <= 2 * nStacks -3; i++)
                {
                    for (int j = 1; j < sphereCirces[i].Points.Count + 1 ; j++)
                    {
                        Triangle.addTriangleToMesh(ListOfPoints[(j - 1) + i * (sphereCirces[i].Points.Count + 1)],
                            ListOfPoints[(j - 1) + (i + 1) * (sphereCirces[i].Points.Count + 1)],
                            ListOfPoints[j + i * (sphereCirces[8].Points.Count + 1)], mesh, combineVertices);
                        Triangle.addTriangleToMesh(ListOfPoints[j + i * (sphereCirces[i].Points.Count + 1)],
                            ListOfPoints[(j - 1) + (i + 1) * (sphereCirces[i].Points.Count + 1)],
                            ListOfPoints[(j) + (i + 1) * (sphereCirces[i].Points.Count + 1)], mesh, combineVertices);
                    }
                }
            }
        }

        public GeometryModel3D createModel(Color color, bool combineVector)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addToMesh(mesh, combineVector);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D returnModel = new GeometryModel3D(mesh, material);

            //returnModel.BackMaterial = material;

            return returnModel;
        }
        #endregion Methods

        #region Properties
        public Point3D CenterCenter
        {
            get
            {
                return centerCenter;
            }
        }
        #endregion Properties

    }
}
