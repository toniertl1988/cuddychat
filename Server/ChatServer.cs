/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 01/19/2012
 * Time: 14:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using Library;

namespace Server
{
	/// <summary>
	/// Description of ChatServer.
	/// </summary>
	public class ChatServer : IDisposable
	{
		private bool disposed = false;
		
		public static List<string> users = new List<string>();
		public static Dictionary<string, User> userInfos = new Dictionary<string, User>();
		
		private IPAddress ipAddress;
		private TcpClient tcpClient;
		
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		private Thread thrListener;
		private TcpListener tlsClient;
		
		bool ServRunning = false;
		
		private Encryption serverEncryption;
		
		private string _selfIpAddress = "";
		
		public ChatServer()
		{
			serverEncryption = new Encryption();
			serverEncryption.setUpRijndael();
			findSelfIpAddress();
		}
		
		public void setIpAddress(string ip)
		{
			try {
				ipAddress = IPAddress.Parse(ip);
			} catch (Exception) {
				throw new Exception("ungültige IP");
			}
		}

		public static void AddUser(TcpClient tcpUser, string strUsername, Encryption encryption)
		{
			// create new user
			User user = new User();
			user.Username = strUsername;
			user.TcpClient = tcpUser;
			user.Encryption = encryption;
			user.LoginTime = new DateTime();
			
			ChatServer.users.Add(strUsername);
			ChatServer.userInfos.Add(strUsername, user);

			SendAdminMessage(strUsername + " has joined us");
		}
		
		public static void RemoveUser(string username)
		{
			if (ChatServer.userInfos.ContainsKey(username) == true)
			{
				SendAdminMessage(username + " has left us");
				
				User user = ChatServer.userInfos[username];
				user.TcpClient.Close();				
				ChatServer.users.Remove(username);
				ChatServer.userInfos.Remove(username);				
			}
		}
		
		public static void OnStatusChanged(StatusChangedEventArgs e)
		{
			StatusChangedEventHandler statusHandler = StatusChanged;
			if (statusHandler != null)
			{
				statusHandler(null, e);
			}
		}
		
		public static void SendAdminMessage(string Message)
		{
			BinaryWriter swSender;
			Chatmessage adminMessage = new Chatmessage();
			adminMessage.Transmitter = "Administrator";
			adminMessage.Receiver = "global";
			adminMessage.Message = Message;
			
			e = new StatusChangedEventArgs(adminMessage);
			OnStatusChanged(e);
			
			foreach (KeyValuePair<string, User> entry in ChatServer.userInfos) {
				try
				{
					if (Message.Trim() == "")
					{
						continue;
					}
					Encryption tmp = entry.Value.Encryption;
					swSender = new BinaryWriter(entry.Value.TcpClient.GetStream());
					byte[] sendMessage = tmp.EncryptRijndael(Converter.fromObjectToByteArray(adminMessage));
					swSender.Write(sendMessage.Length);
					swSender.Flush();
					swSender.Write(sendMessage);
					swSender.Flush();
					swSender = null;
				}
				catch
				{
					RemoveUser(entry.Key);
				}
			}
		}
		
		public static void SendMessage(string From, string Message, string Receiver)
		{
			BinaryWriter swSender;
			Chatmessage userMessage = new Chatmessage();
			userMessage.Transmitter = From;
			userMessage.Message = Message;
			userMessage.Receiver = Receiver;
			
			e = new StatusChangedEventArgs(userMessage);
			OnStatusChanged(e);
			
			if (Receiver == "global" && Message.Trim() != "")
			{
				foreach (KeyValuePair<string, User> entry in ChatServer.userInfos)
				{
					try
					{
						Encryption tmp = entry.Value.Encryption;
						swSender = new BinaryWriter(entry.Value.TcpClient.GetStream());
						userMessage.Message = Message;
						byte[] sendMessage = tmp.EncryptRijndael(Converter.fromObjectToByteArray(userMessage));
						swSender.Write(sendMessage.Length);
						swSender.Flush();
						swSender.Write(sendMessage);
						swSender.Flush();
						swSender = null;
					}
					catch
					{
						RemoveUser(entry.Key);
					}
				}
			}
			else
			{
				User user = ChatServer.userInfos[Receiver];
				Encryption tmp = user.Encryption;
				TcpClient connection = user.TcpClient;
				swSender = new BinaryWriter(connection.GetStream());
				userMessage.Message = Message;
				byte[] sendMessage = tmp.EncryptRijndael(Converter.fromObjectToByteArray(userMessage));
				swSender.Write(sendMessage.Length);
				swSender.Flush();
				swSender.Write(sendMessage);
				swSender.Flush();
				swSender = null;
			}
		}
		
		public bool StartListening()
		{
			try {
				IPAddress ipLocal = ipAddress;
				tlsClient = new TcpListener(ipLocal, 8118);
				tlsClient.Start();
				ServRunning = true;
				thrListener = new Thread(KeepListening);
            	thrListener.Start();
            	return true;
			} catch (Exception) {
				return false;
			}
		}
		
		private void KeepListening()
		{
			while (ServRunning == true)
			{
				tcpClient = tlsClient.AcceptTcpClient();
				Connection newConnection = new Connection(tcpClient, serverEncryption);
			}
		}
		
	    public void Dispose()
    	{
		    Dispose(true);
		    GC.SuppressFinalize(this);
    	}
	    
        protected virtual void Dispose(bool disposing)
    	{
	        if (!disposed)
	        {
	            if (disposing)
	            {
	                // Free other state (managed objects).
	            }
	            // Free your own state (unmanaged objects).
	            // Set large fields to null.
	            StopListening();
	            disposed = true;
	        }
    	}
        
        public void StopListening()
        {
        	ServRunning = false;
        	if (tlsClient != null)
        	{
        		tlsClient.Stop();
        	}
        	if (thrListener != null)
        	{
        		thrListener.Abort();
        	}
        }
        
        public bool getServerStatus()
        {
        	return this.ServRunning;
        }
        
        public string getSelfIpAddress()
        {
        	return this._selfIpAddress;
        }
        
        public void findSelfIpAddress()
        {
        	IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());
    		foreach (IPAddress IP in Host.AddressList)
    		{
    			string tmp = IP.ToString();
    			if (tmp.Length >= 8 && tmp.Length <= 15)
    			{
    				this._selfIpAddress = tmp;
    				break;
    			}
    		}
        }
        
        ~ChatServer()
        {
        	Dispose (false);
        }
        
        public string getRSAPublic()
        {
        	return serverEncryption.getPublicKey();
        }
	}
}
