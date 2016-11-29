using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BitSynk.Converters {
    /// <summary>
    /// CONVERTER: Converts a double value to a 2 decimal format number
    /// </summary>
    public class DoubleFormatConverter : IValueConverter {
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
                return null;
            }

            // Format the double value to a 2 decimal places number
            return String.Format("{0:0.00}", double.Parse(value.ToString()));
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
