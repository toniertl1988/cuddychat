/*
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
		protected string _transmitter;
		
		protected string _receiver = "global";
		
		protected string _message;
		
		protected string _signature = "C0dd1Ch2tCli3nt";
		
		protected string _systemInformation;
		
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
		
		public string SystemInformation {
			get { return _systemInformation; }
			set { _systemInformation = value; }
		}
		
	}
}