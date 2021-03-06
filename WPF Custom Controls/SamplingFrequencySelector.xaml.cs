﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LameEncoderInterface;

namespace WpfCustomControls
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

            ValidValues = LameEncoderInterface.MP3Types.Any.SamplingFrequencies;
        }

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

        public Mp3Type GetAvailableMp3Types()
        {
            if (Value == 0) {
                return Mp3Type.Mpeg10 | Mp3Type.Mpeg20 | Mp3Type.Mpeg25;
            }

            if (LameEncoderInterface.MP3Types.Mpeg10.SamplingFrequencies.Contains(Value)) {
                return Mp3Type.Mpeg10;
            }

            if (LameEncoderInterface.MP3Types.Mpeg20.SamplingFrequencies.Contains(Value)) {
                return Mp3Type.Mpeg20;
            }

            //if (LameEncoderInterface.MP3Types.Mpeg25.SamplingFrequencies.Contains(Value)) {
            return Mp3Type.Mpeg25;
            //}
        }

        public void UpdateValidValues(Mp3Type newMp3Types)
        {
            ValidValues = LameEncoderInterface.MP3Types.Any.GetAvailableSamplingFrequencies(newMp3Types);
        }

        #endregion
    }
}
