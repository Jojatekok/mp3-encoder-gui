using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfCustomControls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown
    {
        #region Declarations

        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private bool _isTextChangedByUser = true;
        private bool _isMinimumWarningActive;

        private uint _value;
        public uint Value
        {
            get { return _value; }

            set {
                if (!IsInitialized) return;

                if (value < Minimum) {
                    _isMinimumWarningActive = true;
                    value = Minimum;

                } else if (value > Maximum) {
                    value = Maximum;
                }

                if (value == _value && value != Minimum) return;

                _value = value;

                var oldSelectionStart = TextBoxNumber.SelectionStart;
                _isTextChangedByUser = false;
                TextBoxNumber.Text = _value.ToString(InvariantCulture);
                _isTextChangedByUser = true;

                if (_value == Minimum && _isMinimumWarningActive) {
                    TextBoxNumber.SelectAll();
                    _isMinimumWarningActive = false;
                } else if (_value == Maximum) {
                    TextBoxNumber.SelectionStart = TextBoxNumber.Text.Length;
                } else {
                    TextBoxNumber.SelectionStart = oldSelectionStart;
                }
            }
        }

        private uint _minimum;
        public uint Minimum {
            get { return _minimum; }

            set {
                if (value > Maximum) { value = Maximum; }
                _minimum = value;

                if (Value < _minimum) {
                    if (TextBoxNumber.Text.Length == 0) {
                        _value = _minimum;
                    } else {
                        Value = _minimum;
                    }
                }
            }
        }

        private uint _maximum = uint.MaxValue;
        public uint Maximum {
            get { return _maximum; }

            set {
                if (value < Minimum) { value = Minimum; }
                _maximum = value;

                if (Value > _maximum) {
                    if (TextBoxNumber.Text.Length == 0) {
                        _value = _maximum;
                    } else {
                        Value = _maximum;
                    }
                }
            }
        }

        #endregion

        #region Creation

        public NumericUpDown()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void ValidateInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text[0]) || (Value == Maximum && TextBoxNumber.SelectionLength == 0)) {
                e.Handled = true;
            }
        }

        private void NumberChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxNumber.Text.Length != 0 && _isTextChangedByUser) {
                uint newValue;
                Value = uint.TryParse(TextBoxNumber.Text, out newValue) ? newValue : Maximum;
            }
        }

        private void BlockSpecialChars(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) {
                e.Handled = true;
            }
        }

        private void IncreaseValue(object sender, RoutedEventArgs e)
        {
            if (TextBoxNumber.Text.Length == 0) {
                Value = Minimum;
            } else {
                Value += 1;
            }
        }

        private void DecreaseValue(object sender, RoutedEventArgs e)
        {
            if (TextBoxNumber.Text.Length == 0) {
                Value = Minimum;
            } else {
                Value -= 1;
            }
        }

        private void Buttons_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Value != Minimum) {
                TextBoxNumber.SelectionStart = TextBoxNumber.Text.Length;
            } else {
                TextBoxNumber.SelectAll();
            }

            TextBoxNumber.Focus();
        }

        #endregion
    }
}
