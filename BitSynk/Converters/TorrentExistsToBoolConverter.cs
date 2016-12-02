using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BitSynk.Converters {
    /// <summary>
    /// CONVERTER: If torrent exists (checked via its name), return a bool value
    /// </summary>
    public class TorrentExistsToBoolConverter : IValueConverter {
        /// <summary>
        /// Converter
        /// </summary>
        /// <param name="value">The value to be converted</param>
        /// <param name="targetType">The type of the target property</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Current culture information</param>
        /// <returns>The converted value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            // If the value to be converted is null, exit, and return null
            if(value == null) {
                return false;
            }

            // Get the full file path
            string file = System.IO.Path.Combine(Helpers.Settings.FILES_DIRECTORY, (value as BitSynkTorrentModel).Name);

            // Return true if the file exists, false if it doesn't
            return System.IO.File.Exists(file);
        }

        /// <summary>
        /// Reverter method. This method is not required, thus is not implemented.
        /// Crashes the app when used.
        /// </summary>
        /// <param name="value">The value to be converted</param>
        /// <param name="targetType">The type of the target property</param>
        /// <param name="parameter">Optional parameter</param>
        /// <param name="culture">Current culture information</param>
        /// <returns>The original value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
