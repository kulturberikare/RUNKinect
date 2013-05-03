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
    class Rectangle
    {
        #region Private Member Variables
        private Point3D p0;
        private Point3D p1;
        private Point3D p2;
        private Point3D p3;
        #endregion Private Memner Variables

        #region Constructor
        public Rectangle(Point3D P0, Point3D P1, Point3D P2, Point3D P3)
        {
            p0 = P0;
            p1 = P1;
            p2 = P2;
            p3 = P3;
        }

        public Rectangle(Point3D P0, double w, double h, double d)
        {
            p0 = P0;

            //framsida/baksida
            if (w != 0.0 && h != 0.0)
            {
                p1 = new Point3D(p0.X + w, p0.Y, p0.Z);
                p2 = new Point3D(p0.X + w, p0.Y - h, p0.Z);
                p3 = new Point3D(p0.X, p0.Y - h, p0.Z);
            }
            //översida/undersida 
            else if (w != 0 && d != 0.0)
            {
                p1 = new Point3D(p0.X, p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X + w, p0.Y, p0.Z + d);
                p3 = new Point3D(p0.X + w, p0.Y, p0.Z);
            }
            // sida/sida
            else if (h != 0.0 && d != 0.0)
            {
                p1 = new Point3D(p0.X, p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X, p0.Y - h, p0.Z + d);
                p3 = new Point3D(p0.X, p0.Y - h, p0.Z);
            }
        }
        #endregion Constructor

        # region Methods
        public static void addRectangleToMesh(Point3D p0, Point3D p1, Point3D p2, Point3D p3, MeshGeometry3D mesh)
        {
            Triangle.addTriangleToMesh(p0, p1, p2, mesh);
            Triangle.addTriangleToMesh(p2, p3, p0, mesh);
        }

        public void addToMesh(MeshGeometry3D mesh)
        {
            Triangle.addTriangleToMesh(p0, p1, p2, mesh);
            Triangle.addTriangleToMesh(p2, p3, p0, mesh);
        }
        #endregion Methods
    }
}
