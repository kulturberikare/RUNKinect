using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Forms;
using System.IO;
using System.Windows.Controls.DataVisualization.Charting;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectSensorOne;
        private KinectSensor _KinectSensorTwo;
        private WriteableBitmap _ImageOne;
        private WriteableBitmap _ImageTwo;
        private Int32Rect _ImageRectOne;
        private Int32Rect _ImageRectTwo;
        private int _ImageStrideOne;
        private int _ImageStrideTwo;

        //Variabler som används vid inläsning av fil och uppritning av figur
        string line;
        string number = null;
        int angle = 0;
        int timestamp = 0;
        ObservableCollection<KeyValuePair<int, int>> valueList = new ObservableCollection<KeyValuePair<int, int>>();

        private MainWindowViewModel viewModel;

        //Djup expriment
        int pixelHeight = 240;
        int pixelWidth = 320;
        private GeometryModel3D[] modelPoints = new GeometryModel3D[240 * 320];
        private GeometryModel3D geometryModel;
        #endregion Member Variables

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { DiscoverKinectSensors(); };
            this.Unloaded += (s, e) => { KinectSensorOne = null; KinectSensorTwo = null; };
            this.Closed += MainWindow_Closed;
            this.viewModel = new MainWindowViewModel();
            this.viewModel.CanStartOne = false;
            this.viewModel.CanStopOne = false;
            this.viewModel.CanStartTwo = false;
            this.viewModel.CanStopTwo = false;
            this.DataContext = this.viewModel;

            valueList.Add(new KeyValuePair<int, int>(0, 0));
            lineChart.DataContext = valueList;
        }
        #endregion Constructor

        #region Methods
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (KinectSensorOne != null && KinectSensorOne.IsRunning)
            {
                KinectSensorOne.Stop();
            }

            if (KinectSensorTwo != null && KinectSensorTwo.IsRunning)
            {
                KinectSensorTwo.Stop();
            }
        }

        public void DiscoverKinectSensors()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;

            KinectSensor FirstKinect = KinectSensor.KinectSensors
                                          .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            KinectSensor LastKinect = KinectSensor.KinectSensors
                               .LastOrDefault(x => x.Status == KinectStatus.Connected);

            if (FirstKinect != null)
            {
                KinectSensorOne = FirstKinect;
            }

            if (FirstKinect != null && LastKinect != null && FirstKinect != LastKinect)
            {
                KinectSensorTwo = LastKinect;
            }

            SetKinectData();
            //Djup Exprimentet i 3D
            SetData();
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            KinectSensor FirstKinect = KinectSensor.KinectSensors
                                                   .FirstOrDefault(x => x.Status == KinectStatus.Connected);
            KinectSensor LastKinect = KinectSensor.KinectSensors
                                                   .LastOrDefault(x => x.Status == KinectStatus.Connected);
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.KinectSensorOne == null && FirstKinect != LastKinect && e.Sensor == FirstKinect)
                    {
                        KinectSensorOne = FirstKinect;
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    if (this.KinectSensorTwo == null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorTwo = LastKinect;
                        this.viewModel.SensorstatusTwo = e.Status.ToString();
                    }
                    if (this.KinectSensorTwo != null && this.KinectSensorOne == null && FirstKinect != LastKinect && e.Sensor == LastKinect)
                    {
                        KinectSensorOne = LastKinect;
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (KinectSensorOne == e.Sensor)
                    {
                        KinectSensorOne = null;
                        this.viewModel.SensorstatusOne = e.Status.ToString();
                    }
                    if (KinectSensorTwo == e.Sensor)
                    {
                        KinectSensorTwo = null;
                        this.viewModel.SensorstatusTwo = e.Status.ToString();
                    }
                    break;
            }
        }

        private void SetKinectData()
        {
            if (KinectSensorOne != null)
            {
                this.viewModel.ConectionIDOne = this.KinectSensorOne.DeviceConnectionId;
                this.viewModel.DeviceIDOne = this.KinectSensorOne.UniqueKinectId;
                this.viewModel.SensorstatusOne = this.KinectSensorOne.Status.ToString();
                this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
                this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
                this.viewModel.IsSkeletonStreamEnabledOne = this.KinectSensorOne.SkeletonStream.IsEnabled;
                this.viewModel.SensorAngleOne = this.KinectSensorOne.ElevationAngle;
            }

            if (KinectSensorTwo != null)
            {
                this.viewModel.ConectionIDTwo = this.KinectSensorTwo.DeviceConnectionId;
                this.viewModel.DeviceIDTwo = this.KinectSensorTwo.UniqueKinectId;
                this.viewModel.SensorstatusTwo = this.KinectSensorTwo.Status.ToString();
                this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorTwo.ColorStream.IsEnabled;
                this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorTwo.DepthStream.IsEnabled;
                this.viewModel.IsSkeletonStreamEnabledTwo = this.KinectSensorTwo.SkeletonStream.IsEnabled;
                this.viewModel.SensorAngleTwo = this.KinectSensorTwo.ElevationAngle;
            }
        }

        private void StartColorImageOne(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsColorStreamEnabledOne)
            {
                KinectSensor sensor = KinectSensorOne;

                StopDepthImageOne(sensor);

                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();

                this._ImageOne = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ImageRectOne = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ImageStrideOne = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageOne.Source = this._ImageOne;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyOne;
                this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
            }
        }

        private void StartColorImageTwo(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsColorStreamEnabledTwo)
            {
                KinectSensor sensor = KinectSensorTwo;

                StopDepthImageTwo(sensor);

                ColorImageStream colorStream = sensor.ColorStream;
                colorStream.Enable();

                this._ImageTwo = new WriteableBitmap(colorStream.FrameWidth,
                                                          colorStream.FrameHeight, 96, 96,
                                                          PixelFormats.Bgr32, null);
                this._ImageRectTwo = new Int32Rect(0, 0, colorStream.FrameWidth,
                                                        colorStream.FrameHeight);
                this._ImageStrideTwo = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                SightImageTwo.Source = this._ImageTwo;

                sensor.ColorFrameReady += Kinect_ColorFrameReadyTwo;
                this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorTwo.ColorStream.IsEnabled;
            }
        }

        private void StopColorImageOne(KinectSensor sensor)
        {
            sensor.ColorFrameReady -= Kinect_ColorFrameReadyOne;
            sensor.ColorStream.Disable();
            this.viewModel.IsColorStreamEnabledOne = this.KinectSensorOne.ColorStream.IsEnabled;
        }

        private void StopColorImageTwo(KinectSensor sensor)
        {
            sensor.ColorFrameReady -= Kinect_ColorFrameReadyTwo;
            sensor.ColorStream.Disable();
            this.viewModel.IsColorStreamEnabledTwo = this.KinectSensorTwo.ColorStream.IsEnabled;
        }

        private void StartDepthImageOne(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsDepthStreamEnabledOne)
            {
                KinectSensor sensor = KinectSensorOne;

                StopColorImageOne(sensor);

                DepthImageStream depthStream = sensor.DepthStream;
                //Har ändrat upplösningen här för djup 3D experimentet 
                depthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                this._ImageOne = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._ImageRectOne = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._ImageStrideOne = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                SightImageOne.Source = this._ImageOne;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyOne;
                this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
            }
        }

        private void StartDepthImageTwo(object sender, RoutedEventArgs e)
        {
            if (!this.viewModel.IsDepthStreamEnabledTwo)
            {
                KinectSensor sensor = KinectSensorTwo;

                StopColorImageTwo(sensor);

                DepthImageStream depthStream = sensor.DepthStream;
                depthStream.Enable();

                this._ImageTwo = new WriteableBitmap(depthStream.FrameWidth,
                    depthStream.FrameHeight, 96, 96,
                    PixelFormats.Gray16, null);
                this._ImageRectTwo = new Int32Rect(0, 0, depthStream.FrameWidth,
                    depthStream.FrameHeight);
                this._ImageStrideTwo = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;
                SightImageTwo.Source = this._ImageTwo;

                sensor.DepthFrameReady += Kinect_DepthFrameReadyTwo;
                this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorTwo.DepthStream.IsEnabled;
            }
        }

        private void StopDepthImageOne(KinectSensor sensor)
        {
            sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
            sensor.DepthStream.Disable();
            this.viewModel.IsDepthStreamEnabledOne = this.KinectSensorOne.DepthStream.IsEnabled;
        }

        private void StopDepthImageTwo(KinectSensor sensor)
        {
            sensor.DepthFrameReady -= Kinect_DepthFrameReadyTwo;
            sensor.DepthStream.Disable();
            this.viewModel.IsDepthStreamEnabledTwo = this.KinectSensorTwo.DepthStream.IsEnabled;
        }

        private void Skeleton1_Checked(object sender, RoutedEventArgs e)
        {
            this._KinectSensorOne.SkeletonStream.Enable();
            SkeletonViewerElement.KinectSensorOne = this.KinectSensorOne;
            this.viewModel.IsSkeletonStreamEnabledOne = this.KinectSensorOne.SkeletonStream.IsEnabled;
            this.viewModel.AngleHip = SkeletonViewerElement.AngleH;
            this.viewModel.AngleRKnee = SkeletonViewerElement.AngleRK;
            this.viewModel.AngleRAnkle = SkeletonViewerElement.AngleRA;
            this.viewModel.AngleLKnee = SkeletonViewerElement.AngleLK;
            this.viewModel.AngleLAnkle = SkeletonViewerElement.AngleLA;
        }

        private void Skeleton1_UnChecked(object sender, RoutedEventArgs e)
        {
            this._KinectSensorOne.SkeletonStream.Disable();
            this.viewModel.IsSkeletonStreamEnabledOne = this.KinectSensorOne.SkeletonStream.IsEnabled;
            SkeletonViewerElement.SkeletonsPanel.Children.Clear();
        }

        private void Skeleton2_Checked(object sender, RoutedEventArgs e)
        {
            this._KinectSensorTwo.SkeletonStream.Enable();
            SkeletonViewerTwoElement.KinectSensorTwo = this.KinectSensorTwo;
            this.viewModel.IsSkeletonStreamEnabledTwo = this.KinectSensorTwo.SkeletonStream.IsEnabled;
        }

        private void Skeleton2_UnChecked(object sender, RoutedEventArgs e)
        {
            this._KinectSensorTwo.SkeletonStream.Disable();
            this.viewModel.IsSkeletonStreamEnabledTwo = this.KinectSensorTwo.SkeletonStream.IsEnabled;
            SkeletonViewerTwoElement.SkeletonsPanelTwo.Children.Clear();
        }

        private void InitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                SightImageOne.Source = this._ImageOne;
                deletePrevFiles();
                sensor.Start();
            }
        }

        //Plockar bort textfiler med vinklar från tidigare körningar. Kallas när kinectsensor 1 initialiseras
        private void deletePrevFiles()
        {
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

        private void InitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                SightImageTwo.Source = this._ImageTwo;
                sensor.Start();
            }
        }

        private void UninitializeKinectSensorOne(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                if (this.viewModel.IsColorStreamEnabledOne)
                {
                    StopColorImageOne(sensor);
                }

                if (this.viewModel.IsDepthStreamEnabledOne)
                {
                    sensor.DepthFrameReady -= Kinect_DepthFrameReadyOne;
                }
            }
        }

        private void UninitializeKinectSensorTwo(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                if (this.viewModel.IsColorStreamEnabledTwo)
                {
                    sensor.ColorFrameReady -= Kinect_ColorFrameReadyTwo;
                }

                if (this.viewModel.IsDepthStreamEnabledOne)
                {
                    sensor.DepthFrameReady -= Kinect_DepthFrameReadyTwo;
                }
            }
        }

        private void Kinect_ColorFrameReadyOne(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ImageOne.WritePixels(this._ImageRectOne,
                                                    pixelData, this._ImageStrideOne, 0);
                    
                }
            }
        }

        private void Kinect_ColorFrameReadyTwo(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ImageTwo.WritePixels(this._ImageRectTwo,
                                                    pixelData, this._ImageStrideTwo, 0);
                }
            }
        }

        private void Kinect_DepthFrameReadyOne(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                //Glöm ej att Lägga tillbaka den här!
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ImageOne.WritePixels(this._ImageRectOne,
                                                   pixelData, this._ImageStrideOne, 0);
                }


                if (frame == null)
                {
                    return;
                }
                short[] pixelData1 = new short[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData1);
                int translatePoint = 0;
                for (int posY = 0; posY < frame.Height; posY += 2)
                {
                    for (int posX = 0; posX < frame.Width; posX += 2)
                    {
                        int depth = ((ushort)pixelData1[posX + posY * frame.Width]) >> 3;
                        if (depth == KinectSensorOne.DepthStream.UnknownDepth)
                        {
                            continue;
                        }
                        ((TranslateTransform3D)modelPoints[translatePoint].Transform).OffsetZ = depth;
                        translatePoint++;
                    }
                }
            }
        }

        private void Kinect_DepthFrameReadyTwo(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._ImageTwo.WritePixels(this._ImageRectTwo,
                                                   pixelData, this._ImageStrideTwo, 0);
                }
            }
        }

        //Eventhanterare som har logiken bakom browserknappen.
        private void BrowseButton1_Click(object sender, RoutedEventArgs e)
        {
            //Definierar i vilken mapp det ska letas i och efter vilken filtyp
            OpenFileDialog Info1 = new OpenFileDialog();
            Info1.InitialDirectory = "SlutProdukt/KinectSystem/bin";
            Info1.Filter = "(*.txt)|*.txt";
            Info1.RestoreDirectory = true;

            //Tar bort tidigare filens värden från valueList
            valueList.Clear();

            if (Info1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info1.FileName;
                InfoLabel1.Content = file;
                ReadFromFile(file);
            }
        }

        //Funktion som läser in tecken från en textfil och plottar dessa i en lineChart.
        private void ReadFromFile(string JointName)
        {
            try
            {
                if (File.Exists(JointName))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(JointName);
                    while ((line = file.ReadLine()) != null)
                    {
                        for (int i = 0; i < line.Length; i++)
                        {
                            //Lägger in objekt i number allt eftersom strängen gås igenom. Slutar när ; eller : påträffas
                            if (Convert.ToString(line[i]) != ":" && Convert.ToString(line[i]) != ";")
                            {
                                number += line[i];
                            }
                            //När det finns ett ":" läggs strängen med siffror in i angle efter det gjorts om till int
                            else if (Convert.ToString(line[i]) == ":") //Första variabeln
                            {
                                angle = Convert.ToInt32(number);
                                number = null;
                            }
                            //Samma fast andra variablen. Lägger också till angle och timestamp i valueList
                            else if (Convert.ToString(line[i]) == ";") //den andra variabeln
                            {
                                timestamp = Convert.ToInt32(number);
                                number = null;
                                valueList.Add(new KeyValuePair<int, int>(timestamp, angle));
                                lineChart.DataContext = valueList;
                            }
                            else
                            {
                                //Borde vara felhantering istället - någon form av exeption
                                System.Windows.MessageBox.Show("Något fel har inträffat");
                            }
                        }
                    }
                    file.Close();
                }
                else
                {
                    System.Windows.MessageBox.Show("Filen finns inte");
                }
            }

            catch
            {
                //Borde kanske vara något annat vettigare.
                System.Windows.MessageBox.Show("FEL");
            }
        }

        private void BrowseButton2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Info2 = new OpenFileDialog();
            Info2.InitialDirectory = "c:\\";
            Info2.Filter = "(*.txt)|*.txt|(*.png)|*.png|(*.jpg)|*.jpg";
            Info2.RestoreDirectory = true;

            if (Info2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info2.FileName;
                InfoLabel2.Content = file;
            }
        }

        private void AngleUpOne(object sender, RoutedEventArgs e)
        {
            if (this.KinectSensorOne.ElevationAngle != this.KinectSensorOne.MaxElevationAngle)
            {
                this.KinectSensorOne.ElevationAngle += 1;
                this.viewModel.SensorAngleOne = this.KinectSensorOne.ElevationAngle;
            }
        }

        private void AngleUpTwo(object sender, RoutedEventArgs e)
        {
            if (this.KinectSensorTwo.ElevationAngle != this.KinectSensorTwo.MaxElevationAngle)
            {
                this.KinectSensorTwo.ElevationAngle += 1;
                this.viewModel.SensorAngleTwo = this.KinectSensorTwo.ElevationAngle;
            }
        }

        private void AngleDownOne(object sender, RoutedEventArgs e)
        {
            if (this.KinectSensorOne.ElevationAngle != this.KinectSensorOne.MinElevationAngle)
            {
                this.KinectSensorOne.ElevationAngle -= 1;
                this.viewModel.SensorAngleOne = this.KinectSensorOne.ElevationAngle;
            }
        }

        private void AngleDownTwo(object sender, RoutedEventArgs e)
        {
            if (this.KinectSensorTwo.ElevationAngle != this.KinectSensorTwo.MinElevationAngle)
            {
                this.KinectSensorTwo.ElevationAngle -= 1;
                this.viewModel.SensorAngleTwo = this.KinectSensorTwo.ElevationAngle;
            }
        }

        #endregion Methods

        #region Properties

        public KinectSensor KinectSensorOne
        {
            get
            {
                return this._KinectSensorOne;
            }
            set
            {
                if (this._KinectSensorOne != value)
                {
                    UninitializeKinectSensorOne(this._KinectSensorOne);
                    this._KinectSensorOne = null;

                }
                if (value != null && value.Status == KinectStatus.Connected)
                {
                    this._KinectSensorOne = value;
                    InitializeKinectSensorOne(this._KinectSensorOne);
                    this._KinectSensorOne.ElevationAngle = 0;
                }
            }
        }

        public KinectSensor KinectSensorTwo
        {
            get
            {
                return this._KinectSensorTwo;
            }
            set
            {
                if (this._KinectSensorTwo != value)
                {
                    if (this._KinectSensorTwo != null)
                    {
                        UninitializeKinectSensorTwo(this._KinectSensorTwo);
                        this._KinectSensorTwo = null;
                    }
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this._KinectSensorTwo = value;
                        InitializeKinectSensorTwo(this._KinectSensorTwo);
                        this._KinectSensorTwo.ElevationAngle = 0;
                    }
                }
            }
        }
        #endregion Properties

        //Kom ihåg att fixa tillbaka depthframe event handlern + konstruktorn + Member variables!!!
        #region Test 3D Depth
        private void SetData()
        {
            int i = 0;
            int posZ = 0;
            for (int posY = 0; posY < pixelHeight; posY += 2)
            {
                for (int posX = 0; posX < pixelWidth; posX += 2)
                {
                    modelPoints[i] = CreateTriangleModel(new Point3D(posX, posY, posZ), new Point3D(posX, posY + 2, posZ), new Point3D(posX + 2, posY + 2, posZ));
                    modelPoints[i].Transform = new TranslateTransform3D(0, 0, 0);
                    SkeletonModelGroup.Children.Add(modelPoints[i]);
                    i++;
                }
            }
        }

        private GeometryModel3D CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
            geometryModel = new GeometryModel3D(mesh, material);
            return geometryModel;
        }
        #endregion Test 3D Depth
    }
}
