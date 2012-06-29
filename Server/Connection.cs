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
		
		private Encryption _self;
		
		private string _acceptedSignature = "C0dd1Ch2tCli3nt";
		
		public Connection(TcpClient tcpCon, Encryption server)
		{
			tcpClient = tcpCon;
			_self = server;
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
			
			char[] response;
			// sende als zeile die länge vom public key
			string publicKey =  _self.getPublicKey();
			char[] ServerPublicKey = publicKey.ToCharArray();
			swSender.WriteLine(ServerPublicKey.Length);
			swSender.Flush();
			// sende server public key
			swSender.Write(ServerPublicKey, 0, ServerPublicKey.Length);
			swSender.Flush();
			
			Int32 messageLength = new Int32();
			
			// empfange laenge client publiy key
			try {
				 messageLength = Convert.ToInt32(srReceiver.ReadLine());
			} catch (Exception) {
				swSender.WriteLine("Ungültiger Rückgabewert!");
				swSender.Flush();
				CloseConnection();
				return;
			}
			
			// empfange client puclic key
			response = new char[messageLength];
			srReceiver.Read(response, 0, messageLength);
			
			string ClientPublicKey = new string(response);
			
			
			// laenge auslesen
			try {
				messageLength = Convert.ToInt32(srReceiver.ReadLine());
			} catch (Exception) {
				swSender.WriteLine("Ungültiger Rückgabewert!");
				swSender.Flush();
				CloseConnection();
				return;
			}
			// mit server public key verschlüsselten Usernamen auslesen
			response = new char[messageLength];
			srReceiver.Read(response, 0, messageLength);
			try {
				currUser = _self.DecryptIncoming(new string(response));
			} catch (Exception) {
				swSender.WriteLine("Ungültiger Rückgabewert!");
				swSender.Flush();
				CloseConnection();
				return;
			}
			int pos = currUser.IndexOf(_acceptedSignature, 0);
			currUser = currUser.Replace(_acceptedSignature + "_", "");
			if (currUser != "")
			{
				Encryption clientEncryption = new Encryption(ClientPublicKey);
				string message = "";
				if (pos == -1)
				{
					message = "0|Wrong Client.";
					swSender.Write(clientEncryption.EncryptOutgoing(message), 0, message.Length);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (ChatServer.htUsers.Contains(currUser) == true)
				{
					message = "0|This username already exists.";
					char[] encrMessage = clientEncryption.EncryptOutgoing(message);
					swSender.WriteLine(encrMessage.Length);
					swSender.Flush();
					swSender.Write(encrMessage, 0, encrMessage.Length);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (currUser == "Administrator")
				{
					message = "0|This username is reserved.";
					char[] encrMessage = clientEncryption.EncryptOutgoing(message);
					swSender.WriteLine(encrMessage.Length);
					swSender.Flush();
					swSender.Write(encrMessage, 0, encrMessage.Length);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else
				{
					message = "1";
					char[] encrMessage = clientEncryption.EncryptOutgoing(message);
					swSender.WriteLine(encrMessage.Length);
					swSender.Flush();
					swSender.Write(encrMessage, 0, encrMessage.Length);
					swSender.Flush();
					ChatServer.AddUser(tcpClient, currUser, ClientPublicKey);
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
					 messageLength = Convert.ToInt32(strResponse);
      				 response = new char[messageLength];
					 srReceiver.Read(response, 0, messageLength);
					 strResponse = _self.DecryptIncoming(new string(response));
					 
					 if (strResponse == null)
					 {
					     ChatServer.RemoveUser(tcpClient);
					 }
					 else if (strResponse == "ClosingChatServerConnectionRequest")
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
