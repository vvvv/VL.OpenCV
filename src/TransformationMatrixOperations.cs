using OpenCvSharp;
using SharpDX;

namespace VL.OpenCV
{
    public class TransformationMatrixOperations{
        /// <summary>
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
        public static Matrix Join(Mat rotationVector, Mat translationVector)
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
        public static void Split(Matrix transform, out Mat rotationVector, out Mat translationVector)
        {
            //rmatrix
            // 11  21  31
            // 12  22  32
            // 13  23  33
            double [,] rmatrix = new double[3,3];
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
    }
}