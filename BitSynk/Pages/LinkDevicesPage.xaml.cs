using BitSynk.Helpers;
using BitSynk.ViewModels;
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
    public partial class LinkDevicesPage : Page {
        public LinkDevicesPage() {
            InitializeComponent();
        }

        private void linkButton_Click(object sender, RoutedEventArgs e) {
            LinkDevices();
        }

        private async void LinkDevices() {
            string userCode = userCodeBox.Text;

            if(userCode != Settings.USER_ID.Substring(0, 5)) {
                if(await new AuthViewModel().LinkDevices(userCode)) {
                    Client client = new Client();
                }
            } else {
                Client client = new Client();
            }
        }
    }
}
