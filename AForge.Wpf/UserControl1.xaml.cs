using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
using AForge.Video;
using AForge.Video.DirectShow;
using Beckhoff.App.Ads.Core;
using Beckhoff.App.Ads.Core.Plc;
using Beckhoff.App.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace CameraUIControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, INotifyPropertyChanged
    {
        BAAdsPlcClient _plcClient;
        private bool _bstartcamera;
        private bool _startcamera;
        public bool StartCameraCommand
        {
            get { return _startcamera; }
            set { _startcamera = _plcClient.ReadSymbol<bool>("Global_HMI.bStartCamera"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        private FilterInfo _currentDevice;
        public FilterInfo CurrentDevice
          {
             get { return _currentDevice; }
             set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
          }
        
        private IVideoSource _videoSource;

        public UserControl1()
        {
          
        }
        public UserControl1(IBAContainerFacade iocContainer)
        {
            InitializeComponent();
            this.DataContext = this;
            
            var adsServer = iocContainer.Resolve<IBAAdsServer>();// get ADSServer
            _plcClient = adsServer.GetAdsClient<IBAAdsPlcClient>("PLC") as BAAdsPlcClient;   // get PLC Client from ADS Server

        }

        protected void OnPropertyChanged([CallerMemberName] string CurrentDevice = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(CurrentDevice));
        }

        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }
        
        private void GetVideoDevices()
        {
            
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        
        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
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

        private void btnGetDevices_Click(object sender, RoutedEventArgs e)
        {
            GetVideoDevices(); // Get video devices connected to this PC
        }
    }
}
