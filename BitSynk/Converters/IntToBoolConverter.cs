using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BitSynk.Converters {
    public class IntToBoolConverter : IValueConverter {
        int value = 0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return null;
            }

            if(int.TryParse(value.ToString(), out this.value)) {
                if(parameter != null && parameter.ToString() == "inverse") {
                    return this.value < 0 ? true : false;
                }

                return this.value > 0 ? true : false;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
