using System;
using System.Diagnostics;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using VL.Lib.Collections;

namespace VL.OpenCV
{
    public static class YOLODetector
    {
        private static readonly string[] Labels = { "aeroplane", "bicycle", "bird", "boat", "bottle", "bus", "car", "cat", "chair", "cow", "diningtable", "dog", "horse", "motorbike", "person", "pottedplant", "sheep", "sofa", "train", "tvmonitor" };
        private static readonly Scalar[] Colors = Enumerable.Repeat(false, 20).Select(x => Scalar.RandomColor()).ToArray();

        public static Spread<Rect> Detect(Mat image, out Spread<float> confidence, out Spread<int> detectedClass, out Spread <float> classProbability, out Spread<string> label, bool enabled)
        {
            SpreadBuilder<Rect> rectSB = Spread.CreateBuilder<Rect>();
            SpreadBuilder<float> confidenceSB = Spread.CreateBuilder<float>();
            SpreadBuilder<int> detectedClassSB = Spread.CreateBuilder<int>();
            SpreadBuilder<float> classProbabilitySB = Spread.CreateBuilder<float>();
            SpreadBuilder<string> labelSB = Spread.CreateBuilder<string>();
            if (enabled)
            {
                // https://pjreddie.com/darknet/yolo/
                var cfg = "yolo-voc.cfg";
                var model = "yolov2-voc.weights"; //YOLOv2 544x544
                var threshold = 0.3;

                var w = image.Width;
                var h = image.Height;
                //setting blob, parameter are important
                var blob = CvDnn.BlobFromImage(image, 1 / 255.0, new Size(544, 544), new Scalar(), true, false);
                var net = CvDnn.ReadNetFromDarknet(cfg, model);
                net.SetInput(blob, "data");

                //forward model
                var prob = net.Forward();

                /* YOLO2 VOC output
                 0 1 : center                    2 3 : w/h
                 4 : confidence                  5 ~24 : class probability */
                const int prefix = 5;   //skip 0~4

                for (int i = 0; i < prob.Rows; i++)
                {
                    var matchConfidence = prob.At<float>(i, 4);
                    if (matchConfidence > threshold)
                    {
                        //get classes probability
                        Point min, max;
                        Cv2.MinMaxLoc(prob.Row[i].ColRange(prefix, prob.Cols), out min, out max);
                        var classes = max.X;
                        var probability = prob.At<float>(i, classes + prefix);

                        if (probability > threshold) //more accuracy
                        {
                            //get center and width/height
                            var centerX = prob.At<float>(i, 0) * w;
                            var centerY = prob.At<float>(i, 1) * h;
                            var width = prob.At<float>(i, 2) * w;
                            var height = prob.At<float>(i, 3) * h;
                            var x1 = (centerX - width / 2) < 0 ? 0 : centerX - width / 2; //avoid left side over edge
                            //org.Rectangle(new Point(centerX - width / 2, centerY - height / 2), new Point(centerX + width / 2, centerY + height / 2), Colors[classes], 2);
                            rectSB.Add(new Rect(new Point(x1, centerY - height / 2), new Size(width, height)));
                            confidenceSB.Add(matchConfidence);
                            detectedClassSB.Add(classes);
                            classProbabilitySB.Add(probability);
                            labelSB.Add(Labels[classes]);
                        }
                    }
                }
            }
            confidence = confidenceSB.ToSpread();
            detectedClass = detectedClassSB.ToSpread();
            classProbability = classProbabilitySB.ToSpread();
            label = labelSB.ToSpread();
            return rectSB.ToSpread<Rect>();
        }
    }
}
