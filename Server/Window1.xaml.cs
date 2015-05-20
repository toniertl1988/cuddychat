/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 01/19/2012
 * Time: 14:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Server
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	///	
	
	public partial class Window1 : Window
	{
		private delegate void UpdateStatusCallback(string strMessage);

        private ChatServer mainServer;
		
		public Window1()
		{
			InitializeComponent();
			mainServer = new ChatServer();
			txtIp.Text = mainServer.getSelfIpAddress();
			ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
		}
		
		private void btnListen_Click(object sender, EventArgs e)
		{
			mainServer.setIpAddress(txtIp.Text);
			if (mainServer.getServerStatus() == false)
			{
				if (mainServer.StartListening() == false) {
					MessageBox.Show("Server konnte nicht gestartet werden!");
				} else {
					this.UpdateStatus("Monitoring for connections...");
					btnListen.Content = "Stop Listening";
					txtIp.IsEnabled = false;
				}
			}
			else
			{
				mainServer.StopListening();
				btnListen.Content = "Start Listening";
				this.UpdateStatus("Stop Monitoring");
				txtIp.IsEnabled = true;
			}
		}
		
		public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
		{
    		// Call the method that updates the form
    		this.Dispatcher.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage.Message });
		}
		
		private void UpdateStatus(string strMessage)
		{
			DateTime today = DateTime.Now;
			string text = "(" + today.ToString("HH:mm:ss") + ") " + strMessage + "\r";
			if (strMessage.IndexOf("Admin") == -1)
			{
				if (System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] == "true")
				{
					txtLog.AppendText(text);
				}
		    }
			else
			{
				txtLog.AppendText(text);
			}
			txtLog.ScrollToEnd();
			
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if (mainServer.getServerStatus() == true) {
        		mainServer.StopListening();
        	}
        	Application.Current.Shutdown();
        }
        
    	public void closeApplication(object sender, EventArgs e)
    	{
        	if (mainServer.getServerStatus() == true) {
        		mainServer.StopListening();
        	}
    		Application.Current.Shutdown();
    	}
    	
    	public void logManagement(object sender, EventArgs e)
    	{
    		if (System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] == "true")
    		{
    			System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] = "false";
				MessageBox.Show("Logging der User Nachrichten ausgeschaltet");
    		}
    		else
    		{
    			System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] = "true";
				MessageBox.Show("Logging der User Nachrichten eingeschaltet");
    		}
    	}
	}
}