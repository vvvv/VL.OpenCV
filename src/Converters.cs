using OpenCvSharp;
using Xenko.Core.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VL.Lib.Basics.Imaging;

namespace VL.OpenCV
{
    public static class Converters
    {
        class MatImage : IImage
        {
            class Data : IImageData
            {
                public Data(Mat mat, ImageInfo info)
                {
                    Pointer = mat.Data;
                    ScanSize = (int)mat.Step();
                    Size = info.ImageSize;
                }

                public IntPtr Pointer { get; }
                public int ScanSize { get; }
                public int Size { get; }
                public void Dispose() { }
            }

            readonly Mat FMat;
            readonly PixelFormat FFormat;
            readonly bool FIsOwner;

            public MatImage(Mat mat, PixelFormat format, bool isOwner)
            {
                FMat = mat;
                FFormat = format;
                FIsOwner = isOwner;
            }

            public ImageInfo Info => new ImageInfo(FMat.Width, FMat.Height, FFormat);

            public bool IsVolatile => !FIsOwner;

            public void Dispose()
            {
                if (FIsOwner)
                    FMat.Dispose();
            }

            public IImageData GetData() => new Data(FMat, Info);
        }

        public static IImage ToImage(this CvImage input, PixelFormat pixelFormat, bool takeOwnership)
        {
            if (input == CvImage.Damon)
                takeOwnership = false;
            return new MatImage(input.Mat, pixelFormat, takeOwnership);
        }

        public static unsafe Mat ToMat(this IImage input)
        {
            var info = input.Info;
            var type = info.Format.ToMatType(info.OriginalFormat);
            using (var srcData = input.GetData())
            {
                var dstData = new byte[srcData.Size];
                fixed (byte* dst = dstData)
                    ImageExtensions.CopyTo(srcData, info, dst);
                return new Mat(info.Height, info.Width, type, dstData, srcData.ScanSize);
            }
        }

        public static unsafe void ToMat(this IImage input, Mat dstMat)
        {
            var info = input.Info;
            var type = info.Format.ToMatType(info.OriginalFormat);
            using (var srcData = input.GetData())
            {
                dstMat.Create(info.Height, info.Width, type);
                ImageExtensions.CopyTo(srcData, info, (byte*)dstMat.Data.ToPointer());
            }
        }

        public static OpenCvSharp.MatType ToMatType(this PixelFormat format, string originalFormat)
        {
            switch (format)
            {
                case PixelFormat.R8:
                    return OpenCvSharp.MatType.CV_8UC1;
                case PixelFormat.R16:
                    return OpenCvSharp.MatType.CV_16UC1;
                case PixelFormat.R32F:
                    return OpenCvSharp.MatType.CV_32FC1;
                case PixelFormat.R8G8B8:
                case PixelFormat.B8G8R8:
                    return OpenCvSharp.MatType.CV_8UC3;
                case PixelFormat.B8G8R8A8:
                case PixelFormat.B8G8R8X8:
                case PixelFormat.R8G8B8A8:
                case PixelFormat.R8G8B8X8:
                    return OpenCvSharp.MatType.CV_8UC4;
                case PixelFormat.Unknown:
                    if (originalFormat.Equals("bgr", StringComparison.OrdinalIgnoreCase)) // HACK
                        return OpenCvSharp.MatType.CV_8UC3;
                    break;
            }
            throw new UnsupportedPixelFormatException(format);
        }

        /// <summary>
        /// Wraps the Cv2.CvtColor in order to simplify the enums presented to the end user.
        /// </summary>
        /// <param name="input">Orignal image</param>
        /// <param name="output">Destination image</param>
        /// <param name="sourceCode">ColorConversionSourceCodes corresponding to the original image</param>
        /// <param name="targetCode">ColorConversionTargetCodes in which the resulting image should be</param>
        /// <param name="channels"></param>
        public static void ConvertColor(InputArray input, OutputArray output, ColorConversionSourceCodes sourceCode, ColorConversionTargetCodes targetCode, int channels)
        {
            var code = sourceCode + "2" + targetCode;
            ColorConversionCodes result;
            if (Enum.TryParse(code, out result))
                Cv2.CvtColor(input, output, result, channels);
            else
                throw new Exception("Specified conversion code does not exist: " + code);
        }

        #region Transformation related code

        ////// <summary>
        /// Combines a 3x3 matrix with a 1x3 translation vector into a 4x4 transformation matrix
        /// </summary>
        /// <param name="matrix">3x3 matrix</param>
        /// <param name="translationVector">1x3 translation vector</param>
        /// <param name="specials">Set of values to be appended to the last column of the resulting matrix</param>
        /// <returns>4x4 transformation matrix in the correct order for vvvv</returns>
        private static Matrix ToTransformationMatrix(Mat matrix, Mat translationVector, FourthColumn fourthColumn)
        {
            Matrix result = Matrix.Identity;
            if (matrix != null && translationVector != null)
            {
                if ((matrix.Width == 3 && matrix.Height == 3)
                    && (translationVector.Width == 1 && translationVector.Height == 3))
                {
                    var fourthColumnArray = fourthColumn == FourthColumn.Standard ? new float[4] { 0, 0, 0, 1 } : new float[4] { 0, 0, 1, 0 };
                    Mat rm = matrix.Clone();
                    Mat tv = translationVector.Clone();
                    rm.ConvertTo(rm, OpenCvSharp.MatType.CV_32FC1);
                    tv.ConvertTo(tv, OpenCvSharp.MatType.CV_32FC1);

                    // we need to transpose our 3x3 matrix here

                    result = new Matrix(
                        rm.At<float>(0, 0),   //11
                        rm.At<float>(1, 0),   //12
                        rm.At<float>(2, 0),   //13
                        fourthColumnArray[0], //14
                        rm.At<float>(0, 1),   //21
                        rm.At<float>(1, 1),   //22
                        rm.At<float>(2, 1),   //23
                        fourthColumnArray[1], //24
                        rm.At<float>(0, 2),   //31
                        rm.At<float>(1, 2),   //32
                        rm.At<float>(2, 2),   //33
                        fourthColumnArray[2], //34
                        tv.At<float>(0),      //41
                        tv.At<float>(1),      //42
                        tv.At<float>(2),      //43
                        fourthColumnArray[3]);//44
                }
            }
            return result;
        }

        /// <summary>
        /// Combines a 1x3 rotation vector with a 1x3 translation vector into a 4x4 transformation matrix
        /// </summary>
        /// <param name="rotationVector">1x3 rotation vector</param>
        /// <param name="translationVector">1x3 translation vector</param>
        /// <returns>4x4 transformation matrix in the correct order for vvvv</returns>
        public static Matrix VectorsToMatrix(Mat rotationVector, Mat translationVector)
        {
            Matrix result = Matrix.Identity;
            if ((rotationVector != null && translationVector != null)
                && (CvImage.Damon.Mat != rotationVector && CvImage.Damon.Mat != rotationVector))
            {
                Mat rotationMatrix = new Mat();
                Cv2.Rodrigues(rotationVector, rotationMatrix);
                result = ToTransformationMatrix(rotationMatrix, translationVector, FourthColumn.Standard);
            }
            return result;
        }

        /// <summary>
        /// Splits a transformation matrix into its corresponding rotation and translation vectors as Mats
        /// </summary>
        /// <param name="transform">Input 4x4 transformation matrix</param>
        /// <param name="rotationVector">1x3 output rotation vector as a Mat</param>
        /// <param name="translationVector">1x3 output translation vector as a Mat</param>
        public static void MatrixToVectors(Matrix transform, out Mat rotationVector, out Mat translationVector)
        {
            //rmatrix
            // 11  21  31
            // 12  22  32
            // 13  23  33
            // we need to transpose our 3x3 matrix here
            double[,] rmatrix = new double[3, 3];
            rmatrix[0, 0] = transform.M11;
            rmatrix[0, 1] = transform.M21;
            rmatrix[0, 2] = transform.M31;
            rmatrix[1, 0] = transform.M12;
            rmatrix[1, 1] = transform.M22;
            rmatrix[1, 2] = transform.M32;
            rmatrix[2, 0] = transform.M13;
            rmatrix[2, 1] = transform.M23;
            rmatrix[2, 2] = transform.M33;
            double[] rvecArray = new double[3];
            Cv2.Rodrigues(rmatrix, out rvecArray);
            rotationVector = new Mat(3, 1, OpenCvSharp.MatType.CV_64FC1, rvecArray);

            //tvec
            //transform.M41
            //transform.M42
            //transform.M43
            double[] tvecArray = new double[] { transform.M41, transform.M42, transform.M43 };
            translationVector = new Mat(3, 1, OpenCvSharp.MatType.CV_64FC1, tvecArray);
        }

        /// <summary>
        /// Converts a 3x3 camera matrix into a 4x4 transformation matrix
        /// </summary>
        /// <param name="cameraMatrix">3x3 camera matrix</param>
        /// <returns>4x4 transformation matrix in the correct order for vvvv</returns>
        public static Matrix ToTransformationMatrix(Mat cameraMatrix)
        {
            Mat translationVector = Mat.Zeros(3, 1, OpenCvSharp.MatType.CV_64FC1);
            return ToTransformationMatrix(cameraMatrix, translationVector, FourthColumn.Projection);
        }

        enum FourthColumn
        {
            Standard,
            Projection
        }

        #endregion Transformation related code
    }
}
