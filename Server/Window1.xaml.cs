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
using System.Security.Principal;
using Library;

namespace Server
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	///	
	
	public partial class Window1 : Window
	{		
		private delegate void UpdateStatusCallback(Chatmessage message);

        private ChatServer mainServer;
		
		public Window1()
		{
			CheckInstance();
			InitializeComponent();
			if (System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] == "true") {
				logUserMessagesMenuItem.IsChecked = true;
			} else {
				logUserMessagesMenuItem.IsChecked = false;
			}
			mainServer = new ChatServer();
			txtIp.Text = mainServer.getSelfIpAddress();
			ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
		}
		
		protected void CheckInstance()
		{
			bool ok;
			string appName = "Cuddy-Chatclient-Server";
			string mutex = WindowsIdentity.GetCurrent().Name.ToString();
			mutex = mutex.Split('\\')[1] + appName;
			Mutex m = new Mutex(true, mutex, out ok);
			if (!ok) {
				MessageBox.Show("Server already running");
				Application.Current.Shutdown();
			}
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
    		try {
    			this.Dispatcher.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
    		} catch (Exception) {
    			// do nothing
    		}
    		
		}
		
		private void UpdateStatus(string strMessage)
		{
			DateTime today = DateTime.Now;
			string text = "(" + today.ToString("HH:mm:ss") + ") " + strMessage + "\r";
			txtLog.AppendText(text);
			txtLog.ScrollToEnd();
		}
		
		private void UpdateStatus(Chatmessage message)
		{
			if (message.MessageType == Chatmessage.MESSAGE_TYPE_USER_INFO) {
				return;
			}
			if (message.Transmitter == "Administrator") {
				UpdateStatus(message.Message);
			} else {
				if (System.Configuration.ConfigurationManager.AppSettings["LogUserMessages"] == "true") {
					UpdateStatus(message.Message);
				}
			}
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