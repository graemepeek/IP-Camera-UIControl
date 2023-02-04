using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using Beckhoff.App.Ads.Core;
using Beckhoff.App.Ads.Core.Plc;
using Beckhoff.App.Core.Interfaces;
using System.Runtime.CompilerServices;


namespace IPCameraUIControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, INotifyPropertyChanged
    {
        BAAdsPlcClient _plcClient; // not used in this example but here for reference

        #region Public properties

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; this.OnPropertyChanged("ConnectionString"); }
        }
        public bool UseMjpegStream
        {
            get { return _useMJPEGStream; }
            set { _useMJPEGStream = value; this.OnPropertyChanged("UseMjpegStream"); }
        }
        public bool UseJpegStream
        {
            get { return _useJPEGStream; }
            set { _useJPEGStream = value; this.OnPropertyChanged("UseJpegStream"); }
        }

        #endregion
        #region Private fields

        private string _connectionString;
        private bool _useMJPEGStream;
        private bool _useJPEGStream;
        private IVideoSource _videoSource;
        private bool _startcamera;
        #endregion

        // not used in this example but here for reference
        public bool StartCameraCommand
        {
            get { return _startcamera; }
            set { _startcamera = _plcClient.ReadSymbol<bool>("Global_HMI.bStartCamera"); }
        }
        
        //public event PropertyChangedEventHandler PropertyChanged;
       
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        private FilterInfo _currentDevice;
        public FilterInfo CurrentDevice
          {
             get { return _currentDevice; }
             set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
          }

        public UserControl1()
        {
          
        }
        public UserControl1(IBAContainerFacade iocContainer)
        {
            InitializeComponent();
            this.DataContext = this;
            var adsServer = iocContainer.Resolve<IBAAdsServer>();// get ADSServer
            _plcClient = adsServer.GetAdsClient<IBAAdsPlcClient>("PLC") as BAAdsPlcClient;   // get PLC Client from ADS Server
            ConnectionString = "http://<axis_camera_ip>/axis-cgi/jpg/image.cgi";
            // Test address
            ConnectionString = "http://88.53.197.250/axis-cgi/mjpg/video.cgi?resolution=320x240";
            UseMjpegStream = true;
            tbConnectionString.Text = ConnectionString;
            rbMPeg.IsChecked = true;
        }
        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
           UseJpegStream = ((bool)rbJpeg.IsChecked); // force read of radio button state as binding not working at this point TODO:
           ConnectionString = tbConnectionString.Text;
            // create JPEG video source
            if (UseJpegStream)
            {
                _videoSource = new JPEGStream(ConnectionString);
            }
            else // UseMJpegStream
            {
                _videoSource = new MJPEGStream(ConnectionString);
            }
            _videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            _videoSource.Start();
        }
        

         
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

       public void StartCamera()
        {
            if (CurrentDevice != null)
              {
                 _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                 _videoSource.NewFrame += video_NewFrame;
                 _videoSource.Start();
             }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
        }

        private void btnStopCamera_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }



        #region INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}
