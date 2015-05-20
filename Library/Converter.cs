/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 30.11.2012
 * Time: 12:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Library
{
	/// <summary>
	/// Description of Converter.
	/// </summary>
	public class Converter
	{
		public Converter()
		{
		}
		
		public static byte[] fromStringToByteArray(string message)
		{
			return Encoding.UTF8.GetBytes(message);
		}
		
		public static string fromByteArrayToString(byte[] message)
		{
			return Encoding.UTF8.GetString(message);
		}
		
		public static byte[] fromObjectToByteArray(object convObject)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(memoryStream, convObject);
			return memoryStream.ToArray();
		}
		
		public static object fromByteArrayToObject(byte[] data)
		{
			MemoryStream newStream = new MemoryStream(data);
			BinaryFormatter formatter = new BinaryFormatter();
			return formatter.Deserialize(newStream);
		}
	}
}
