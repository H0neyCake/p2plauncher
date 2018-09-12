using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OGSLauncher.Converters
{
	public class FileSizeConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			double size = ( value is long ) ? ( long ) value : ( ( value is int ) ? ( int ) value : ( double ) value );
			if ( size / 1024.0f < 1 ) return $"{size:0.0}B";
			size /= 1024.0f;
			if ( size / 1024.0f < 1 ) return $"{size:0.0}KB";
			size /= 1024.0f;
			if ( size / 1024.0f < 1 ) return $"{size:0.0}MB";
			size /= 1024.0f;
			if ( size / 1024.0f < 1 ) return $"{size:0.0}GB";
			size /= 1024.0f;
			if ( size / 1024.0f < 1 ) return $"{size:0.0}TB";
			size /= 1024.0f;
			return $"{size:0.0}PB";
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
