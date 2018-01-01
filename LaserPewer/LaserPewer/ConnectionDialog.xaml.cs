using System.IO.Ports;
using System.Windows;

namespace LaserPewer
{
    public partial class ConnectionDialog : Window
    {
        public string SelectedPortName { get; private set; }

        public ConnectionDialog()
        {
            InitializeComponent();

            rescan();
        }

        private void rescan()
        {
            portListComboBox.Items.Clear();

            foreach (string portName in SerialPort.GetPortNames())
            {
                portListComboBox.Items.Add(portName);
            }

            portListComboBox.SelectedIndex = 0;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPortName = (string)portListComboBox.SelectedValue;
            DialogResult = true;
        }

        private void rescanButton_Click(object sender, RoutedEventArgs e)
        {
            rescan();
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
