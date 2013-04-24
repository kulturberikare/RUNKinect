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

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class SkeletonViewerTwo : UserControl
    {
        #region Member Variables
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Pink, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Green };
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables

        #region Constructor
        public SkeletonViewerTwo()
        {
            InitializeComponent();
        }        
        #endregion Constructor

        #region Methods

        public void Kinect_SkeletonFrameReadyTwo(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonsPanelTwo.Children.Clear();

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
                SkeletonsPanelTwo.Children.Add(figure);

                figure = CreateFigure(skeleton, brush, new[] { JointType.HipLeft, JointType.HipRight });
                SkeletonsPanelTwo.Children.Add(figure);

                //Draw left leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                SkeletonsPanelTwo.Children.Add(figure);

                //Draw right leg
                figure = CreateFigure(skeleton, brush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                SkeletonsPanelTwo.Children.Add(figure);

                //Draw left arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                SkeletonsPanelTwo.Children.Add(figure);

                //Draw right arm
                figure = CreateFigure(skeleton, brush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                SkeletonsPanelTwo.Children.Add(figure);
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

            DepthImagePoint point = this.KinectSensorTwo.MapSkeletonPointToDepth(joint.Position, this.KinectSensorTwo.DepthStream.Format);
            point.X *= (int)this.LayoutRootTwo.ActualWidth / KinectSensorTwo.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRootTwo.ActualHeight / KinectSensorTwo.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }
        #endregion Methods

        #region Properties
        #region KinectSensorTwo
        protected const string KinectDevicePropertyName = "KinectSensorTwo";
        public static readonly DependencyProperty KinectDeviceProperty = DependencyProperty.Register(KinectDevicePropertyName, typeof(KinectSensor), typeof(SkeletonViewerTwo), new PropertyMetadata(null, KinectDeviceChanged));


        private static void KinectDeviceChanged(DependencyObject owner, DependencyPropertyChangedEventArgs e)
        {
            SkeletonViewerTwo viewer = (SkeletonViewerTwo)owner;

            if (e.OldValue != null)
            {
                ((KinectSensor)e.OldValue).SkeletonFrameReady -= viewer.Kinect_SkeletonFrameReadyTwo;
                viewer._FrameSkeletons = null;
            }

            if (e.NewValue != null)
            {
                viewer.KinectSensorTwo = (KinectSensor)e.NewValue;
                viewer.KinectSensorTwo.SkeletonFrameReady += viewer.Kinect_SkeletonFrameReadyTwo;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectSensorTwo.SkeletonStream.FrameSkeletonArrayLength];
            }
        }


        public KinectSensor KinectSensorTwo
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion KinectSensorTwo
        #endregion Properties
    }
}