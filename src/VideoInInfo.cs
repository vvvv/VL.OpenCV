using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.Win32.Foundation;
using Windows.Win32.Media.DirectShow;
using Windows.Win32.Media.KernelStreaming;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.System.Variant;
using static Windows.Win32.PInvoke;

namespace VL.OpenCV
{
    public unsafe class VideoInInfo
    {
        public record struct VideoDeviceInfo(string Name, int Index, ImmutableArray<Format> Formats)
        {
            public override string ToString() => $"{Name} ({Index})";
        }

        public record struct Format(int w, int h, float fr, string format)
        {
            public override string ToString() => $"{format} {w}x{h} @ {fr:F2}";
        }

        public static string GetSupportedFormats(VideoInputDevice videoInputDevice)
        {
            if (videoInputDevice is null)
                return string.Empty;

            if (videoInputDevice.Tag is not VideoDeviceInfo deviceInfo)
                return string.Empty;

            try
            {
                return String.Join(Environment.NewLine, deviceInfo.Formats
                    .OrderByDescending(x => x.w)
                    .ThenByDescending(x => x.h)
                    .ThenByDescending(x => x.fr)
                    .ThenByDescending(x => x.format)
                    .Select(x => x.ToString())
                    .Distinct()
                    .ToArray());
            }
            catch (Exception)
            {
                return "Error: Your device does not allow listing of supported formats.";
            }
        }

        public static int GetDeviceIndex(VideoInputDevice videoInputDevice)
        {
            if (videoInputDevice is null)
                return -1;
            if (videoInputDevice.Tag is not VideoDeviceInfo deviceInfo)
                return -1;
            return deviceInfo.Index;
        }

        public static List<VideoDeviceInfo> EnumerateCaptureDevicesDS()
        {
            var capDevicesDS = new List<VideoDeviceInfo>();
            CoCreateInstance<ICreateDevEnum>(in CLSID_SystemDeviceEnum, null, CLSCTX.CLSCTX_INPROC_SERVER, out var devEnum).ThrowOnFailure();

            IEnumMoniker* pEnum = null;
            var hr = devEnum->CreateClassEnumerator(in CLSID_VideoInputDeviceCategory, &pEnum, 0);
            if (pEnum != null)
            {
                IMoniker* moniker = null;
                var index = 0;
                while (pEnum->Next(1, &moniker, null) == HRESULT.S_OK)
                {
                    try
                    {
                        var id = typeof(IPropertyBag).GUID;
                        moniker->BindToStorage(null, null, in id, out var bag);
                        var propertyBag = (IPropertyBag*)bag;
                        VARIANT val = default;
                        VariantInit(&val);
                        propertyBag->Read("FriendlyName", ref val, null);
                        var name = val.Anonymous.Anonymous.Anonymous.bstrVal.AsSpan().ToString();
                        VariantClear(&val);
                        propertyBag->Release();

                        id = typeof(IBaseFilter).GUID;
                        moniker->BindToObject(null, null, in id, out var obj);
                        var filter = (IBaseFilter*)obj;

                        var formats = ImmutableArray<Format>.Empty;
                        if (FindPinByCategory(filter, PIN_DIRECTION.PINDIR_OUTPUT, PIN_CATEGORY_CAPTURE, out var pin))
                        {
                            try
                            {
                                formats = GetVideoFormats(pin);
                            }
                            finally
                            {
                                pin->Release();
                            }
                        }

                        capDevicesDS.Add(new VideoDeviceInfo(name, index++, formats));

                        filter->Release();
                    }
                    finally
                    {
                        moniker->Release();
                    }
                }
                pEnum->Release();
            }
            devEnum->Release();
            return capDevicesDS;
        }

        private static ImmutableArray<Format> GetVideoFormats(IPin* pin)
        {
            var availableFormats = ImmutableArray.CreateBuilder<Format>();

            IEnumMediaTypes* pEnumMediaTypes;
            pin->EnumMediaTypes(&pEnumMediaTypes);

            try
            {
                AM_MEDIA_TYPE* mediaType;
                while (pEnumMediaTypes->Next(1, &mediaType) == HRESULT.S_OK)
                {
                    if (mediaType->formattype != FORMAT_VideoInfo)
                        continue;

                    var v = (VIDEOINFOHEADER*)mediaType->pbFormat;
                    if (v->bmiHeader.biSize != 0 && v->bmiHeader.biBitCount != 0)
                    {
                        var fps = (float)Math.Floor((1 / ((v->AvgTimePerFrame * 100) * 0.000000001)));
                        var fourCC = FourCCToString(v->bmiHeader.biCompression);
                        var format = new Format(v->bmiHeader.biWidth, v->bmiHeader.biHeight, fps, fourCC);
                        availableFormats.Add(format);
                    }
                }
            }
            finally
            {
                pEnumMediaTypes->Release();
            }

            return availableFormats.ToImmutable();
        }

        // https://learn.microsoft.com/en-us/windows/win32/directshow/working-with-pin-categories
        static bool FindPinByCategory(IBaseFilter* pFilter, PIN_DIRECTION PinDir, Guid Category, out IPin* pin)
        {
            pin = null;

            IEnumPins* pEnum;
            IPin* pPin;

            pFilter->EnumPins(&pEnum);

            try
            {
                while (pEnum->Next(1, &pPin) == HRESULT.S_OK)
                {
                    PIN_DIRECTION ThisPinDir;
                    pPin->QueryDirection(&ThisPinDir);
                    if ((ThisPinDir == PinDir) &&
                        PinMatchesCategory(pPin, Category))
                    {
                        pin = pPin;
                        return true;
                    }
                    pPin->Release();
                }
            }
            finally
            {
                pEnum->Release();
            }
            return false;
        }

        // https://learn.microsoft.com/en-us/windows/win32/directshow/working-with-pin-categories
        static bool PinMatchesCategory(IPin* pPin, Guid Category)
        {
            var hr = pPin->QueryInterface(typeof(IKsPropertySet).GUID, out var obj);
            if (hr == HRESULT.S_OK)
            {
                var pKs = (IKsPropertySet*)obj;
                try
                {
                    Guid PinCategory;
                    pKs->Get(AMPROPSETID_Pin, (uint)AMPROPERTY_PIN.AMPROPERTY_PIN_CATEGORY, null, 0,
                        &PinCategory, (uint)sizeof(Guid), out var cbReturned);
                    if ((cbReturned == sizeof(Guid)))
                    {
                        return (PinCategory == Category);
                    }
                }
                finally
                {
                    pKs->Release();
                }
            }
            return false;
        }

        private static string FourCCToString(uint value)
        {
            return string.Format("{0}", new string(
            [
                (char) (value & 0xFF),
                (char) (value >> 8 & 0xFF),
                (char) (value >> 16 & 0xFF),
                (char) (value >> 24 & 0xFF),
            ]));
        }
    }
}
