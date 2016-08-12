using BitSynk.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public HomePage(object parameter) {
            InitializeComponent();

            this.parameter = parameter;

            this.DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            ClearBackEntries();

            client = new Client(parameter);
            client.OnTorrentsAdded += Client_OnTorrentsAdded;
            client.OnPeerChanged += Client_OnPeerChanged;

            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, ev) => {
                client.StartEngineUsingTorrents();
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
    }
}
