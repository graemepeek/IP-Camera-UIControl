using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beckhoff.App.Ads.Core;
using Beckhoff.App.Ads.Core.Plc;
using Beckhoff.App.Core.Interfaces;
using GalaSoft.MvvmLight;


namespace CameraUIControl
{
    public class CameraVM : ViewModelBase
    {
        BAAdsPlcClient _plcClient;
        private bool _bstartcamera; // Local varible to hold PLC var toggles value
        private List<IBAAdsNotification> _notifications;
        
        /// <summary>
        /// Constructor; class is used as ViewModel for Control
        /// </summary>
        public CameraVM()
        {

        }

        public CameraVM(IBAContainerFacade iocContainer) : this()
        {
            var adsServer = iocContainer.Resolve<IBAAdsServer>();// get ADSServer
            _notifications = new List<IBAAdsNotification>(); // create instance of notifications list for ADS events
            _plcClient = adsServer.GetAdsClient<IBAAdsPlcClient>("PLC") as BAAdsPlcClient;   // get PLC Client from ADS Server
            var notifyStartCamera = new OnChangeDeviceNotification("Global_HMI.bStartCamera", StartCameraVarHandler); // Setup notification on PLC toggle var changing state
            // Add to notifications list each on change notification instance
            _notifications.Add(notifyStartCamera);
     
            // Create notications (should add a remove for a form but for UI i'm not adding at this point)
            // So these should in form load and a a remove on a form unload or visability change
            _plcClient.AddPlcDeviceNotification(notifyStartCamera);
        }

        

        // Call handler if change in state event is notification happens for toggle PLC variable
        private void StartCameraVarHandler(object sender, BAAdsNotificationItem notification)
        {
            if (notification.Name.Equals("Global_HMI.bStartCamera"))
            {
                if (notification.PlcObject is string)
                {
                    _bstartcamera = (bool)(notification.PlcObject);
                    if (_bstartcamera)
                    {
                       // CameraUIControl.UserControl1;
                    }
                }
            }
        }
    }
}
