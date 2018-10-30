using System.Windows;
using LINQPad.Extensibility.DataContext;

namespace DataContextDriverDemo.Astoria
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
		ConnectionProperties _properties;

		public ConnectionDialog (IConnectionInfo cxInfo)
		{
			DataContext = _properties = new ConnectionProperties (cxInfo);
			Background = SystemColors.ControlBrush;
			InitializeComponent ();
		}	

		void btnOK_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
