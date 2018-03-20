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

        /// <summary>
        /// Combines a 3x3 rotation matrix output by Rodrigues with a 1x3 translation vector into a 4x4 Transformation matrix
        /// </summary>
        /// <param name="rotationMatrix">3x3 rotation matrix resulting from a call to Rodrigues</param>
        /// <param name="translationVector">1x3 translation vector</param>
        /// <param name="specials">Set of values to be appended to the last column of the resulting matrix</param>
        /// <returns>4x4 Transformation matrix in the correct order for vvvv</returns>
        public unsafe static Matrix ToTransformationMatrix(Mat rotationMatrix, Mat translationVector, float[] specials)
        {
            Matrix result = new Matrix();
            float[] matrix = new float[16];
            if (rotationMatrix != null && translationVector != null)
            {
                if ((rotationMatrix.Width == 3 && rotationMatrix.Height == 3) 
                    && (translationVector.Width == 1 && translationVector.Height == 3))
                {
                    //clone matrix and vector to avoid modifying the originals
                    Mat rm = rotationMatrix.Clone();
                    Mat tv = translationVector.Clone();
                    rm.ConvertTo(rm, OpenCvSharp.MatType.CV_32FC1);
                    tv.ConvertTo(tv, OpenCvSharp.MatType.CV_32FC1);
                    //transpose to change into VL's row,col format
                    rm = rm.Transpose();
                    //copy translation vector at the end of the rotation matrix
                    rm.Add(tv.Transpose()); //4x3 matrix
                    rm = rm.Transpose();
                    //Add new row with all 0's
                    rm.Add(Mat.Zeros(1, 4, OpenCvSharp.MatType.CV_32FC1)); //4x4 matrix

                    //Perform an unsafe copy of the Mat data into the array
                    IntPtr pointer = rm.Data;
                    var src = pointer.ToPointer();
                    fixed (float* dst = matrix)
                        Unsafe.CopyBlock(dst, src, (uint)sizeof(float)*16);

                    //set values in last row of the matrix (currently all 0)
                    matrix[12] = specials[0];
                    matrix[13] = specials[1];
                    matrix[14] = specials[2];
                    matrix[15] = specials[3];

                    //create new Matrix based on the array
                    result = new Matrix(matrix);
                    result.Transpose();
                }
            }
            return result;
        }

        /// <summary>
        /// Combines a 3x3 rotation matrix output by Rodrigues with a 1x3 translation vector into a 4x4 Transformation matrix
        /// </summary>
        /// <param name="rotationMatrix">3x3 rotation matrix resulting from a call to Rodrigues</param>
        /// <param name="translationVector">1x3 translation vector</param>
        /// <returns>4x4 Transformation matrix in the correct order for vvvv</returns>
        public unsafe static Matrix ToTransformationMatrix(Mat rotationMatrix, Mat translationVector)
        {
            float[] specials = new float[4] { 0, 0, 0, 1 };
            return ToTransformationMatrix(rotationMatrix, translationVector, specials);
        }

        /// <summary>
        /// Converts a 3x3 rotation matrix output by Rodrigues into a 4x4 Transformation matrix
        /// </summary>
        /// <param name="rotationMatrix">3x3 rotation matrix resulting from a call to Rodrigues</param>
        /// <returns>4x4 Transformation matrix in the correct order for vvvv</returns>
        public unsafe static Matrix ToTransformationMatrix(Mat rotationMatrix)
        {
            Mat translationVector = Mat.Zeros(3, 1, OpenCvSharp.MatType.CV_64FC1);
            float[] specials = new float[4] { 0, 0, 1, 0 };
            return ToTransformationMatrix(rotationMatrix, translationVector, specials);
        }
    }
}
