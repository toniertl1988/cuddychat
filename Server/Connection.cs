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
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
	/// <summary>
	/// Description of Connection.
	/// </summary>
	public class Connection
	{
		TcpClient tcpClient;
		private Thread thrSender;
		private BinaryReader srReceiver;
		private BinaryWriter swSender;
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
			srReceiver = new System.IO.BinaryReader(tcpClient.GetStream());
			swSender = new System.IO.BinaryWriter(tcpClient.GetStream());
			
			byte[] answer;
			byte[] response;
			Int32 length;
			// lese laenge von pub key von client
			length = srReceiver.ReadInt32();
			// empfange client pub key
			response = new byte[length];
			response = srReceiver.ReadBytes(length);
			string clientPubKey = Converter.fromByteArrayToString(response);
			Encryption clientEncryption = new Encryption(clientPubKey);
			
			// sende mit client pub key verschlüsselt rij key + iv
			answer = clientEncryption.EncryptRSA(_self.getRijKey());
			swSender.Write(answer.Length);
			swSender.Flush();
			swSender.Write(answer);
			swSender.Flush();
			answer = clientEncryption.EncryptRSA(_self.getRijIV());
			swSender.Write(answer.Length);
			swSender.Flush();
			swSender.Write(answer);
			swSender.Flush();
			
			// empfange mit rij verschlüsselten usernamen vom client
			length = srReceiver.ReadInt32();
			response = new byte[length];
			response = srReceiver.ReadBytes(length);
			
			currUser = Converter.fromByteArrayToString(_self.DecryptRijndael(response));
			int pos = currUser.IndexOf(_acceptedSignature, 0);
			currUser = currUser.Replace(_acceptedSignature + "_", "");
			if (currUser != "")
			{
				string message = "";
				if (pos == -1)
				{
					message = "0|Wrong Client.";
					byte[] msg = _self.EncryptRijndael(Converter.fromStringToByteArray(message));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (ChatServer.htUsers.Contains(currUser) == true)
				{
					message = "0|This username already exists.";
					byte[] msg = _self.EncryptRijndael(Converter.fromStringToByteArray(message));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (currUser == "Administrator")
				{
					message = "0|This username is reserved.";
					byte[] msg = _self.EncryptRijndael(Converter.fromStringToByteArray(message));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else
				{
					message = "1";
					byte[] msg = _self.EncryptRijndael(Converter.fromStringToByteArray(message));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg, 0, msg.Length);
					swSender.Flush();
					// send actual user list
					MemoryStream stream = new MemoryStream();
					BinaryFormatter bf = new BinaryFormatter();
					bf.Serialize(stream, ChatServer.users);
					msg = stream.ToArray();
					Int32 msgLength = msg.Length;
					swSender.Write(msgLength);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					// add new user
					ChatServer.AddUser(tcpClient, currUser, _self);
				}
			}
			else
			{
				CloseConnection();
				return;
			}
			
			try
			{
				while ((length = Convert.ToInt32(srReceiver.ReadInt32())) != 0)
				{
      				 response = new byte[length];
					 response = srReceiver.ReadBytes(length);
					 strResponse = Converter.fromByteArrayToString(_self.DecryptRijndael(response));
					 
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
