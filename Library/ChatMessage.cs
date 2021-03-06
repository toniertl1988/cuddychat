﻿/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 29.11.2013
 * Time: 08:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Library
{
	/// <summary>
	/// Description of Chatmessage.
	/// </summary>
	[Serializable()]
	public class Chatmessage
	{
		public const string MESSAGE_TYPE_MESSAGE = "message";
		
		public const string MESSAGE_TYPE_USER_INFO = "user-info";
		
		public const string MESSAGE_TYPE_CONNECT = "connect";
		
		public const string MESSAGE_TYPE_DISCONNECT = "disconnect";
			
		public const string MESSAGE_TYPE_FILE_TRANSFER = "file-transfer";
		
		protected string _transmitter;
		
		protected string _receiver = "global";
		
		protected string _message;
		
		protected string _signature = "C0dd1Ch2tCli3nt";
		
		protected OperatingSystem _operatingSystem;
		
		protected string _messageType = Chatmessage.MESSAGE_TYPE_MESSAGE;
		
		public Chatmessage()
		{
			
		}
		
		public Chatmessage(string Transmitter)
		{
			_transmitter = Transmitter;
		}
		
		public Chatmessage(string Transmitter, string Receiver) : this(Transmitter)
		{
			_receiver = Receiver;
		}
		
		public Chatmessage(string Transmitter, string Receiver, string Message) : this(Transmitter, Receiver)
		{
			_message = Message;
		}
		
		public Chatmessage(string Transmitter, string Receiver, string Message, string MessageType) : this(Transmitter, Receiver, Message)
		{
			_messageType = MessageType;
		}
		
		public string Signature {
			get { return _signature; }
		}
		
		public string Transmitter {
			get { return _transmitter; }
			set { _transmitter = value; }
		}
		
		public string Receiver {
			get { return _receiver; }
			set { _receiver = value; }
		}
		
		public string Message {
			get { return _message; }
			set { _message = value; }
		}
		
		public OperatingSystem OperatingSystem {
			get { return _operatingSystem; }
			set { _operatingSystem = value; }
		}
		
		public string MessageType {
			get { return _messageType; }
			set { _messageType = value; }
		}
		
	}
}