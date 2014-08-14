using System.Windows.Media;
using Bomberman.Client.WPF.MVVM;
using Bomberman.Common.DataContracts;

namespace Bomberman.Client.WPF.ViewModels.Play
{
    public class CellItem : ObservableObject
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
        
        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
                if (Set(() => X, ref _x, value))
                    OnPropertyChanged("PositionX");
            }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                if (Set(() => Y, ref _y, value))
                    OnPropertyChanged("PositionY");
            }
        }

        private int _z;
        public int Z
        {
            get { return _z; }
            set { Set(() => Z, ref _z, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                if (Set(() => Width, ref _width, value))
                    OnPropertyChanged("PositionX");
            }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                if (Set(() => Height, ref _height, value))
                    OnPropertyChanged("PositionY");
            }
        }
        
        public int PositionX
        {
            get { return X*Width; }
        }

        public int PositionY
        {
            get { return Y*Height; }
        }
    }
}
