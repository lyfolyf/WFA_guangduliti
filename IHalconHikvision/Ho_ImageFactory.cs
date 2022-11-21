using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging;

namespace IHalconHikvision
{
    class Ho_ImageFactory
    {
        public static void Createho_Image(out HObject bitmap, byte[] buffer, int width, int height,bool color)
        {
            try
            {
                if (color)
                {
                    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                    HOperatorSet.GenImageInterleaved(out bitmap, ptr, "rgb", width, height, -1, "byte", 0, 0, 0, 0, -1, 0);
                }
                else
                {
                    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                    HOperatorSet.GenImage1(out bitmap, "byte", width, height, ptr);
                }
            }
            catch
            {
                bitmap = null;
            }
        }
        public static void Createho_Image(out Bitmap bitmap, byte[] buffer, int width, int height, bool color)
        {
            try
            {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                Bitmap bmp = new Bitmap(width, height, width, PixelFormat.Format8bppIndexed, ptr);

                ColorPalette cp = bmp.Palette;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                // set palette back
                bmp.Palette = cp;
                bitmap = bmp;
            }
            catch
            {
                bitmap = null;
            }
        }


        public static void Createho_24Image(out Bitmap bitmap, byte[] buffer, int width, int height, bool color)
        {
            try
            {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                Bitmap bmp = new Bitmap(width, height, width*3, PixelFormat.Format24bppRgb, ptr);
                ColorPalette cp = bmp.Palette;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                // set palette back
                bmp.Palette = cp;
                bitmap = bmp;
            }
            catch
            {
                bitmap = null;
            }
        }
    }
}
