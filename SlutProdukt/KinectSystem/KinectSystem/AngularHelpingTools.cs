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
    class AngularHelpingTools
    {
        #region Member Variables
        static double OneRadInDegrees = (double)360.0 / (2 * Math.PI);
        #endregion Member Variables

        #region Methods
        public static Point3D RotatePointXY(Point3D point, Point3D rotationPoint, double radians)
        {
            Point3D newPoint = new Point3D(rotationPoint.X, rotationPoint.Y, rotationPoint.Z);

            if (radians != 0)
            {
                double xDiff = point.X - rotationPoint.X;
                double yDiff = point.Y - rotationPoint.Y;

                double xd = (xDiff * Math.Cos(radians) - yDiff * Math.Sin(radians));
                double yd = (xDiff * Math.Sin(radians) + (yDiff * Math.Cos(radians)));

                newPoint.X += xd;
                newPoint.Y += yd;
                newPoint.Z = point.Z;
            }
            else
            {
                newPoint.X = point.X;
                newPoint.Y = point.Y;
                newPoint.Z = point.Z;
            }
            return newPoint;
        }

        public static Point3D RotatePointXZ(Point3D point, Point3D rotationPoint, double radians)
        {
            Point3D newPoint = new Point3D(rotationPoint.X, rotationPoint.Y, rotationPoint.Z);

            if(radians != 0)
            {
                double xDiff = point.X - rotationPoint.X;
                double zDiff = point.Z - rotationPoint.Z;

                double xd = (xDiff * Math.Cos(radians)) - (zDiff * Math.Sin(radians));
                double zd = (xDiff * Math.Sin(radians)) + (zDiff * Math.Cos(radians));

                newPoint.X += xd;
                newPoint.Y = point.Y;
                newPoint.Z += zd;
            }
            else
            {
                newPoint.X = point.X;
                newPoint.Y = point.Y;
                newPoint.Z = point.Z;
            }
            return newPoint;
        }

        public static Point3D RotatPointYZ(Point3D point, Point3D rotationPoint, double radians)
        {
            Point3D newPoint = new Point3D(rotationPoint.X, rotationPoint.Y, rotationPoint.Z);

            if (radians != 0)
            {
                double yDiff = point.Y - rotationPoint.Y;
                double zDiff = point.Z - rotationPoint.Z;

                double yd = (zDiff * Math.Sin(radians)) + (yDiff * Math.Cos(radians));
                double zd = (zDiff * Math.Cos(radians)) - (yDiff * Math.Sin(radians));

                newPoint.X = point.X;
                newPoint.Y += yd;
                newPoint.Z += zd;
            }
            else
            {
                newPoint.X = point.X;
                newPoint.Y = point.Y;
                newPoint.Z = point.Z;
            }
            return newPoint;
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees / OneRadInDegrees;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * OneRadInDegrees;
        }
        #endregion Methods
    }
}
