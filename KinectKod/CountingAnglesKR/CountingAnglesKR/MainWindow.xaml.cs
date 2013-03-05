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
using System.Windows.Media.Media3D;

using Microsoft.Kinect;
using System.IO;

namespace SkeletontrackingKR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private const float FeetPerMeters = 3.2808399f;
        private KinectSensor _Kinect;
        private readonly Brush[] _SkeletonBrushes;
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this._SkeletonBrushes = new[] {Brushes.Black, Brushes.Crimson, Brushes.Indigo,
                                           Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);


            
            //this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            //this.Unloaded += (s, e) => { this.Kinect = null; };
        }

        #endregion Constructor

        #region Methods
        //private void DiscoverKinectSensor()
        //{

        //}

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                case KinectStatus.Initializing:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.Kinect = e.Sensor;
                    break;

                case KinectStatus.Disconnected:
                    MessageBox.Show("Plug in Kinect Device!");
                    this.Kinect = null;
                    break;

                default:
                    MessageBox.Show("//TODO Show an error state");
                    break;
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Polyline figure;
                    Brush userBrush;
                    Skeleton skeleton;

                    LayoutRoot.Children.Clear();
                    frame.CopySkeletonDataTo(this._FrameSkeletons);

                    

                    for (int i = 0; i < this._FrameSkeletons.Length; i++)
                    {
                        skeleton = this._FrameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            userBrush = this._SkeletonBrushes[i % this._SkeletonBrushes.Length];

                            //DrawSkeletons head and torso
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.Head, JointType.ShoulderCenter,
                                                    JointType.ShoulderLeft, JointType.Spine,
                                                    JointType.ShoulderRight, JointType.ShoulderCenter,
                                                    JointType.HipCenter, JointType.HipLeft,
                                                    JointType.Spine, JointType.HipRight, 
                                                    JointType.HipCenter});
                            LayoutRoot.Children.Add(figure);

                            //draws SkeletonsLeft left leg
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.HipLeft, JointType.KneeLeft,
                                                    JointType.AnkleLeft, JointType.FootLeft});
                            LayoutRoot.Children.Add(figure);

                            //Draws skeletons right leg
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.HipRight, JointType.KneeRight,
                                                    JointType.AnkleRight, JointType.AnkleRight});
                            LayoutRoot.Children.Add(figure);

                            //Draws skeletons left arm
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.ShoulderLeft, JointType.ElbowLeft,
                                                    JointType.WristLeft, JointType.HandLeft});
                            LayoutRoot.Children.Add(figure);

                            //Draws skeletons right arm
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.ShoulderRight, JointType.ElbowRight,
                                                    JointType.WristRight, JointType.HandRight});
                            LayoutRoot.Children.Add(figure);
                        }
                    }
                }
            }
        }

        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {

            Polyline figure = new Polyline();
            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
                FindAngles(skeleton);
            }
            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {
            
            DepthImagePoint point = this.Kinect.MapSkeletonPointToDepth(joint.Position, this.Kinect.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / Kinect.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / Kinect.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        private Vector3D GetJointPoint3D(Joint joint)
        {
          //  MessageBox.Show("GetJointPoint3D");
            return new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }
        //Hittar vinkeln i knät.
        private void FindAngles(Skeleton skeleton)
        {
            
            Vector3D vector1 = GetJointPoint3D(skeleton.Joints[JointType.AnkleLeft]);
            Vector3D vector2 = GetJointPoint3D(skeleton.Joints[JointType.KneeLeft]);
            Vector3D vector3 = GetJointPoint3D(skeleton.Joints[JointType.HipLeft]);

            Vector3D a1 = vector1 - vector2;
            Vector3D a2 = vector3 - vector2;

           Double angle = Vector3D.AngleBetween(a1, a2);
            MessageBox.Show(Convert.ToString(angle));

            Angle.Text = string.Format("{0}", angle); //Fungerar inte riktigt MEN vinklarna räknas ut på rätt sätt
        }

        #endregion Methods

        #region Properties
        public KinectSensor Kinect
        {
            get { return this._Kinect; }

            set
            {
                if (this._Kinect != value)
                {
                    //Uninitialize
                    if (this._Kinect != null)
                    {
                        this._Kinect.Stop();
                        this._Kinect.SkeletonFrameReady -= Kinect_SkeletonFrameReady;
                        this._Kinect.SkeletonStream.Disable();
                        //  this._Kinect.DepthStream.Disable();
                        this._FrameSkeletons = null;
                        // this._Kinect = null;
                    }

                    this._Kinect = value;

                    //Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            //this._Kinect = value;
                            this._Kinect.SkeletonStream.Enable();
                            //   this._Kinect.DepthStream.Enable();
                            this._FrameSkeletons = new Skeleton[this._Kinect.SkeletonStream.FrameSkeletonArrayLength];
                            this._Kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
                            this._Kinect.Start();
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}
