using FeralTic.DX11.Resources;
using OpenCvSharp;
using SlimDX.DXGI;
using System;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VL.OpenCV
{
    [PluginInfo(Name = "AsMat",
                Category = "DX11.Texture2D",
                Version = "",
                Help = "Converts DX11.Texture2D to VL.OpenCV Mat",
                Tags = "cv, source")]
    public unsafe class AsImageDX11Node : IPluginEvaluate, IDX11ResourceDataRetriever, IDisposable
    {
        [Input("Input")]
        Pin<DX11Resource<DX11Texture2D>> FInput;

        [Output("Output")]
        ISpread<Mat> FOutput;

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected ILogger FLogger;

        Spread<AsMatDX11Instance> FInstances = new Spread<AsMatDX11Instance>();

        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FInput.PluginIO.IsConnected)
                {
                    if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                    if (this.AssignedContext == null) { return; }

                    var device = this.AssignedContext.Device;
                    var context = this.AssignedContext;

                    FInstances.ResizeAndDispose(SpreadMax);
                    FOutput.SliceCount = SpreadMax;
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        FInstances[i].Process(this.FInput[i], this.AssignedContext);
                        FOutput[i] = FInstances[i].Output;
                    }
                }
            }
            catch (Exception e)
            {
                FLogger.Log(e);
            }
        }

        public FeralTic.DX11.DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        public void Dispose()
        {
            FInstances.ResizeAndDispose(0);
        }
    }
}