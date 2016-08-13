using BitSynk.Helpers;
using BitSynk.Models;
using BitSynk.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private object parameter = null;
        private DispatcherTimer timer;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public HomePage(object parameter) {
            InitializeComponent();

            this.parameter = parameter;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;

            this.DataContext = this;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            if(client != null) {
                client.Refresh();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            ClearBackEntries();

            client = new Client(parameter);
            client.OnTorrentsAdded += Client_OnTorrentsAdded;
            client.OnPeerChanged += Client_OnPeerChanged;

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, ev) => {
                client.StartEngineUsingTorrents();
                timer.Start();
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

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                torrentsDataGrid.ItemsSource = torrents;
            }));
        }

        private void linkButton_Click(object sender, RoutedEventArgs e) {
            GoToPage(new LinkDevicesPage());
        }

        private void torrentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //var torrent = (sender as DataGrid).SelectedItem as BitSynk.Models.BitSynkTorrentModel;

            //var peers = torrent.BitSynkPeers;
        }

        private void browseButton_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.ValidateNames = false;
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = false;
            //dlg.FileName = "Document"; // Default file name
            //dlg.DefaultExt = ".text"; // Default file extension
            //dlg.Filter = "Torrent files (.torrent)|*.torrent"; // Filter files by extension
            
            dlg.FileName = "folder";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if(result == true) {
                // Save document
                string filename = dlg.FileName;
                if(Path.GetFileName(filename) == "folder") {
                    fileBox.Text = Path.GetDirectoryName(filename);
                } else {
                    fileBox.Text = filename;
                }
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e) {
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
