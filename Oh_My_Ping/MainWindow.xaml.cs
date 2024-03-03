using Oh_My_Ping.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oh_My_Ping
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static double delay = 0;
        private bool delayTextChanged = false;
        private bool delaySliderChanged = false;
        private ProxyServer proxy;

        public MainWindow() {
            InitializeComponent();
            Console.WriteLine("test");
        }



        private void delaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (delayTextChanged)
            {
                delayTextChanged = false;
                return;
            }

            delayLabel.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            delaySliderChanged = true;
            delay = e.NewValue;
            delayLabel.Text = ((int)delay).ToString();
            
        }


        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //new SimpleProxy(addressText.Text);
            proxy = new ProxyServer(addressText.Text);
        }


        private void delayLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (delaySliderChanged) { 
                delaySliderChanged = false;
                return; 
            }

            try
            {
                delay = double.Parse(delayLabel.Text);
                delayLabel.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            catch (Exception ex)
            {
                delayLabel.Background = new SolidColorBrush(Color.FromRgb(255, 170, 170));
                return;
            }

            delayTextChanged = true;
            delaySlider.Value = delay;
        }
    }
}
