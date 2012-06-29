﻿/*
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
    		this.Dispatcher.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
		}
		
		private void UpdateStatus(string strMessage)
		{
			// Updates the log with the message
			DateTime today = DateTime.Now;
			
			txtLog.AppendText("(" + today.ToString("HH:mm:ss") + ") " + strMessage + "\r");
			txtLog.ScrollToEnd();
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if (mainServer.getServerStatus() == true) {
        		mainServer.StopListening();
        	}
        }
        
    	public void closeApplication(object sender, EventArgs e)
    	{
        	if (mainServer.getServerStatus() == true) {
        		mainServer.StopListening();
        	}
    		Application.Current.Shutdown();
    	}
	}
}