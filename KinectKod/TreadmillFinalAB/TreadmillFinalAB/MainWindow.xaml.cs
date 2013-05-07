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
    /// Slutgiltig version för att få vinkel och hastighet från löpbandet.
    /// Inga testfunktioner i denna version, endast siffror på hastighet och
    /// vinkel visas som TextBlock
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _Kinect;
        private Skeleton[] _FrameSkeletons;
        private Joint prevRightFoot = new Joint();
        private Joint prevLeftFoot = new Joint();
        private Joint startPoint = new Joint();
        public Int64 startTime = 0;
        public bool readyToStart = true;

        private int index = 0;
        private double[] speedArray = new double[10];
        private double[] angleArray = new double[10];
        private double meanSpeed;
        private double meanAngle;
        private double speed;
        private double angle;
        private double feetDistance;
        private double prevFeetDistance;

        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
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
                    frame.CopySkeletonDataTo(this._FrameSkeletons);
                  
                    for (int i = 0; i < this._FrameSkeletons.Length; i++)
                    {
                        currentskeleton = this._FrameSkeletons[i];

                        if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Joint rightFoot = currentskeleton.Joints[JointType.FootRight];
                            Joint leftFoot = currentskeleton.Joints[JointType.FootLeft];

                            feetDistance = GetJointDistance(rightFoot, leftFoot);
                            prevFeetDistance = GetJointDistance(prevRightFoot, prevLeftFoot);

                            // Kolla om kriterierna uppfylls för att starta mätningen
                            if (feetDistance < 0.5 &&
                                feetDistance > 0.25 &&
                                feetDistance < prevFeetDistance &&
                                leftFoot.Position.X < rightFoot.Position.X && // Högerfoten närmast främre änden av löpbandet
                                readyToStart == true &&
                                startTime == 0)
                            {
                                startTime = frame.Timestamp;
                                startPoint = leftFoot;

                                readyToStart = false;
                            }

                            // Test för att se till så man inte missar slutpunkten och genomför mätningen på flera steg.
                            if (readyToStart == false &&
                                startTime != 0 &&
                                (frame.Timestamp - startTime) / 1000 > 0.75)
                            {
                                readyToStart = true;
                                startTime = 0;
                            }

                            // Kolla om kriterierna uppfylls för att avsluta mätningen.
                            if (feetDistance > 0.25 &&
                                feetDistance > prevFeetDistance &&
                                leftFoot.Position.X > rightFoot.Position.X &&
                                startTime != 0 &&
                                readyToStart == false)
                            {
                                double timeDifferenceMs = frame.Timestamp - startTime; // Skillnad mellan nuvarande tid och startTime i ms.
                                double time = timeDifferenceMs / 1000; // Gör om till s

                                Joint endPoint = leftFoot;
                                float leftFootDistance = GetJointDistance(startPoint, endPoint);

                                if (leftFootDistance > 0.2 && leftFootDistance < 0.5)
                                {
                                    speed = TreadmillSpeed(leftFootDistance, time);
                                    speedArray[index] = speed;

                                    angle = TreadmillAngle(startPoint, endPoint);
                                    angleArray[index] = angle;
                                    index++;
                                }

                                if (index == 9)
                                {
                                    Array.Sort(speedArray); 
                                    // Gör om medelvärdet till km/h
                                    meanSpeed = Math.Round(SortedArrayMean(speedArray) * 3.6, 1);

                                    Array.Sort(angleArray);
                                    meanAngle = Math.Round(SortedArrayMean(angleArray), 1);
                                    index = 0;
                                }

                                SpeedText.Text = String.Format("{0} km/h", meanSpeed);
                                AngleText.Text = String.Format("{1} degrees", meanAngle);

                                startTime = 0; // Nu är vi klara med StartTime. Förbereder för nästa mätning.
                                readyToStart = true;
                            }

                            // Spara positioner för att kunna jämföra med nästa frame.
                            prevRightFoot = rightFoot;
                            prevLeftFoot = leftFoot;
                        }
                    }
                }
            }
        }

        #endregion Primary Methods

        #region Helper Methods

        // Metod för att räkna ut avståndet mellan två joints.
        public float GetJointDistance(Joint rightJoint, Joint leftJoint)
        {
            float xDistance = rightJoint.Position.X - leftJoint.Position.X;
            float yDistance = rightJoint.Position.Y - leftJoint.Position.Y;
            float zDistance = rightJoint.Position.Z - leftJoint.Position.Z;

            return (float)Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2) + Math.Pow(zDistance, 2));
        }

        // Metod för att få ut vinkeln på löpbandet.
        // Beräknar motstående katet (skillnad i y-led) samt närstående katet (skillnad i x-led).
        // Därefter arctan på hela kalaset.
        public double TreadmillAngle(Joint footstart, Joint footend)
        {
            double oppcat = (footstart.Position.Y - footend.Position.Y);
            double nearcat = (footstart.Position.X - footend.Position.X);

            double angle = (Math.Atan(oppcat / nearcat)) * (180 / Math.PI);
            angle = Math.Abs(angle);
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

        // Ta medelvärde från en array. Denna funktion är framtagen för en array som är sorterad från
        // lägsta till högsta värde. Den tar bort de två lägsta och de två högsta och tar medel på
        // resterande.
        public double SortedArrayMean(double[] array)
        {
            double sum = 0;
            for (int i = 2; i < array.Length - 2; i++)
            {
                sum += array[i];
            }
            return sum / (array.Length - 4);
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
                        this._FrameSkeletons = null;
                    }

                    this._Kinect = value;

                    // Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            // Smoothing, kan modifieras. Se http://msdn.microsoft.com/en-us/library/microsoft.kinect.transformsmoothparameters_members.aspx
                            TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                            {
                                smoothingParam.Smoothing = 0.5f;
                                smoothingParam.Correction = 0.1f;
                                smoothingParam.Prediction = 0.5f;
                                smoothingParam.JitterRadius = 0.1f;
                                smoothingParam.MaxDeviationRadius = 0.1f;
                            };

                            this._Kinect.SkeletonStream.Enable(smoothingParam);
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

