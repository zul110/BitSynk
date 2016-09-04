using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BitSynk.Pages {
    public class BasePage : Page, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public BasePage() {

        }

        protected void GoToPage(string page) {
            NavigationService.Navigate(new Uri(page, UriKind.RelativeOrAbsolute));
        }

        protected void GoToPage(object page) {
            NavigationService.Navigate(page);
        }

        protected void ClearBackEntries() {
            while(NavigationService.CanGoBack) {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
