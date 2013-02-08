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
		
		private BinaryWriter _swSender;
		private BinaryReader _srReceiver;
		
		public bool connected = false;
		
		private int _port = 8118;
		
		private string _signature = "C0dd1Ch2tCli3nt";
		
		private IPAddress _ipAddr;
		
		private TcpClient _tcpServer;
		
		private Thread _thrMessaging;
		
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
			byte[] response;
			byte[] answer;
			
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
			
			_swSender = new BinaryWriter(_tcpServer.GetStream());
			_srReceiver = new BinaryReader(_tcpServer.GetStream());
			
			// sende meinen public key zum server
			byte[] myPubKey = Converter.fromStringToByteArray(_self.getPublicKey());
			// sende länge
			_swSender.Write(myPubKey.Length);
			_swSender.Flush();
			// sende pubKey
			_swSender.Write(myPubKey);
			_swSender.Flush();
			
			// empfange rij key + iv vom server (verschlüsselt mit meinem pub key)
			Int32 length;
			length = _srReceiver.ReadInt32();
			response = new byte[length];
			response = _srReceiver.ReadBytes(length);
			byte[] rijKey = _self.DecryptRSA(response);
			length = _srReceiver.ReadInt32();
			response = new byte[length];
			response = _srReceiver.ReadBytes(length);
			byte[] rijIV = _self.DecryptRSA(response);
			
			// setup rij für spätere kommunikation
			_self.setUpRijndael(rijKey, rijIV);
			
			// sende meinen usernamen mit rij verschlüsselt
			answer = _self.EncryptRijndael(Converter.fromStringToByteArray(_signature + "_" + User));
			_swSender.Write(answer.Length);
			_swSender.Flush();
			_swSender.Write(answer);
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
			Int32 messageLength = Convert.ToInt32(_srReceiver.ReadInt32());
			byte[] response = new byte[messageLength];
			response = _srReceiver.ReadBytes(messageLength);
			String ConResponse = Converter.fromByteArrayToString(_self.DecryptRijndael(response));
			
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
				messageLength = Convert.ToInt32(_srReceiver.ReadInt32());
				response = new byte[messageLength];
				response = _srReceiver.ReadBytes(messageLength);
				e = new StatusChangedEventArgs(Converter.fromByteArrayToString(_self.DecryptRijndael(response)));
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
			byte[] msg = _self.EncryptRijndael(Converter.fromStringToByteArray(message));
			_swSender.Write(msg.Length);
			_swSender.Flush();
			_swSender.Write(msg);
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
			Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
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
