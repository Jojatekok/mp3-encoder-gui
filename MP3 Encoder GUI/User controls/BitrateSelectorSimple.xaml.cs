using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace MP3EncoderGUI.UserControls
{
    public sealed partial class BitrateSelectorSimple
    {
        #region Events

        public event EventHandler<UshortValueChangedEventArgs> ValueChanged;

        #endregion

        #region Declarations

        private static IList<ushort> _validValues;
        private IList<ushort> ValidValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _validValues; }

            set {
                if (!Equals(value, _validValues)) {
                    _validValues = value;
                    ComboBox1.ItemsSource = _validValues;
                }
            }
        }

        private ushort _value;
        public ushort Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _value; }
            
            set {
                if (value != _value && ValidValues.Contains(value)) {
                    ComboBox1.SelectedItem = value;
                }
            }
        }

        #endregion

        #region Methods

        public BitrateSelectorSimple()
        {
            InitializeComponent();

            ValidValues = MP3Types.All.Bitrates;
            ComboBox1.SelectedIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox1.SelectedItem == null) {
                ComboBox1.SelectedIndex = 0;
                return;
            }

            _value = (ushort)ComboBox1.SelectedItem;
            OnValueChanged(new UshortValueChangedEventArgs(Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(UshortValueChangedEventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        public Mp3Types GetAvailableMp3Types()
        {
            Mp3Types output = 0;

            if (MP3Types.Mpeg10.Bitrates.Contains(Value)) {
                output |= Mp3Types.Mpeg10;
            }

            if (MP3Types.Mpeg20.Bitrates.Contains(Value)) {
                output |= Mp3Types.Mpeg20;
            }

            if (MP3Types.Mpeg25.Bitrates.Contains(Value)) {
                output |= Mp3Types.Mpeg25;
            }

            return output;
        }

        public void UpdateValidValues(Mp3Types newMp3Types)
        {
            var tmp = MP3Types.Any.GetAvailableBitrates(newMp3Types);
            ValidValues = tmp;
        }

        #endregion
    }
}
