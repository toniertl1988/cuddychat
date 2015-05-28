/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 20.05.2015
 * Time: 11:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net.Sockets;

using Library;

namespace Server
{
	/// <summary>
	/// Description of User.
	/// </summary>
	public class User
	{		
		protected string username;
		
		protected TcpClient tcpClient;
		
		protected Encryption encryption;
		
		protected DateTime loginTime;
		
		public string Username {
			get { return username; }
			set { username = value; }
		}
		
		public TcpClient TcpClient {
			get { return tcpClient; }
			set { tcpClient = value; }
		}
		
		public Encryption Encryption {
			get { return encryption; }
			set { encryption = value; }
		}
		
		public DateTime LoginTime {
			get { return loginTime; }
			set { loginTime = value; }
		}
	}
}
