using System;
using VL.Lib.Reactive;
using OpenCvSharp;

namespace VL.OpenCV
{
    /// <summary>
    /// Holds on to a copy of the latest received image. Whenever an image gets pushed to the node a copy is made and stored internally.
    /// </summary>
    public class HoldLatestCopy : HoldLatestCopy<Mat, Mat>
    {
        public HoldLatestCopy(Mat initialResult) : base(initialResult)
        {
        }

        // HACK: Override needed as forwarding mechanism in VL can't properly deal with generic base class
        public override Mat Update(IObservable<Mat> asyncNotifications, int timeout, bool reset, out int swapCount, out int dropCount)
        {
            return base.Update(asyncNotifications, timeout, reset, out swapCount, out dropCount);
        }

        protected override void CopyTo(Mat source, ref Mat destination)
        {
            if (destination == null)
                destination = new Mat();
            source.CopyTo(destination);
        }
    }
}
