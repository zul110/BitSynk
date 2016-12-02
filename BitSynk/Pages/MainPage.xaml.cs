using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BitSynk.Pages {
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// The main page that directs the navigation of the app on launch
    /// Derives from the BasePage
    /// </summary>
    public partial class MainPage : BasePage {
        /// <summary>
        /// Initializes the MainPage
        /// </summary>
        public MainPage() {
            InitializeComponent();
        } 

        /// <summary>
        /// When the page is loaded, check if it is the first time it has been launched
        /// </summary>
        /// <param name="sender">The page</param>
        /// <param name="e">Routed events</param>
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            CheckFirstRun();
        }
        
        private void CheckFirstRun() {
            /*
             * RESETS THE APP TO THE FIRST LAUNCH STATE
             * 
             * Settings.ResetValue(Constants.FIRST_RUN);
             * Settings.ResetValue(Constants.USER_ID);
             * Settings.ResetValue(Constants.DEVICE_ID);
             * Settings.ResetValue(Constants.DEVICE_NAME);
            */
            
            // If the app is launched for the first time,
            // Take the user to the device link page
            // Else take them to the home page
            if(Settings.FIRST_RUN) {
                GoToLinkDevicesPage();

                Settings.FIRST_RUN = false;

                ClearBackEntries();
            } else {
                GoToHomePage();
            }
        }

        private void GoToHomePage() {
            GoToPage(new HomePage());
        }

        private void GoToLinkDevicesPage() {
            GoToPage(new LinkDevicesPage());
        }
    }
}
