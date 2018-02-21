using DirectShowLib;
using System;
using System.Runtime.InteropServices;

namespace VL.OpenCV
{
    public static class VideoInInfo
    {
        public static string GetVideoFormats(int deviceId)
        {
            var AvailableFormats = "No device information available";

            var capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (capDevices.Length > 0 && deviceId < capDevices.Length)
            {
                var device = capDevices[deviceId];
                if (device != null)
                {
                    try
                    {
                        int hr;

                        IBaseFilter sourceFilter = null;

                        var m_FilterGraph2 = new FilterGraph() as IFilterGraph2;

                        hr = m_FilterGraph2.AddSourceFilterForMoniker(device.Mon, null, device.Name, out sourceFilter);

                        var pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);

                        VideoInfoHeader v = new VideoInfoHeader();
                        IEnumMediaTypes mediaTypeEnum;
                        hr = pRaw2.EnumMediaTypes(out mediaTypeEnum);

                        AMMediaType[] mediaTypes = new AMMediaType[1];
                        IntPtr fetched = IntPtr.Zero;
                        hr = mediaTypeEnum.Next(1, mediaTypes, fetched);
                        AvailableFormats = "";
                        while (fetched != null && mediaTypes[0] != null)
                        {
                            Marshal.PtrToStructure(mediaTypes[0].formatPtr, v);
                            if (v.BmiHeader.Size != 0 && v.BmiHeader.BitCount != 0)
                            {
                                var fps = Math.Floor((1 / ((v.AvgTimePerFrame * 100) * 0.000000001))).ToString();
                                var format = v.BmiHeader.Width.ToString() + "x" + v.BmiHeader.Height.ToString() + " @" + fps.ToString() + " fps\n";
                                if (!AvailableFormats.Contains(format))
                                    AvailableFormats += format;
                            }
                            hr = mediaTypeEnum.Next(1, mediaTypes, fetched);
                        }
                    }

                    catch
                    {
                        return AvailableFormats;
                    }
                }
            }
            return AvailableFormats;
        }
    }
}
