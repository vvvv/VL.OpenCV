using System;
using System.Collections.Immutable;
using System.Linq;
using OpenCvSharp;
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
    public static unsafe class VideoInInfo
    {
        public record struct VideoDeviceInfo(string Name, VideoCaptureAPIs CaptureAPI, int Index, ImmutableArray<Format> Formats)
        {
            public override string ToString() => $"{Name} ({CaptureAPI} {Index})";
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

        public static void GetDeviceIndexAndBackend(VideoInputDevice videoInputDevice, out int deviceIndex, out VideoCaptureAPIs captureAPI)
        {
            deviceIndex = -1;
            captureAPI = VideoCaptureAPIs.ANY;

            if (videoInputDevice is null)
                return;
            if (videoInputDevice.Tag is not VideoDeviceInfo deviceInfo)
                return;

            deviceIndex = deviceInfo.Index;
            captureAPI = deviceInfo.CaptureAPI;
        }

        internal static void EnumerateCaptureDevicesMF(ImmutableArray<VideoDeviceInfo>.Builder devices)
        {
            IMFAttributes* attributes;
            MFCreateAttributes(&attributes, 1);
            if (attributes is null)
                return;

            attributes->SetGUID(MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
            MFEnumDeviceSources(attributes, out var mfDevices, out var count);
            if (mfDevices is null || count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var friendlyName = GetString(mfDevices[i], in MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME);
                var formats = GetVideoFormats(mfDevices[i]);
                devices.Add(new VideoDeviceInfo(friendlyName, VideoCaptureAPIs.MSMF, i, formats));
            }

            static unsafe ImmutableArray<Format> GetVideoFormats(IMFActivate* device)
            {
                var formats = ImmutableArray.CreateBuilder<Format>();
                var mediaSource = (IMFMediaSource*)device->ActivateObject(in IMFMediaSource.IID_Guid);

                IMFPresentationDescriptor* descriptor = null;
                IMFStreamDescriptor* streamDescriptor = null;
                IMFMediaTypeHandler* handler = null;

                try
                {
                    mediaSource->CreatePresentationDescriptor(&descriptor);
                    descriptor->GetStreamDescriptorByIndex(0, out _, &streamDescriptor);
                    streamDescriptor->GetMediaTypeHandler(&handler);
                    handler->GetMediaTypeCount(out var mediaTypeCount);

                    for (uint i = 0; i < mediaTypeCount; i++)
                    {
                        IMFMediaType* mediaType;
                        handler->GetMediaTypeByIndex(i, &mediaType);
                        try
                        {
                            mediaType->GetMajorType(out var majorType);
                            if (majorType == MFMediaType_Video)
                            {
                                mediaType->GetUINT64(MF_MT_FRAME_RATE, out var fr);
                                var frameRate = ParseFrameRate(fr);
                                mediaType->GetUINT64(MF_MT_FRAME_SIZE, out var fs);
                                ParseSize(fs, out int width, out int height);

                                var format = GetVideoFormat(mediaType);

                                formats.Add(new Format(width, height, frameRate, format));
                            }
                        }
                        finally
                        {
                            mediaType->Release();
                        }
                    }
                }
                finally
                {
                    handler->Release();
                    streamDescriptor->Release();
                    descriptor->Release();
                }

                return formats.ToImmutable();

                static float ParseFrameRate(ulong value)
                {
                    var numerator = (int)(value >> 32);
                    var denominator = (int)(value & 0x00000000FFFFFFFF);
                    return numerator * 100 / denominator / 100f;
                }

                static void ParseSize(ulong value, out int width, out int height)
                {
                    width = (int)(value >> 32);
                    height = (int)(value & 0x00000000FFFFFFFF);
                }

                static string GetVideoFormat(IMFMediaType* mediaType)
                {
                    // https://docs.microsoft.com/en-us/windows/desktop/medfound/video-subtype-guids
                    mediaType->GetGUID(MF_MT_SUBTYPE, out var subTypeId);
                    var fourccEncoded = BitConverter.ToUInt32(subTypeId.ToByteArray(), 0);
                    return FourCCToString(fourccEncoded);
                }
            }

            static string GetString(IMFActivate* activate, in Guid guid)
            {
                activate->GetStringLength(in guid, out var length);

                Span<char> buffer = stackalloc char[(int)length + 1];
                fixed (char* c = buffer)
                {
                    activate->GetString(in guid, new PWSTR(c), (uint)buffer.Length, &length);
                }

                return buffer.Slice(0, (int)length).ToString();
            }
        }

        internal static void EnumerateCaptureDevicesDS(ImmutableArray<VideoDeviceInfo>.Builder devices)
        {
            CoCreateInstance<ICreateDevEnum>(in CLSID_SystemDeviceEnum, null, CLSCTX.CLSCTX_INPROC_SERVER, out var devEnum);
            if (devEnum is null)
                return;

            IEnumMoniker* pEnum;
            devEnum->CreateClassEnumerator(in CLSID_VideoInputDeviceCategory, &pEnum, 0);
            if (devEnum is null)
                return;

            IMoniker* moniker;
            var index = 0;
            while (pEnum->Next(1, &moniker) == HRESULT.S_OK)
            {
                try
                {
                    moniker->BindToStorage(null, null, typeof(IPropertyBag).GUID, out var bag);
                    var propertyBag = (IPropertyBag*)bag;
                    VARIANT val = default;
                    VariantInit(&val);
                    propertyBag->Read("FriendlyName", ref val, null);
                    var name = val.Anonymous.Anonymous.Anonymous.bstrVal.AsSpan().ToString();
                    VariantClear(&val);
                    propertyBag->Release();

                    moniker->BindToObject(null, null, typeof(IBaseFilter).GUID, out var obj);
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

                    devices.Add(new VideoDeviceInfo(name, VideoCaptureAPIs.DSHOW, index++, formats));

                    filter->Release();
                }
                catch
                {
                    // Ignore errors - if the device can't be opened, it will be skipped
                }
                finally
                {
                    moniker->Release();
                }
            }
            pEnum->Release();
            devEnum->Release();

            static ImmutableArray<Format> GetVideoFormats(IPin* pin)
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
