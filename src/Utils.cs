using OpenCvSharp;
using System.Collections.Generic;

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

        public static unsafe IEnumerable<float> GetPixelAsFloats(Mat source, uint column, uint row)
        {
            OpenCvSharp.MatType format = source.Type();
            var depth = format.Depth;

            uint channelCount = (uint)source.Channels();

            if (channelCount == 0)
            {
                return new float[0];
            }

            uint width = (uint)source.Width;
            uint height = (uint)source.Height;
            float[] output = new float[(int)channelCount];

            row %= height;
            column %= width;

            switch (depth)
            {
                case OpenCvSharp.MatType.CV_8U:
                    {
                        byte* d = (byte*)source.Data.ToPointer();
                        for (uint channel = 0; channel < channelCount; channel++)
                        {
                            output[(int)channel] = d[(column + row * width) * channelCount + channel];
                        }
                        break;
                    }

                case OpenCvSharp.MatType.CV_32F:
                    {
                        float* d = (float*)source.Data.ToPointer();
                        for (uint channel = 0; channel < channelCount; channel++)
                        {
                            output[(int)channel] = d[(column + row * width) * channelCount + channel];
                        }
                        break;
                    }

                case OpenCvSharp.MatType.CV_64F:
                    {
                        double* d = (double*)source.Data.ToPointer();
                        for (uint channel = 0; channel < channelCount; channel++)
                        {
                            output[(int)channel] = (float)d[(column + row * width) * channelCount + channel];
                        }
                        break;
                    }
            }
            return output;
        }

        public static int ToDType(MatType type)
        {
            switch (type)
            {
                case MatType.CV_8UC1:
                    return 0;
                case MatType.CV_8UC2:
                    return 8;
                case MatType.CV_8UC3:
                    return 16;
                case MatType.CV_8UC4:
                    return 24;
                case MatType.CV_8SC1:
                    return 1;
                case MatType.CV_8SC2:
                    return 9;
                case MatType.CV_8SC3:
                    return 17;
                case MatType.CV_8SC4:
                    return 25;
                case MatType.CV_16UC1:
                    return 2;
                case MatType.CV_16UC2:
                    return 10;
                case MatType.CV_16UC3:
                    return 18;
                case MatType.CV_16UC4:
                    return 26;
                case MatType.CV_16SC1:
                    return 3;
                case MatType.CV_16SC2:
                    return 11;
                case MatType.CV_16SC3:
                    return 19;
                case MatType.CV_16SC4:
                    return 27;
                case MatType.CV_32SC1:
                    return 4;
                case MatType.CV_32SC2:
                    return 12;
                case MatType.CV_32SC3:
                    return 20;
                case MatType.CV_32SC4:
                    return 28;
                case MatType.CV_32FC1:
                    return 5;
                case MatType.CV_32FC2:
                    return 13;
                case MatType.CV_32FC3:
                    return 21;
                case MatType.CV_32FC4:
                    return 29;
                case MatType.CV_64FC1:
                    return 6;
                case MatType.CV_64FC2:
                    return 14;
                case MatType.CV_64FC3:
                    return 22;
                case MatType.CV_64FC4:
                    return 30;
                case MatType.Unknown:
                default:
                    return -1;
            }
        }
    }
}
