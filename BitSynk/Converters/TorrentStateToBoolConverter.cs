using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BitSynk.Converters {
    public class TorrentStateToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) {
                return false;
            }

            string file = System.IO.Path.Combine(Helpers.Settings.FILES_DIRECTORY, (value as BitSynkTorrentModel).Name);

            return System.IO.File.Exists(file);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
