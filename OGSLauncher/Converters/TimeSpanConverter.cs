using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OGSLauncher.Converters
{
	public class TimeSpanConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			TimeSpan timeSpan = ( TimeSpan ) value;
			return $"{timeSpan.Hours:00}hours {timeSpan.Minutes:00}minute {timeSpan.Seconds:00}second";
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
