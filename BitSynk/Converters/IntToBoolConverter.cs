using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BitSynk.Converters {
    /// <summary>
    /// CONVERTER: Converts an integer to a boolean
    /// </summary>
    public class IntToBoolConverter : IValueConverter {
        // An integer to store the parsed integer
        private int value = 0;

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

            // Try to parse the value into an integer, and store it in the 'value' variable
            if(int.TryParse(value.ToString(), out this.value)) {
                // If an optional parameter is passed, return a value according to it.
                // In this converter, the only parameter that can be parsed and interpreted is 'inverse'.
                // If 'inverse' is parsed, the converter returns an inverse result.
                // Normally, 1 = true, and 0 = false; inverse changes this to 1 = false, and 0 = true
                if(parameter != null && parameter.ToString() == "inverse") {
                    return this.value < 0 ? true : false;
                }

                // If no parameter is passed, return the normal result
                return this.value > 0 ? true : false;
            }

            // If the parser fails to parse the value, return false.
            return false;
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
