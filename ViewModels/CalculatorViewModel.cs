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
        private string _mode = "calc"; // "calc", "currency", "date"
        private DateTime _dateFrom = DateTime.Now;
        private DateTime _dateTo = DateTime.Now;
        private string _currencyFrom = "EUR €";
        private string _currencyTo = "EUR €";

        public string Display { get => _display; set { _display = value; OnPropertyChanged(); } }
        public string Mode { get => _mode; set { _mode = value; OnPropertyChanged(); ResetCalculator(); } }
        public DateTime DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }
        public DateTime DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }
        public string CurrencyFrom { get => _currencyFrom; set { _currencyFrom = value; OnPropertyChanged(); } }
        public string CurrencyTo { get => _currencyTo; set { _currencyTo = value; OnPropertyChanged(); } }

        public ICommand DigitCommand { get; }
        public ICommand OpCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand EqualCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand ConvertCurrencyCommand { get; }
        public ICommand CalculateDateDiffCommand { get; }

        public CalculatorViewModel()
        {
            DigitCommand = new RelayCommand(p => Digit(p?.ToString()), _ => true);
            OpCommand = new RelayCommand(p => Op(p?.ToString()), _ => true);
            ClearCommand = new RelayCommand(_ => Clear(), _ => true);
            EqualCommand = new RelayCommand(_ => Equal(), _ => true);
            SwitchModeCommand = new RelayCommand(p => SwitchMode(p?.ToString()), _ => true);
            ConvertCurrencyCommand = new RelayCommand(_ => ConvertCurrency(), _ => true);
            CalculateDateDiffCommand = new RelayCommand(_ => CalculateDateDifference(), _ => true);
        }

        private void SwitchMode(string? mode)
        {
            if (!string.IsNullOrEmpty(mode))
            {
                Mode = mode;
            }
        }

        private void ResetCalculator()
        {
            Display = "0";
            _acc = 0;
            _op = string.Empty;
            _newEntry = true;
        }

        private void Digit(string? d)
        {
            if (string.IsNullOrEmpty(d)) return;

            // Utiliser une virgule ou un point pour la décimale selon la culture
            string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string normalizedDecimal = (d == "," || d == ".") ? decimalSeparator : d;

            // Éviter les doublons de décimales
            if ((d == "," || d == ".") && Display.Contains(decimalSeparator)) return;

            if (_newEntry)
            {
                Display = (d == "," || d == ".") ? "0" + decimalSeparator : d;
                _newEntry = false;
            }
            else
            {
                if (Display == "0" && d != "," && d != ".")
                {
                    Display = d;
                }
                else if (d == "," || d == ".")
                {
                    Display += decimalSeparator;
                }
                else
                {
                    Display += d;
                }
            }
        }

        private void Op(string? o)
        {
            if (string.IsNullOrEmpty(o)) return;

            double cur;
            if (!double.TryParse(Display, out cur)) cur = 0;

            if (!string.IsNullOrEmpty(_op))
            {
                Compute(cur);
            }
            else
            {
                _acc = cur;
            }

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
                case "/": 
                    if (cur != 0) 
                        _acc /= cur;
                    else
                    {
                        Display = "Erreur: Division par 0";
                        return;
                    }
                    break;
            }

            // Formater pour éviter trop de décimales
            string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string result = _acc.ToString("F10").TrimEnd('0').TrimEnd('.');

            // Remplacer par le séparateur local
            result = result.Replace(".", decimalSeparator);
            Display = result;
        }

        private void Equal()
        {
            if (string.IsNullOrEmpty(_op)) return;

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

        private void ConvertCurrency()
        {
            if (!double.TryParse(Display, out var amount))
            {
                Display = "Erreur";
                return;
            }

            // Extraire le code devises (EUR, USD, FCFA) depuis la sélection
            string fromCode = ExtractCurrencyCode(CurrencyFrom);
            string toCode = ExtractCurrencyCode(CurrencyTo);

            // Taux de change par rapport à EUR (1 EUR = ...)
            var rates = new Dictionary<string, double>
            {
                { "EUR", 1.0 },
                { "USD", 1.08 },      // 1 EUR = 1.08 USD
                { "FCFA", 655.957 }   // 1 EUR = 655.957 FCFA
            };

            if (!rates.ContainsKey(fromCode) || !rates.ContainsKey(toCode))
            {
                Display = "Erreur";
                return;
            }

            // Convertir en EUR, puis dans la deviseible
            var amountInEur = amount / rates[fromCode];
            var convertedAmount = amountInEur * rates[toCode];

            Display = Math.Round(convertedAmount, 2).ToString();
            _newEntry = true;
        }

        private string ExtractCurrencyCode(string currencyString)
        {
            if (string.IsNullOrEmpty(currencyString)) return "EUR";

            // Extraire le code avant l'espace (ex: "EUR €" -> "EUR")
            var parts = currencyString.Split(' ');
            return parts.Length > 0 ? parts[0] : "EUR";
        }

        private void CalculateDateDifference()
        {
            var diff = DateTo.Date - DateFrom.Date;
            Display = diff.Days.ToString();
            _newEntry = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
