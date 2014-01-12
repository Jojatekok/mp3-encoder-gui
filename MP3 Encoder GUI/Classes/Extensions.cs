using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MP3EncoderGUI
{
    public static class Mp3TypesExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this Mp3Types value, Mp3Types flag)
        {
            return (value & flag) == flag;
        }
    }

    internal static class IconExtension
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(this Icon icon)
        {
            using (var bitmap = icon.ToBitmap()) {
                var hBitmap = bitmap.GetHbitmap();

                ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );

                if (!DeleteObject(hBitmap)) {
                    throw new Win32Exception();
                }

                return wpfBitmap;
            }
        }
    }
}
