using OpenCvSharp;

namespace VL.OpenCV
{
    public static class DefaultMat
    {
        public static readonly Mat Damon = new Mat(new int[] { 1, 1 }, MatType.CV_8UC3, new byte[] { 0xFF, 0xFF, 0xFF });
    }
}
