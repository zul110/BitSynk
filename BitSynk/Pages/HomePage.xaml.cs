using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage {
        private Client client;
        private object parameter = null;

        public HomePage(object parameter) {
            InitializeComponent();

            this.parameter = parameter;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            ClearBackEntries();

            client = new Client(parameter);

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
    }
}
