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

namespace NewTreadmillAngleSpeedAB
{
    /// <summary>
    /// Beräkning av vinkel samt hastighet på löpbandet. Förenklad version jämfört med TreadmillAngleSpeedAB.
    /// Kollar på avståndet i x-led mellan höger och vänster fot istället för momentana hastigheter.
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private Skeleton[] _CurrentFrameSkeletons;
        private Vector3D prevrightfoot = new Vector3D(0, 0, 0);
        private Vector3D prevleftfoot = new Vector3D(0, 0, 0);
        private Vector3D StartPoint = new Vector3D(0, 0, 0);
        private Vector3D EndPoint = new Vector3D(0, 0, 0);
        private DateTime StartTime = DateTime.MinValue;

        // Testgrejer
        private DateTime prevtime = DateTime.MinValue;
        private double[] TestArray = { 0, 0, 2, 3, 4, 5, 6, 7, 8, 0 };

        // private int index = 0;
        private double[] SpeedArray = new double[10];

        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            //this.Kinect.ElevationAngle = 5;
        }

        #endregion Constructor

        #region Primary Methods
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
                    Skeleton currentskeleton;
                    frame.CopySkeletonDataTo(this._CurrentFrameSkeletons);

                    for(int i = 0; i < this._CurrentFrameSkeletons.Length; i++)
                    {
                        currentskeleton = this._CurrentFrameSkeletons[i];



                        if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Joint footright = currentskeleton.Joints[JointType.HandRight];
                            Joint footleft = currentskeleton.Joints[JointType.HandLeft];

                            float rightxpos = footright.Position.X;
                            float leftxpos = footleft.Position.X;
                            float rightypos = footright.Position.Y;
                            float leftypos = footleft.Position.Y;
                            float rightzpos = footright.Position.Z;
                            float leftzpos = footleft.Position.Z;

                            if ((rightxpos - leftxpos) < 0.5 && (prevrightfoot.X - prevleftfoot.X) > 0.5 && StartTime == DateTime.MinValue)
                            {
                                StartTime = DateTime.Now;
                                StartPoint = prevrightfoot;
                            }

                            //FootBack(vel.X, StartTime);

                            //FootUp(vel.Y, StartTime, SpeedArray, index);

                            if ((leftxpos - rightxpos) > 0.5 && (prevleftfoot.X - prevrightfoot.X) < 0.5 && StartTime != DateTime.MinValue)
                            {
                                double TimeDifferenceMs = (DateTime.Now - StartTime).Milliseconds; // Skillnad mellan nuvarande tid och StartTime i ms.
                                double Time = TimeDifferenceMs / 1000; // Gör om till s

                                EndPoint = new Vector3D(rightxpos, rightypos, rightzpos);    
                                double Distance = AbsDistance(StartPoint, EndPoint);

                                double Velocity = TreadmillSpeed(Distance, Time);

                                double Angle = TreadmillAngle(StartPoint, EndPoint);

                                TestText.Text = String.Format("{0} m/s \n {1} grader", Velocity, Angle);

                                StartTime = DateTime.MinValue; // Nu är vi klara med StartTime. Förbereder för nästa mätning.
                            }

                            prevtime = DateTime.Now;

                            prevrightfoot = new Vector3D(rightxpos, rightypos, rightzpos);
                            prevleftfoot = new Vector3D(leftxpos, leftypos, leftzpos);


                            FootDistance.Text = String.Format("{0}",(rightxpos - leftxpos)); 
                        }
                    }
                }
            }
        }
        
        #endregion Primary Methods

        #region Helper Methods

        // Hjälpmetod för att beräkna absolutbeloppet av avståndet mellan två 3D-vektorer
    public double AbsDistance(Vector3D v1, Vector3D v2)
    {
        double xdiff = v1.X - v2.X;
        double ydiff = v1.Y - v2.Y;
        double zdiff = v1.Z - v2.Z;

        double distance = Math.Sqrt(xdiff * xdiff + ydiff * ydiff + zdiff * zdiff);

        return distance;
    }

        // Metod för att få ut vinkeln på löpbandet.
        // Beräknar motstående katet (skillnad i y-led) samt närstående katet (skillnad i x-led).
        // Därefter arctan på hela kalaset.
        public double TreadmillAngle(Vector3D footstart, Vector3D footend)
        {
            double oppcat = (footstart.Y - footend.Y);
            double nearcat = (footstart.X - footend.X);

            // Lägg ev. till Math.Round
            double angle = (Math.Atan(oppcat / nearcat)) * (180 / Math.PI);

            return angle;
        }

        // Metod för att få ut hastigheten på löpbandet.
        // Använder sig av tiden som förflutit mellan att foten sätts i löpbandet (neg. hastighet x-riktning)
        // och när foten dras upp från löpbandet (pos. hastighet y-riktning).
        public double TreadmillSpeed(double distance, double time)
        {
            double speed = (distance / time);

            return speed;
        }

        // Ta medelvärde från en array. Funkar!
        public double ArrayMean(double[] array)
        {
            double sum = 0;

            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }

            return sum / array.Length;
        }

        #endregion Helper Methods

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
                        this._CurrentFrameSkeletons = null;
                    }

                    this._Kinect = value;

                    //Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            this._Kinect.SkeletonStream.Enable();
                            this._CurrentFrameSkeletons = new Skeleton[this._Kinect.SkeletonStream.FrameSkeletonArrayLength];
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
