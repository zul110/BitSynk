using Helpers;
using Models;
using ViewModels;
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
        private DeviceViewModel deviceVM;

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

        public HomePage() {
            InitializeComponent();

            deviceVM = new DeviceViewModel();

            this.DataContext = this;
        }
        
        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            ClearBackEntries();

            client = new Client(await deviceVM.GetInitialNodes());
            client.OnTorrentsAdded += Client_OnTorrentsAdded;
            client.OnPeerChanged += Client_OnPeerChanged;

            InitClient();
        }

        private void InitClient() {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, ev) => {
                client.StartEngine();
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
            var torrents = (sender as Client).bitSynkTorrents;

            if(Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    torrentsDataGrid.ItemsSource = torrents;
                }));
            }
        }

        private void torrentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            
        }

        private void linkButton_Click(object sender, RoutedEventArgs e) {
            GoToPage(new LinkDevicesPage());
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

            client.bitSynkTorrents.Remove(client.bitSynkTorrents.Where(t => t.Hash == bitSynkTorrentModel.Hash).FirstOrDefault());
            client.Torrents.Remove(client.Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == bitSynkTorrentModel.Hash).FirstOrDefault());
        }
    }
}
