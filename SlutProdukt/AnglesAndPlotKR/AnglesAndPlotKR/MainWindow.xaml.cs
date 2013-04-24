/*
Fil där en textfil med alla vinklar skapas.
*/
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

using System.Windows.Media.Media3D;

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
        private double count = 0;
        private KinectSensor _Kinect;
        private Skeleton[] _FrameSkeletons;
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

        //Eventhanterare för skeletonstream
        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton skeleton;

                    frame.CopySkeletonDataTo(this._FrameSkeletons);

                    for (int i = 0; i < this._FrameSkeletons.Length; i++)
                    {
                        skeleton = this._FrameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            DefineVectors(skeleton);
                            count++;
                        }
                    }
                }
            }
        }

        //Definierar vektorer som används för att plocka fram vinkeln
        private void DefineVectors(Skeleton skeleton)
        {
            //Definierar aktuella leder och skapar en vektor med ledens x-, y-, z-position mha GetJointPoint3D.
            Vector3D ankleL = GetJointPoint3D(skeleton.Joints[JointType.AnkleLeft]);
            Vector3D ankleR = GetJointPoint3D(skeleton.Joints[JointType.AnkleRight]);
            Vector3D footL = GetJointPoint3D(skeleton.Joints[JointType.FootLeft]);
            Vector3D footR = GetJointPoint3D(skeleton.Joints[JointType.FootRight]);
            Vector3D hipL = GetJointPoint3D(skeleton.Joints[JointType.HipLeft]);
            Vector3D hipR = GetJointPoint3D(skeleton.Joints[JointType.HipRight]);
            Vector3D hipCenter = GetJointPoint3D(skeleton.Joints[JointType.HipCenter]);
            Vector3D kneeL = GetJointPoint3D(skeleton.Joints[JointType.KneeLeft]);
            Vector3D kneeR = GetJointPoint3D(skeleton.Joints[JointType.KneeRight]);

            //Hittar vinkeln i en led mha FindAngles och skickar sedan respektive led till respektive fil 
            WriterLK(FindAngles(ankleL, kneeL, hipL));
            WriterRK(FindAngles(ankleR, kneeR, hipR));
            WriterLA(FindAngles(footL, ankleL, kneeL));
            WriterRA(FindAngles(footR, ankleR, kneeR));
            WriterH(FindAngles(hipL, hipCenter, hipR));
        }

        //Skapar en 3D-vektor med en leds x-, y-, z-position.
        private Vector3D GetJointPoint3D(Joint joint)
        {
            return new Vector3D(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        //Hittar vinkeln i en led utiftrån 3 punkter.
        private double FindAngles(Vector3D vector1, Vector3D vector2, Vector3D vector3)
        {
            Vector3D a1 = vector1 - vector2;
            Vector3D a2 = vector3 - vector2;

            return Vector3D.AngleBetween(a1, a2);
        }

        //Skriver vinkeln i vänster knä till en textfil som heter LeftKnee
        private void WriterLK(double angle)
        {
            if (!File.Exists("LeftKnee.txt"))
            {
                StreamWriter file = new StreamWriter("LeftKnee.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("LeftKnee.txt", true))
                {
                    file.WriteLine(Convert.ToString(Math.Round(angle, 0)) + ":" + Convert.ToString(count) + ";");
                    file.Close();
                }
            }
        }

        //Skriver vinkeln i höger knä till en textfil som heter RightKnee
        private void WriterRK(double angle)
        {
            if (!File.Exists("RightKnee.txt"))
            {
                StreamWriter file = new StreamWriter("RightKnee.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightKnee.txt", true))
                {
                    file.WriteLine(Convert.ToString(Math.Round(angle, 0)) + ":" + Convert.ToString(count) + ";");
                    file.Close();
                }
            }
        }

        //Skriv vinkeln i höger fotled till en textfil som heter RightAnkle
        private void WriterRA(double angle)
        {
            if (!File.Exists("RightAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("RightAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("RightAnkle.txt", true))
                {
                    file.WriteLine(Convert.ToString(Math.Round(angle, 0)) + ":" + Convert.ToString(count) + ";");
                    file.Close();
                }
            }
        }

        //Skriver vinkeln i vänster fotled till en textfil som heter LeftAnkle
        private void WriterLA(double angle)
        {
            if (!File.Exists("LeftAnkle.txt"))
            {
                StreamWriter file = new StreamWriter("LeftAnkle.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("LeftAnkle.txt", true))
                {
                    file.WriteLine(Convert.ToString(Math.Round(angle, 0)) + ":" + Convert.ToString(count) + ";");
                    file.Close();
                }
            }
        }

        //Skriver vinkeln i höftern till en textfil som heter Hip
        private void WriterH(double angle)
        {
            if (!File.Exists("Hip.txt"))
            {
                StreamWriter file = new StreamWriter("Hip.txt");
                file.Close();
            }
            else
            {
                using (StreamWriter file = new StreamWriter("Hip.txt", true))
                {
                    file.WriteLine(Convert.ToString(Math.Round(angle, 0)) + ":" + Convert.ToString(count) + ";");
                    file.Close();
                }
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
                        this._FrameSkeletons = null;
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

                            //Tar bort filer från den föregående körningen.
                            if (File.Exists("Hip.txt"))
                            {
                                StreamWriter file = new StreamWriter("Hip.txt");
                                file.Close();
                            }
                            if (File.Exists("RightAnkle.txt"))
                            {
                                StreamWriter file = new StreamWriter("RightAnkle.txt");
                                file.Close();
                            }
                            if (File.Exists("LeftAnkle.txt"))
                            {
                                StreamWriter file = new StreamWriter("LeftAnkle.txt");
                                file.Close();
                            }
                            if (File.Exists("LeftKnee.txt"))
                            {
                                StreamWriter file = new StreamWriter("LeftKnee.txt");
                                file.Close();
                            }
                            if (File.Exists("RightKnee.txt"))
                            {
                                StreamWriter file = new StreamWriter("RightKnee.txt");
                                file.Close();
                            }
                        }
                    }
                }
            }
        }
        #endregion Properties
    }
}