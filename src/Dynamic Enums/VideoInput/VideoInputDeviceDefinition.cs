using System;
using System.Collections.Generic;
using VL.Lib;
using VL.Lib.Collections;
using static Windows.Win32.PInvoke;

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
            var capDevices = VideoInInfo.EnumerateVideoDevices();
            Dictionary<string, object> devices = new Dictionary<string, object>(capDevices.Length);
            if (capDevices.Length > 0)
            {
                devices["Default"] = 0;
            }
            
            for (int i = 0; i < capDevices.Length; i++)
            {
                var j = 1;
                var friendlyName = VideoInInfo.GetString(capDevices[i], in MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME);
                var finalName = friendlyName;
                while (devices.ContainsKey(finalName))
                {
                    finalName = friendlyName + " #" + j++;
                }
                devices[finalName] = i;
                capDevices[i]->Release();
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
