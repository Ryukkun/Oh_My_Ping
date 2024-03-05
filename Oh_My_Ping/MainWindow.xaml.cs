using Oh_My_Ping.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
        private static Button _startButton;
        private static bool _startButton_isEnabled = false;

        private static Label _statusLabel;


        private bool delayTextChanged = false;
        private bool delaySliderChanged = false;
        private ProxyServer proxy = null;
        private string checkedAddress = null;

        private Action startButton_start = () => {
            _startButton.Content = "② Start";
            _startButton_isEnabled = true;
            _startButton.Background = new SolidColorBrush(Color.FromRgb(30, 150, 50));
            _startButton.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 200, 80));
            changeStatus("Let's start proxy server", 1);
        };

        private Action startButton_stop = () => {
            _startButton.Content = "Stop";
            _startButton_isEnabled = true;
            _startButton.Background = new SolidColorBrush(Color.FromRgb(150, 30, 30));
            _startButton.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 80, 80));;
        };

        private Action startButton_startDisable = () => {
            _startButton.Content = "② Start";
            _startButton_isEnabled = false;
            _startButton.Background = new SolidColorBrush(Color.FromRgb(120, 120, 120));
            _startButton.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            changeStatus("Server not found", 0);
        };

        public static void changeStatus(string text, byte warningLebel) {
            _statusLabel.Content = text;
            
            switch (warningLebel) {
                case 0:
                    _statusLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 80, 80));
                    break;
                case 1:
                    _statusLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 80));
                    break ;
                case 2:
                    _statusLabel.Foreground = new SolidColorBrush(Color.FromRgb(80, 255, 80));
                    break ;
            }
        }



        public MainWindow() {
            InitializeComponent();
            _startButton = startButton;
            _statusLabel = statusLabel;
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
            if (!_startButton_isEnabled) { return; }

            if (proxy?.isRunning == true) {
                proxy.close();
                proxy = null;
                if (checkedAddress == null) {
                    startButton_startDisable();
                } else {
                    startButton_start();
                }

            } else {
                //new SimpleProxy(addressText.Text);
                proxy = new ProxyServer(addressText.Text);
                startButton_stop();
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

        private async void addressText_TextChanged(object sender, TextChangedEventArgs e) {
            if (proxy?.isRunning != true) {
                startButton_startDisable();
            }
            addressText.Background = new SolidColorBrush(Color.FromRgb(255, 170, 170));
            checkedAddress = null;
            string text = addressText.Text;

            try {
                (string address, int port) = ProxyServer.getAddress(text);
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(address, port);
            
            } catch (Exception) {
                return;
            }

            if (text.Equals(addressText.Text)) {
                checkedAddress = text;
                addressText.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                if (proxy?.isRunning != true) {
                    startButton_start();
                }
            }
        }
    }
}
