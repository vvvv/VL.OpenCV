using OpenCvSharp;
using System;

namespace VL.OpenCV
{
    public class CvImage
    {
        public static readonly CvImage Damon = new CvImage(new UMat(1, 1, OpenCvSharp.MatType.CV_8UC3, Scalar.Gray));
        private static readonly CvImage Gray = new CvImage(new UMat(1, 1, OpenCvSharp.MatType.CV_8UC1, Scalar.Gray));

        private readonly InputArray _inputArray;
        public CvImage(InputArray inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException("Mat cannot be null.");
            _inputArray = inputArray;
        }

        public InputArray InputArray => _inputArray;

        public int Width => _inputArray.Cols();

        public int Height => _inputArray.Rows();

        public int Rows => _inputArray.Rows();

        public int Cols => _inputArray.Cols();

        public int Channels => _inputArray.Channels();

        public CvImage Clone()
        {
            if (_inputArray.IsMat())
            {
                var copy = new Mat();
                _inputArray.CopyTo(copy);
                return new CvImage(copy);
            }
            else if (_inputArray.IsUMat())
            {
                var copy = new UMat();
                _inputArray.CopyTo(copy);
                return new CvImage(copy);
            }
            return null;
        }

        public void CopyTo(CvImage image)
        {
            OutputArray outputArray;
            if (image.InputArray.IsMat())
            {
                outputArray = image.InputArray.GetMat();
                CopyTo(outputArray);
            }
            else if (image.InputArray.IsUMat())
            {
                outputArray = image.InputArray.GetUMat();
                CopyTo(outputArray);
            }
        }

        public void CopyTo(OutputArray outputArray)
        {
            _inputArray.CopyTo(outputArray);
        }

        public static CvImage EnforceGrayDefault(CvImage img) => img != Damon ? img : Gray;
    }
}
