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
    public class DeviceViewModel : BaseViewModel {
        private DeviceManager deviceManager;
        private DispatcherTimer heartBeatTimer;

        public DeviceViewModel() : base() {
            deviceManager = new DeviceManager();

            heartBeatTimer = new DispatcherTimer();
            heartBeatTimer.Interval = TimeSpan.FromMinutes(5);
            heartBeatTimer.Tick += HeartBeatTimer_Tick;
        }

        private void HeartBeatTimer_Tick(object sender, EventArgs e) {
            HeartBeat();
        }

        public async void HeartBeat() {
            await deviceManager.UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), Settings.USER_ID, DateTime.UtcNow);
        }

        public async Task<List<IPEndPoint>> GetInitialNodes() {
            List<IPEndPoint> initialNodes = new List<IPEndPoint>();

            foreach(var device in await deviceManager.GetAllDevicesByUserAsync(Settings.USER_ID)) {
                initialNodes.Add(new IPEndPoint(IPAddress.Parse(device.DeviceAddress), 52111));
            }

            return initialNodes;
        }
    }
}
