using OpenCvSharp;

namespace VL.OpenCV
{
    public static class DefaultMat
    {
        public static readonly Mat Damon = new Mat(new Size(1, 1), MatType.CV_8UC3);
    }
}
