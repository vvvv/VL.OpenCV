using FeralTic.DX11.Resources;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.Runtime.CompilerServices;
using VVVV.DX11;
using VL.Lib.Basics.Imaging;

namespace VL.OpenCV
{
    public class AsImageDX11Instance : IDisposable
    {
        Texture2D FOffscreenBuffer = null;
        public IImage Output;

        public AsImageDX11Instance()
        {
            var arr = new byte[0];
            Output = arr.ToImage(0, 0, PixelFormat.Unknown);
        }

        public void Dispose()
        {
            FOffscreenBuffer?.Dispose();
        }

        public unsafe void Process(DX11Resource<DX11Texture2D> input, FeralTic.DX11.DX11RenderContext context)
        {
            DX11Texture2D t = input[context];
            var height = t.Height;
            var width = t.Width;

            var image = Output;
            PixelFormat pixelFormat = ToPixelFormat(t.Format);

            if (pixelFormat == PixelFormat.Unknown)
                throw (new Exception("Unsupported texture format: " + t.Format.ToString()));

            var info = image.Info;
            var targetInfo = new ImageInfo(t.Width, t.Height, pixelFormat);
            //--
            //check attributes and reinitialise the image and offscreen buffer if we haven't got the right description ready
            //
            if (FOffscreenBuffer == null || info != targetInfo)
            {

                FOffscreenBuffer?.Dispose();

                var description = new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
                    Format = t.Format,
                    MipLevels = 1,
                    Usage = ResourceUsage.Staging,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    SampleDescription = new SampleDescription(1, 0),
                    ArraySize = 1
                };

                FOffscreenBuffer = new Texture2D(context.Device, description);

                var arr = new byte[targetInfo.ImageSize];
                image = arr.ToImage(targetInfo.Width, targetInfo.Height, pixelFormat);
            }
            //
            //--

            var ctx = context.CurrentDeviceContext;

            //--
            //copy the texture to offscreen buffer
            //
            ctx.CopyResource(t.Resource, FOffscreenBuffer);
            //
            //--


            //--
            //copy the data out of the offscreen buffer
            //
            var surface = FOffscreenBuffer.AsSurface();

            var srcData = ctx.MapSubresource(FOffscreenBuffer, 0, MapMode.Read, SlimDX.Direct3D11.MapFlags.None);
            var dstData = image.GetData();
            try
            {
                var srcPitch = srcData.RowPitch;
                var dstPitch = dstData.ScanSize;
                var srcPtr = srcData.Data.DataPointer;
                var dstPtr = dstData.Pointer;
                if (dstPitch == srcPitch)
                {
                    var srcTotalLength = (uint)srcData.Data.Length;
                    Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), srcTotalLength);
                }
                else
                {
                    for (int row = 0; row < height; row++)
                    {
                        Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint)dstPitch);

                        srcPtr += srcPitch;
                        dstPtr += dstPitch;
                    }
                }
            }
            //
            //--
            finally
            {
                ctx.UnmapSubresource(FOffscreenBuffer, 0);
                dstData.Dispose();
            }
            
            Output = image;
        }

        PixelFormat ToPixelFormat(Format format)
        {
            switch (format)
            {
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8A8_UNorm_SRGB:
                    return PixelFormat.B8G8R8A8;

                case Format.B8G8R8X8_Typeless:
                case Format.B8G8R8X8_UNorm:
                case Format.B8G8R8X8_UNorm_SRGB:
                    return PixelFormat.B8G8R8X8;

                case Format.R32_Float:
                    return PixelFormat.R32F;

                case Format.R32G32B32A32_Float:
                    return PixelFormat.R32G32B32A32F;

                case Format.R8_Typeless:
                case Format.R8_UInt:
                case Format.R8_UNorm:
                    return PixelFormat.R8;

                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRGB:
                    return PixelFormat.R8G8B8A8;

                default:
                    return PixelFormat.Unknown;
            }
        }
    }
}
