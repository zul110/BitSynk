using BitSynk.Helpers;
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
    /// </summary>
    public partial class MainPage : BasePage {
        public MainPage() {
            InitializeComponent();
        } 

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            CheckFirstRun();
        }
        
        private void CheckFirstRun() {
            Settings.ResetValue(Constants.FIRST_RUN);

            string firstRun = Settings.GetValue(Constants.FIRST_RUN);

            if(Settings.FIRST_RUN) {
                GoToLinkDevicesPage();

                Settings.FIRST_RUN = false;

                ClearBackEntries();
            } else {
                GoToHomePage();
            }
        }

        private void GoToHomePage() {
            GoToPage(new HomePage("torrents"));//("/Pages/HomePage.xaml");
        }

        private void GoToLinkDevicesPage() {
            GoToPage(new LinkDevicesPage());// "/Pages/LinkDevicesPage.xaml");
        }
    }
}
