using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BitSynk.Pages {
    /// <summary>
    /// The Base page for all pages in the app.
    /// Inherits from the Page class, and implements the INotifyPropertyChanged interface
    /// </summary>
    public class BasePage : Page, INotifyPropertyChanged {
        /// <summary>
        /// PropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public BasePage() {

        }

        /// <summary>
        /// A method that simplifies page navigation using the page's name
        /// </summary>
        /// <param name="page">Name of the page</param>
        protected void GoToPage(string page) {
            NavigationService.Navigate(new Uri(page, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// A method that simplifies page navigation using the page object
        /// </summary>
        /// <param name="page">The actual page object</param>
        protected void GoToPage(object page) {
            NavigationService.Navigate(page);
        }

        /// <summary>
        /// Clear previous pages from the back stack
        /// </summary>
        protected void ClearBackEntries() {
            while(NavigationService.CanGoBack) {
                NavigationService.RemoveBackEntry();
            }
        }
    }
}
