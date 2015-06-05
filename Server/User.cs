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
		
		protected OperatingSystem operatingSystem;
		
		public User()
		{
			
		}
		
		public User(string username)
		{
			this.username = Username;
		}
		
		public User(string username, TcpClient tcpClient) : this(username)
		{
			this.tcpClient = tcpClient;
		}
		
		public User(string username, TcpClient tcpClient, Encryption encryption) : this(username, tcpClient)
		{
			this.encryption = encryption;
		}
		
		public User(string username, TcpClient tcpClient, Encryption encryption, DateTime loginTime) : this(username, tcpClient, encryption)
		{
			this.loginTime = loginTime;
		}
		
		public User(string username, TcpClient tcpClient, Encryption encryption, DateTime loginTime, OperatingSystem operatingSystem) : this(username, tcpClient, encryption, loginTime)
		{
			this.operatingSystem = operatingSystem;
		}
		
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
		
		public OperatingSystem OperatingSystem {
			get { return operatingSystem; }
			set { operatingSystem = value; }
		}
	}
}
