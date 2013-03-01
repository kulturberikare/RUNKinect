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

namespace KinectTheDotsKR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>   

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private Skeleton[] _FrameSkeletons;
        private readonly Brush[] _SkeletonBrushes;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this._SkeletonBrushes = new[] {Brushes.Black, Brushes.Crimson, Brushes.Indigo,
                                           Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
        #endregion Constructor

        #region Methods
        private void KinectSensors_StatusChanged(object sendet, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    this.Kinect = e.Sensor;
                    break;

                case KinectStatus.Disconnected:
                    MessageBox.Show("Plug in Kinect");
                    this.Kinect = null;
                    break;
                default:
                    MessageBox.Show("Error!");
                    break;
            }
        }
        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    frame.CopySkeletonDataTo(this._FrameSkeletons);
                    Skeleton skeleton = GetPrimarySkeleton(this._FrameSkeletons);

                    if (skeleton == null)
                    {
                        HandCursorElement.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Joint primaryHand = GetPrimaryHand(skeleton);
                        TrackHand(primaryHand);
                    }
                }
            }
        }

        private static Skeleton GetPrimarySkeleton(Skeleton[] skeletons)
        {
            Skeleton skeleton = null;

            if (skeletons != null)
            {
                //Find the closest skeleton
                for (int i = 0; i < skeletons.Length; i++)
                {
                    if (skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        if (skeleton == null)
                        {
                            skeleton = skeletons[i];
                        }
                        else
                        {
                            if (skeleton.Position.Z > skeletons[i].Position.Z)
                            {
                                skeleton = skeletons[i];
                            }
                        }
                    }

                }
            }
            return skeleton;
        }

        private static Joint GetPrimaryHand(Skeleton skeleton)
        {
            Joint primaryHand = new Joint();

            if (skeleton != null)
            {
                primaryHand = skeleton.Joints[JointType.HandLeft];
                Joint rightHand = skeleton.Joints[JointType.HandRight];

                if (rightHand.TrackingState != JointTrackingState.NotTracked)
                {
                    primaryHand = rightHand;
                }
                else
                {
                    if (primaryHand.Position.Z > rightHand.Position.Z)
                    {
                        primaryHand = rightHand;
                    }
                }
            }
            return primaryHand;
        }

        private void TrackHand(Joint hand)
        {
            if (hand.TrackingState == JointTrackingState.NotTracked)
            {
                HandCursorElement.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                HandCursorElement.Visibility = System.Windows.Visibility.Visible;

                float x;
                float y;

                DepthImagePoint point = this.Kinect.MapSkeletonPointToDepth(hand.Position, DepthImageFormat.Resolution640x480Fps30);
                point.X = (int)((point.X * LayoutRoot.ActualWidth / this.Kinect.DepthStream.FrameWidth) - (HandCursorElement.ActualWidth / 2.0));
                point.Y = (int)((point.Y * LayoutRoot.ActualWidth / this.Kinect.DepthStream.FrameHeight) - (HandCursorElement.ActualHeight / 2.0));

                Canvas.SetLeft(HandCursorElement, x);
                Canvas.SetTop(HandCursorElement, y);

                if (hand.ID == JointType.HandRight)
                {
                    HandCursorScale.ScaleX = 1;
                }
                else
                {
                    HandCursorScale.ScaleX = -1;
                }
            }
        }

        #endregion Methods

        #region Propertes
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
                        //this._FrameSkeletons = null;
                    }
                    this._Kinect = value;

                    //Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            this._Kinect.SkeletonStream.Enable();
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
