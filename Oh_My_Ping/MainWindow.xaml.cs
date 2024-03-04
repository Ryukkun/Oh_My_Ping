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
            if (proxy?.isRunnning == true) {
                proxy.close();
                startButton.Content = "② Start";
                startButton.Background = new SolidColorBrush(Color.FromRgb(30, 150, 50));
                startButton.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 200, 80));

            } else {
                //new SimpleProxy(addressText.Text);
                proxy = new ProxyServer(addressText.Text);
                startButton.Content = "Stop";
                startButton.Background = new SolidColorBrush(Color.FromRgb(150, 30, 30));
                startButton.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 80, 80));
            }
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
            catch (Exception)
            {
                delayLabel.Background = new SolidColorBrush(Color.FromRgb(255, 170, 170));
                return;
            }

            delayTextChanged = true;
            delaySlider.Value = delay;
        }
    }
}
