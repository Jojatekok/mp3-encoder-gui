using System.Runtime.CompilerServices;
using System.Windows;

namespace MP3EncoderGUI.UserControls
{
    public sealed partial class VbrQualitySlider
    {
        #region Declarations

        private byte _value;
        public byte Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _value; }

            set {
                if (value <= 9 && value != (byte)SliderVbrQuality.Value) {
                    SliderVbrQuality.Value = value;
                }
            }
        }

        #endregion

        #region Methods

        public VbrQualitySlider()
        {
            InitializeComponent();

            AdjustedQuality(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SliderVbrQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _value = (byte)e.NewValue;
            AdjustedQuality(Value);
        }

        private void AdjustedQuality(byte newValue)
        {
            string tmp;
            Dictionaries.VbrQualities.TryGetValue(newValue, out tmp);

            TextBlockVbrQuality.Text = newValue.ToString(Helper.InvariantCulture) + " (" + tmp + ")";
        }

        #endregion
    }
}
