using OpenCvSharp;
using System;
using VL.Lib.Reactive;

namespace VL.OpenCV
{
    /// <summary>
    /// Holds on to a copy of the latest received image. Whenever an image gets pushed to the node a copy is made and stored internally.
    /// </summary>
    public class HoldLatestCopy : HoldLatestCopy<CvImage, CvImage>
    {
        public HoldLatestCopy(CvImage initialResult) : base(initialResult)
        {
        }

        // HACK: Override needed as forwarding mechanism in VL can't properly deal with generic base class
        public override CvImage Update(IObservable<CvImage> asyncNotifications, int timeout, bool reset, out int swapCount, out int dropCount)
        {
            return base.Update(asyncNotifications, timeout, reset, out swapCount, out dropCount);
        }

        protected override void CopyTo(CvImage source, ref CvImage destination)
        {
            if (destination == null)
            {
                destination = new CvImage(new Mat());
            }

            source.Mat.CopyTo(destination.Mat);
        }
    }
}
