using System;
using System.Drawing;

namespace VL.OpenCV
{
    class DIPHelpers
    {
        public static int DIPToPixel(int dip)
        {
            return (int)(dip * DIPFactor());
        }

        public static Point DIPToPixel(Point dip)
        {
            return new Point(DIPToPixel(dip.X), DIPToPixel(dip.Y));
        }

        public static Size DIPToPixel(Size pixel)
        {
            return new Size(DIPToPixel(pixel.Width), DIPToPixel(pixel.Height));
        }

        public static Rectangle DIPToPixel(Rectangle pixel)
        {
            return new Rectangle(DIPToPixel(pixel.Location), DIPToPixel(pixel.Size));
        }

        public static int DIP(int pixel)
        {
            return (int)(pixel / DIPFactor());
        }

        public static Point DIP(Point pixel)
        {
            return new Point((int)(pixel.X / DIPFactor()), (int)(pixel.Y / DIPFactor()));
        }

        public static Size DIP(Size pixel)
        {
            return new Size(DIP(pixel.Width), DIP(pixel.Height));
        }

        public static Rectangle DIP(Rectangle pixel)
        {
            return new Rectangle(DIP(pixel.Location), DIP(pixel.Size));
        }

        static float FDIPFactor = -1;
        public static float DIPFactor()
        {
            if (FDIPFactor == -1)
            {
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    FDIPFactor = g.DpiX / 96;
                }
            }

            return FDIPFactor;
        }
    }
}
