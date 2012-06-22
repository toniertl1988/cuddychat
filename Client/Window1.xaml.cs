/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 19.01.2012
 * Time: 13:23
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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace Client
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		private string UserName = "Unknown";
		private StreamWriter swSender;
		private StreamReader srReceiver;
		private TcpClient tcpServer;
		// Needed to update the form with messages from another thread
		private delegate void UpdateLogCallback(string strMessage);
		// Needed to set the form to a "disconnected" state from another thread
		private delegate void CloseConnectionCallback(string strReason);
		private Thread thrMessaging;
		private IPAddress ipAddr;
		private bool Connected;
		
		private static readonly Regex regexSmilies = new Regex(@":[-]{0,1}[)|D]");
		
		private Smiley _smileys = new Smiley();
		
		public Window1()
		{
			InitializeComponent();
			txtLog.Document.Blocks.Clear();
			//StatusBarText.Text = _smileys.getRegex();
		}
		
		private void btnConnect_Click(object sender, EventArgs e)
		{
			if (Connected == false)
			{
				if (txtUser.Text.Length != 0 && txtIp.Text.Length != 0)
				{
					InitializeConnection();
					btnConnect.Content = "Disconnect";
					txtIp.IsEnabled = false;
					txtUser.IsEnabled = false;
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
				CloseConnection("Disconnected at user's request.");
			}
		}
		
		private void InitializeConnection()
		{
			ipAddr = IPAddress.Parse(txtIp.Text);
			tcpServer = new TcpClient();
			tcpServer.Connect(ipAddr, 8118);
			Connected = true;
			UserName = txtUser.Text;
			swSender = new StreamWriter(tcpServer.GetStream());
			swSender.WriteLine(txtUser.Text);
			swSender.Flush();
			thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
			thrMessaging.IsBackground = true;
			thrMessaging.Start();
		}
		
		private void ReceiveMessages()
		{
			srReceiver = new StreamReader(tcpServer.GetStream());
			String ConResponse = srReceiver.ReadLine();
			
			if (ConResponse[0] == '1')
			{
				this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
			}
			else
			{
				string Reason = "Not Connected!";
				Reason += ConResponse.Substring(2, ConResponse.Length-2);
				this.Dispatcher.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
				return;
			}
			
			while (Connected)
			{
				this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
			}
		}
		
		private void UpdateLog(string strMessage)
		{
			if (strMessage.Length != 0)
			{
				Paragraph p = new Paragraph();
				p.LineHeight = 1;
				p = _smileys.insertSmileys(p, strMessage);
				txtLog.Document.Blocks.Add(p);
				txtLog.ScrollToEnd();
			}
		}
		
		private void btnSend_Click(object sender, EventArgs e)
		{
			if (txtMessage.Text.Length != 0)
			{
				SendMessage();
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
				SendMessage();
			}
		}
		
		private void SendMessage()
		{
			swSender.WriteLine(txtMessage.Text);
			swSender.Flush();
			txtMessage.Text = "";
			txtMessage.Focus();
		}
		
		private void CloseConnection(string Reason)
		{
			UpdateLog(Reason);
			Connected = false;
			if (thrMessaging != null) {
				thrMessaging.Abort();
			}
			if (swSender != null) {
				swSender.Close();
			}
			if (srReceiver != null)
			{
				srReceiver.Close();
			}
			if (tcpServer != null)
			{
				tcpServer.Close();
			}
		}
		
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if (tcpServer != null) {
        		CloseConnection("Disconnect");
        	}
        }
        
    	private static void OnUrlClick(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start((sender as Hyperlink).NavigateUri.AbsoluteUri);
		}
    	
    	public void closeApplication(object sender, EventArgs e)
    	{
        	if (tcpServer != null) {
        		CloseConnection("Disconnect");
        	}
    		Application.Current.Shutdown();
    	}
	}
}