/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 27.06.2012
 * Time: 11:47
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Library
{
	/// <summary>
	/// Description of Encyption.
	/// </summary>
	public class Encryption
	{
		RSACryptoServiceProvider publicKey;
		RSACryptoServiceProvider privateKey;
		private Rijndael _rijndael;
		private ICryptoTransform _rijEncryptor;
		private ICryptoTransform _rijDecryptor;
		
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
		
		public string DecryptIncoming(char[] incomingMessage)
		{
			string message = incomingMessage.ToString();
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
		
		public void setUpRijndael()
		{
			byte[] key = new byte[16];
			byte[] iv = new byte[16];
			RandomNumberGenerator rand = RandomNumberGenerator.Create();
			rand.GetBytes(key);
			rand.GetBytes(iv);
			setUpRijndael(key, iv);
		}
		
		public void setUpRijndael(byte[] key, byte[] iv)
		{
			_rijndael = new RijndaelManaged();
			_rijndael.Key = key;
			_rijndael.IV = iv;
			_rijEncryptor = _rijndael.CreateEncryptor(_rijndael.Key, _rijndael.IV);
			_rijDecryptor = _rijndael.CreateDecryptor(_rijndael.Key, _rijndael.IV);
		}
		
		public byte[] DecryptRijndael(byte[] incomingMessage)
		{
			return CryptRijndael(incomingMessage, _rijDecryptor);
		}
		
		public byte[] EncryptRijndael(byte[] outgoingMessage)
		{
			return CryptRijndael(outgoingMessage, _rijEncryptor);
		}
		
		public byte[] CryptRijndael(byte[] data, ICryptoTransform cryptor)
		{
			MemoryStream m = new MemoryStream();
			using (Stream c = new CryptoStream(m, cryptor, CryptoStreamMode.Write))
				c.Write(data, 0, data.Length);
			return m.ToArray();
		}
		
		public byte[] EncryptRSA(byte[] message)
		{
			return publicKey.Encrypt(message, true);
		}
		
		
		public byte[] DecryptRSA(byte[] data)
		{
			return privateKey.Decrypt(data, true);
		}
		
		public byte[] getRijKey()
		{
			return _rijndael.Key;
		}
		
		public byte[] getRijIV()
		{
			return _rijndael.IV;
		}
	}
}
