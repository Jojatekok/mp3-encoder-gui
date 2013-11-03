using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfCustomControls
{
    /// <summary>
    /// Interaction logic for ProgressBarWithText.xaml
    /// </summary>
    public partial class ProgressBarWithText
    {
        #region Declarations

        private string _text;
        public string Text
        {
            get { return _text; }

            set {
                if (value != _text) {
                    _text = value;
                    RefreshText();
                }
            }
        }

        private double _value;
        public double Value
        {
            get { return _value; }

            set {
                if (!IsInitialized || value == Value) return;

                if (value < Minimum) {
                    value = Minimum;

                } else if (value > Maximum) {
                    value = Maximum;
                }

                _value = value;
                ProgressBar1.Value = value;
                RefreshText();
            }
        }

        private double _minimum;
        public double Minimum {
            get { return _minimum; }

            set {
                if (value == Minimum) return;

                if (IsInitialized) {
                    if (value > Maximum) { value = Maximum; }
                    if (Value < value) { _value = value; }
                }
                
                _minimum = value;
                ProgressBar1.Minimum = value;
                RefreshText();
            }
        }

        private double _maximum = uint.MaxValue;
        public double Maximum {
            get { return _maximum; }

            set {
                if (value == Maximum) return;

                if (IsInitialized) {
                    if (value < Minimum) { value = Minimum; }
                    if (Value > value) { _value = value; }
                }

                _maximum = value;
                ProgressBar1.Maximum = value;
                RefreshText();
            }
        }

        #endregion

        #region Methods

        public ProgressBarWithText()
        {
            InitializeComponent();
            RefreshText();
        }

        public void Reset() {
            _minimum = 0D;
            Value = 0D;
            RefreshText();
        }

        private void RefreshText()
        {
            if (Value == Maximum) {
                TextBlock1.Text = "Done!";
            } else {
                TextBlock1.Text = Text + " (" + Math.Floor(Value / Maximum * 100) + "%)";
            }
        }

        #endregion

    }
}
