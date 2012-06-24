/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 01/19/2012
 * Time: 14:39
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
	/// Description of Connection.
	/// </summary>
	public class Connection
	{
		TcpClient tcpClient;
		private Thread thrSender;
		private StreamReader srReceiver;
		private StreamWriter swSender;
		private string currUser;
		private string strResponse;
		
		private string _acceptedSignature = "C0dd1Ch2tCli3nt";
		
		public Connection(TcpClient tcpCon)
		{
			tcpClient = tcpCon;
			thrSender = new Thread(AcceptClient);
			thrSender.Start();
		}
		
		private void CloseConnection()
		{
			tcpClient.Close();
			srReceiver.Close();
			swSender.Close();
		}
		
		private void AcceptClient()
		{
			srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
			swSender = new System.IO.StreamWriter(tcpClient.GetStream());
			currUser = srReceiver.ReadLine();
			int pos = currUser.IndexOf(_acceptedSignature, 0);
			currUser = currUser.Replace(_acceptedSignature + "_", "");
			if (currUser != "")
			{
				if (pos == -1)
				{
					swSender.WriteLine("0|Wrong Client.");
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (ChatServer.htUsers.Contains(currUser) == true)
				{
					swSender.WriteLine("0|This username already exists.");
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (currUser == "Administrator")
				{
					swSender.WriteLine("0|This username is reserved.");
					swSender.Flush();
					CloseConnection();
					return;
				}
				else
				{
					swSender.WriteLine("1");
					swSender.Flush();
					ChatServer.AddUser(tcpClient, currUser);
				}
			}
			else
			{
				CloseConnection();
				return;
			}
			
			try
			{
				while ((strResponse = srReceiver.ReadLine()) != "")
				{
					 if (strResponse == null)
					 {
					     ChatServer.RemoveUser(tcpClient);
					 }
					 else
					 {
					 	ChatServer.SendMessage(currUser, strResponse);
					 }
				}
			}
			catch
			{
				ChatServer.RemoveUser(tcpClient);
			}
		}
	}
}
