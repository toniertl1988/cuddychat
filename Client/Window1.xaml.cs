/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 19.01.2012
 * Time: 13:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Client
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		// Needed to update the form with messages from another thread
		private delegate void UpdateLogCallback(string strMessage, string transmitter, string receiver);
		// Needed to set the form to a "disconnected" state from another thread
		private delegate void CloseConnectionCallback(string strReason);
		
		private Smiley _smileys = new Smiley();
		
		private Parser _parser = new Parser();
		
		private ChatClient _client;
		
		protected Hashtable _privateChats = new Hashtable();
		
		public Window1()
		{		
			InitializeComponent();
			txtLog.Document.Blocks.Clear();
			_client = new ChatClient();
			ChatClient.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
		}
		
		private void btnConnect_Click(object sender, EventArgs e)
		{
			if (_client.connected == false)
			{
				if (txtUser.Text.Length != 0 && txtIp.Text.Length != 0)
				{
					_client.InitializeConnection(txtIp.Text, txtUser.Text);
					btnConnect.Content = "Disconnect";
					txtIp.IsEnabled = false;
					txtUser.IsEnabled = false;
					txtMessage.Focus();
				}
				else
				{
					MessageBox.Show("Benutzername und Server IP müssen eingetragen sein!");
				}
			}
			else
			{
				btnConnect.Content = "Connect";
				txtIp.IsEnabled = true;
				txtUser.IsEnabled = true;
				_client.closeConnection("Disconnected at user's request.");
			}
		}
		
		private void UpdateGui(string strMessage, string transmitter, string receiver)
		{
			if (_client.connected == true)
			{
				btnConnect.Content = "Disconnect";
				txtIp.IsEnabled = false;
				txtUser.IsEnabled = false;
			}
			else
			{
				btnConnect.Content = "Connect";
				txtIp.IsEnabled = true;
				txtUser.IsEnabled = true;				
			}
			if (strMessage.Length != 0)
			{
				if (transmitter == "Administrator")
				{
					transmitter = transmitter + ": ";
					txtLog.Document.Blocks.Add( _parser.parse(transmitter + strMessage));
					txtLog.ScrollToEnd();
					if (_client.checkIfAdminMessage(transmitter + strMessage) == true)
					{
						_client.manageAdminMessage(transmitter + strMessage);
						listUser.ItemsSource = new ObservableCollection<string>(_client.getUsers());
					}
				} else {
					if (receiver == "global")
					{
						transmitter = transmitter + ": ";
						txtLog.Document.Blocks.Add( _parser.parse(transmitter + strMessage));
						txtLog.ScrollToEnd();
					}
					else
					{
						if (_privateChats.ContainsKey(transmitter) == false)
					    {
							PrivateWindow newWindow = new PrivateWindow(transmitter);
							newWindow.setOwner(this);
							newWindow.Show();
							_privateChats.Add(transmitter, newWindow);
					    }
						PrivateWindow partnerWindow = (PrivateWindow) _privateChats[transmitter];
						transmitter = transmitter + ": ";
						partnerWindow.txtLog.Document.Blocks.Add( _parser.parse(transmitter + strMessage));
						partnerWindow.txtLog.ScrollToEnd();
					}
				}
			}
		}
		
		private void btnSend_Click(object sender, EventArgs e)
		{
			if (txtMessage.Text.Length != 0)
			{
				SendMessage(txtMessage.Text, "global");
			}
		}
		
		private void txtMessage_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
			{
				txtMessage.Text += Environment.NewLine;
			}
			else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
			{
				SendMessage(txtMessage.Text, "global");
			}
		}
		
		public void SendMessage()
		{
			_client.sendMessage(txtMessage.Text);
			txtMessage.Text = "";
			txtMessage.Focus();
		}
		
		public void SendMessage(string message, string receiver)
		{
			_client.sendMessage(message, receiver);
			if (receiver == "global")
			{
				txtMessage.Text = "";
				txtMessage.Focus();
			}
			else
			{
				PrivateWindow partnerWindow = (PrivateWindow) _privateChats[receiver];
				string transmitter = txtUser.Text + ": ";
				partnerWindow.txtLog.Document.Blocks.Add( _parser.parse(transmitter + message));
				partnerWindow.txtLog.ScrollToEnd();
				partnerWindow.txtMessage.Text = "";
				partnerWindow.txtMessage.Focus();
			}
		}
		
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if (_client.getTcpServer() != null) {
        		_client.closeConnection("Disconnect");
        	}
        	foreach (DictionaryEntry element in _privateChats) {
        		PrivateWindow actualWindow = (PrivateWindow) element.Value;
        		actualWindow.Close();
        	}
        	Application.Current.Shutdown();
        }
    	
    	public void closeApplication(object sender, EventArgs e)
    	{
    		if (_client.getTcpServer() != null) {
        		_client.closeConnection("Disconnect");
        	}
    		Application.Current.Shutdown();
    	}
    	
    	public void openSmileyWindow(object sender, EventArgs e)
    	{
    		SmileyWindow window = new SmileyWindow(_smileys);
    		window.Show();
    	}
    	
		public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
		{
    		// Call the method that updates the form
    		this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateGui), new object[] { e.EventMessage.Message, e.EventMessage.Transmitter, e.EventMessage.Receiver });
		}
		
		public void openUserInfo(object sender, EventArgs e)
		{
			MessageBox.Show(listUser.SelectedItem.ToString());
		}
		
		public void TxtUser_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btnConnect_Click(sender, e);
			}
		}
		
		public void openPrivateChat(object sender, EventArgs e)
		{
			openPrivateChat(listUser.SelectedItem.ToString());
		}
		
		public void ListUser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			openPrivateChat(listUser.SelectedItem.ToString());
		}
		
		public void closePrivateChat(string partner)
		{
			if (_privateChats.ContainsKey(partner))
		    {
				_privateChats.Remove(partner);
		    }
		}
		
		protected void openPrivateChat(string partner)
		{
			PrivateWindow window = new PrivateWindow(partner);
			window.setOwner(this);
			if (_privateChats.ContainsKey(partner)) {
				_privateChats.Remove(partner);
			}
			_privateChats.Add(partner, window);
			window.Show();
		}
	}
}