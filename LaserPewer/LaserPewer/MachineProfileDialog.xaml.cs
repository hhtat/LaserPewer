using System.Windows;
using System.Windows.Input;

namespace LaserPewer
{
    public partial class MachineProfileDialog : Window
    {
        public MachineProfile Profile { get; private set; }

        public MachineProfileDialog(MachineProfile profile)
        {
            InitializeComponent();

            load(profile);
        }

        private void load(MachineProfile profile)
        {
            Profile = profile;

            nameTextBox.Text = profile.FriendlyName;
            widthTextBox.Text = profile.TableWidth.ToString();
            heightTextBox.Text = profile.TableHeight.ToString();
            maxFeedTextBox.Text = profile.MaxFeedRate.ToString();
        }

        private bool apply(MachineProfile profile)
        {
            double width;
            double height;
            double feedRate;

            if (nameTextBox.Text.Length == 0) return false;
            if (!(double.TryParse(widthTextBox.Text, out width) && width > 0.0 && width <= 2000.0)) return false;
            if (!(double.TryParse(heightTextBox.Text, out height) && height > 0.0 && height <= 2000.0)) return false;
            if (!(double.TryParse(maxFeedTextBox.Text, out feedRate) && feedRate > 0.0 && feedRate <= 100000.0)) return false;

            profile.FriendlyName = nameTextBox.Text;
            profile.TableWidth = width;
            profile.TableHeight = height;
            profile.MaxFeedRate = feedRate;

            return true;
        }

        private void save()
        {
            if (apply(Profile))
            {
                AppCore.Instance.SaveSettings();
                Close();
            }
            else
            {
                MessageBox.Show(this, "Check values!");
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void duplicateButton_Click(object sender, RoutedEventArgs e)
        {
            MachineProfile profile = new MachineProfile();
            if (apply(profile))
            {
                profile.FriendlyName += " (Duplicate)";
                AppCore.Instance.AddProfile(profile);
                AppCore.Instance.SaveSettings();
                load(profile);
            }
            else
            {
                MessageBox.Show(this, "Check values!");
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppCore.Instance.TryDeleteProfile(Profile))
            {
                AppCore.Instance.SaveSettings();
                Profile = null;
            }
            Close();
        }

        private void formGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                save();
            }
        }
    }
}
