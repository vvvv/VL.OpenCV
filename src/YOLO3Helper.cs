using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FeralTic.DX11.Geometry;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using Stride.Core.Mathematics;
using VL.Lib.Collections;

/// <summary>
/// OpenCvSharp V4 with YOLO v3
/// Thank @shimat and Joseph Redmon
///
/// OpenCvSharp
/// https://github.com/shimat/opencvsharp/
///
/// YOLO
/// https://pjreddie.com/darknet/yolo/
/// 
/// Thanks To died:
/// https://www.died.tw/2019/01/c-yolo3-with-opencvsharp4.html
/// https://github.com/died/YOLO3-With-OpenCvSharp4
/// </summary>

namespace VL.OpenCV
{
    public static class YOLO3Helper
    {
        /// <summary>
        /// Get result form all output
        /// </summary>
        /// <param name="output"></param>
        /// <param name="image"></param>
        /// <param name="threshold"></param>
        /// <param name="nmsThreshold">threshold for nms</param>
        /// <param name="nms">Enable Non-maximum suppression or not</param>
        public static void GetResult(IEnumerable<Mat> output, Vector2 imageSize, float threshold, float nmsThreshold, IEnumerable<String> Labels, out List<YOLO3Descriptor> yOLO3Descriptors, bool nms = true)
        {
            yOLO3Descriptors = new List<YOLO3Descriptor>();

            //for nms
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var boxes = new List<Rect2d>();

            var w = imageSize.X;
            var h = imageSize.Y;
            /*
             YOLO3 COCO trainval output
             0 1 : center                    2 3 : w/h
             4 : confidence                  5 ~ 84 : class probability 
            */
            const int prefix = 5;   //skip 0~4

            foreach (var prob in output)
            {
                for (var i = 0; i < prob.Rows; i++)
                {
                    var confidence = prob.At<float>(i, 4);
                    if (confidence > threshold)
                    {
                        //get classes probability
                        Cv2.MinMaxLoc(prob.Row(i).ColRange(prefix, prob.Cols), out _, out OpenCvSharp.Point max);
                        var classes = max.X;
                        var probability = prob.At<float>(i, classes + prefix);

                        if (probability > threshold) //more accuracy, you can cancel it
                        {
                            //get center and width/height
                            var centerX = prob.At<float>(i, 0) * w;
                            var centerY = prob.At<float>(i, 1) * h;
                            var width = prob.At<float>(i, 2) * w;
                            var height = prob.At<float>(i, 3) * h;

                            if (!nms)
                            {
                                // add result (if don't use NMSBoxes)
                                var descriptor = new YOLO3Descriptor();
                                descriptor.Rectangle = new Stride.Core.Mathematics.RectangleF(centerX - (width/2.0f), centerY - (height / 2.0f), width, height);
                                descriptor.Confidence = confidence;
                                descriptor.DetectedClass = classes;
                                descriptor.ClassProbability = probability;
                                descriptor.ClassLabel = Labels.ToArray()[classes];
                                yOLO3Descriptors.Add(descriptor);
                                continue;
                            }

                            //put data to list for NMSBoxes
                            classIds.Add(classes);
                            confidences.Add(confidence);
                            probabilities.Add(probability);
                            boxes.Add(new Rect2d(centerX, centerY, width, height));
                        }
                    }
                }
            }

            if (!nms) return;

            //using non-maximum suppression to reduce overlapping low confidence box
            CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);

            Console.WriteLine($"NMSBoxes drop {confidences.Count - indices.Length} overlapping result.");

            foreach (var i in indices)
            {
                var box = boxes[i];
                var descriptor = new YOLO3Descriptor();
                float x = Convert.ToSingle(box.X - (box.Width  / 2.0));
                float y = Convert.ToSingle(box.Y - (box.Height / 2.0));
                float w1 = Convert.ToSingle(box.Width);
                float h1 = Convert.ToSingle(box.Height);               
                descriptor.Rectangle = new Stride.Core.Mathematics.RectangleF(x, y, w1, h1);
                descriptor.Confidence = confidences[i];
                descriptor.DetectedClass = classIds[i];
                descriptor.ClassProbability = probabilities[i];
                descriptor.ClassLabel = Labels.ToArray()[classIds[i]];
                yOLO3Descriptors.Add(descriptor);
            }
        }
    }
}
