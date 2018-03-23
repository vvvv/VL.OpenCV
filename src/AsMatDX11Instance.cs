using FeralTic.DX11.Resources;
using OpenCvSharp;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System;
using System.Runtime.CompilerServices;
using VVVV.DX11;

namespace VL.OpenCV
{
    public class AsMatDX11Instance : IDisposable
    {
        Texture2D FOffscreenBuffer = null;
        public Mat Output;
        OpenCvSharp.MatType UnrecognizedType = new OpenCvSharp.MatType(-1);

        public AsMatDX11Instance()
        {
            Output = new Mat();
        }

        public void Dispose()
        {
            Output.Dispose();
            if (FOffscreenBuffer != null)
                FOffscreenBuffer.Dispose();
        }

        public unsafe void Process(DX11Resource<DX11Texture2D> input, FeralTic.DX11.DX11RenderContext context)
        {
            DX11Texture2D t = input[context];
            var height = t.Height;
            var width = t.Width;

            var mat = Output;
            var desiredMatFormat = ToOpenCVFormat(t.Format);

            if (desiredMatFormat == UnrecognizedType)
                throw (new Exception("No suitible image type available for this texture type " + t.Format.ToString()));


            //--
            //check attributes and reinitialise the image and offscreen buffer if we haven't got the right description ready
            //
            if (FOffscreenBuffer == null || FOffscreenBuffer.Description.Format != t.Description.Format || mat.Width != t.Width || mat.Height != t.Height || mat.Type() != desiredMatFormat)
            {
                if (FOffscreenBuffer != null)
                    FOffscreenBuffer.Dispose();

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

                mat = new Mat(t.Height, t.Width, desiredMatFormat);
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

            var data = ctx.MapSubresource(FOffscreenBuffer, 0, MapMode.Read, SlimDX.Direct3D11.MapFlags.None);

            try
            {
                var source = data.Data.DataPointer;
                var size = (uint)data.Data.Length;

                var bytesPerRow = (uint)(mat.Size().Width * mat.ElemSize());
                var destination = mat.DataPointer;

                if (bytesPerRow == data.RowPitch)
                {
                    Unsafe.CopyBlock(mat.DataPointer, source.ToPointer(), size);
                }
                else
                {
                    for (int row = 0; row < t.Height; row++)
                    {
                        Unsafe.CopyBlock(destination, source.ToPointer(), bytesPerRow);

                        source += data.RowPitch;
                        destination += bytesPerRow;
                    }
                }
            }
            //
            //--
            finally
            {
                ctx.UnmapSubresource(FOffscreenBuffer, 0);
            }
            
            Output = mat;
        }

        OpenCvSharp.MatType ToOpenCVFormat(SlimDX.DXGI.Format format)
        {
            switch (format)
            {
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                case Format.B8G8R8A8_UNorm_SRGB:
                case Format.B8G8R8X8_UNorm_SRGB:
                case Format.B8G8R8A8_Typeless:
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRGB:
                    return OpenCvSharp.MatType.CV_8UC4;

                case Format.R16_UInt:
                case Format.R16_UNorm:
                case Format.R16_Typeless:
                    return OpenCvSharp.MatType.CV_16UC1;

                case Format.R32G32B32A32_Float:
                    return OpenCvSharp.MatType.CV_32FC4;

                case Format.R32_Float:
                    return OpenCvSharp.MatType.CV_32FC1;

                case Format.R32_SInt:
                    return OpenCvSharp.MatType.CV_32SC1;

                default:
                    return UnrecognizedType;
            }
        }
    }
}
