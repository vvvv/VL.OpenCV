using System;
using System.Drawing;

namespace VL.OpenCV
{
    class DIPHelpers
    {
        static public int DIPToPixel(int dip)
        {
            return (int)(dip * DIPFactor());
        }

        static public Point DIPToPixel(Point dip)
        {
            return new Point(DIPToPixel(dip.X), DIPToPixel(dip.Y));
        }

        static public Size DIPToPixel(Size pixel)
        {
            return new Size(DIPToPixel(pixel.Width), DIPToPixel(pixel.Height));
        }

        static public Rectangle DIPToPixel(Rectangle pixel)
        {
            return new Rectangle(DIPToPixel(pixel.Location), DIPToPixel(pixel.Size));
        }

        static public int DIP(int pixel)
        {
            return (int)(pixel / DIPFactor());
        }

        static public Point DIP(Point pixel)
        {
            return new Point((int)(pixel.X / DIPFactor()), (int)(pixel.Y / DIPFactor()));
        }

        static public Size DIP(Size pixel)
        {
            return new Size(DIP(pixel.Width), DIP(pixel.Height));
        }

        static public Rectangle DIP(Rectangle pixel)
        {
            return new Rectangle(DIP(pixel.Location), DIP(pixel.Size));
        }

        static float FDIPFactor = -1;
        static public float DIPFactor()
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
