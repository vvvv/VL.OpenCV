using OpenCvSharp;
using SharpDX;
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
                readonly Mat FMat;
                readonly ImageInfo FInfo;

                public Data(Mat mat, ImageInfo info)
                {
                    FMat = mat;
                    FInfo = info;
                }

                public IntPtr Pointer => FMat.Data;
                public int ScanSize => FInfo.ScanSize;
                public int Size => FInfo.ImageSize;
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

        public static IImage ToImage(this Mat input, bool takeOwnership)
        {
            if (input == DefaultMat.Damon)
                takeOwnership = false;
            var format = input.Type().ToPixelFormat();
            return new MatImage(input, format, takeOwnership);
        }

        public static unsafe Mat ToMat(this IImage input)
        {
            var info = input.Info;
            var type = info.Format.ToMatType(info.OriginalFormat);
            using (var srcData = input.GetData())
            {
                var size = srcData.Size;
                var dstData = new byte[size];
                var src = srcData.Pointer.ToPointer();
                fixed (byte* dst = dstData)
                    Unsafe.CopyBlock(dst, src, (uint)size);
                return new Mat(info.Height, info.Width, type, dstData, srcData.ScanSize);
            }
        }

        public static unsafe void ToMat(this IImage input, Mat dstMat)
        {
            var info = input.Info;
            var type = info.Format.ToMatType(info.OriginalFormat);
            using (var srcData = input.GetData())
            {
                var size = srcData.Size;
                var src = srcData.Pointer.ToPointer();
                dstMat.Create(info.Height, info.Width, type);
                Unsafe.CopyBlock(dstMat.DataPointer, src, (uint)size);
            }
        }

        public static PixelFormat ToPixelFormat(this OpenCvSharp.MatType type)
        {
            switch (type.Depth)
            {
                case OpenCvSharp.MatType.CV_8U:
                    switch (type.Channels)
                    {
                        case 1:
                            return PixelFormat.R8;
                        case 4:
                            return PixelFormat.B8G8R8A8;
                    }
                    break;
                case OpenCvSharp.MatType.CV_32F:
                    switch (type.Channels)
                    {
                        case 1:
                            return PixelFormat.R32F;
                    }
                    break;
            }
            throw new UnsupportedMatTypeException(type);
        }

        public static OpenCvSharp.MatType ToMatType(this PixelFormat format, string originalFormat)
        {
            switch (format)
            {
                case PixelFormat.R8:
                    return OpenCvSharp.MatType.CV_8UC1;
                case PixelFormat.R32F:
                    return OpenCvSharp.MatType.CV_32FC1;
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
        private static Matrix ToTransformationMatrix(Mat matrix, Mat translationVector, float[] specials)
        {
            Matrix result = Matrix.Identity;
            if (matrix != null && translationVector != null)
            {
                if ((matrix.Width == 3 && matrix.Height == 3)
                    && (translationVector.Width == 1 && translationVector.Height == 3))
                {
                    Mat rm = matrix.Clone();
                    Mat tv = translationVector.Clone();
                    rm.ConvertTo(rm, OpenCvSharp.MatType.CV_32FC1);
                    tv.ConvertTo(tv, OpenCvSharp.MatType.CV_32FC1);
                    result = new Matrix(
                        rm.At<float>(0, 0),
                        rm.At<float>(1, 0),
                        rm.At<float>(2, 0),
                        specials[0],
                        rm.At<float>(0, 1),
                        rm.At<float>(1, 1),
                        rm.At<float>(2, 1),
                        specials[1],
                        rm.At<float>(0, 2),
                        rm.At<float>(1, 2),
                        rm.At<float>(2, 2),
                        specials[2],
                        tv.At<float>(0),
                        tv.At<float>(1),
                        tv.At<float>(2),
                        specials[3]);
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
            Mat rotationMatrix = new Mat();
            Cv2.Rodrigues(rotationVector, rotationMatrix);
            float[] specials = new float[4] { 0, 0, 0, 1 };
            return ToTransformationMatrix(rotationMatrix, translationVector, specials);
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
            float[] specials = new float[4] { 0, 0, 1, 0 };
            return ToTransformationMatrix(cameraMatrix, translationVector, specials);
        }

        #endregion Transformation related code
    }
}
