//using OpenCvSharp;
//using VL.Lib.Collections;
using Stride.Core.Mathematics;

namespace VL.OpenCV
{
    public class YOLO3Descriptor
    {
        public RectangleF Rectangle
        {
            get; set;
        }

        public float Confidence
        {
            get; set;
        }

        public int DetectedClass
        {
            get; set;
        }

        public float ClassProbability
        {
            get; set;
        }

        public string ClassLabel
        {
            get; set;
        }

        public static RectangleF Split(YOLO3Descriptor descriptor, out float confidence, out int detectedClass, out float classProbability, out string classLabel)
        {
            descriptor = descriptor == null ? new YOLO3Descriptor() : descriptor;
            confidence = descriptor.Confidence;
            detectedClass = descriptor.DetectedClass;
            classProbability = descriptor.ClassProbability;
            classLabel = descriptor.ClassLabel;
            return descriptor.Rectangle;
        }

    }
}
