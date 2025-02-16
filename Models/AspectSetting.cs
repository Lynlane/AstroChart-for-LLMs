// Models/AspectSetting.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AstrologyChart.Models
{
    public enum PhaseType { Major, Minor, Other }

    public class AspectSetting : INotifyPropertyChanged
    {
        private PhaseType _phaseType;
        private double _degree;
        private double _orb;


        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        } 


        public PhaseType PhaseType
        {
            get => _phaseType;
            set { _phaseType = value; OnPropertyChanged(); }
        }

        public double Degree
        {
            get => _degree;
            set { _degree = value; OnPropertyChanged(); }
        }

        public double Orb
        {
            get => _orb;
            set { _orb = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}