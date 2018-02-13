using System.Windows;

namespace LaserPewer
{
    public partial class GenerationDialog : Window
    {
        public GenerationDialog()
        {
            InitializeComponent();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
