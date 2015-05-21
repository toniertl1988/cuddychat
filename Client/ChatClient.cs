/*
 * Created by SharpDevelop.
 * User: Toni
 * Date: 23.06.2012
 * Time: 13:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using Library;

namespace Client
{
	/// <summary>
	/// Description of ChatClient.
	/// </summary>
	public class ChatClient
	{
		
		private List<string> _users = new List<string>();
		
		private BinaryWriter _swSender;
		private BinaryReader _srReceiver;
		
		public bool connected = false;
		
		private int _port = 8118;
		
		private IPAddress _ipAddr;
		
		private TcpClient _tcpServer;
		
		private Thread _thrMessaging;
		
		private Encryption _self;
		
		private string _user;
				
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		public ChatClient()
		{
			_self = new Encryption();
		}
		
		public void InitializeConnection(string IP, string User)
		{
			_user = User;
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
			
			// sende chatmessage klasse mit signature, usernamen und willkommensnachricht
			Library.Chatmessage message = new Library.Chatmessage();
			message.Transmitter = User;
			message.Message = "Welcome";
			message.OperatingSystem = Environment.OSVersion;
			
			answer = _self.EncryptRijndael(Converter.fromObjectToByteArray(message));
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
			Chatmessage ConResponse = (Chatmessage) Converter.fromByteArrayToObject(_self.DecryptRijndael(response));
			
			if (ConResponse.Message[0] == '1')
			{
				ConResponse.Message = ConResponse.Message.Substring(2);
				e = new StatusChangedEventArgs(ConResponse);
				OnStatusChanged(e);
				
				// read actual user list from server
				
				BinaryFormatter bf = new BinaryFormatter();
				messageLength = Convert.ToInt32(_srReceiver.ReadInt32());
				response = new byte[messageLength];
				response = _srReceiver.ReadBytes(messageLength);
				MemoryStream ms = new MemoryStream(response);
				List<string> user = (List<string>) bf.Deserialize(ms);
				foreach (var element in user) {
					_users.Add(element);
				}
				
				
			}
			else if (ConResponse.Message[0] == '0')
			{
				connected = false;
				string Reason = "Not Connected! - ";
				Reason += ConResponse.Message.Substring(2, ConResponse.Message.Length-2);
				e = new StatusChangedEventArgs(ConResponse);
				OnStatusChanged(e);
				return;
			}
			
			while (connected)
			{
				messageLength = Convert.ToInt32(_srReceiver.ReadInt32());
				response = new byte[messageLength];
				response = _srReceiver.ReadBytes(messageLength);
				e = new StatusChangedEventArgs((Chatmessage) Converter.fromByteArrayToObject(_self.DecryptRijndael(response)));
				OnStatusChanged(e);
			}
		}
		
		public void closeConnection(string Reason)
		{
			connected = false;
			Chatmessage message = new Chatmessage();
			message.Transmitter = _user;
			message.Receiver = "global";
			message.Message = "ClosingChatServerConnectionRequest";
			sendMessage("ClosingChatServerConnectionRequest");
			
			// needed here?!
			//e = new StatusChangedEventArgs(message);
			//OnStatusChanged(e);
			
			_users.Clear();
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
			Chatmessage chatMessage = new Chatmessage();
			chatMessage.Transmitter = _user;
			chatMessage.Receiver = "global";
			chatMessage.Message = message;
			byte[] msg = _self.EncryptRijndael(Converter.fromObjectToByteArray(chatMessage));
			_swSender.Write(msg.Length);
			_swSender.Flush();
			_swSender.Write(msg);
			_swSender.Flush();
		}
		
		public void sendMessage(string message, string receiver)
		{
			Chatmessage chatMessage = new Chatmessage();
			chatMessage.Transmitter = _user;
			chatMessage.Receiver = receiver;
			chatMessage.Message = message;
			byte[] msg = _self.EncryptRijndael(Converter.fromObjectToByteArray(chatMessage));
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
		
		public bool checkIfAdminMessage(string message)
		{
			if (message.IndexOfAny("Administrator".ToCharArray()) != -1) {
				return true;
			}
			return false;
		}
		
		public void manageAdminMessage(string message)
		{
			if (message.IndexOf("join") != -1) {
				_users.Add(message.Replace("Administrator: ", "").Replace(" has joined us", ""));
			}
			if (message.IndexOf("left") != -1) {
				_users.Remove(message.Replace("Administrator: ", "").Replace(" has left us", ""));
			}
			_users.Sort();
		}
		
		public List<string> getUsers()
		{
			return _users;
		}
	}
}
