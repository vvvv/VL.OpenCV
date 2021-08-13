using System;
using VL.Lib.Collections;

namespace VL.OpenCV
{

    [Serializable]
    public class VideoInputDevice : DynamicEnumBase<VideoInputDevice, VideoInputDeviceDefinition>
    {
        public VideoInputDevice(string value) : base(value)
        {
        }

        //this method needs to be imported in VL to set the default
        public static VideoInputDevice CreateDefault()
        {
            return CreateDefaultBase("No video input device found");
        }
    }
}