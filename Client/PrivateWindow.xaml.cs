/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 14.02.2014
 * Time: 14:58
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for PrivateWindow.xaml
	/// </summary>
	public partial class PrivateWindow : Window, BaseWindow
	{
		protected Window1 mainWindow;
		
		protected string chatPartner;
		
		protected AddSmileyWindow addSmileyWindow;
		
		public PrivateWindow(string chatPartner)
		{
			InitializeComponent();
			txtLog.Document.Blocks.Clear();
			this.chatPartner = chatPartner;
			this.Title = "Privater Chat mit " + chatPartner;
			addSmileyWindow = new AddSmileyWindow(this);
		}
		
		public void setOwner(Window1 owner)
		{
			mainWindow = owner;
			addSmileyWindow.addSmileysToGrid(owner.smileyClass.getAllSmileys());
		}
		
		private void txtMessage_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (Keyboard.Modifiers == ModifierKeys.Shift) {
					int selectionStart = txtMessage.SelectionStart;
					txtMessage.Text += Environment.NewLine;
					txtMessage.Select(selectionStart + 1, 0);
				} else {
					if (txtMessage.Text.Length > 0) {
						mainWindow.SendMessage(txtMessage.Text, chatPartner);
					}
				}

			}
		}
		
    	public void closeWindow(object sender, EventArgs e)
    	{
    		this.mainWindow.closePrivateChat(this.chatPartner);
    	}
    	
		public void BtnSend_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.SendMessage(txtMessage.Text, chatPartner);
		}
		
		public TextBox getMessageElement()
		{
			return txtMessage;
		}
		
		protected void openAddSmileyWindow(object sender, RoutedEventArgs e)
		{
    		addSmileyWindow.Show();
    		addSmileyWindow.Activate();
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
		    base.OnClosing(e);
		    e.Cancel = true;
		    this.Hide();
		}
	}
}