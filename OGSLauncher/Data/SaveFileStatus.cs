using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Common;

namespace OGSLauncher.Data
{
	[Serializable]
	public class SaveFileStatus
	{
		Dictionary<string, bool> fileStatus = new Dictionary<string, bool> ();

		public Dictionary<string, bool> NativeCollection { get { return fileStatus; } }

		public SaveFileStatus ( TorrentFile [] files )
		{
			foreach ( TorrentFile i in files )
				Add ( i.Path, i.Priority );
		}

		public void Add ( string path, Priority priority )
		{
			fileStatus.Add ( path, ( priority == Priority.DoNotDownload ) ? false : true );
		}

		public void Remove ( string path )
		{
			fileStatus.Remove ( path );
		}

		public Priority this [ string path ] { get { return fileStatus [ path ] ? Priority.Normal : Priority.DoNotDownload; } }
	}
}
