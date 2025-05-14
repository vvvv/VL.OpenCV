using System;
using System.Collections.Generic;
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
            var capDevices = VideoInInfo.EnumerateCaptureDevicesDS();
            Dictionary<string, object> devices = new Dictionary<string, object>(capDevices.Count);
            if (capDevices.Count > 0)
            {
                devices["Default"] = capDevices[0];
            }
            
            foreach (var device in capDevices)
            {
                var j = 1;
                var friendlyName = device.Name;
                var finalName = friendlyName;
                while (devices.ContainsKey(finalName))
                {
                    finalName = friendlyName + " #" + j++;
                }
                devices[finalName] = device;
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
