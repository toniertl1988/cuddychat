/*
 * Created by SharpDevelop.
 * User: Toni
 * Date: 23.06.2012
 * Time: 13:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Threading;

namespace Client
{
	/// <summary>
	/// Description of ChatClient.
	/// </summary>
	public class ChatClient
	{
		
		private StreamWriter _swSender;
		private StreamReader _srReceiver;
		
		public bool connected = false;
		
		private int _port = 8118;
		
		private string _signature = "C0dd1Ch2tCli3nt";
		
		private IPAddress _ipAddr;
		
		private TcpClient _tcpServer;
		
		private Thread _thrMessaging;
				
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		public ChatClient()
		{
		}
		
		public void InitializeConnection(string IP, string User)
		{
			_ipAddr = IPAddress.Parse(IP);
			// Check if IP is reachable
			_tcpServer = new TcpClient();
			//_tcpServer.ReceiveTimeout = 2000;
			//_tcpServer.SendTimeout = 2000;
			_tcpServer.Connect(_ipAddr, _port);
			_swSender = new StreamWriter(_tcpServer.GetStream());
			_swSender.WriteLine(_signature + "_" + User);
			_swSender.Flush();
			_thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
			_thrMessaging.IsBackground = true;
			_thrMessaging.Start();
			connected = true;
		}
		
		public static void OnStatusChanged(StatusChangedEventArgs e)
		{
			StatusChangedEventHandler statusHandler = StatusChanged;
			if (statusHandler != null)
			{
				statusHandler(null, e);
			}
		}
		
		private void ReceiveMessages()
		{
			_srReceiver = new StreamReader(_tcpServer.GetStream());
			String ConResponse = _srReceiver.ReadLine();
			
			if (ConResponse[0] == '1')
			{
				e = new StatusChangedEventArgs("Connected Successfully!");
				OnStatusChanged(e);
			}
			else if (ConResponse[0] == '0')
			{
				connected = false;
				string Reason = "Not Connected! - ";
				Reason += ConResponse.Substring(2, ConResponse.Length-2);
				e = new StatusChangedEventArgs(Reason);
				OnStatusChanged(e);
				return;
			}
			
			while (connected)
			{
				e = new StatusChangedEventArgs(_srReceiver.ReadLine());
				OnStatusChanged(e);
			}
		}
		
		public void closeConnection(string Reason)
		{
			connected = false;
			e = new StatusChangedEventArgs(Reason);
			OnStatusChanged(e);
			if (_thrMessaging != null) {
				_thrMessaging.Abort();
			}
			if (_swSender != null) {
				_swSender.Close();
			}
			if (_srReceiver != null)
			{
				_srReceiver.Close();
			}
			if (_tcpServer != null)
			{
				_tcpServer.Close();
			}
		}
		
		public void sendMessage(string message)
		{
			_swSender.WriteLine(message);
			_swSender.Flush();
		}
		
		public TcpClient getTcpServer()
		{
			return this._tcpServer;
		}
	}
}
