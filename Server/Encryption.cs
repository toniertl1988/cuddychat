/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 25.06.2012
 * Time: 12:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;
using System.Collections;

namespace Server
{
	/// <summary>
	/// Description of Encryption.
	/// </summary>
	public class Encryption
	{
		RSACryptoServiceProvider publicKey;
		RSACryptoServiceProvider privateKey;
		
		public Encryption()
		{
			publicKey = new RSACryptoServiceProvider(1024);
			privateKey = new RSACryptoServiceProvider(1024);
		}
		
		public Encryption(string xmlString)
		{
			publicKey = new RSACryptoServiceProvider(1024);
			publicKey.FromXmlString(xmlString);
		}
		
		public char[] EncryptOutgoing(string message)
		{
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(message);
			byte[] encr = publicKey.Encrypt(msg, true);
			string encrString = Convert.ToBase64String(encr);
		    return encrString.ToCharArray();
		}
		
		public string DecryptIncoming(string message)
		{
			byte[] msg = Convert.FromBase64String(message);
			byte[] decr = privateKey.Decrypt(msg, true);
			string ret = System.Text.Encoding.UTF8.GetString(decr);
			return ret;
		}
		
		public string DecryptIncoming(char[] icomingMessage)
		{
			string message = icomingMessage.ToString();
			byte[] msg = Convert.FromBase64String(message);
			byte[] decr = privateKey.Decrypt(msg, true);
			string ret = System.Text.Encoding.UTF8.GetString(decr);
			return ret;
		}
		
		public string getPublicKey()
		{
			return privateKey.ToXmlString(false);
		}
		
		public void setPublicKey(string xmlString)
		{
			publicKey.FromXmlString(xmlString);
		}
	}
}
