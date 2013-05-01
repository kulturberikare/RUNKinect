using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.ComponentModel;
using System.Windows.Forms;
namespace KinectSystem
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Member Variables
        private string sensorStatusValueOne;
        private string ConectionIDValueOne;
        private string deviceIDValueOne;
        private bool isColorStreamEnabledValueOne;
        private bool isDepthStreamEnabledValueOne;
        private bool isSkeletonStreamEnabledValueOne;
        private int sensorAngleValueOne;
        private bool canStartValueOne;
        private bool canStopValueOne;

        private string sensorStatusValueTwo;
        private string ConectionIDValueTwo;
        private string deviceIDValueTwo;
        private bool isColorStreamEnabledValueTwo;
        private bool isDepthStreamEnabledValueTwo;
        private bool isSkeletonStreamEnabledValueTwo;
        private int sensorAngleValueTwo;
        private bool canStartValueTwo;
        private bool canStopValueTwo;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Member Variables

        #region Methods
        public void OnNotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion Methods

        #region Properties
        public string SensorstatusOne
        {
            get
            {
                return this.sensorStatusValueOne;
            }

            set
            {
                if (this.sensorStatusValueOne != value)
                {
                    this.sensorStatusValueOne = value;
                    this.OnNotifyPropertyChanged("SensorStatusOne");
                }
            }
        }

        public string ConectionIDOne
        {
            get
            {
                return this.ConectionIDValueOne;
            }

            set
            {
                if (this.ConectionIDValueOne != value)
                {
                    this.ConectionIDValueOne = value;
                    this.OnNotifyPropertyChanged("ConectionIDOne");
                }
            }
        }

        public string DeviceIDOne
        {
            get
            {
                return this.deviceIDValueOne;
            }

            set
            {
                if (this.deviceIDValueOne != value)
                {
                    this.deviceIDValueOne = value;
                    this.OnNotifyPropertyChanged("DeviceIDOne");
                }
            }
        }

        public bool IsColorStreamEnabledOne
        {
            get
            {
                return this.isColorStreamEnabledValueOne;
            }

            set
            {
                if (isColorStreamEnabledValueOne != value)
                {
                    this.isColorStreamEnabledValueOne = value;
                    this.OnNotifyPropertyChanged("IsColorstreamEnabledOne");
                }
            }
        }

        public bool IsDepthStreamEnabledOne
        {
            get
            {
                return this.isDepthStreamEnabledValueOne;
            }

            set
            {
                if (isDepthStreamEnabledValueOne != value)
                {
                    this.isDepthStreamEnabledValueOne = value;
                    this.OnNotifyPropertyChanged("IsDepthStreamEnabledOne");
                }
            }
        }

        public bool IsSkeletonStreamEnabledOne
        {
            get
            {
                return this.isSkeletonStreamEnabledValueOne;
            }
            set
            {
                if (isSkeletonStreamEnabledValueOne != value)
                {
                    this.isSkeletonStreamEnabledValueOne = value;
                    this.OnNotifyPropertyChanged("IsSkeletonStreamEnabledOne");
                }
            }
        }

        public int SensorAngleOne
        {
            get
            {
                return this.sensorAngleValueOne;
            }

            set
            {
                if (this.sensorAngleValueOne != value)
                {
                    this.sensorAngleValueOne = value;
                    this.OnNotifyPropertyChanged("SensorAngleOne");
                }
            }
        }

        public bool CanStartOne
        {
            get
            {
                return this.canStartValueOne;
            }

            set
            {
                if (this.canStartValueOne != value)
                {
                    this.canStartValueOne = value;
                    this.OnNotifyPropertyChanged("CanStartOne");
                }
            }
        }

        public bool CanStopOne
        {
            get
            {
                return this.canStopValueOne;
            }

            set
            {
                if (this.canStopValueOne != value)
                {
                    this.canStopValueOne = value;
                    this.OnNotifyPropertyChanged("CanStopOne");
                }
            }
        }

        public string SensorstatusTwo
        {
            get
            {
                return this.sensorStatusValueTwo;
            }

            set
            {
                if (this.sensorStatusValueTwo != value)
                {
                    this.sensorStatusValueTwo = value;
                    this.OnNotifyPropertyChanged("SensorStatusTwo");
                }
            }
        }

        public string ConectionIDTwo
        {
            get
            {
                return this.ConectionIDValueTwo;
            }

            set
            {
                if (this.ConectionIDValueTwo != value)
                {
                    this.ConectionIDValueTwo = value;
                    this.OnNotifyPropertyChanged("ConectionIDTwo");
                }
            }
        }

        public string DeviceIDTwo
        {
            get
            {
                return this.deviceIDValueTwo;
            }

            set
            {
                if (this.deviceIDValueTwo != value)
                {
                    this.deviceIDValueTwo = value;
                    this.OnNotifyPropertyChanged("DeviceIDTwo");
                }
            }
        }

        public bool IsColorStreamEnabledTwo
        {
            get
            {
                return this.isColorStreamEnabledValueTwo;
            }

            set
            {
                if (isColorStreamEnabledValueTwo != value)
                {
                    this.isColorStreamEnabledValueTwo = value;                    
                    this.OnNotifyPropertyChanged("IsColorstreamEnabledTwo");
                }
            }
        }

        public bool IsDepthStreamEnabledTwo
        {
            get
            {
                return this.isDepthStreamEnabledValueTwo;
            }

            set
            {
                if (isDepthStreamEnabledValueTwo != value)
                {
                    this.isDepthStreamEnabledValueTwo = value;
                    this.OnNotifyPropertyChanged("IsDepthStreamEnabledTwo");
                }
            }
        }

        public bool IsSkeletonStreamEnabledTwo
        {
            get
            {
                return this.isSkeletonStreamEnabledValueTwo;
            }
            set
            {
                if (isSkeletonStreamEnabledValueTwo != value)
                {
                    this.isSkeletonStreamEnabledValueTwo = value;
                    this.OnNotifyPropertyChanged("IsSkeletonStreamEnabledTwo");
                }
            }
        }

        public int SensorAngleTwo
        {
            get
            {
                return this.sensorAngleValueTwo;
            }

            set
            {
                if (this.sensorAngleValueTwo != value)
                {
                    this.sensorAngleValueTwo = value;
                    this.OnNotifyPropertyChanged("SensorAngleTwo");
                }
            }
        }

        public bool CanStartTwo
        {
            get
            {
                return this.canStartValueTwo;
            }

            set
            {
                if (this.canStartValueTwo != value)
                {
                    this.canStartValueTwo = value;
                    this.OnNotifyPropertyChanged("CanStartTwo");
                }
            }
        }

        public bool CanStopTwo
        {
            get
            {
                return this.canStopValueTwo;
            }

            set
            {
                if (this.canStopValueTwo != value)
                {
                    this.canStopValueTwo = value;
                    this.OnNotifyPropertyChanged("CanStopTwo");
                }
            }
        }

        #endregion Properties
    }
}
