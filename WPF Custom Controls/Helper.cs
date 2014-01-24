using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace WpfCustomControls
{
    internal static class Helper
    {
        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        internal static CultureInfo InvariantCulture {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _invariantCulture; }
        }
    }

    public class UshortValueChangedEventArgs : EventArgs
    {
        public ushort NewValue { get; private set; }

        public UshortValueChangedEventArgs(ushort newValue)
        {
            NewValue = newValue;
        }
    }
}
