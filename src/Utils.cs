using OpenCvSharp;

namespace VL.OpenCV
{
    public static class Utils
    {
        public static Mat Crop(Mat input, OpenCvSharp.Rect ROI)
        {
            Mat result = new Mat(input, ROI);
            return result;
        }
    }
}
