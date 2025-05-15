using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VL.Lib;
using VL.Lib.Collections;

namespace VL.OpenCV
{
    /// <summary>
    /// Dynamic enum of available video input devices
    /// </summary>
    public class VideoInputDeviceDefinition : DynamicEnumDefinitionBase<VideoInputDeviceDefinition>
    {
        //return the current enum entries
        protected override unsafe IReadOnlyDictionary<string, object> GetEntries()
        {
            // Get the collection of video devices
            var devices = ImmutableArray.CreateBuilder<VideoInInfo.VideoDeviceInfo>();
            VideoInInfo.EnumerateCaptureDevicesDS(devices);
            VideoInInfo.EnumerateCaptureDevicesMF(devices);

            var entries = new Dictionary<string, object>();
            if (devices.Count > 0)
            {
                entries["Default"] = devices[0];
            }
            
            foreach (var device in devices)
            {
                if (!entries.ContainsKey(device.Name))
                    entries[device.Name] = device;
            }

            return entries;
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
