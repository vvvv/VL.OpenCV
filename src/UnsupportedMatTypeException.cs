using System;

namespace VL.OpenCV
{
    public class UnsupportedMatTypeException : Exception
    {
        public OpenCvSharp.MatType Type { get; }

        public UnsupportedMatTypeException(OpenCvSharp.MatType type)
            : base($"Unsupported mat type {type}")
        {
            Type = type;
        }
    }
}
