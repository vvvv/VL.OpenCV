namespace VL.OpenCV
{
    public static class Utils
    {
        public static int GetMatTypeFromEnum(MatType enumType)
        {
            switch (enumType)
            {
                case MatType.CV_8UC1:
                    return OpenCvSharp.MatType.CV_8UC1;
                case MatType.CV_8UC2:
                    return OpenCvSharp.MatType.CV_8UC2;
                case MatType.CV_8UC3:
                    return OpenCvSharp.MatType.CV_8UC3;
                case MatType.CV_8UC4:
                    return OpenCvSharp.MatType.CV_8UC4;
                case MatType.CV_8SC1:
                    return OpenCvSharp.MatType.CV_8SC1;
                case MatType.CV_8SC2:
                    return OpenCvSharp.MatType.CV_8SC2;
                case MatType.CV_8SC3:
                    return OpenCvSharp.MatType.CV_8SC3;
                case MatType.CV_8SC4:
                    return OpenCvSharp.MatType.CV_8SC4;
                case MatType.CV_16UC1:
                    return OpenCvSharp.MatType.CV_16UC1;
                case MatType.CV_16UC2:
                    return OpenCvSharp.MatType.CV_16UC2;
                case MatType.CV_16UC3:
                    return OpenCvSharp.MatType.CV_16UC3;
                case MatType.CV_16UC4:
                    return OpenCvSharp.MatType.CV_16UC4;
                case MatType.CV_16SC1:
                    return OpenCvSharp.MatType.CV_16SC1;
                case MatType.CV_16SC2:
                    return OpenCvSharp.MatType.CV_16SC2;
                case MatType.CV_16SC3:
                    return OpenCvSharp.MatType.CV_16SC3;
                case MatType.CV_16SC4:
                    return OpenCvSharp.MatType.CV_16SC4;
                case MatType.CV_32FC1:
                    return OpenCvSharp.MatType.CV_32FC1;
                case MatType.CV_32FC2:
                    return OpenCvSharp.MatType.CV_32FC2;
                case MatType.CV_32FC3:
                    return OpenCvSharp.MatType.CV_32FC3;
                case MatType.CV_32FC4:
                    return OpenCvSharp.MatType.CV_32FC4;
                case MatType.CV_32SC1:
                    return OpenCvSharp.MatType.CV_32SC1;
                case MatType.CV_32SC2:
                    return OpenCvSharp.MatType.CV_32SC2;
                case MatType.CV_32SC3:
                    return OpenCvSharp.MatType.CV_32SC3;
                case MatType.CV_32SC4:
                    return OpenCvSharp.MatType.CV_32SC4;
                case MatType.CV_64FC1:
                    return OpenCvSharp.MatType.CV_64FC1;
                case MatType.CV_64FC2:
                    return OpenCvSharp.MatType.CV_64FC2;
                case MatType.CV_64FC3:
                    return OpenCvSharp.MatType.CV_64FC3;
                case MatType.CV_64FC4:
                    return OpenCvSharp.MatType.CV_64FC4;
                default:
                    return -1;
            }
        }
    }
}
