using DirectShowLib;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VL.OpenCV
{


    public class VideoInInfo
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

        static IEnumerable<Format> EnumerateSupportedFormats(int deviceIndex)
        {
            var devices = EnumerateVideoDevices();
            if (deviceIndex < devices.Length)
            {
                var device = devices[deviceIndex];
                var mediaSource = device.ActivateObject<MediaSource>();
                mediaSource.CreatePresentationDescriptor(out PresentationDescriptor descriptor);
                var streamDescriptor = descriptor.GetStreamDescriptorByIndex(0, out SharpDX.Mathematics.Interop.RawBool _);
                var handler = streamDescriptor.MediaTypeHandler;
                for (int i = 0; i < handler.MediaTypeCount; i++)
                {
                    var mediaType = handler.GetMediaTypeByIndex(i);
                    if (mediaType.MajorType == MediaTypeGuids.Video)
                    {
                        var frameRate = ParseFrameRate(mediaType.Get(MediaTypeAttributeKeys.FrameRate));
                        ParseSize(mediaType.Get(MediaTypeAttributeKeys.FrameSize), out int width, out int height);

                        var format = GetVideoFormat(mediaType);

                        yield return new Format() { w = width, h = height, fr = frameRate, format = format };
                    }
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public static Activate[] EnumerateVideoDevices()
        {
            var attributes = new MediaAttributes();
            MediaFactory.CreateAttributes(attributes, 1);
            attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);
            var mediaFoundationActivates = MediaFactory.EnumDeviceSources(attributes);
            Activate[] result = new Activate[mediaFoundationActivates.Length];
            Dictionary<string, int[]> order = new Dictionary<string, int[]>();

            DsDevice[] capDevicesDS;
            capDevicesDS = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            //tries to match the order of the found devices in DirectShow and MediaFoundation
            for (int i = 0; i < mediaFoundationActivates.Length; i++)
            {
                var friendlyName = mediaFoundationActivates[i].Get(CaptureDeviceAttributeKeys.FriendlyName);
                var suffix = ""; //used to handle multiple devices listed with the same name
                var counter = 1;
                for (int j = 0; j < capDevicesDS.Length; j++)
                {
                    var friendlyNameDS = capDevicesDS[j].Name + suffix;
                    if (friendlyName + suffix == friendlyNameDS)
                    {
                        if (!order.ContainsKey(friendlyName + suffix))
                        {
                            order.Add(friendlyName + suffix, new int[] { i, j });
                            result[j] = mediaFoundationActivates[i];
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


        private static void ParseSize(long value, out int width, out int height)
        {
            width = (int)(value >> 32);
            height = (int)(value & 0x00000000FFFFFFFF);
        }

        private static float ParseFrameRate(long value)
        {
            var numerator = (int)(value >> 32);
            var denominator = (int)(value & 0x00000000FFFFFFFF);
            return numerator * 100 / denominator / 100f;
        }

        private static string GetVideoFormat(SharpDX.MediaFoundation.MediaType mediaType)
        {
            // https://docs.microsoft.com/en-us/windows/desktop/medfound/video-subtype-guids
            var subTypeId = mediaType.Get(MediaTypeAttributeKeys.Subtype);
            var fourccEncoded = BitConverter.ToInt32(subTypeId.ToByteArray(), 0);
            var fourcc = new FourCC(fourccEncoded);
            return fourcc.ToString();
        }
    }
}
