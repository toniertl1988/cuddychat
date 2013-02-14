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

namespace Server
{
	/// <summary>
	/// Description of ChatServer.
	/// </summary>
	public class ChatServer : IDisposable
	{
		private bool disposed = false;
		public static List<string> users = new List<string>();
		public static Hashtable htUsers = new Hashtable(30);
		public static Hashtable htConnections = new Hashtable(30);
		public static Hashtable htEncryptions = new Hashtable(30);
		private IPAddress ipAddress;
		private TcpClient tcpClient;
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		private Thread thrListener;
		private TcpListener tlsClient;
		bool ServRunning = false;
		
		private Encryption _self;
		
		private string _selfIpAddress = "";
		
		public ChatServer()
		{
			_self = new Encryption();
			_self.setUpRijndael();
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
			ChatServer.users.Add(strUsername);
			ChatServer.htUsers.Add(strUsername, tcpUser);
			ChatServer.htConnections.Add(tcpUser, strUsername);
			ChatServer.htEncryptions.Add(tcpUser, encryption);
			SendAdminMessage(htConnections[tcpUser] + " has joined us");
		}
		
		public static void RemoveUser(TcpClient tcpUser)
		{
			if (htConnections[tcpUser] != null)
			{
				SendAdminMessage(htConnections[tcpUser] + " has left us");
				ChatServer.users.Remove(ChatServer.htConnections[tcpUser].ToString());
				ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
				ChatServer.htEncryptions.Remove(htConnections[tcpUser]);
				ChatServer.htConnections.Remove(tcpUser);
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
			e = new StatusChangedEventArgs("Administrator: " + Message);
			OnStatusChanged(e);
			TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
			ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
			string message = "";
			for (int i = 0; i < tcpClients.Length; i++)
			{
				try
				{
					if (Message.Trim() == "" || tcpClients[i] == null)
					{
						continue;
					}
					Encryption tmp = (Encryption)ChatServer.htEncryptions[tcpClients[i]];
					swSender = new BinaryWriter(tcpClients[i].GetStream());
					message = "Administrator: " + Message;
					byte[] sendMessage = tmp.EncryptRijndael(Converter.fromStringToByteArray(message));
					swSender.Write(sendMessage.Length);
					swSender.Flush();
					swSender.Write(sendMessage);
					swSender.Flush();
					swSender = null;
				}
				catch
				{
					RemoveUser(tcpClients[i]);
				}
			}
		}
		
		public static void SendMessage(string From, string Message)
		{
			BinaryWriter swSender;
			e = new StatusChangedEventArgs(From + " says: " + Message);
			OnStatusChanged(e);
			TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
			ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
			string message = "";
			for (int i = 0; i < tcpClients.Length; i++)
			{
				try
				{
					if (Message.Trim() == "" || tcpClients[i] == null)
					{
						continue;
					}
					else
					{
						Encryption tmp = (Encryption)ChatServer.htEncryptions[tcpClients[i]];
						swSender = new BinaryWriter(tcpClients[i].GetStream());
						message = From + " says: " + Message;
						byte[] sendMessage = tmp.EncryptRijndael(Converter.fromStringToByteArray(message));
						swSender.Write(sendMessage.Length);
						swSender.Flush();
						swSender.Write(sendMessage);
						swSender.Flush();
						swSender = null;
					}
				}
				catch
				{
					RemoveUser(tcpClients[i]);
				}
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
				Connection newConnection = new Connection(tcpClient, _self);
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
        	if (thrListener != null)
        	{
        		thrListener.Abort();
        	}
        	if (tlsClient != null)
        	{
        		tlsClient.Stop();
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
        	return _self.getPublicKey();
        }
	}
}
