using System.Windows.Media;
using Bomberman.Client.WPF.MVVM;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.WPF.ViewModels.Play
{
    public class BonusItem : ObservableObject
    {
        private EntityTypes _type;
        public EntityTypes Type
        {
            get { return _type; }
            set { Set(() => Type, ref _type, value); }
        }

        private SolidColorBrush _color; // TODO: replace with image
        public SolidColorBrush Color
        {
            get { return _color; }
            set { Set(() => Color, ref _color, value); }
        }

        private string _text; // TODO: replace with image
        public string Text
        {
            get { return _text; }
            set { Set(() => Text, ref _text, value); }
        }
    }
}
