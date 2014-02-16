using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WpfCustomControls
{
    public sealed partial class QualitySlider
    {
        #region Declarations

        private byte _value;
        public byte Value
        {
            get { return _value; }

            set {
                if (value != (byte)Slider1.Value || value >= Minimum && value <= Maximum) {
                    Slider1.Value = value;
                }
            }
        }

        private byte _minimum;
        public byte Minimum
        {
            get { return _minimum; }

            set {
                if (value == Minimum) return;

                if (value > Maximum) {
                    value = Maximum;
                }

                _minimum = value;
                Slider1.Minimum = value;
                if (Value < value) {
                    Value = value;
                }

                RecalculateSeparations();
            }
        }

        private byte _maximum;
        public byte Maximum
        {
            get { return _maximum; }

            set {
                if (value == Maximum) return;

                if (value < Minimum) {
                    value = Minimum;
                }

                _maximum = value;
                Slider1.Maximum = value;
                if (Value > value) {
                    Value = value;
                }

                RecalculateSeparations();
            }
        }

        private byte _separatorMediumFrom;
        private byte _separatorHighFrom;

        #endregion

        #region Methods

        public QualitySlider()
        {
            InitializeComponent();
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _value = (byte)e.NewValue;
            AdjustedQuality();
        }

        private void AdjustedQuality()
        {
            string text;

            if (Value < _separatorMediumFrom) {
                if (Value == Minimum) {
                    text = "Highest, biggest file";
                } else {
                    text = "High";
                }

            } else if (Value < _separatorHighFrom) {
                text = "Medium";

            } else {
                if (Value == Maximum) {
                    text = "Lowest, smallest file";
                } else {
                    text = "Low";
                }
            }

            TextBlock1.Text = Value.ToString(Helper.InvariantCulture) + " (" + text + ")";
        }

        private void RecalculateSeparations()
        {
            var sectionSeparator = (Maximum - Minimum + 1) / 3D;

            _separatorMediumFrom = (byte)Math.Round(sectionSeparator);
            _separatorHighFrom = (byte)Math.Round(sectionSeparator * 2D);

            AdjustedQuality();
        }

        #endregion
    }
}
