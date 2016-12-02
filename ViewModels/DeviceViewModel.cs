using Helpers;
using DatabaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ViewModels;

namespace ViewModels {
    /// <summary>
    /// Device ViewModel. Acts as a junction between all the devices related local and database functionalities.
    /// </summary>
    public class DeviceViewModel : BaseViewModel {
        // DeviceManager object accessible throughout the class
        private DeviceManager deviceManager;

        // Timer for the device 'heart beat' (device information update interval)
        private DispatcherTimer heartBeatTimer;

        /// <summary>
        /// Constructor, initializes the DeviceManager and the timer
        /// </summary>
        public DeviceViewModel() : base() {
            deviceManager = new DeviceManager();

            heartBeatTimer = new DispatcherTimer();
            heartBeatTimer.Interval = TimeSpan.FromMinutes(5);              // Updates the device information every 5 minutes
            heartBeatTimer.Tick += HeartBeatTimer_Tick;
        }

        /// <summary>
        /// The 'Tick' event handler for the heart beat timer
        /// Gets triggered every time the timer interval is hit
        /// </summary>
        /// <param name="sender">The timer object</param>
        /// <param name="e">Empty EventArgs</param>
        private void HeartBeatTimer_Tick(object sender, EventArgs e) {
            HeartBeat();
        }

        /// <summary>
        /// Updates the device information in the database asynchronously
        /// The IP Address, and the date/time are the main fields of interest
        /// </summary>
        public async void HeartBeat() {
            await deviceManager.UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), Settings.USER_ID, DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the IP addresses of all linked devices (nodes) for the DHT asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<List<IPEndPoint>> GetInitialNodes() {
            List<IPEndPoint> initialNodes = new List<IPEndPoint>();

            foreach(var device in await deviceManager.GetAllDevicesByUserAsync(Settings.USER_ID)) {
                initialNodes.Add(new IPEndPoint(IPAddress.Parse(device.DeviceAddress), 52111));
            }

            return initialNodes;
        }
    }
}
