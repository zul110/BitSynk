using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels {
    /// <summary>
    /// Base ViewModel ABSTRACT class that is inhereted by all ViewModels
    /// Implements the INotifyPropertyChanged interface, which raises the PropertyChanged event, in case a passed property changes.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged {
        /// <summary>
        /// PropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string property = "") {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// An empty VIRTUAL method that can be overridden by the derived classes.
        /// It could be an ABSTRACT method, as it is declared, but not defined.
        /// It was initially thought to contain some base functionality, and thus has been left as it is.
        /// </summary>
        protected virtual void InitViewModel() {

        }
    }
}
