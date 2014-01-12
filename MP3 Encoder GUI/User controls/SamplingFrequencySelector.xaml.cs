using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MP3EncoderGUI.UserControls
{
    public sealed partial class SamplingFrequencySelector
    {
        #region Events

        public event EventHandler<UshortValueChangedEventArgs> ValueChanged;

        #endregion

        #region Declarations

        private IList<ushort> _validValues;
        private IList<ushort> ValidValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _validValues; }

            set {
                if (!Equals(value, _validValues)) {
                    _validValues = value;

                    var tmp = ComboBox1.SelectedItem;

                    var tmp2 = ComboBox1.Items.Count;
                    for (var i = 1; i < tmp2; i++) {
                        ComboBox1.Items.RemoveAt(1);
                    }

                    for (var i = 1; i < value.Count; i++) {
                        ComboBox1.Items.Add(value[i]);
                    }

                    if (ComboBox1.Items.Contains(tmp)) {
                        ComboBox1.SelectedItem = tmp;
                    }
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

        public SamplingFrequencySelector()
        {
            InitializeComponent();

            ComboBox1.Items.Add("Automatic");
            ComboBox1.SelectedIndex = 0;
            ComboBox1.ItemStringFormat = "## ###";

            ValidValues = MP3Types.All.SamplingFrequencies;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ComboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBox1.SelectedItem == null) {
                ComboBox1.SelectedIndex = 0;
                return;
            }

            var tmp = ComboBox1.SelectedItem as string;

            if (tmp != null) {
                _value = 0; // Set value to 'Automatic'
            } else {
                _value = (ushort)ComboBox1.SelectedItem;
            }

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
            if (Value == 0) {
                return Mp3Types.All;
            }

            if (MP3Types.Mpeg10.SamplingFrequencies.Contains(Value)) {
                return Mp3Types.Mpeg10;
            }

            if (MP3Types.Mpeg20.SamplingFrequencies.Contains(Value)) {
                return Mp3Types.Mpeg20;
            }

            //if (MP3Types.Mpeg25.SamplingFrequencies.Contains(Value)) {
            return Mp3Types.Mpeg25;
            //}
        }

        public void UpdateValidValues(Mp3Types newMp3Types)
        {
            ValidValues = MP3Types.Any.GetAvailableSamplingFrequencies(newMp3Types);
        }

        #endregion
    }
}
