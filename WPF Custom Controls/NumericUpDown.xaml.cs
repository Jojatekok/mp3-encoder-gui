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

        #endregion

        #region Methods

        public NumericUpDown()
        {
            InitializeComponent();
        }

        private void IncreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value == null) {
                Value = Minimum;
            } else {
                Value += 1;
            }
        }

        private void DecreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value == null) {
                Value = Maximum;
            } else {
                Value -= 1;
            }
        }

        private void Buttons_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Value != Minimum) {
                NumberBox1.TextBoxNumber.SelectionStart = NumberBox1.TextBoxNumber.Text.Length;
            } else {
                NumberBox1.TextBoxNumber.SelectAll();
            }

            NumberBox1.TextBoxNumber.Focus();
        }

        #endregion
    }
}
