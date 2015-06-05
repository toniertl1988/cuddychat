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
		
		private Encryption _clientEncryption;
		
		private string _user;
				
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		public ChatClient()
		{
			_clientEncryption = new Encryption();
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
			byte[] myPubKey = Converter.fromStringToByteArray(_clientEncryption.getPublicKey());
			sendMessage(myPubKey);
			
			// empfange rij key + iv vom server (verschlüsselt mit meinem pub key)
			Int32 length;
			length = _srReceiver.ReadInt32();
			response = new byte[length];
			response = _srReceiver.ReadBytes(length);
			byte[] rijKey = _clientEncryption.DecryptRSA(response);
			length = _srReceiver.ReadInt32();
			response = new byte[length];
			response = _srReceiver.ReadBytes(length);
			byte[] rijIV = _clientEncryption.DecryptRSA(response);
			
			// setup rij für spätere kommunikation
			_clientEncryption.setUpRijndael(rijKey, rijIV);
			
			// sende chatmessage klasse mit signature, usernamen und willkommensnachricht
			Library.Chatmessage message = new Library.Chatmessage();
			message.Transmitter = User;
			message.Message = "Welcome";
			message.MessageType = Library.Chatmessage.MESSAGE_TYPE_CONNECT;
			message.OperatingSystem = Environment.OSVersion;
			
			answer = _clientEncryption.EncryptRijndael(Converter.fromObjectToByteArray(message));
			sendMessage(answer);
			
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
			Chatmessage ConResponse = (Chatmessage) Converter.fromByteArrayToObject(_clientEncryption.DecryptRijndael(response));
			
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
				e = new StatusChangedEventArgs((Chatmessage) Converter.fromByteArrayToObject(_clientEncryption.DecryptRijndael(response)));
				OnStatusChanged(e);
			}
		}
		
		public void closeConnection(string Reason)
		{
			if (connected) {
				connected = false;
				Chatmessage message = new Chatmessage();
				message.Transmitter = _user;
				message.Receiver = "global";
				message.Message = "ClosingChatServerConnectionRequest";
				message.MessageType = Chatmessage.MESSAGE_TYPE_DISCONNECT;
				sendMessage(message);
			
				// needed here?!
				//e = new StatusChangedEventArgs(message);
				//OnStatusChanged(e);
			}
			
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
			byte[] msg = _clientEncryption.EncryptRijndael(Converter.fromObjectToByteArray(chatMessage));
			sendMessage(msg);
		}
		
		public void sendMessage(Chatmessage message)
		{
			byte[] msg = _clientEncryption.EncryptRijndael(Converter.fromObjectToByteArray(message));
			sendMessage(msg);
		}
		
		public void sendMessage(string message, string receiver)
		{
			Chatmessage chatMessage = new Chatmessage();
			chatMessage.Transmitter = _user;
			chatMessage.Receiver = receiver;
			chatMessage.Message = message;
			byte[] msg = _clientEncryption.EncryptRijndael(Converter.fromObjectToByteArray(chatMessage));
			sendMessage(msg);
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
		
		protected void sendMessage(byte[] message)
		{
			_swSender.Write(message.Length);
			_swSender.Flush();
			_swSender.Write(message);
			_swSender.Flush();
		}
	}
}
