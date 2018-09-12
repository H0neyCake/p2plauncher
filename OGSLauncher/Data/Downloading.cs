using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent.Common;

namespace OGSLauncher.Data
{
	public class Downloading : INotifyPropertyChanged
	{
		TorrentManager manager;
		long totalBytes;

		public TorrentManager TorrentManager => manager;

	    public string Name { get => manager.Torrent.Name;
	        set { } }
		public double Percentage { get => manager.Progress;
		    set { } }
		public double DownloadBytesPerSeconds { get => manager.Monitor.DownloadSpeed;
		    set { } }
		public double UploadBytesPerSeconds { get => manager.Monitor.UploadSpeed;
		    set { } }
		public TorrentState Status { get => manager.State;
		    set { } }
		public TimeSpan RemainingTime
		{
			get
			{
				return ( DownloadBytesPerSeconds != 0 ) ?
					TimeSpan.FromSeconds ( ( totalBytes - manager.Monitor.DataBytesDownloaded ) / DownloadBytesPerSeconds ) :
					TimeSpan.FromDays ( 99999 );
			}
		}
		public int Seeders { get { return manager.Peers.Seeds; } }
		public int Leechers { get { return manager.Peers.Leechs; } }

		public Downloading ( TorrentManager manager )
		{
			this.manager = manager;

			foreach ( TorrentFile file in manager.Torrent.Files )
				if ( file.Priority != Priority.DoNotDownload )
					totalBytes += file.Length;
		}

		private void PC ( string name )
		{
			if ( PropertyChanged != null )
				PropertyChanged ( this, new PropertyChangedEventArgs ( name ) );
		}

		public void Refresh ()
		{
			PC ( "Percentage" );
			PC ( "DownloadBytesPerSeconds" );
			PC ( "UploadBytesPerSeconds" );
			PC ( "Status" );
			PC ( "RemainingTime" );
			PC ( "Seeders" );
			PC ( "Leechers" );
		}

		public event PropertyChangedEventHandler PropertyChanged;

	}
}
