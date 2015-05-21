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
using Library;

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
		
		private Encryption serverEncryption;
		
		private string _acceptedSignature = "C0dd1Ch2tCli3nt";
		
		public Connection(TcpClient tcpCon, Encryption server)
		{
			tcpClient = tcpCon;
			serverEncryption = server;
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
			answer = clientEncryption.EncryptRSA(serverEncryption.getRijKey());
			swSender.Write(answer.Length);
			swSender.Flush();
			swSender.Write(answer);
			swSender.Flush();
			answer = clientEncryption.EncryptRSA(serverEncryption.getRijIV());
			swSender.Write(answer.Length);
			swSender.Flush();
			swSender.Write(answer);
			swSender.Flush();
			
			// empfange chatmessage objekt mit usernamen signature und message
			length = srReceiver.ReadInt32();
			response = new byte[length];
			response = srReceiver.ReadBytes(length);
			
			Chatmessage message = (Chatmessage) Converter.fromByteArrayToObject(serverEncryption.DecryptRijndael(response));
			int pos = 0;
			if (message.Signature != _acceptedSignature)
			{
				pos = -1;
			}
			currUser = message.Transmitter;
			if (currUser != "")
			{
				Chatmessage serverResponse = new Chatmessage();
				serverResponse.Transmitter = "Administrator";
				serverResponse.Receiver = currUser;
				if (pos == -1)
				{
					serverResponse.Message = "0|Wrong Client.";
					byte[] msg = serverEncryption.EncryptRijndael(Converter.fromObjectToByteArray(serverResponse));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (ChatServer.userInfos.ContainsKey(currUser) == true)
				{
					serverResponse.Message = "0|This username already exists.";
					byte[] msg = serverEncryption.EncryptRijndael(Converter.fromObjectToByteArray(serverResponse));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else if (currUser == "Administrator")
				{
					serverResponse.Message = "0|This username is reserved.";
					byte[] msg = serverEncryption.EncryptRijndael(Converter.fromObjectToByteArray(serverResponse));
					swSender.Write(msg.Length);
					swSender.Flush();
					swSender.Write(msg);
					swSender.Flush();
					CloseConnection();
					return;
				}
				else
				{
					serverResponse.Message = "1|Connected Successfully";
					byte[] msg = serverEncryption.EncryptRijndael(Converter.fromObjectToByteArray(serverResponse));
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
					ChatServer.AddUser(tcpClient, currUser, serverEncryption);
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
					 var rawResponse = Converter.fromByteArrayToObject(serverEncryption.DecryptRijndael(response));
					 Chatmessage chatResponse = (Chatmessage) rawResponse;
					 
					 if (chatResponse.Message == null)
					 {
					     ChatServer.RemoveUser(currUser);
					 }
					 
					 else if (chatResponse.Message == "ClosingChatServerConnectionRequest")
					 {
					 	ChatServer.RemoveUser(currUser);
					 }
					 else 
					 {
				 		ChatServer.SendMessage(currUser, chatResponse.Message, chatResponse.Receiver);
					 }
				}
			}
			catch
			{
				ChatServer.RemoveUser(currUser);
			}
		}
	}
}
