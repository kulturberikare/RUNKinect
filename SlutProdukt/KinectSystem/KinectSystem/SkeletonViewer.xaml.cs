using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.IO;
using System.Windows.Media.Media3D;

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class SkeletonViewer : UserControl
    {
        #region Member Variables
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Pink, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Green };
        private Skeleton[] _FrameSkeletons;

        private double count = 0;
        public double AngleRK = 0;
        public double AngleLK = 0;
        public double AngleRA = 0;
        public double AngleLA = 0;
        public double AngleH = 0;
        #endregion Member Variables

        #region Constructor

        public SkeletonViewer()
        {
            InitializeComponent();
        }
        

        #endregion Constructor

        #region Methods

        public void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonsPanel.Children.Clear();

            if (this.IsEnabled)
            {
                using (SkeletonFrame frame = e.OpenSkeletonFrame())
                {
                    if (frame != null)
                    {
                        if (this.IsEnabled)
                        {
                            frame.CopySkeletonDataTo(this._FrameSkeletons);

                                for (int i = 0; i < this._FrameSkeletons.Length; i++)
                                {
                                    DrawSkeleton(this._FrameSkeletons[i], this._SkeletonBrushes[i]);

                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HandLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HandRight], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HipLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.HipRight], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.KneeLeft], this._SkeletonBrushes[i]);
                                    TrackJoint(this._FrameSkeletons[i].Joints[JointType.KneeRight], this._SkeletonBrushes[i]);
                                }
                        }
                    }
                }
            }
        }


        private void DrawSkeleton(Skeleton skeleton, Brush brush)
        {
            if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                //Draw head and torso
                Polyline figure = CreateFigure(skeleton, brush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                             JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter});
                SkeletonsPanel.Children.Add(figure);

                figure = CreateFigure(skeleton, brush, new[] { JointType.HipLeft, JointType.HipRight });
                SkeletonsPanel.Children.Add(figure);

                //Draw left leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                SkeletonsPanel.Children.Add(figure);

                //Draw left arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonsPanel.Children.Add(figure);

                //Draw right arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonsPanel.Children.Add(figure);

                DefineVectors(skeleton);
                count++;
            }
        }


        private void TrackJoint(Joint joint, Brush brush)
        {
            if (joint.TrackingState != JointTrackingState.NotTracked)
            {
                Point jointPoint = GetJointPoint(joint);
            }
        }

        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 4;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }


        private Point GetJointPoint(Joint joint)
        {

            DepthImagePoint point = KinectSensorOne.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.KinectSensorOne.DepthStream.Format);
            point.X *= (int)this.LayoutRootOne.ActualWidth / KinectSensorOne.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRootOne.ActualHeight / KinectSensorOne.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        //Definierar vektorer som används för att plocka fram vinkeln
        private void DefineVectors(Skeleton skeleton)
        {
            //Definierar aktuella leder och skapar en vektor med ledens x-, y-, z-position mha GetJointPoint3D.
            Vector3D ankleL = GetJointPoint3D(skeleton.Joints[JointType.AnkleLeft]);
            Vector3D ankleR = GetJointPoint3D(skeleton.Joints[JointType.AnkleRight]);
            Vector3D footL = GetJointPoint3D(skeleton.Joints[JointType.FootLeft]);
            Vector3D footR = GetJointPoint3D(skeleton.Joints[JointType.FootRight]);
            Vector3D hipL = GetJointPoint3D(skeleton.Joints[JointType.HipLeft]);
            Vector3D hipR = GetJointPoint3D(skeleton.Joints[JointType.HipRight]);
            Vector3D hipCenter = GetJointPoint3D(skeleton.Joints[JointType.HipCenter]);
            Vector3D kneeL = GetJointPoint3D(skeleton.Joints[JointType.KneeLeft]);
            Vector3D kneeR = GetJointPoint3D(skeleton.Joints[JointType.KneeRight]);

            //Definierar vinklar för liveutskrift och sparning till fil
            AngleRK = FindAngles(ankleR, kneeR, hipR);
            AngleLK = FindAngles(ankleL, kneeL, hipL);
            AngleRA = FindAngles(footR, ankleR, kneeR);
            AngleLA = FindAngles(footL, ankleL, kneeL);
            AngleH = FindAngles(hipL, hipCenter, hipR);

            //Hittar vinkeln i en led mha FindAngles och skickar sedan respektive led till respektive fil 
            WriterLK(AngleLK);
            WriterRK(AngleRK);
            WriterLA(AngleLA);
            WriterRA(AngleRA);
            WriterH(AngleH);
        }

        //Skapar en 3D-vektor med en leds x-, y-, z-position.
        private Vector3D GetJointPoint3D(Joint joint)
        {
            return new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        //Hittar vinkeln i en led utiftrån 3 punkter.
        private double FindAngles(Vector3D vector1, Vector3D vector2, Vector3D vector3)
        {
            Vector3D a1 = vector1 - vector2;
            Vector3D a2 = vector3 - vector2;

            return Vector3D.AngleBetween(a1, a2);
        }

        //Skriver vinkeln i vänster knä till en textfil som heter LeftKnee
        private void WriterLK(double angle)
        {
            if (!File.Exists("LeftKnee.txt"))
            {
                StreamWriter file = new StreamWriter("LeftKnee.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("LeftKnee.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i höger knä till en textfil som heter RightKnee
        private void WriterRK(double angle)
        {
            if (!File.Exists("RightKnee.txt"))
            {
                StreamWriter file = new StreamWriter("RightKnee.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightKnee.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriv vinkeln i höger fotled till en textfil som heter RightAnkle
        private void WriterRA(double angle)
        {
            if (!File.Exists("RightAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("RightAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightAnkle.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i vänster fotled till en textfil som heter LeftAnkle
        private void WriterLA(double angle)
        {
            if (!File.Exists("LeftAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("LeftAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("LeftAnkle.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }

        //Skriver vinkeln i höftern till en textfil som heter Hip
        private void WriterH(double angle)
        {
            if (!File.Exists("Hip.txt"))
            {
                StreamWriter file = new StreamWriter("Hip.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("Hip.txt", true))
                {
                    string _angle = Convert.ToString(Math.Round(angle));

                    if (_angle != "NaN")
                    {
                        file.WriteLine(_angle + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }

                    else
                    {
                        file.WriteLine("0" + ":" + Convert.ToString(count) + ";");
                        file.Close();
                    }
                }
            }
        }
        #endregion Methods

        #region Properties
        #region KinectSensorOne
        protected const string KinectDevicePropertyName = "KinectSensorOne";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewer), new PropertyMetadata(null, KinectDeviceChanged));


        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewer viewer = (SkeletonViewer)owner;

            if (e.OldValue != null)
            {
                ((KinectSensor)e.OldValue).SkeletonFrameReady -= viewer.Kinect_SkeletonFrameReady;
                viewer._FrameSkeletons = null;
            }

            if (e.NewValue != null)
            {
                viewer.KinectSensorOne = (KinectSensor)e.NewValue;
                viewer.KinectSensorOne.SkeletonFrameReady += viewer.Kinect_SkeletonFrameReady;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectSensorOne.SkeletonStream.FrameSkeletonArrayLength];
            }
        }


        public KinectSensor KinectSensorOne
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion KinectSensorOne
        #endregion Properties
    }
}