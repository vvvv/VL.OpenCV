using OpenCvSharp;

namespace VL.OpenCV
{
    public static class Utils
    {
        public static bool FindChessboardCorners(
            InputArray image,
            Size patternSize,
            out Point2f[] corners,
            ChessboardFlags flags = ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage)
        {
            return Cv2.FindChessboardCorners(image, patternSize, out corners, flags);
        }
    }
}
