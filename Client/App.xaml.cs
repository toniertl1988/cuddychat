using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.IO;
using System.Configuration;
using System.Windows.Threading;

namespace Chat
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = false;
        }
	}
}