using System;

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

        private uint _value;
        public uint Value {
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

        private uint _minimum;
        public uint Minimum {
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

        private uint _maximum = uint.MaxValue;
        public uint Maximum {
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

        public bool IsMarquee {
            get { return ProgressBar1.IsIndeterminate; }
            set {
                if (value == IsMarquee) return;

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

        public void Reset() {
            _minimum = 0U;
            Value = 0U;
        }

        private void RefreshText()
        {
            if (Value == Maximum) {
                TextBlock1.Text = "Done!";
            } else if (IsMarquee) {
                TextBlock1.Text = Text;
            } else {
                TextBlock1.Text = Text + " (" + Math.Floor((double)Value / Maximum * 100D) + "%)";
            }
        }

        #endregion

    }
}
