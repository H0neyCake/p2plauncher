using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DerAtrox.Arma3LauncherLib.Model;
using DerAtrox.Arma3LauncherLib.Utilities;
using Microsoft.Win32;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Common;
using OGSLauncher.Data;
using OGSLauncher.Properties;
using Path = System.IO.Path;

namespace OGSLauncher
{
   public partial class MainWindow
    {
        #region Varibles

	    ClientEngine clientEngine;
	    Thread statusRefresher;
	    ObservableCollection<Downloading> downloadingTorrents;


	    private string ResumeFileSavePath
	    {
	        get
	        {
	            string path = System.Windows.Forms.Application.UserAppDataPath;
	            string[] temp = path.Split('\\');
	            temp[temp.Length - 1] = "";
	            path = string.Join("\\", temp) + "Cache.bin";
	            return path;
	        }
	    }

	    private string StatusFileSavePath
	    {
	        get
	        {
	            string path = System.Windows.Forms.Application.UserAppDataPath;
	            string[] temp = path.Split('\\');
	            temp[temp.Length - 1] = "";
	            path = string.Join("\\", temp) + "Status.bin";
	            return path;
	        }
	    }

	    private string TorrentFileDownloadPath
	    {
	        get
	        {
	            string path = System.Windows.Forms.Application.UserAppDataPath;
	            string[] temp = path.Split('\\');
	            temp[temp.Length - 1] = "";
	            path = string.Join("\\", temp) + "Torrents\\";
	            if (!System.IO.Directory.Exists(path))
	                System.IO.Directory.CreateDirectory(path);
	            return path;
	        }
	    }
	    static string _torrentsPath;
        #endregion

        public MainWindow ()
		{
			InitializeComponent ();
           DownloadTorrent();

            _torrentsPath = Environment.CurrentDirectory;


            Settings.Default.DownloadPath = _torrentsPath;
            //Settings.Default.DownloadPath = System.IO.Path.Combine(ArmaUtils.GetArma3Path(), "@1321");
		    Settings.Default.Save ();

		    EngineSettings engineSettings = new EngineSettings
		    {
		        PreferEncryption = true,
		        AllowedEncryption = EncryptionTypes.All,
		        SavePath = TorrentFileDownloadPath
		    };
		    clientEngine = new ClientEngine ( engineSettings );
			clientEngine.ChangeListenEndpoint ( new IPEndPoint ( IPAddress.Parse ( "127.0.0.1" ), 50769 ) );

			downloadingTorrents = new ObservableCollection<Downloading> ();
		    progressList.ItemsSource = downloadingTorrents;


            progressList.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            progressList.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

            statusRefresher = new Thread ( () =>
			{
				while ( true )
				{
					Dispatcher.BeginInvoke ( new Action ( () =>
					{
						foreach ( Downloading d in downloadingTorrents )
							d.Refresh ();
					} ) );
					Thread.Sleep ( 500 );
				}
			} );
			statusRefresher.Start ();
			LoadState ();
		    StartDownload();

		   
		}
        
	    private static void DownloadTorrent()
	    {
	        WebClient webClient = new WebClient();
	        Uri uri = new Uri();
	        webClient.DownloadFileAsync(uri, "Test.torrent");
	    }

        protected override void OnClosing ( CancelEventArgs e )
		{
			SaveState ();

			statusRefresher.Abort ();
			statusRefresher.Join ();
			clientEngine.Dispose ();

			try
			{
				RegistryKey key = Registry.CurrentUser.CreateSubKey ( "Software\\Microsoft\\Windows\\CurrentVersion\\Run" );
				if ( Settings.Default.AutoStart )
				{
					if ( downloadingTorrents.Count != 0 )
						key.SetValue ("OGSLauncher", System.Windows.Forms.Application.ExecutablePath );
					else
						key.DeleteValue ("OGSLauncher");
				}
				else
					key.DeleteValue ("OGSLauncher");
			}
			catch { }

			base.OnClosing ( e );
		}

		private void LoadState ()
		{
			if ( !File.Exists ( ResumeFileSavePath ) ) return;
			if ( !File.Exists ( StatusFileSavePath ) ) return;
			BEncodedList torrents = BEncodedValue.Decode<BEncodedList> ( File.ReadAllBytes ( ResumeFileSavePath ) );
			Dictionary<InfoHash, FastResume> fastResumes = new Dictionary<InfoHash, FastResume> ();
			Dictionary<InfoHash, SaveFileStatus> statuses;

			foreach ( BEncodedDictionary dict in torrents )
			{
				FastResume resume = new FastResume ( dict );
				fastResumes.Add ( resume.Infohash, resume );
			}

			BinaryFormatter df = new BinaryFormatter ();
			using ( Stream stream = File.Open ( StatusFileSavePath, FileMode.Open ) )
			{
				statuses = df.Deserialize ( stream ) as Dictionary<InfoHash, SaveFileStatus>;
			}

			foreach ( string filename in Directory.GetFiles ( TorrentFileDownloadPath, "*.torrent" ) )
			{
				Torrent torrent = Torrent.Load ( filename );
				if ( !fastResumes.ContainsKey ( torrent.InfoHash ) ) continue;
				TorrentManager manager = new TorrentManager ( torrent, Settings.Default.DownloadPath, new TorrentSettings () );
				manager.LoadFastResume ( fastResumes [ torrent.InfoHash ] );
				SaveFileStatus status = statuses [ torrent.InfoHash ];
				foreach ( TorrentFile file in manager.Torrent.Files )
					file.Priority = status [ file.Path ];
				downloadingTorrents.Add ( new Downloading ( manager ) );
				clientEngine.Register ( manager );

            }
		}

		private void SaveState ()
		{
			if ( downloadingTorrents.Count == 0 )
			{
				File.Delete ( ResumeFileSavePath );
				File.Delete ( StatusFileSavePath );
				return;
			}

			BEncodedList torrents = new BEncodedList ();
			Dictionary<InfoHash, SaveFileStatus> statuses = new Dictionary<InfoHash, SaveFileStatus> ();
			foreach ( Downloading d in downloadingTorrents )
			{
				FastResume resume = d.TorrentManager.SaveFastResume ();
				BEncodedDictionary dict = resume.Encode ();
				torrents.Add ( dict );
				statuses.Add ( d.TorrentManager.Torrent.InfoHash, new SaveFileStatus ( d.TorrentManager.Torrent.Files ) );
			}
			File.WriteAllBytes ( ResumeFileSavePath, torrents.Encode () );

			BinaryFormatter df = new BinaryFormatter ();
			using ( Stream stream = File.Create ( StatusFileSavePath ) )
			{
				df.Serialize ( stream, statuses );
			}
		}

        private void StartDownload()
	    {
	        foreach (string file in Directory.GetFiles(_torrentsPath))
	        {
	            if (file.EndsWith(".torrent"))
	            {
	                Torrent torrent;
	                try
	                {
	                    torrent = Torrent.Load(file);
	                }
	                catch 
	                {
	                    continue;
	                }

	                TorrentManager torrentManager = new TorrentManager(torrent, Settings.Default.DownloadPath, new TorrentSettings());
	                torrentManager.ChangePicker(new PriorityPicker(new StandardPicker()));
	                downloadingTorrents.Add(new Downloading(torrentManager));

	                clientEngine.Register(torrentManager);
	                torrentManager.Start();
                }
	        }
	    }

	    public void RunArma()
	    {
	        string arma3StartPath = Path.Combine(ArmaUtils.GetArma3Path(), "arma3battleye.exe");

	        var armaSettings = new ArmaStartSettings() { NoSplash = true, Mods = new List<string> {"@aaaa"}};

	        new ArmaLauncher().Connect(arma3StartPath, armaSettings, true);
	    }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            RunArma();
        }

        #region contextMenu

        private void menuStartDownload_Click(object sender, RoutedEventArgs e)
	    {
	        if (progressList.SelectedItems.Count == 0) return;
	        foreach (Downloading d in progressList.SelectedItems)
	            d.TorrentManager.Start();
	    }

	    private void menuPauseDownload_Click(object sender, RoutedEventArgs e)
	    {
	        if (progressList.SelectedItems.Count == 0) return;
	        foreach (Downloading d in progressList.SelectedItems)
	            d.TorrentManager.Pause();
	    }

	    private void menuStopDownload_Click(object sender, RoutedEventArgs e)
	    {
	        if (progressList.SelectedItems.Count == 0) return;
	        foreach (Downloading d in progressList.SelectedItems)
	            d.TorrentManager.Stop();
	    }

	    private void menuDeleteTorrent_Click(object sender, RoutedEventArgs e)
	    {
	        if (progressList.SelectedItems.Count == 0) return;
	        List<Downloading> removable = new List<Downloading>();
	        foreach (Downloading d in progressList.SelectedItems)
	        {
	            d.TorrentManager.Stop();
	            clientEngine.Unregister(d.TorrentManager);
	            removable.Add(d);
	        }
	        foreach (Downloading d in removable)
	            downloadingTorrents.Remove(d);
	    }

	    private void menuDeleteTorrentAndFile_Click(object sender, RoutedEventArgs e)
	    {
	        if (progressList.SelectedItems.Count == 0) return;
	        List<Downloading> removable = new List<Downloading>();
	        foreach (Downloading d in progressList.SelectedItems)
	        {
	            d.TorrentManager.Stop();
	            clientEngine.Unregister(d.TorrentManager);
	            foreach (TorrentFile file in d.TorrentManager.Torrent.Files)
	            {
	                try { File.Delete(file.FullPath); }
	                catch { }
	            }
	            removable.Add(d);
	        }
	        foreach (Downloading d in removable)
	            downloadingTorrents.Remove(d);
	    }

        #endregion
        #region ToolBox

	   
	    private void btnClose_Click(object sender, RoutedEventArgs e)
	    {
	        this.Close();
	    }

	    private void btnMinimize_Click(object sender, RoutedEventArgs e)
	    {
	        this.WindowState = WindowState.Minimized;
	    }





        #endregion

        #region Left Buttons

        private void imgBtn1_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }

        private void btnFb_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }

        private void ytBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }

        private void InstBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start();
        }

        private void tsBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }

        private void gdBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start();
        }

        private void settingBtn_Click(object sender, RoutedEventArgs e)
        {
            new OptionWindow().ShowDialog();
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void pbMain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
        }

       
    }
}
    
