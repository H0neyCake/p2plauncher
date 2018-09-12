using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;

namespace OGSLauncher.Data
{
	class SelectableFileItem : INotifyPropertyChanged
	{
		#region Icon
		private static ImageSource GetBitmapImage ( Stream stream )
		{
			BitmapImage image = new BitmapImage ();
			image.BeginInit ();
			image.StreamSource = stream;
			image.EndInit ();
			return image;
		}

		private static Stream GetResourceStream ( string path )
		{
			return Assembly.GetEntryAssembly ().GetManifestResourceStream ( path );
		}

		private static ImageSource folderIcon;
		private static ImageSource FolderIcon
		{
			get
			{
				if ( folderIcon != null ) return folderIcon;
				return folderIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.folder.png" ) );
			}
		}

		private static ImageSource documentIcon;
		private static ImageSource DocumentIcon
		{
			get
			{
				if ( documentIcon != null ) return documentIcon;
				return documentIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.page_white_text.png" ) );
			}
		}

		private static ImageSource compressionIcon;
		private static ImageSource CompressionIcon
		{
			get
			{
				if ( compressionIcon != null ) return compressionIcon;
				return compressionIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.compress.png" ) );
			}
		}

		private static ImageSource pdfIcon;
		private static ImageSource PDFIcon
		{
			get
			{
				if ( pdfIcon != null ) return pdfIcon;
				return pdfIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.book.png" ) );
			}
		}

		private static ImageSource movieIcon;
		private static ImageSource MovieIcon
		{
			get
			{
				if ( movieIcon != null ) return movieIcon;
				return movieIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.film.png" ) );
			}
		}

		private static ImageSource musicIcon;
		private static ImageSource MusicIcon
		{
			get
			{
				if ( musicIcon != null ) return musicIcon;
				return musicIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.music.png" ) );
			}
		}

		private static ImageSource imageIcon;
		private static ImageSource ImageIcon
		{
			get
			{
				if ( imageIcon != null ) return imageIcon;
				return imageIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.picture.png" ) );
			}
		}

		private static ImageSource unknownIcon;
		private static ImageSource UnknownIcon
		{
			get
			{
				if ( unknownIcon != null ) return unknownIcon;
				return unknownIcon = GetBitmapImage ( GetResourceStream ( "DaramTorrent.Images.page_white.png" ) );
			}
		}
		#endregion

		#region Icon Definition
		private static string [] DocumentType = { "txt", "doc", "rtf", "docx", "odt", "hwp", "page" };
		private static string [] MusicType = { "mp3", "mp2", "mp1", "ogg", "flac", "aac", "wav", "mid", "midi", "oga", "mka", "wma", "aiff" };
		private static string [] MovieType = { "avi", "mp4", "mpg", "mpeg", "mkv", "rbmv", "wmv", "rm" };
		private static string [] CompressionType = { "tar", "gz", "bz2", "bz", "zip", "rar", "7z", "lzma", "alz", "egg", "zipx", "tgz",
													   "tbz", "tbz2", "z", "001", "cab", "lzh", "ace", "txz", "tlz" };
		private static string [] ImageType = { "bmp", "dib", "png", "jpg", "jpeg", "gif", "tiff", "psd", "dds", "tif", "jpe", "hdp", "jpc" };
		private static string [] PDFType = { "pdf", "opf", "epub", "xmdf", "fb2", "bbeb", "cbr", "cbz", "xps", "chm", "mobi" };
		#endregion

		bool isChecked;
		public bool IsChecked
		{
			get { return isChecked; }
			set
			{
				isChecked = value;
				foreach ( SelectableFileItem i in SubItems )
					i.IsChecked = value;
				PC ( "IsChecked" );
			}
		}

		string name;
		public string Name { get { return name; } set { name = value; PC ( "Name" ); } }

		ImageSource icon;
		public ImageSource Icon
		{
			get
			{
				if ( icon != null ) return icon;
				if ( subItems == null || subItems.Count != 0 ) return icon = FolderIcon;
				else
				{
					string ext = System.IO.Path.GetExtension ( name ).Substring ( 1 ).ToLower ();
					if ( DocumentType.Contains ( ext ) ) return icon = DocumentIcon;
					else if ( MusicType.Contains ( ext ) ) return icon = MusicIcon;
					else if ( MovieType.Contains ( ext ) ) return icon = MovieIcon;
					else if ( CompressionType.Contains ( ext ) ) return icon = CompressionIcon;
					else if ( ImageType.Contains ( ext ) ) return icon = ImageIcon;
					else if ( PDFType.Contains ( ext ) ) return icon = PDFIcon;
					else return icon = UnknownIcon;
				}
			}
		}

		List<SelectableFileItem> subItems;
		public List<SelectableFileItem> SubItems { get { return subItems; } private set { subItems = value; } }

		public SelectableFileItem ()
		{
			SubItems = new List<SelectableFileItem> ();
			IsChecked = true;
		}

		private void PC ( string name )
		{
			if ( PropertyChanged != null )
				PropertyChanged ( this, new PropertyChangedEventArgs ( name ) );
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
