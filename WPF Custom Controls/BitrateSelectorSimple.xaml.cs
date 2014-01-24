using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using LameEncoderInterface;

namespace WpfCustomControls
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

            ValidValues = LameEncoderInterface.MP3Types.All.Bitrates;
            ComboBox1.SelectedIndex = 0;
        }

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

        public Mp3Type GetAvailableMp3Types()
        {
            Mp3Type output = 0;

            if (LameEncoderInterface.MP3Types.Mpeg10.Bitrates.Contains(Value)) {
                output |= Mp3Type.Mpeg10;
            }

            if (LameEncoderInterface.MP3Types.Mpeg20.Bitrates.Contains(Value)) {
                output |= Mp3Type.Mpeg20;
            }

            if (LameEncoderInterface.MP3Types.Mpeg25.Bitrates.Contains(Value)) {
                output |= Mp3Type.Mpeg25;
            }

            return output;
        }

        public void UpdateValidValues(Mp3Type newMp3Types)
        {
            ValidValues = LameEncoderInterface.MP3Types.Any.GetAvailableBitrates(newMp3Types);
        }

        #endregion
    }
}
