
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Core;
using VL.Lib.Reactive;
using System.Reflection;

namespace VL.OpenCV
{
    public static class DirectoryChangedEvents
    {
        static FileSystemWatcher fsw = new FileSystemWatcher(Assembly.GetExecutingAssembly().Location + "\\..\\..\\..\\content\\haarcascades");
        
        static readonly IObservable<EventPattern<FileSystemEventArgs>> FileCreatedObservable;

        static readonly IObservable<EventPattern<FileSystemEventArgs>> FileDeletedObservable;

        static readonly IObservable<EventPattern<FileSystemEventArgs>> DirectoryChangedObservable;

        /// <summary>
        /// Checks for device additions and removals, buffers multiple events in 2 second windows. Can fire multiple times
        /// </summary>
        public static IObservable<EventPattern<FileSystemEventArgs>> DirectoryChanged => DirectoryChangedObservable;

        static DirectoryChangedEvents()
        {
            fsw.EnableRaisingEvents = true;

            FileCreatedObservable = Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Created");
            FileCreatedObservable.Subscribe();

            FileDeletedObservable = Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Deleted");
            FileDeletedObservable.Subscribe();

            DirectoryChangedObservable = Observable.Merge(FileCreatedObservable, FileDeletedObservable);
        }
    }

}