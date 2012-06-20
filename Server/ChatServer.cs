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

namespace Server
{
	/// <summary>
	/// Description of ChatServer.
	/// </summary>
	public class ChatServer : IDisposable
	{
		private bool disposed = false;
		
		public static Hashtable htUsers = new Hashtable(30);
		public static Hashtable htConnections = new Hashtable(30);
		private IPAddress ipAddress;
		private TcpClient tcpClient;
		public static event StatusChangedEventHandler StatusChanged;
		private static StatusChangedEventArgs e;
		
		private Thread thrListener;
		private TcpListener tlsClient;
		bool ServRunning = false;
		
		public ChatServer(IPAddress address)
		{
			ipAddress = address;
		}
				
		public static void AddUser(TcpClient tcpUser, string strUsername)
		{
			ChatServer.htUsers.Add(strUsername, tcpUser);
			ChatServer.htConnections.Add(tcpUser, strUsername);
			SendAdminMessage(htConnections[tcpUser] + " has joined us");
		}
		
		public static void RemoveUser(TcpClient tcpUser)
		{
			if (htConnections[tcpUser] != null)
			{
				SendAdminMessage(htConnections[tcpUser] + " has left us");
				ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
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
			StreamWriter swSenderSender;
			e = new StatusChangedEventArgs("Administrator: " + Message);
			OnStatusChanged(e);
			TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
			ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
			for (int i = 0; i < tcpClients.Length; i++)
			{
				try
				{
					if (Message.Trim() == "" || tcpClients[i] == null)
					{
						continue;
					}
					swSenderSender = new StreamWriter(tcpClients[i].GetStream());
					swSenderSender.WriteLine("Administrator: " + Message);
					swSenderSender.Flush();
					swSenderSender = null;
				}
				catch
				{
					RemoveUser(tcpClients[i]);
				}
			}
		}
		
		public static void SendMessage(string From, string Message)
		{
			StreamWriter swSenderSender;
			e = new StatusChangedEventArgs(From + " says: " + Message);
			OnStatusChanged(e);
			TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
			ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
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
						swSenderSender = new StreamWriter(tcpClients[i].GetStream());
		                swSenderSender.WriteLine(From + " says: " + Message);
		                swSenderSender.Flush();
		                swSenderSender = null;
					}
				}
				catch
				{
					RemoveUser(tcpClients[i]);
				}
			}
		}
		
		public void StartListening()
		{
			IPAddress ipLocal = ipAddress;
			tlsClient = new TcpListener(ipLocal, 8118);
			tlsClient.Start();
			ServRunning = true;
			thrListener = new Thread(KeepListening);
            thrListener.Start();
		}
		
		private void KeepListening()
		{
			while (ServRunning == true)
			{
				tcpClient = tlsClient.AcceptTcpClient();
				Connection newConnection = new Connection(tcpClient);
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
        	thrListener.Abort();
        	tlsClient.Stop();
        }
        
        public bool getServerStatus()
        {
        	return this.ServRunning;
        }
        
        ~ChatServer()
        {
        	Dispose (false);
        }
	}
}
