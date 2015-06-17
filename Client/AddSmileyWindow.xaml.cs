/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 08.06.2015
 * Time: 10:41
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
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Client
{
	/// <summary>
	/// Interaction logic for AddSmileyWindow.xaml
	/// </summary>
	public partial class AddSmileyWindow : Window
	{
		protected Window1 mainWindow;
		
		public AddSmileyWindow(Window1 mainWindow)
		{
			this.mainWindow = mainWindow;
			InitializeComponent();
		}
		
		public void addSmileysToGrid(Hashtable smileys)
		{
			int columnCount = 0;
			int rowCount = 0;
			
    		// Add Rows
    		int tmpRowCount = (int) Math.Ceiling((decimal) (smileys.Count/3));
    		for (int tmp = 0; tmp < tmpRowCount; tmp++) {
    			RowDefinition gridRow = new RowDefinition();
				gridRow.Height = new GridLength(30);
				windowGrid.RowDefinitions.Add(gridRow);
    		}

    		List<string> alreadyShownSmileys = new List<string>();
    		
    		foreach (var element in smileys.Keys) {
    			string uri = (string)smileys[element];
    			// dont show duplettes
    			if (alreadyShownSmileys.Contains(uri)) {
    				continue;
    			}
    			alreadyShownSmileys.Add(uri);
                BitmapImage bitmapSmiley = new BitmapImage();
            	bitmapSmiley.BeginInit();
            	bitmapSmiley.UriSource = new Uri("pack://application:,,,/Smileys/" + uri);
            	bitmapSmiley.EndInit();
            	
                Image image = new Image();
            	image.Source = bitmapSmiley;
                image.Width = bitmapSmiley.Width;
                image.Height = bitmapSmiley.Height;
                image.SnapsToDevicePixels = true;
                image.VerticalAlignment = VerticalAlignment.Center;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                ImageBehavior.SetAnimatedSource(image, bitmapSmiley);
            	
            	image.ToolTip = element;
            	StackPanel panel = new StackPanel();
            	panel.MouseDown += new MouseButtonEventHandler(mainWindow.addSmileyClickEvent);
            	panel.MouseEnter += new MouseEventHandler(smileyImageMouseEnter);
            	panel.MouseLeave += new MouseEventHandler(smileyImageMouseLeave);
            	panel.Background = System.Windows.Media.Brushes.Transparent;
            	panel.Children.Add(image);
                
                Grid.SetRow(panel, rowCount);
    			Grid.SetColumn(panel, columnCount);
    			
    			windowGrid.Children.Add(panel);
    			
				columnCount++;
				
				if (columnCount == 3) {
					columnCount = 0;
					rowCount++;
				}
			}
		}
		
		public void smileyImageMouseEnter(object sender, RoutedEventArgs e)
		{
			StackPanel panel = (StackPanel) sender;
			panel.Background = System.Windows.Media.Brushes.Silver;
		}
		
		public void smileyImageMouseLeave(object sender, RoutedEventArgs e)
		{
			StackPanel panel = (StackPanel) sender;
			panel.Background = System.Windows.Media.Brushes.WhiteSmoke;
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
		    base.OnClosing(e);
		    e.Cancel = true;
		    this.Hide();
		}
	}
}