using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MonoTorrent.Common;

namespace OGSLauncher.Converters
{
	public class FileStatusConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			if ( !( value is Priority ) ) return value.ToString ();
			Priority priority = ( Priority ) value;
			if ( priority == Priority.DoNotDownload ) return "Do not download";
			else return "Downloading";
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
