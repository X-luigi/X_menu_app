using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace X_menu_app.ViewModels
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private string _display = "0";
        private double _acc = 0;
        private string _op = string.Empty;
        private bool _newEntry = true;

        public string Display { get => _display; set { _display = value; OnPropertyChanged(); } }

        public ICommand DigitCommand { get; }
        public ICommand OpCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand EqualCommand { get; }

        public CalculatorViewModel()
        {
            DigitCommand = new RelayCommand(p => Digit(p?.ToString()), _ => true);
            OpCommand = new RelayCommand(p => Op(p?.ToString()), _ => true);
            ClearCommand = new RelayCommand(_ => Clear(), _ => true);
            EqualCommand = new RelayCommand(_ => Equal(), _ => true);
        }

        private void Digit(string? d)
        {
            if (string.IsNullOrEmpty(d)) return;
            if (_newEntry)
            {
                Display = d;
                _newEntry = false;
            }
            else
            {
                Display = Display == "0" ? d : Display + d;
            }
        }

        private void Op(string? o)
        {
            if (string.IsNullOrEmpty(o)) return;
            double cur;
            if (!double.TryParse(Display, out cur)) cur = 0;
            if (!string.IsNullOrEmpty(_op)) Compute(cur);
            else _acc = cur;
            _op = o;
            _newEntry = true;
        }

        private void Compute(double cur)
        {
            switch (_op)
            {
                case "+": _acc += cur; break;
                case "-": _acc -= cur; break;
                case "*": _acc *= cur; break;
                case "/": if (cur != 0) _acc /= cur; break;
            }
            Display = _acc.ToString();
        }

        private void Equal()
        {
            double cur;
            if (!double.TryParse(Display, out cur)) cur = 0;
            Compute(cur);
            _op = string.Empty;
            _newEntry = true;
        }

        private void Clear()
        {
            Display = "0";
            _acc = 0;
            _op = string.Empty;
            _newEntry = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
