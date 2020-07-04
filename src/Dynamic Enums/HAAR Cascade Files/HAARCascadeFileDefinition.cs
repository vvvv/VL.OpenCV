using System;
using System.Collections.Generic;
using System.IO;
using VL.Lib.Collections;

namespace VL.OpenCV
{
    /// <summary>
    /// Dynamic enum of available HAAR cascade files for object detection
    /// </summary>
    public class HAARCascadeFileDefinition : DynamicEnumDefinitionBase<HAARCascadeFileDefinition>
    {
        //return the current enum entries
        protected override IReadOnlyDictionary<string, object> GetEntries()
        {
            var haarPaths = new Dictionary<string, object>();
            // Get the paths for available HAAR cascade files
            var haarcascadeDir = HAARCascadeResolver.ResolveHaarcascadesDirectory();
            if (haarcascadeDir != null)
            {
                var haarDir = new DirectoryInfo(haarcascadeDir);
                var haarFiles = haarDir.EnumerateFiles("*.xml");
                foreach (var haarFile in haarFiles)
                {
                    haarPaths[haarFile.Name] = haarFile.FullName;
                }
            }
            return haarPaths;
        }

        //inform the system that the enum has changed
        protected override IObservable<object> GetEntriesChangedObservable()
        {
            return DirectoryChangedEvents.DirectoryChanged;
        }

        //disable alphabetic sorting
        protected override bool AutoSortAlphabetically => false;
    }
}
