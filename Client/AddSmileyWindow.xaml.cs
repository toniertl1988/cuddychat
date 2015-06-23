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
				gridRow.Height = new GridLength(35);
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
            	
            	Border border = new Border();
            	border.BorderBrush = System.Windows.Media.Brushes.WhiteSmoke;
            	border.CornerRadius = new CornerRadius(5.0, 5.0, 5.0, 5.0);
            	border.BorderThickness = new System.Windows.Thickness(1.0, 1.0, 1.0, 1.0);
            	border.Margin = new System.Windows.Thickness(2.0);
            	border.MouseDown += new MouseButtonEventHandler(mainWindow.addSmileyClickEvent);
            	border.MouseEnter += new MouseEventHandler(smileyImageMouseEnter);
            	border.MouseLeave += new MouseEventHandler(smileyImageMouseLeave);
            	
            	StackPanel panel = new StackPanel();
            	panel.Background = System.Windows.Media.Brushes.Transparent;
            	panel.HorizontalAlignment = HorizontalAlignment.Center;
            	panel.VerticalAlignment = VerticalAlignment.Center;
            	            
            	panel.Children.Add(image);
            	
            	border.Child = panel;
            	border.Resources.Add("smiley", image);
                
                Grid.SetRow(border, rowCount);
    			Grid.SetColumn(border, columnCount);
    			
    			windowGrid.Children.Add(border);
    			
				columnCount++;
				
				if (columnCount == 3) {
					columnCount = 0;
					rowCount++;
				}
			}
		}
		
		public void smileyImageMouseEnter(object sender, RoutedEventArgs e)
		{
			Border border = (Border) sender;
			border.Background = System.Windows.Media.Brushes.LightGray;
			border.BorderBrush = System.Windows.Media.Brushes.Silver;
		}
		
		public void smileyImageMouseLeave(object sender, RoutedEventArgs e)
		{
			Border border = (Border) sender;
			border.Background = System.Windows.Media.Brushes.WhiteSmoke;
			border.BorderBrush = System.Windows.Media.Brushes.WhiteSmoke;
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
		    base.OnClosing(e);
		    e.Cancel = true;
		    this.Hide();
		}
	}
}