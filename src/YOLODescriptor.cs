using OpenCvSharp;
using VL.Lib.Collections;

namespace VL.OpenCV
{
    public class YOLODescriptor
    {
        public Spread<Rect> rectangles
        {
            get; set;
        }

        public Spread<float> confidence
        {
            get; set;
        }

        public Spread<int> detectedClass
        {
            get; set;
        }

        public Spread<float> classProbability
        {
            get; set;
        }

        public Spread<string> classLabel
        {
            get; set;
        }

        public static Spread<Rect> Split(YOLODescriptor descriptor, out Spread<float> confidence, out Spread<int> detectedClass, out Spread<float> classProbability, out Spread<string> classLabel)
        {
            descriptor = descriptor == null ? new YOLODescriptor() : descriptor;
            confidence = descriptor.confidence;
            detectedClass = descriptor.detectedClass;
            classProbability = descriptor.classProbability;
            classLabel = descriptor.classLabel;
            return descriptor.rectangles;
        }

    }
}
