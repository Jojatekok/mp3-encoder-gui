using System.Runtime.CompilerServices;

namespace WpfCustomControls
{
    public partial class ProgressBarWithText
    {
        #region Declarations

        private string _text;
        public string Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _text; }

            set {
                if (value != _text) {
                    _text = value;
                    RefreshText();
                }
            }
        }

        private byte _value;
        public byte Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private byte _minimum;
        public byte Minimum {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private byte _maximum = 100;
        public byte Maximum {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public bool IsIndeterminate {
            get { return ProgressBar1.IsIndeterminate; }

            set {
                if (value == IsIndeterminate) return;
                
                ProgressBar1.IsIndeterminate = value;
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

        private void RefreshText()
        {
            if (IsIndeterminate) {
                TextBlock1.Text = Text;
            } else if (Value == Maximum) {
                TextBlock1.Text = "Done!";
            } else {
                TextBlock1.Text = Text + " (" + Value + "%)";
            }
        }

        #endregion
    }
}
