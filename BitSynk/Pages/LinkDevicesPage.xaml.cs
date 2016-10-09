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
    /// </summary>
    public partial class LinkDevicesPage : BasePage {
        public static string PARENT_PAGE = Constants.MAIN_PAGE;

        public LinkDevicesPage() {
            InitializeComponent();

            skipButton.Visibility = PARENT_PAGE == Constants.MAIN_PAGE ? Visibility.Visible : Visibility.Collapsed;

            codeBlock.Text = Settings.USER_ID.Substring(0, 5);
        }

        private void linkButton_Click(object sender, RoutedEventArgs e) {
            LinkDevices();
        }

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

        private void skipButton_Click(object sender, RoutedEventArgs e) {
            GoToPage(new HomePage());
        }
    }
}
