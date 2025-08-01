    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unloadSchedule.MVVM.Model
{
    public class DataGridRowItem : INotifyPropertyChanged
    {
        private string[] _parts;
        private bool _isEdited;

        public DataGridRowItem(string line)
        {
            _parts = line.Split(new[] { '|' }, StringSplitOptions.None);
            _isEdited = false;
        }

        public string FirstPart => _parts.Length > 0 ? _parts[0] : string.Empty;

        public string SecondPart
        {
            get => _parts.Length > 1 ? _parts[1] : string.Empty;
            set
            {
                if (_parts.Length > 1 && _parts[1] != value)
                {
                    _parts[1] = value;
                    IsEdited = true;
                    OnPropertyChanged(nameof(SecondPart));
                }
            }
        }

        public string ThirdPart
        {
            get => _parts.Length > 2 ? _parts[2] : string.Empty;
            set
            {
                if (_parts.Length > 2 && _parts[2] != value)
                {
                    _parts[2] = value;
                    IsEdited = true;
                    OnPropertyChanged(nameof(ThirdPart));
                }
            }
        }

        public bool IsEdited
        {
            get => _isEdited;
            set
            {
                if (_isEdited != value)
                {
                    _isEdited = value;
                    OnPropertyChanged(nameof(IsEdited));
                }
            }
        }

        public string GetEditedLine() => string.Join("|", _parts);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
