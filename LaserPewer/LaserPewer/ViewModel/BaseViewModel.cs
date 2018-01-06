using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LaserPewer.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected const string FIELD_PLACEHOLDER = "-";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
