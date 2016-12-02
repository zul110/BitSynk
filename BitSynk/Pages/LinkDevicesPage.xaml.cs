using Helpers;
using ViewModels;
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
    /// Interaction logic for LinkDevicesPage.xaml
    /// Allows the user to link devices
    /// Derives from the BasePage
    /// </summary>
    public partial class LinkDevicesPage : BasePage {
        // Since this page can be accessed from multiple pages (Main and Home for the current version of the app),
        // it maintains a single record of its parent page, i.e. the main page. The default value is the main page,
        // but since this string is accessible from anywhere within the project (public static), it can be changed by any page.
        public static string PARENT_PAGE = Constants.MAIN_PAGE;

        /// <summary>
        /// Constructor: initializes the page's UI
        /// </summary>
        public LinkDevicesPage() {
            InitializeComponent();

            // Skip button is only shown if the parent page is the main page
            skipButton.Visibility = PARENT_PAGE == Constants.MAIN_PAGE ? Visibility.Visible : Visibility.Collapsed;

            // The user code is just the first 5 characters of the User ID.
            // This can be improved further in numerous ways, including hashing
            // the User ID first, then extracting the first 5 characters.
            // However, for the scope of the current app, it has been limited
            // to the first 5 characters of the original User ID, for the sake of simplicity
            codeBlock.Text = Settings.USER_ID.Substring(0, 5);
        }

        /// <summary>
        /// The click event of the link button
        /// </summary>
        /// <param name="sender">The link button</param>
        /// <param name="e">Routed event arguments</param>
        private void linkButton_Click(object sender, RoutedEventArgs e) {
            LinkDevices();
        }

        /// <summary>
        /// Links the devices, and takes the user to the home page if successful.
        /// </summary>
        private async void LinkDevices() {
            string userCode = userCodeBox.Text;

            if(userCode.Length < 5) {
                MessageBox.Show("Invalid code. Please make sure that you have the correct 5 digit code, and try agian.");

                return;
            }

            if(userCode != Settings.USER_ID.Substring(0, 5)) {
                if(await new AuthViewModel().LinkDevices(userCode)) {
                    GoToPage(new HomePage());
                } else {
                    MessageBox.Show("Invalid code. Please make sure that you have the correct 5 digit code, and try agian.");
                }
            } else {
                MessageBox.Show("Invalid code. You cannot link this device to itself!");
            }
        }

        /// <summary>
        /// Takes the user directly to the home page, without linking the devices
        /// </summary>
        /// <param name="sender">The skip button</param>
        /// <param name="e">Routed event arguments</param>
        private void skipButton_Click(object sender, RoutedEventArgs e) {
            GoToPage(new HomePage());
        }
    }
}
