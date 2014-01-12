using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace WpfCustomControls
{
    public partial class NumericUpDown
    {
        #region Declarations

        public uint? Value
        {
            get { return NumberBox1.Value; }
            set { NumberBox1.Value = value; }
        }

        public uint Minimum {
            get { return NumberBox1.Minimum; }
            set { NumberBox1.Minimum = value; }
        }

        public uint Maximum {
            get { return NumberBox1.Maximum; }
            set { NumberBox1.Maximum = value; }
        }

        public bool IsReadOnly
        {
            get { return NumberBox1.IsReadOnly; }

            set {
                RepeatButtonUp.IsEnabled = !value;
                RepeatButtonDown.IsEnabled = !value;
                NumberBox1.IsReadOnly = value;
            }
        }

        #endregion

        #region Methods

        public NumericUpDown()
        {
            InitializeComponent();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value == null) {
                Value = Minimum;
            } else {
                Value += 1U;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value == null) {
                Value = Minimum;
            } else {
                Value -= 1U;
            }
        }

        private void Buttons_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Value != Minimum) {
                NumberBox1.TextBox1.SelectionStart = NumberBox1.TextBox1.Text.Length;
            } else {
                NumberBox1.TextBox1.SelectAll();
            }

            NumberBox1.TextBox1.Focus();
        }

        #endregion
    }
}
