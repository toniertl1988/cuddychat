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
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Threading;
using System.Security.Cryptography;

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
		
		private Encryption _serverKey;
		
		private Encryption _self;
				
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		public ChatClient()
		{
			_self = new Encryption();
		}
		
		public void InitializeConnection(string IP, string User)
		{
			_ipAddr = IPAddress.Parse(IP);
			
			// Check if IP is reachable
			bool check = checkIfServerIsReachable();
			
			if (check == false)
			{
				throw new Exception("Server ist zur Zeit nicht erreichbar!");
			}
			
			// erstmal verbinden
			try {
				_tcpServer = new TcpClient();
				_tcpServer.Connect(_ipAddr, _port);
			}
			catch (Exception)
			{
				_tcpServer = null;
				throw new Exception("Server ist zur Zeit nicht erreichbar! (Antwortet nicht auf Anfragen)");
			}
			
			_swSender = new StreamWriter(_tcpServer.GetStream());
			_srReceiver = new StreamReader(_tcpServer.GetStream());
			
			// laenge vom public key empfangen
			Int32 msgLength = Convert.ToInt32(_srReceiver.ReadLine());
			// server public key empfangen
			char[] response = new char[msgLength];
			_srReceiver.Read(response, 0, msgLength);
			string xmlString = new String(response);
			// Encryption Klasse mit Public Key vom Server füttern
			_serverKey = new Encryption(xmlString);
			
			// laenge von meinem public key senden
			char[] publicKey = _self.getPublicKey().ToCharArray();
			_swSender.WriteLine(publicKey.Length);
			_swSender.Flush();
			// meinen public key senden
			_swSender.Write(publicKey, 0, publicKey.Length);
			_swSender.Flush();
			
			// mit dem empfangenem server key meinen benutzernamen an server senden
			string msg = _signature + "_" + User;
			char[] sendmessage = _serverKey.EncryptOutgoing(msg);
			_swSender.WriteLine(sendmessage.Length);
			_swSender.Flush();
			_swSender.Write(sendmessage, 0, sendmessage.Length);
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
			//_srReceiver = new StreamReader(_tcpServer.GetStream());
			Int32 messageLength = Convert.ToInt32(_srReceiver.ReadLine());
			char[] response = new char[messageLength];
			_srReceiver.Read(response, 0, messageLength);
			String ConResponse = _self.DecryptIncoming(new string(response));
			
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
				messageLength = Convert.ToInt16(_srReceiver.ReadLine());
				response = new char[messageLength];
				_srReceiver.Read(response, 0, messageLength);
				e = new StatusChangedEventArgs(_self.DecryptIncoming(new string(response)));
				OnStatusChanged(e);
			}
		}
		
		public void closeConnection(string Reason)
		{
			connected = false;
			sendMessage("ClosingChatServerConnectionRequest");
			e = new StatusChangedEventArgs(Reason);
			OnStatusChanged(e);
			if (_thrMessaging != null) {
				//_thrMessaging.Abort();
			}
			if (_swSender != null) {
				//_swSender.Close();
			}
			if (_srReceiver != null)
			{
				//_srReceiver.Close();
			}
			if (_tcpServer != null)
			{
				//_tcpServer.Close();
			}
		}
		
		public void sendMessage(string message)
		{
			char[] msg = _serverKey.EncryptOutgoing(message);
			_swSender.WriteLine(msg.Length);
			_swSender.Flush();
			_swSender.Write(msg, 0, msg.Length);
			_swSender.Flush();
		}
		
		public TcpClient getTcpServer()
		{
			return this._tcpServer;
		}
		
		public string getPublicKey()
		{
			return _self.getPublicKey();
		}
		
		public bool checkIfServerIsReachable()
		{
			Ping pingSender = new Ping ();
            PingOptions options = new PingOptions ();
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes (data);
            int timeout = 120;
            PingReply reply = pingSender.Send (_ipAddr, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
            	return true;
            }
            return false;
		}
	}
}
