using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Media.DirectShow;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.System.Variant;
using static Windows.Win32.PInvoke;

namespace VL.OpenCV
{
    public unsafe class VideoInInfo
    {
        public class Format
        {
            public int w, h;
            public float fr;
            public string format;
        }

        public static string GetSupportedFormats(int deviceIndex)
        {
            try
            {
                return String.Join(Environment.NewLine, EnumerateSupportedFormats(deviceIndex)
                    .OrderByDescending(x => x.w)
                    .ThenByDescending(x => x.h)
                    .ThenByDescending(x => x.fr)
                    .ThenByDescending(x => x.format)
                    .Select(x => $"{x.format} {x.w}x{x.h} @ {x.fr:F2}")
                    .Distinct()
                    .ToArray());
            }
            catch (Exception)
            {
                return "Error: Your device does not allow listing of supported formats.";
            }
        }

        static unsafe IEnumerable<Format> EnumerateSupportedFormats(int deviceIndex)
        {
            var result = new List<Format>();
            var devices = EnumerateVideoDevices();
            if (deviceIndex < devices.Length)
            {
                var device = devices[deviceIndex];
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

                                result.Add(new Format() { w = width, h = height, fr = frameRate, format = format });
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
                    device->Release();
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
            return result;
        }

        internal static unsafe IMFActivate*[] EnumerateVideoDevices()
        {
            IMFAttributes* attributes;
            MFCreateAttributes(&attributes, 1).ThrowOnFailure();

            attributes->SetGUID(MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
            MFEnumDeviceSources(attributes, out var devicePtr, out var count).ThrowOnFailure();

            var result = new IMFActivate*[count];
            for (int i = 0; i < result.Length; i++)
                result[i] = devicePtr[i];

            Dictionary<string, int[]> order = new Dictionary<string, int[]>();

            var capDevicesDS = EnumerateCaptureDevicesDS();

            //tries to match the order of the found devices in DirectShow and MediaFoundation
            for (int i = 0; i < result.Length; i++)
            {
                var friendlyName = GetString(result[i], in MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME);
                var suffix = ""; //used to handle multiple devices listed with the same name
                var counter = 1;
                for (int j = 0; j < capDevicesDS.Count; j++)
                {
                    var friendlyNameDS = capDevicesDS[j] + suffix;
                    if (friendlyName + suffix == friendlyNameDS)
                    {
                        if (!order.ContainsKey(friendlyName + suffix))
                        {
                            order.Add(friendlyName + suffix, new int[] { i, j });
                            var tmp = result[j];
                            result[j] = result[i];
                            result[i] = tmp;
                            suffix = "";
                            break;
                        }
                        else
                        {
                            suffix = counter++.ToString();
                            continue;
                        }
                    }
                }
            }
            return result;
        }

        private static List<string> EnumerateCaptureDevicesDS()
        {
            var capDevicesDS = new List<string>();
            CoCreateInstance<ICreateDevEnum>(in CLSID_SystemDeviceEnum, null, CLSCTX.CLSCTX_INPROC_SERVER, out var devEnum).ThrowOnFailure();

            IEnumMoniker* pEnum = null;
            var hr = devEnum->CreateClassEnumerator(in CLSID_VideoInputDeviceCategory, &pEnum, 0);
            if (pEnum != null)
            {
                IMoniker* moniker = null;
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
                        capDevicesDS.Add(val.Anonymous.Anonymous.Anonymous.bstrVal.AsSpan().ToString());
                        VariantClear(&val);
                        propertyBag->Release();
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

        private static void ParseSize(ulong value, out int width, out int height)
        {
            width = (int)(value >> 32);
            height = (int)(value & 0x00000000FFFFFFFF);
        }

        private static float ParseFrameRate(ulong value)
        {
            var numerator = (int)(value >> 32);
            var denominator = (int)(value & 0x00000000FFFFFFFF);
            return numerator * 100 / denominator / 100f;
        }

        private static string GetVideoFormat(IMFMediaType* mediaType)
        {
            // https://docs.microsoft.com/en-us/windows/desktop/medfound/video-subtype-guids
            mediaType->GetGUID(MF_MT_SUBTYPE, out var subTypeId);
            var fourccEncoded = BitConverter.ToInt32(subTypeId.ToByteArray(), 0);
            var fourcc = new FourCC(fourccEncoded);
            return fourcc.ToString();
        }

        internal static unsafe string GetString(IMFActivate* activate, in Guid guid)
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
}
