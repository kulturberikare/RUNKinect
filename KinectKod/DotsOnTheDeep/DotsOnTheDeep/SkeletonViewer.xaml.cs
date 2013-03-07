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

namespace DotsOnTheDeep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class SkeletonViewer : UserControl
    {
        #region Member Variables
        // private KinectSensor _Kinect; //Saknas
        private readonly Brush[] _SkeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
        private Skeleton[] _FrameSkeletons;
        private const float FeetPerMeters = 3.2808399f;
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
            JointInfoPanel.Children.Clear();

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
                Canvas container = new Canvas();
                Point jointPoint = GetJointPoint(joint);

                //FeetPerMeters is a class constant of 3.2808399f;
                double z = joint.Position.Z * FeetPerMeters;

                Ellipse element = new Ellipse();
                element.Height = 10;
                element.Width = 10;
                element.Fill = brush;
                Canvas.SetLeft(element, 0 - (element.Width / 2));
                Canvas.SetTop(element, 0 - (element.Height / 2));
                container.Children.Add(element);

                TextBlock positionText = new TextBlock();
                positionText.Text = string.Format("<{0:0.00}, {1:0.00}, {2:0.00}>", jointPoint.X, jointPoint.Y, z);

                positionText.Foreground = brush;
                positionText.FontSize = 24;
                Canvas.SetLeft(positionText, 0 - (positionText.Width / 2));
                Canvas.SetTop(positionText, 25);
                container.Children.Add(positionText);

                Canvas.SetLeft(container, jointPoint.X);
                Canvas.SetTop(container, jointPoint.Y);
                JointInfoPanel.Children.Add(container);
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

            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position, this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }
        #endregion Methods

        #region Properties
        #region KinectDevice
        protected const string KinectDevicePropertyName = "KinectDevice";
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
                viewer.KinectDevice = (KinectSensor)e.NewValue;
                viewer.KinectDevice.SkeletonFrameReady += viewer.Kinect_SkeletonFrameReady;
                viewer._FrameSkeletons = new Skeleton[viewer.KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
            }
        }


        public KinectSensor KinectDevice
        {
            get { return (KinectSensor)GetValue(KinectDeviceProperty); }
            set { SetValue(KinectDeviceProperty, value); }
        }
        #endregion KinectDevice
        #endregion Properties
    }
}