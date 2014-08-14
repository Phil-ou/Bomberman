using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bomberman.Common.Helpers;

namespace Bomberman.Client.WPF.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler.Do(x => x(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
