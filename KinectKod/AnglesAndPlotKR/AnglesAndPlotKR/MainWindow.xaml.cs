using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
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

using Microsoft.Windows.Controls;
using System.Data;
using System.Windows.Media.Media3D;

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
using System.Windows.Markup;

using Microsoft.Kinect;
using System.IO;

namespace CountingAngles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private const float FeetPerMeters = 3.2808399f;
        private double count = 5;
        //----Borde kanske ligga i initialize så att det initialiseras samtidigt som kinecten.
        private ObservableCollection<KeyValuePair<double, double>> valueListOne = new ObservableCollection<KeyValuePair<double, double>>();
        private ObservableCollection<KeyValuePair<double, double>> valueListTwo = new ObservableCollection<KeyValuePair<double, double>>();
        private ObservableCollection<ObservableCollection<KeyValuePair<double, double>>> valueList = new ObservableCollection<ObservableCollection<KeyValuePair<double, double>>>();
        private double _Angle = 0;
        private string _Joint;
        //----
        private KinectSensor _Kinect;
        private readonly Brush[] _SkeletonBrushes;
        private Skeleton[] _FrameSkeletons;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            //  Borde kanske inte ligga här men..
            showColumnChart();
            DispatcherTimer _Timer = new DispatcherTimer();
            _Timer.Interval = new TimeSpan(0, 0, 1);
            _Timer.Tick += new EventHandler(timer_Tick);
            _Timer.IsEnabled = true;

            this._SkeletonBrushes = new[] {Brushes.Black, Brushes.Crimson, Brushes.Indigo,
                                           Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        #endregion Constructor

        #region Methods

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

                          showColumnChart();
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
                DefineVectors(skeleton);
            }
            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.Kinect.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / Kinect.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / Kinect.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }

        private Vector3D GetJointPoint3D(Joint joint)
        {
            return new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        private void DefineVectors(Skeleton skeleton)
        {
            //Definition av vinklar
            Vector3D ankleL = GetJointPoint3D(skeleton.Joints[JointType.AnkleLeft]);
            Vector3D ankleR = GetJointPoint3D(skeleton.Joints[JointType.AnkleRight]);
            Vector3D footL = GetJointPoint3D(skeleton.Joints[JointType.FootLeft]);
            Vector3D footR = GetJointPoint3D(skeleton.Joints[JointType.FootRight]);
            Vector3D hipL = GetJointPoint3D(skeleton.Joints[JointType.HipLeft]);
            Vector3D hipR = GetJointPoint3D(skeleton.Joints[JointType.HipRight]);
            Vector3D hipCenter = GetJointPoint3D(skeleton.Joints[JointType.HipCenter]);
            Vector3D kneeL = GetJointPoint3D(skeleton.Joints[JointType.KneeLeft]);
            Vector3D kneeR = GetJointPoint3D(skeleton.Joints[JointType.KneeRight]);

            //Hittar vinkeln i en led och skickar till WriteToFile med lednamnet
            Writer(FindAngles(ankleL, kneeL, hipL), "LeftKnee");
            Writer(FindAngles(ankleR, kneeR, hipR), "RightKnee");
            Writer(FindAngles(footL, ankleL, kneeL), "LeftAnkle");
            Writer(FindAngles(footR, ankleR, kneeR), "RightAnkle");
            Writer(FindAngles(hipL, hipCenter, hipR), "Hip");
        }

        //Hittar vinkeln i en led utiftrån 3 punkter.
        private double FindAngles(Vector3D vector1, Vector3D vector2, Vector3D vector3)
        {
            Vector3D a1 = vector1 - vector2;
            Vector3D a2 = vector3 - vector2;

            return Vector3D.AngleBetween(a1, a2);
        }

        //TILL PLOTTNINGSDELEN: Kolla vilken textsträng det är och lägg till i resp. lista för plottning!!
        //Skriver vinkeln, count och lednamnet till filen.
        private void Writer(double angle, string joint)
        {
            if (!File.Exists("Vinklar.txt"))
            {
                StreamWriter file = new StreamWriter("Vinklar.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("Vinklar.txt", true))
                {
                    file.WriteLine(joint + " " + Convert.ToString(angle) + " " + Convert.ToString(count));
                    file.Close();
                }
            }
            _Angle = angle;
            _Joint = joint;
            //showColumnChart();
        }

        private void showColumnChart()
        {
            valueListOne.Add(new KeyValuePair<double, double>(count, _Angle));
            valueListTwo.Add(new KeyValuePair<double, double>(count, _Angle + 25));
            valueList.Add(valueListOne);
            valueList.Add(valueListTwo);

            count++;

            //Setting data for line chart
            lineChart.DataContext = valueList;
        }

        Random random = new Random(DateTime.Now.Millisecond);

        void timer_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("timer_Tick");    
        }

        private void ReadFromFile()
        {
            string line;

            int counter = 0;

            System.IO.StreamReader file = new System.IO.StreamReader("Vinklar.txt");

            while ((line = file.ReadLine()) != null)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    // ReadFromFile(line[i]);
                }
                counter++;
            }
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
                    if (this._Kinect != null && this._Kinect.SkeletonStream != null)
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