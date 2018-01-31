using OpenCvSharp;
using System;

namespace VL.OpenCV
{
    public class UnsupportedMatTypeException : Exception
    {
        public MatType Type { get; }

        public UnsupportedMatTypeException(MatType type)
            : base($"Unsupported mat type {type}")
        {
            Type = type;
        }
    }
}
