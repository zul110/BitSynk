using Helpers;
using Models;
using ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BitSynk.Pages {
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// The core synchronization UI page. Displays the status of each sync task,
    /// allows the user to add and remove tasks, link devices, etc.
    /// Derives from the Base Page
    /// </summary>
    public partial class HomePage : BasePage {
        /// <summary>
        /// The actual BitSynk client
        /// </summary>
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

        /// <summary>
        /// When the user selects a torrent from the list in the UI, it is stored here for easier reference
        /// </summary>
        private BitSynkTorrentModel selectedTorrent;
        public BitSynkTorrentModel SelectedTorrent
        {
            get { return selectedTorrent; }
            set {
                selectedTorrent = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor: Initializes the page and the device view model, and sets the page's data context to itself
        /// </summary>
        public HomePage() {
            InitializeComponent();

            this.DataContext = this;
        }
        
        /// <summary>
        /// Page load event handler
        /// </summary>
        /// <param name="sender">The page</param>
        /// <param name="e">Routed event arguments</param>
        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            // When the page loads, clear the back stack (remove previous pages)
            ClearBackEntries();

            // Initialize the client by getting initial DHT nodes from the device view model
            client = new Client(await new DeviceViewModel().GetInitialNodes());
            client.OnTorrentsAdded += Client_OnTorrentsAdded;                       // Handler for when torrents are added in the client
            client.OnPeerChanged += Client_OnPeerChanged;                           // Handler for when a peer is changed

            // Startss the client engine
            InitClient();
        }

        /// <summary>
        /// Starts the client as a background task
        /// </summary>
        private void InitClient() {
            // Initialize the BackgroundWorker
            BackgroundWorker bw = new BackgroundWorker();
            
            // Start the engine in the background
            bw.DoWork += (s, ev) => {
                client.StartEngine();
            };

            // Handle cases after the background worker has been completed
            bw.RunWorkerCompleted += (s, ev) => {
                if(ev.Cancelled) {
                    // Do nothing if cancelled
                }

                if(ev.Error != null) {
                    // Do nothing on error
                }

                if(ev.Result != null) {
                    // Do nothing even if there is a result
                }
            };

            // Run the background worker
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the case when a torrent's peer changes
        /// Updates the UI with the peers' information
        /// </summary>
        /// <param name="sender">The client that triggers the event</param>
        /// <param name="e">Empty event arguments</param>
        private void Client_OnPeerChanged(object sender, EventArgs e) {
            try {
                // Update the list of peers on the UI thread (in the peerDataGrid)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    if(torrentsDataGrid.SelectedItem != null) {
                        // Get all the peers for the selected torrent
                        var peers = (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).BitSynkPeers;
                        
                        // Set the peerDataGrid's ItemsSource to the peers list
                        peerDataGrid.ItemsSource = peers;
                    }
                }));
            } catch(Exception ex) {
                // Do nothing in case of an exception; the event will be triggerred again when a change occurs
            }
        }

        /// <summary>
        /// Handles the addition of torrents to the client
        /// Updates the UI with the new torrents' information
        /// </summary>
        /// <param name="sender">The client</param>
        /// <param name="e">Empty event arguments</param>
        private void Client_OnTorrentsAdded(object sender, EventArgs e) {
            var torrents = (sender as Client).bitSynkTorrents;

            if(Application.Current != null) {
                // On the UI thread, update the torrentsDataGrid's ItemsSource to the new list of torrents
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    torrentsDataGrid.ItemsSource = torrents;
                }));
            }
        }

        /// <summary>
        /// Handles the event when the user clicks the link button
        /// Takes the user to the link devices page
        /// </summary>
        /// <param name="sender">The link button</param>
        /// <param name="e">Routed event arguments</param>
        private void linkButton_Click(object sender, RoutedEventArgs e) {
            // Set the LinkDevicesPage's PARENT_PAGE static variable to home page
            LinkDevicesPage.PARENT_PAGE = Constants.HOME_PAGE;

            // Navigate to the LinkDevicesPage
            GoToPage(new LinkDevicesPage());
        }

        /// <summary>
        /// Handles the event when the user clicks the add file button
        /// Allows the user to select a file from the file system
        /// </summary>
        /// <param name="sender">The add file button</param>
        /// <param name="e">Routed event arguments</param>
        private void addFileButton_Click(object sender, RoutedEventArgs e) {
            AddFileOrFolder(false);
        }

        /// <summary>
        /// Handles the event when the user clicks the add folder button
        /// Allows the user to select a folder from the file system
        /// Disabled in the current build due to a bug in the MonoTorrent library
        /// </summary>
        /// <param name="sender">The add folder button</param>
        /// <param name="e">Routed system arguments</param>
        private void addFolderButton_Click(object sender, RoutedEventArgs e) {
            AddFileOrFolder(true);
        }
        
        /// <summary>
        /// Adds a file or folder to the client engine
        /// </summary>
        /// <param name="isFolder">
        /// OPTIONAL:
        /// Check to allow the dialog to either select a file or a folder
        /// Adds the selected item as a file or folder in the client engine
        /// </param>
        private void AddFileOrFolder(bool isFolder = false) {
            // Initialize the CommonOpenFileDialog (from external library called WindowsAPICodePack)
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = isFolder;
            dialog.EnsureValidNames = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;

            // Store the result of the dialog after the user has interacted with it
            CommonFileDialogResult result = dialog.ShowDialog();

            // If the result is Ok (user selects a valid file, and clicks Ok)
            if(result == CommonFileDialogResult.Ok) {
                // Get the file's path and show it in the fileBox TextBox
                string filename = dialog.FileName;
                fileBox.Text = filename;

                // Add the new file to the client engine as a torrent task
                // Done on the UI thread as it also updates the GUI
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    client.AddNewTorrent(fileBox.Text, isFolder);
                }));
            }
        }
        
        /// <summary>
        /// Handles the event when the user double clicks an item
        /// Toggles between start and pause states of the torrent task
        /// </summary>
        /// <param name="sender">The data grid</param>
        /// <param name="e">Mouse button event arguments</param>
        private void torrentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(torrentsDataGrid.SelectedItem != null) {
                // Get the selected torrent
                var selectedTorrent = client.Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).Hash).FirstOrDefault();
                
                // Toggle its state between start and pause
                if(selectedTorrent.State == MonoTorrent.Common.TorrentState.Paused) {
                    selectedTorrent.Start();
                } else {
                    selectedTorrent.Pause();
                }
            }
        }
        
        /// <summary>
        /// Removes the selected item
        /// </summary>
        /// <param name="sender">Context menu item</param>
        /// <param name="e">Routed event arguments</param>
        private void removeMenuItem_Click(object sender, RoutedEventArgs e) {
            RemoveFile(torrentsDataGrid.SelectedItem as BitSynkTorrentModel);
        }

        /// <summary>
        /// Removes the file of the selected item from the system (locally, its torrent, from the database, and other devices)
        /// </summary>
        /// <param name="bitSynkTorrentModel">The BitSynk custom torrent model</param>
        private async void RemoveFile(BitSynkTorrentModel bitSynkTorrentModel) {
            // Remove file asynchronously
            await new FileTrackerViewModel().RemoveFileAsync(bitSynkTorrentModel);

            // Remove the records of the file
            client.bitSynkTorrents.Remove(client.bitSynkTorrents.Where(t => t.Hash == bitSynkTorrentModel.Hash).FirstOrDefault());
            client.Torrents.Remove(client.Torrents.Where(t => t.InfoHash.ToString().Replace("-", "") == bitSynkTorrentModel.Hash).FirstOrDefault());
        }

        /// <summary>
        /// Launches the selected file
        /// </summary>
        /// <param name="sender">The context menu</param>
        /// <param name="e">Routed event arguments</param>
        private void openMenuItem_Click(object sender, RoutedEventArgs e) {
            string file = (torrentsDataGrid.SelectedItem as BitSynkTorrentModel).Name;
            string fullPath = Settings.FILES_DIRECTORY + "//" + file;

            Process.Start(fullPath);
        }

        /// <summary>
        /// Opens the directory the file in the file system
        /// </summary>
        /// <param name="sender">The context menu item</param>
        /// <param name="e">Routed event arguments</param>
        private void showInExplorerMenuItem_Click(object sender, RoutedEventArgs e) {
            Process.Start(Settings.FILES_DIRECTORY);
        }

        /// <summary>
        /// Handles the event when the user opens the context menu of the selected torrent by right-clicking it
        /// </summary>
        /// <param name="sender">The context menu</param>
        /// <param name="e">Routed event arguments</param>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            // Set the SelectedTorrent to the torrent that was right-clicked
            SelectedTorrent = torrentsDataGrid.SelectedItem as BitSynkTorrentModel;
        }
    }
}
