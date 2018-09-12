using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MonoTorrent.Common;

namespace OGSLauncher.Converters
{
	public class StatusConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			TorrentState state = ( TorrentState ) value;
			switch ( state )
			{
				case TorrentState.Downloading: return "Скачивание...";
				case TorrentState.Error: return "Ошибка!";
				case TorrentState.Hashing: return "Хэширование...";
				case TorrentState.Metadata: return "Импорт метаданных...";
				case TorrentState.Paused: return "Приостановлено...";
				case TorrentState.Seeding: return "Раздача...";
				case TorrentState.Stopped: return "Остановлено...";
				case TorrentState.Stopping: return "Остановка...";
				default: return "Статус не известен.";
			}
		}

		public object ConvertBack ( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			throw new NotImplementedException ();
		}
	}
}
