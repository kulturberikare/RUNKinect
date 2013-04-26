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
        private Joint prevRightFoot = new Joint();
        private Joint prevLeftFoot = new Joint();
        private Joint startPoint = new Joint();
        private DateTime startTime = DateTime.MinValue;

        private int index = 0;
        private double[] speedArray = new double[10];
        private double[] angleArray = new double[10];
        private double meanVelocity;
        private double meanAngle;
        private double velocity;
        private double angle;
        private double maxFeetDistance = 0;
        private double feetDistance;

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

        private void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);

                    ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,
                                                                  PixelFormats.Bgr32, null, pixelData,
                                                                  frame.Width * frame.BytesPerPixel);
                }
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

                    for (int i = 0; i < this._CurrentFrameSkeletons.Length; i++)
                    {
                        currentskeleton = this._CurrentFrameSkeletons[i];

                        if (currentskeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            Joint rightFoot = currentskeleton.Joints[JointType.FootRight];
                            Joint leftFoot = currentskeleton.Joints[JointType.FootLeft];

                            feetDistance = GetJointDistance(rightFoot, leftFoot);

                            FootDistance.Text = String.Format("{0} \n {1} maxavstånd", Math.Round(feetDistance, 2),
                                                                                       Math.Round(maxFeetDistance, 2));

                            if (feetDistance < 0.4 &&
                                rightFoot.Position.X > prevRightFoot.Position.X &&
                                rightFoot.Position.X < leftFoot.Position.X && // Högerfoten närmast främre änden av löpbandet
                                startTime == DateTime.MinValue)
                            {
                                startTime = DateTime.Now;
                                startPoint = prevRightFoot;
                            }

                            if (feetDistance > 0.2 &&
                                rightFoot.Position.X > prevRightFoot.Position.X &&
                                rightFoot.Position.X > leftFoot.Position.X &&                               
                                startTime != DateTime.MinValue)
                            {
                                double timeDifferenceMs = (DateTime.Now - startTime).Ticks; // Skillnad mellan nuvarande tid och StartTime i Ticks, 10 µs.
                                double time = timeDifferenceMs / 10000000; // Gör om till s

                                Joint endPoint = rightFoot;
                                float rightFootDistance = GetJointDistance(startPoint, endPoint);

                                if (rightFootDistance > 0.12 && rightFootDistance < 0.4)
                                {
                                    velocity = TreadmillSpeed(rightFootDistance, time);
                                    speedArray[index] = velocity;

                                    angle = TreadmillAngle(startPoint, endPoint);
                                    angleArray[index] = angle;
                                    index++;
                                }

                                if (index == 9)
                                {
                                    meanVelocity = Math.Round(ArrayMean(speedArray) * 3.6, 1);
                                    meanAngle = Math.Round(ArrayMean(angleArray), 1);
                                    index = 0;
                                    maxFeetDistance = 0;
                                }

                                if (feetDistance > maxFeetDistance)
                                {
                                    maxFeetDistance = feetDistance;
                                }

                                TestText.Text = String.Format("{0} m/s \n {1} s \n {2} avstånd(m) \n {3} mätningar har gjorts \n {4} km/h medel",
                                                              Math.Round(velocity, 2), Math.Round(time, 2), Math.Round(rightFootDistance, 2),
                                                              index, meanVelocity);

                                startTime = DateTime.MinValue; // Nu är vi klara med StartTime. Förbereder för nästa mätning.                               
                            }

                            ExtraTest.Text = String.Format("\n\n\n {0} i x \n {1} i y \n {2} i z", Math.Round(startPoint.Position.X, 2),
                                                                                                   Math.Round(startPoint.Position.Y, 2),
                                                                                                   Math.Round(startPoint.Position.Z, 2));

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

            // Lägg ev. till Math.Round
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

        // Set ElevationAngle.
        private void SetSensorAngle(int angleValue)
        {
            if (angleValue > this._Kinect.MinElevationAngle ||
                angleValue < this._Kinect.MaxElevationAngle ||
                angleValue == this._Kinect.ElevationAngle)
            {
                this._Kinect.ElevationAngle = angleValue;
            }
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
                        this._Kinect.ColorFrameReady -= Kinect_ColorFrameReady;
                        this._Kinect.SkeletonStream.Disable();
                        this._CurrentFrameSkeletons = null;
                    }

                    this._Kinect = value;

                    // Initialize
                    if (this._Kinect != null)
                    {
                        if (this._Kinect.Status == KinectStatus.Connected)
                        {
                            this._Kinect.SkeletonStream.Enable();
                            this._Kinect.ColorStream.Enable();
                            this._CurrentFrameSkeletons = new Skeleton[this._Kinect.SkeletonStream.FrameSkeletonArrayLength];
                            this._Kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
                            this._Kinect.ColorFrameReady += Kinect_ColorFrameReady;
                            this._Kinect.Start();
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}
