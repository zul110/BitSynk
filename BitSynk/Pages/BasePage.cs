using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BitSynk.Pages {
    public class BasePage : Page {
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
