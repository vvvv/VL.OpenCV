using System;
using System.Collections.Generic;
using VL.Lib;
using VL.Lib.Collections;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace VL.OpenCV
{
    /// <summary>
    /// Dynamic enum of available video input devices
    /// </summary>
    public class VideoInputDeviceDefinition : DynamicEnumDefinitionBase<VideoInputDeviceDefinition>
    {
        //return the current enum entries
        protected override IReadOnlyDictionary<string, object> GetEntries()
        {
            // Get the collection of video devices
            Activate[] capDevices = VideoInInfo.EnumerateVideoDevices();
            Dictionary<string, object> devices = new Dictionary<string, object>(capDevices.Length);
            for (int i = 0; i < capDevices.Length; i++)
            {
                devices[capDevices[i].Get(CaptureDeviceAttributeKeys.FriendlyName)] = i;
            }
            return devices;
        }

        //inform the system that the enum has changed
        protected override IObservable<object> GetEntriesChangedObservable()
        {
            return HardwareChangedEvents.HardwareChanged;
        }

        //disable alphabetic sorting
        protected override bool AutoSortAlphabetically => false;
    }
}
