using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfCustomControls
{
    public sealed partial class NumberBox
    {
        #region Events

        public event EventHandler<NuintValueChangedEventArgs> ValueChanged;

        #endregion

        #region Declarations

        private bool _isTextChangingProgrammatically;

        private uint? _value;
        public uint? Value
        {
            get { return _value; }

            set {
                if (!IsInitialized) return;

                if (value != null) {
                    if (value < Minimum) {
                        value = Minimum;
                        OverwriteValue(Minimum, true);
                        TextBox1.SelectAll();

                    } else if (value > Maximum) {
                        value = Maximum;
                    }
                }

                if (value == _value) { return; }

                _value = value;
                if (value != null && TextBox1.Text != value.ToString()) {
                    OverwriteValue(value.Value, true);
                }

                OnValueChanged(new NuintValueChangedEventArgs(value));
            }
        }

        private uint _minimum;
        public uint Minimum {
            get { return _minimum; }

            set {
                if (value > Maximum) { value = Maximum; }
                if (_value < value) { _value = value; }

                _minimum = value;
            }
        }

        private uint _maximum = uint.MaxValue;
        public uint Maximum {
            get { return _maximum; }

            set {
                if (value < Minimum) { value = Minimum; }
                if (_value > value) { _value = value; }

                _maximum = value;
            }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }

            set
            {
                if (value == _isReadOnly) { return; }

                TextBox1.IsReadOnly = value;
                _isReadOnly = value;
            }
        }

        #endregion

        #region Methods

        public NumberBox()
        {
            InitializeComponent();

            DataObject.AddPastingHandler(TextBox1, TextBoxNumber_OnPaste);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            TextBox1.Text = string.Empty;
        }

        private void TextBoxNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsInputValid(e.Text)) {
                e.Handled = true;
            }
        }

        private void TextBoxNumber_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.SourceDataObject.GetDataPresent(DataFormats.Text)) {
                if (IsInputValid(e.SourceDataObject.GetData(DataFormats.Text) as string)) {
                    return;
                }
            }

            e.CancelCommand();
        }

        private void TextBoxNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isTextChangingProgrammatically) {
                if (TextBox1.Text.Length != 0) {
                    uint parsedNum;
                    if (uint.TryParse(TextBox1.Text, out parsedNum)) {
                        if (parsedNum < Maximum) {
                            Value = parsedNum;
                        } else {
                            OverwriteValue(Maximum);
                        }

                    } else {
                        OverwriteValue(Maximum);
                    }

                } else {
                    Value = null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(NuintValueChangedEventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        private bool IsInputValid(string input)
        {
            if (!StringIsNumber(input) || Value == Maximum) {
                return false;
            }

            return true;
        }

        private static bool StringIsNumber(string input)
        {
            for (var i = input.Length - 1; i >= 0; i--) {
                var tmp = input[i];
                if (tmp < '0' || tmp > '9') {
                    return false;
                }
            }

            return true;
        }

        private void OverwriteValue(uint newValue, bool textOnly = false)
        {
            _isTextChangingProgrammatically = true;
            var selStart = TextBox1.SelectionStart;
            TextBox1.Text = newValue.ToString(Helper.InvariantCulture);
            TextBox1.SelectionStart = selStart;
            _isTextChangingProgrammatically = false;

            if (!textOnly) { Value = newValue; }
        }

        #endregion
    }

    public class NuintValueChangedEventArgs : EventArgs
    {
        public uint? NewValue { get; private set; }

        public NuintValueChangedEventArgs(uint? newValue)
        {
            NewValue = newValue;
        }
    }
}
