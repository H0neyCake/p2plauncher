using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OGSLauncher.Properties;


namespace OGSLauncher
{
	/// <summary>
	/// OptionWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class OptionWindow : Window
	{
		public OptionWindow ()
		{
			InitializeComponent ();			
		}	       

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
