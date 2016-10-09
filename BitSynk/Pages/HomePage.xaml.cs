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
using Microsoft.WindowsAPICodePack.Dialogs;

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

        private BitSynkTorrentModel selectedTorrent;
        public BitSynkTorrentModel SelectedTorrent
        {
            get { return selectedTorrent; }
            set {
                selectedTorrent = value;
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
            LinkDevicesPage.PARENT_PAGE = Constants.HOME_PAGE;

            GoToPage(new LinkDevicesPage());
        }

        private void addFileButton_Click(object sender, RoutedEventArgs e) {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = false;
            dialog.EnsureValidNames = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;

            CommonFileDialogResult result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok) {
                string filename = dialog.FileName;
                fileBox.Text = filename;

                AddFileOrDirectory();
            }
        }

        private void addFolderButton_Click(object sender, RoutedEventArgs e) {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.EnsureValidNames = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;

            CommonFileDialogResult result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok) {
                string filename = dialog.FileName;
                fileBox.Text = filename;

                AddFileOrDirectory(true);
            }
        }

        private void AddFileOrDirectory(bool isFolder = false) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                client.AddNewTorrent(fileBox.Text, isFolder);
            }));
        }

        private void torrentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(torrentsDataGrid.SelectedItem != null) {
                var selectedTorrent = client.Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).Hash).FirstOrDefault();
                
                if(selectedTorrent.State == MonoTorrent.Common.TorrentState.Paused) {
                    selectedTorrent.Start();
                } else {
                    selectedTorrent.Pause();
                }
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

        private void openMenuItem_Click(object sender, RoutedEventArgs e) {
            string directory = Settings.FILES_DIRECTORY;
            string file = (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).Name;
            string fullPath = directory + "//" + file;

            Process.Start(fullPath);
        }

        private void showInExplorerMenuItem_Click(object sender, RoutedEventArgs e) {
            string directory = Settings.FILES_DIRECTORY;

            Process.Start(directory);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            SelectedTorrent = torrentsDataGrid.SelectedItem as BitSynkTorrentModel;
        }
    }
}
