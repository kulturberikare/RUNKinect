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

    public partial class SkeletonViewer : UserControl
    {
        #region Member Variables
        // private KinectSensor _Kinect; //Saknas
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Pink, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Green };
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables

        #region Constructor

        public SkeletonViewer()
        {
            InitializeComponent();
        }
        

        #endregion Constructor

        #region Methods

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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

            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }


        private Point GetJointPoint(Joint joint)
        {

            DepthImagePoint point = this.KinectSensorOne.MapSkeletonPointToDepth(joint.Position, this.KinectSensorOne.DepthStream.Format);
            point.X *= (int)this.LayoutRootOne.ActualWidth / KinectSensorOne.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRootOne.ActualHeight / KinectSensorOne.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
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