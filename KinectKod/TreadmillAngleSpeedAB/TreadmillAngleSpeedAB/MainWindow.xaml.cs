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

namespace TreadmillAngleSpeedAB
{
    /// <summary>
    /// Beräkning av vinkel samt hastighet på löpbandet. Använder sig av principen att foten får en negativ hastighet i x-led när den sätts i löpbandet
    /// och en positiv hastighet i y-led när den lämnar löpbandet samt att hastigheten i foten är konstant samma som löpbandets däremellan.
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private Skeleton[] _CurrentFrameSkeletons;
        Vector3D prev = new Vector3D(0, 0, 0);
        Vector3D StartPoint = new Vector3D(0, 0, 0);
        Vector3D EndPoint = new Vector3D(0, 0, 0);
        DateTime StartTime;

        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

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
                    Skeleton currentskeleton;
                    frame.CopySkeletonDataTo(this._CurrentFrameSkeletons);
                    // previousskeleton = null;

                    for(int i = 0; i < this._CurrentFrameSkeletons.Length; i++)
                    {
                        currentskeleton = this._CurrentFrameSkeletons[i];
                        // previousskeleton = this._PreviousFrameSkeletons[i];

                        if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Joint j = currentskeleton.Joints[JointType.HandRight];

                            float xpos = j.Position.X;
                            float ypos = j.Position.Y;
                            float zpos = j.Position.Z;

                           
                            Vector3D vel = JointVelocity(currentskeleton, prev);
                            prev = new Vector3D(xpos, ypos, zpos); // prev kommer användas i föregående rad nästa "gång".

                            //Skriver ut koordinaterna för den JointType som valts
                           // FootVelocity.Text = String.Format("{0} m/s i x-led\n\n {1} m/s i y-led\n\n {2} m/s i z-led", vel.X, vel.Y, vel.Z);

                            if(vel.X < 0 && StartTime == DateTime.MinValue) // Börjar foten gå bakåt? Är StartTime satt till MinValue?
                            {
                                StartTime = DateTime.Now;
                                StartPoint = prev; 
                            }

                            if (vel.Y > 0 && StartTime != DateTime.MinValue) // Börjar foten gå uppåt? Har StartTime fått ett värde från föregående if-sats?
                            {
                                double TimeDifferenceMs = (DateTime.Now - StartTime).Milliseconds; // Skillnad mellan nuvarande tid och StartTime i ms.
                                double Time = TimeDifferenceMs * 1000; // Gör om till s

                                EndPoint = prev; // Ny prev jämfört med StartPoint    
                                double Distance = AbsDistance(StartPoint, EndPoint);

                                double Velocity = TreadmillSpeed(Distance, Time);

                                double Angle = TreadmillAngle(StartPoint, EndPoint);

                                StartTime = DateTime.MinValue; // Nu är vi klara med StartTime. Förbereder för nästa mätning.

                                FootVelocity.Text = String.Format("{0} grader \n, {1} m/s", Angle, Velocity);
                            }
                        }
                    }
                }
            }
        }

        // Metod som är tänkt att hålla koll på hastigheten i en specifik JointType (höger fot i vårt fall). 
        // Jämför Skeleton-data från nuvarande framen med en Vector3D innehållande Joint-koordinaterna från föregående frame.
    public Vector3D JointVelocity(Skeleton currentskeleton, Vector3D prev)    
    {
            if (currentskeleton != null)
            {
                Joint currentFoot = currentskeleton.Joints[JointType.FootRight];

                // 30 eftersom 30 fps (1/30 s per frame).
                // velocity = sträcka / tid

                double xvelocity = (currentFoot.Position.X - prev.X) * 30;
                double yvelocity = (currentFoot.Position.Y - prev.Y) * 30;
                double zvelocity = (currentFoot.Position.Z - prev.Z) * 30;            

                //MessageBox.Show("Yes");
                //JointPosition.Text = String.Format("current {0} \n\n previous {1}", currentFoot.Position.X, prev.X);
                return new Vector3D(xvelocity, yvelocity, zvelocity); 
            }

            else
            {
                return new Vector3D(0, 0, 0); // Behövs ej?
            }
        }

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
        public double TreadmillAngle(Vector3D FootStart, Vector3D FootEnd)
        {
            double oppcat = (FootStart.Y - FootEnd.Y);
            double nearcat = (FootStart.X - FootStart.X);

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
