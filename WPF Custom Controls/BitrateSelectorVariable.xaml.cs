using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace WpfCustomControls
{
    public sealed partial class BitrateSelectorVariable
    {
        #region Declarations

        private static readonly List<ushort> ValidValues = (List<ushort>)LameEncoderInterface.MP3Types.All.Bitrates;

        private ushort _minValue;
        public ushort MinValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _minValue; }
            
            set {
                if (value != _minValue && ValidValues.Contains(value)) {
                    ComboBoxMinimum.SelectedItem = value;
                }
            }
        }

        private ushort _maxValue;
        public ushort MaxValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _maxValue; }
            
            set {
                if (value != _maxValue && ValidValues.Contains(value)) {
                    ComboBoxMaximum.SelectedItem = value;
                }
            }
        }

        #endregion

        #region Methods

        public BitrateSelectorVariable()
        {
            InitializeComponent();

            for (var i = 0; i < ValidValues.Count; i++) {
                ComboBoxMinimum.Items.Insert(0, ValidValues[i]);
                ComboBoxMaximum.Items.Add(ValidValues[i]);
            }
            ComboBoxMinimum.SelectedIndex = 0;
            ComboBoxMaximum.SelectedIndex = 0;
        }

        private void ComboBoxMinimum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _minValue = (ushort)ComboBoxMinimum.SelectedItem;
            DisplayMaxValues(MinValue);
        }

        private void ComboBoxMaximum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _maxValue = (ushort)ComboBoxMaximum.SelectedItem;
            DisplayMinValues(MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisplayMinValues(ushort before)
        {
            var tryAdd = true;

            for (var i = ComboBoxMinimum.Items.Count - 1; i >= 0; i--) {
                if ((ushort)ComboBoxMinimum.Items[i] >= before) {
                    ComboBoxMinimum.Items.RemoveAt(i);
                    tryAdd = false;

                } else {
                    break;
                }
            }

            if (!tryAdd) return;

            var tmp = ValidValues.Count - 1;
            for (var i = ComboBoxMinimum.Items.Count; i < tmp - ValidValues.IndexOf(before); i++) {
                ComboBoxMinimum.Items.Add(ValidValues[tmp - i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisplayMaxValues(ushort after)
        {
            var tryAdd = true;

            for (var i = ComboBoxMaximum.Items.Count - 1; i >= 0; i--) {
                if ((ushort)ComboBoxMaximum.Items[i] <= after) {
                    ComboBoxMaximum.Items.RemoveAt(i);
                    tryAdd = false;

                } else {
                    break;
                }
            }

            if (!tryAdd) return;

            for (var i = ComboBoxMaximum.Items.Count; i < ValidValues.IndexOf(after); i++) {
                ComboBoxMaximum.Items.Add(ValidValues[i]);
            }
        }

        #endregion
    }
}
