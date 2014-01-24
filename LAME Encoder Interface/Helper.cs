using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LameEncoderInterface
{
    internal static class Helper
    {
        internal static readonly string DefaultLamePath = AppDomain.CurrentDomain.BaseDirectory + @"lame\lame.exe";
        internal static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    }

    public static class Mp3TypesExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Mp3Type value, Mp3Type flag)
        {
            return (value & flag) == flag;
        }
    }
}
