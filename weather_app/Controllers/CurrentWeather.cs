using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using weather_app.Services;

namespace weather_app.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentProvider1;
        public string CurrentProvider1
        {
            get => _currentProvider1;
            set
            {
                if (_currentProvider1 != value)
                {
                    _currentProvider1 = value;
                    OnPropertyChanged(nameof(CurrentProvider1));
                }
            }
        }

        private double _currentTemperature1;
        public double CurrentTemperature1
        {
            get => _currentTemperature1;
            set
            {
                if (_currentTemperature1 != value)
                {
                    _currentTemperature1 = value;
                    OnPropertyChanged(nameof(CurrentTemperature1));
                }
            }
        }

        private double _currentWindSpeed1;
        public double CurrentWindSpeed1
        {
            get => _currentWindSpeed1;
            set
            {
                if (_currentWindSpeed1 != value)
                {
                    _currentWindSpeed1 = value;
                    OnPropertyChanged(nameof(CurrentWindSpeed1));
                }
            }
        }

        private double _currentRadiation1;
        public double CurrentRadiation1
        {
            get => _currentRadiation1;
            set
            {
                if (_currentRadiation1 != value)
                {
                    _currentRadiation1 = value;
                    OnPropertyChanged(nameof(CurrentRadiation1));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


