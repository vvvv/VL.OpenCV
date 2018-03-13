using OpenCvSharp;
using System.Collections.Generic;
using VL.Lib.Collections;

namespace VL.OpenCV
{
    public static class Calibration
    {
        /// <summary>
        /// Outputs error margin, intrinsics, extrinsics and the amount of images where the pattern was detected in a camera calibration process
        /// using a rectangular chessboard pattern
        /// </summary>
        /// <param name="images">A spread of Mat representing the images to be used as input during the camera calibration operation</param>
        /// <param name="rows">Amount of rows in the chessboard pattern</param>
        /// <param name="cols">Amount of columns in the chessboard pattern</param>
        /// <param name="intrinsics">Output intrinsics value, also called cameraMatrix</param>
        /// <param name="extrinsics">Output extrinsics value, also called distortion coefficients</param>
        /// <param name="foundPatternCount">Integer representing the amount of images where the chessboard patter was found</param>
        /// <returns></returns>
        public static double CalibrateCamera(Spread<Mat> images, int rows, int cols, out Mat intrinsics, out Mat extrinsics, out int foundPatternCount)
        {
            int imageCount = images.Count;
            int patternSizeInt = rows * cols;
            Size patternSize = new Size(cols, rows);
            float rectangleSize = 24.0f;

            //initialization of the base objectPoint IEnumerable
            var objectPoint = new List<Point3f>();
            for (int i = 0; i < patternSize.Height; i++)
            {
                for (int j = 0; j < patternSize.Width; j++)
                {
                    objectPoint.Add(new Point3f(j * rectangleSize, i * rectangleSize, 0.0F));
                }
            }

            Mat[] objectPoints = new Mat[imageCount];
            Mat[] imagePoints = new Mat[imageCount];
            foundPatternCount = 0;

            for (int i = 0; i < imageCount; i++)
            {
                Point2f[] corners;
                bool found = Cv2.FindChessboardCorners(images[i], patternSize, out corners, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage);
                if (found)
                {
                    //add object to objectPoints
                    objectPoints[i] = new Mat(1, patternSizeInt, OpenCvSharp.MatType.CV_32FC3, objectPoint.ToArray());
                    //add corners to allCorners
                    imagePoints[i] = new Mat(1, patternSizeInt, OpenCvSharp.MatType.CV_32FC2, corners);
                    foundPatternCount++;
                }
            }

            Mat cameraMatrix = new Mat(3, 3, OpenCvSharp.MatType.CV_64FC1);
            Mat distortion = new Mat(1, 4, OpenCvSharp.MatType.CV_64FC1);
            Mat[] rotations = new Mat[imageCount];
            Mat[] translations = new Mat[imageCount];

            double error = Cv2.CalibrateCamera(objectPoints, imagePoints, images[0].Size(), cameraMatrix, distortion, out rotations, out translations);

            /*Mat newMatrix = new Mat();
            for (int i = 0; i < imageCount; i++)
            {
                Rect ROI = new Rect();
                Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distortion, images[i].Size(), 1, images[i].Size(), out ROI);

                Mat dest = new Mat(images[i], ROI);
                Cv2.Undistort(images[i], dest, cameraMatrix, distortion);
            }*/

            intrinsics = cameraMatrix;
            extrinsics = distortion;
            return error;
        }
    }
}