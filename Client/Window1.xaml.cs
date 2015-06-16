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
using Library;

namespace Client
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		// Needed to update the form with messages from another thread
		private delegate void UpdateLogCallback(string strMessage, string transmitter, string receiver, string MessageType);
		
		private Smiley _smileyClass = new Smiley();
		
		private Parser _parser = new Parser();
		
		private ChatClient _client;
		
		protected Hashtable _privateChats = new Hashtable();
		
		protected AddSmileyWindow addSmileyWindow;
		
		public Window1()
		{		
			InitializeComponent();
			txtLog.Document.Blocks.Clear();
			_client = new ChatClient();
			ChatClient.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
			
			addSmileyWindow = new AddSmileyWindow(this);
			addSmileyWindow.addSmileysToGrid(_smileyClass.getAllSmileys());
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
		
		private void UpdateGui(string strMessage, string transmitter, string receiver, string MessageType)
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
				string completeMessage = transmitter + ": " + strMessage;
				if (transmitter == "Administrator")
				{
					txtLog.Document.Blocks.Add( _parser.parse(completeMessage));
					txtLog.ScrollToEnd();
					if (_client.checkIfAdminMessage(completeMessage) == true)
					{
						_client.manageAdminMessage(completeMessage);
						listUser.ItemsSource = new ObservableCollection<string>(_client.getUsers());
					}
				} else {
					if (receiver == "global")
					{
						txtLog.Document.Blocks.Add( _parser.parse(completeMessage));
						txtLog.ScrollToEnd();
					}
					else
					{
						if (MessageType == Chatmessage.MESSAGE_TYPE_USER_INFO) {
							MessageBox.Show(strMessage);
						} else {
							if (_privateChats.ContainsKey(transmitter) == false)
						    {
								PrivateWindow newWindow = new PrivateWindow(transmitter);
								newWindow.setOwner(this);
								newWindow.Show();
								_privateChats.Add(transmitter, newWindow);
						    }
							PrivateWindow partnerWindow = (PrivateWindow) _privateChats[transmitter];
	                        partnerWindow.Show();
							partnerWindow.txtLog.Document.Blocks.Add( _parser.parse(completeMessage));
							partnerWindow.txtLog.ScrollToEnd();
						}
					}
				}
			}
		}
		
		private void btnSend_Click(object sender, EventArgs e)
		{
			if (txtMessage.Text.Length != 0)
			{
				if (_client.connected == true) {
					SendMessage(txtMessage.Text, "global");
				} else {
					MessageBox.Show("Mit keinem Server verbunden!");
				}
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
        	if (_client.connected != false) {
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
    		if (_client.connected != false) {
        		_client.closeConnection("Disconnect");
        	}
    		Application.Current.Shutdown();
    	}
    	
    	public void openSmileyWindow(object sender, EventArgs e)
    	{
    		SmileyWindow window = new SmileyWindow(_smileyClass);
    		window.Show();
    	}
    	
		public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
		{
    		// Call the method that updates the form
    		this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateGui), new object[] { e.EventMessage.Message, e.EventMessage.Transmitter, e.EventMessage.Receiver, e.EventMessage.MessageType });
		}
		
		public void openUserInfo(object sender, EventArgs e)
		{
			Chatmessage message = new Chatmessage();
			message.MessageType = Chatmessage.MESSAGE_TYPE_USER_INFO;
			message.Transmitter = txtUser.Text;
			message.Receiver = listUser.SelectedItem.ToString();
			message.Message = "userInfo";
			_client.sendMessage(message);
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
				PrivateWindow window = (PrivateWindow) _privateChats[partner];
				window.Hide();
		    }
		}
		
		protected void openPrivateChat(string partner)
		{
			PrivateWindow window;
			if (_privateChats.ContainsKey(partner) == false) {
				window = new PrivateWindow(partner);
				window.setOwner(this);
				_privateChats.Add(partner, window);
			} else {
				window = (PrivateWindow) _privateChats[partner];
			}
			window.Show();
		}
		
		protected void openAddSmileyWindow(object sender, RoutedEventArgs e)
		{
    		addSmileyWindow.Show();
		}
		
		public void addSmileyClickEvent(object sender, RoutedEventArgs e)
		{
			var actualPosition = txtMessage.SelectionStart;
			System.Windows.Controls.Image image = (System.Windows.Controls.Image) VisualTreeHelper.GetChild((DependencyObject) sender, 0);
			string text = image.ToolTip.ToString();
			txtMessage.Text = txtMessage.Text.Insert(actualPosition, text);
			txtMessage.SelectionStart = actualPosition + text.Length;
		}
	}
}