using BitSynk.Helpers;
using BitSynk.Models;
using BitSynk.ViewModels;
using DatabaseManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace BitSynk.Pages {
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage, INotifyPropertyChanged {
        private Client _client;
        public Client client {
            get {
                return _client;
            }

            set {
                _client = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public HomePage() {
            InitializeComponent();

            this.DataContext = this;
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            ClearBackEntries();

            List<IPEndPoint> initialNodes = new List<IPEndPoint>();

            DeviceManager deviceManager = new DeviceManager();

            await deviceManager.UpdateDeviceAsync(Settings.DEVICE_ID, Settings.DEVICE_NAME, Utils.GetPublicIPAddress(), Settings.USER_ID, DatabaseManager.Models.DeviceStatus.Online);

            foreach(var device in await deviceManager.GetAllDevicesByUserAsync(Settings.USER_ID)) {
                initialNodes.Add(new IPEndPoint(IPAddress.Parse(device.DeviceAddress), 52111));
            }

            client = new Client(initialNodes);
            client.OnTorrentsAdded += Client_OnTorrentsAdded;
            client.OnPeerChanged += Client_OnPeerChanged;

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, ev) => {
                client.StartEngine();//.StartEngineUsingTorrents();
            };
            
            bw.RunWorkerCompleted += (s, ev) => {
                if(ev.Error != null) {

                }
            };

            bw.RunWorkerAsync();
        }

        private void Client_OnPeerChanged(object sender, EventArgs e) {
            try {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    if(torrentsDataGrid.SelectedItem != null) {
                        var peers = (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).BitSynkPeers;

                        peerDataGrid.ItemsSource = peers;
                    }
                }));
            } catch(Exception ex) {

            }
        }

        private void Client_OnTorrentsAdded(object sender, EventArgs e) {
            var torrents = (sender as Client).BitSynkTorrents;

            if(Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    torrentsDataGrid.ItemsSource = torrents;
                }));
            }
        }

        private void linkButton_Click(object sender, RoutedEventArgs e) {
            GoToPage(new LinkDevicesPage());
        }

        private void torrentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //var torrent = (sender as DataGrid).SelectedItem as BitSynk.Models.BitSynkTorrentModel;

            //var peers = torrent.BitSynkPeers;
        }

        private void browseButton_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = false;

            dialog.FileName = "folder";
            
            Nullable<bool> result = dialog.ShowDialog();
            
            if(result == true) {
                string filename = dialog.FileName;

                if(Path.GetFileName(filename) == "folder") {
                    fileBox.Text = Path.GetDirectoryName(filename);
                } else {
                    fileBox.Text = filename;
                }

                if(Settings.AUTO_ADD) {
                    AddFileOrDirectory();
                }
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e) {
            AddFileOrDirectory();
        }

        private void AddFileOrDirectory() {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                client.AddNewTorrent(fileBox.Text);
            }));
        }

        private void torrentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(torrentsDataGrid.SelectedItem != null) {
                string directory = Settings.FILES_DIRECTORY;
                string file = (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).Name;
                string fullPath = directory + "//" + file;

                Process.Start(directory);
            }
        }

        private void removeMenuItem_Click(object sender, RoutedEventArgs e) {
            RemoveFile(torrentsDataGrid.SelectedItem as BitSynkTorrentModel);
        }

        private void RemoveFile(BitSynkTorrentModel bitSynkTorrentModel) {
            new FileTrackerViewModel().RemoveFile(bitSynkTorrentModel);

            client.BitSynkTorrents.Remove(client.BitSynkTorrents.Where(t => t.Hash == bitSynkTorrentModel.Hash).FirstOrDefault());
            client.Torrents.Remove(client.Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == bitSynkTorrentModel.Hash).FirstOrDefault());
        }
    }
}
