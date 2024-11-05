#nullable enable
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using System;
using VL.Core;
using VL.Core.Utils;
using VL.Lib.Animation;
using VL.Lib.Basics.Resources;
using VL.Lib.Basics.Video;
using VL.Lib.Video;

namespace VL.OpenCV
{
    public class VideoSourceToCvImage : VideoSourceToImage<CvImage>
    {
        public VideoSourceToCvImage(NodeContext nodeContext)
        {
            var frameClock = nodeContext.AppHost.Services.GetRequiredService<IFrameClock>();
            Context = new VideoPlaybackContext(frameClock);
        }

        protected override VideoPlaybackContext Context { get; }

        protected override void OnPush(IResourceProvider<VideoFrame> videoFrameProvider, bool mipmapped)
        {
            var handle = videoFrameProvider.GetHandle();
            var imageHandle = ToCvImage(handle).GetHandle();
            if (!resultQueue.TryAddSafe(imageHandle, millisecondsTimeout: 10))
                imageHandle.Dispose();
        }

        protected override void OnPull(IResourceProvider<VideoFrame>? videoFrameProvider, bool mipmapped)
        {
            if (videoFrameProvider is null)
                return;

            var handle = ToCvImage(videoFrameProvider.GetHandle()).GetHandle();
            if (handle != null && !resultQueue.TryAddSafe(handle))
                handle.Dispose();
        }

        private static unsafe IResourceProvider<CvImage> ToCvImage(IResourceHandle<VideoFrame> handle)
        {
            var videoFrame = handle.Resource;
            if (!videoFrame.TryGetMemory(out var memory))
                throw new NotImplementedException();

            var memoryHandle = memory.Pin();
            var mat = new Mat(videoFrame.Height, videoFrame.Width, videoFrame.PixelFormat.ToMatType(string.Empty), data: (nint)memoryHandle.Pointer, step: videoFrame.RowLengthInBytes);
            return ResourceProvider.Return(new CvImage(mat), (mat, memoryHandle, handle), x =>
            {
                x.mat.Dispose();
                x.memoryHandle.Dispose();
                x.handle.Dispose();
            });
        }
    }
}
