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
    class Cube
    {
        #region Private Member Variables
        private Point3D orgin;
        private double width;
        private double height;
        private double depth;
        #endregion Private Member Variables

        #region Constructor
        public Cube(Point3D P0, double w, double h, double d)
        {
            width = w;
            height = h;
            depth = d;

            orgin = P0;
        }
        #endregion Constructor

        #region Methods

        public Point3D center()
        {
            Point3D c = new Point3D(orgin.X + (width / 2),
                                    orgin.Y - (height / 2),
                                    orgin.Z + (depth / 2));
            return c;
        }
        public static void addCubeToMesh(Point3D p0, double w, double h, double d, MeshGeometry3D mesh)
        {
            Cube cube = new Cube(p0, w, h, d);

            double maxDimension = Math.Max(d, Math.Max(w, h));

            Rectangle front = cube.Front();
            Rectangle back = cube.Back();
            Rectangle right = cube.Right();
            Rectangle left = cube.Left();
            Rectangle top = cube.Top();
            Rectangle bottom = cube.Bottom();

            front.addToMesh(mesh);
            back.addToMesh(mesh);
            right.addToMesh(mesh);
            left.addToMesh(mesh);
            top.addToMesh(mesh);
            bottom.addToMesh(mesh);
        }

        public static GeometryModel3D CreateCubeModel(Point3D P0, double w, double h, double d, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            addCubeToMesh(P0, w, h, d, mesh);

            Material material = new DiffuseMaterial(new SolidColorBrush(color));

            GeometryModel3D model = new GeometryModel3D(mesh, material);

            return model;
        }

        public GeometryModel3D CreateModel(Color color)
        {
            return CreateCubeModel(orgin, width, height, depth, color);
        }

        public Rectangle Front()
        {
            Rectangle rec = new Rectangle(orgin, width, height, 0);

            return rec; 

        }

        public Rectangle Back()
        {
            Rectangle rec = new Rectangle(new Point3D(orgin.X + width, orgin.Y, orgin.Z + depth), -width, height, 0);

            return rec;
        }

        public Rectangle Left()
        {
            Rectangle rec = new Rectangle(new Point3D(orgin.X, orgin.Y, orgin.Z + depth), 0, height, -depth);

            return rec;
        }

        public Rectangle Right()
        {
            Rectangle rec = new Rectangle(new Point3D(orgin.X + width, orgin.Y, orgin.Z), 0, height, depth);

            return rec;
        }

        public Rectangle Top()
        {
            Rectangle rec = new Rectangle(orgin, width, 0, depth);

            return rec;
        }

        public Rectangle Bottom()
        {
            Rectangle rec = new Rectangle(new Point3D(orgin.X + width, orgin.Y - height, orgin.Z), -width, 0, depth);

            return rec;
        }
        #endregion Methods
    }
}
