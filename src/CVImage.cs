using OpenCvSharp;
using System;

namespace VL.OpenCV
{
    public class CvImage
    {
        public static readonly CvImage Damon = new CvImage(new Mat(new int[] { 1, 1 }, OpenCvSharp.MatType.CV_8UC3, Scalar.Gray), true);

        private readonly Mat _image;
        private readonly bool _readOnly;
        private static readonly CvImage Gray = new CvImage(new Mat(new int[] { 1, 1 }, OpenCvSharp.MatType.CV_8UC1, Scalar.Gray));

        public CvImage(Mat mat)
            : this(mat, false)
        {
        }

        private CvImage(Mat mat, bool readOnly)
        {
            if (mat == null)
            {
                throw new ArgumentNullException("Mat cannot be null.");
            }

            _image = mat;
            _readOnly = readOnly;
        }

        public Mat Mat => _image;

        public int Width => _image.Width;

        public int Height => _image.Height;

        public int Rows => _image.Rows;

        public int Cols => _image.Cols;

        public int Channels => _image.Channels();

        public InputArray GetInputArray()
        {
            return _image;
        }

        public InputOutputArray GetInputOutputArray()
        {
            if (_readOnly)
            {
                throw new InvalidOperationException("Cannot generate InputOutputArray for readonly Mat.");
            }

            return _image;
        }

        public OutputArray GetOutputArray()
        {
            if (_readOnly)
            {
                throw new InvalidOperationException("Cannot generate OutputArray for readonly Mat.");
            }

            return _image;
        }

        public static CvImage EnforceGrayDefault(CvImage img)
        {
            return img != Damon ? img : Gray;
        }
    }
}
