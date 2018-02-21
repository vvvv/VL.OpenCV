using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            // Get the paths for available HAAR cascade files
            
            DirectoryInfo haarDir = new DirectoryInfo(Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\content\\haarcascades");
            Spread<FileInfo> haarFiles = haarDir.EnumerateFiles("*.xml").ToSpread();
            Dictionary<string, object> haarPaths = new Dictionary<string, object>(haarFiles.Count);
            foreach (FileInfo haarFile in haarFiles)
            {
                haarPaths[haarFile.Name] = haarFile.FullName;
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
