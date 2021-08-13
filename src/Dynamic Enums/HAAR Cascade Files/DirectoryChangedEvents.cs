using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace VL.OpenCV
{
    public static class DirectoryChangedEvents
    {
        static readonly FileSystemWatcher fsw;

        static readonly IObservable<EventPattern<FileSystemEventArgs>> FileCreatedObservable;

        static readonly IObservable<EventPattern<FileSystemEventArgs>> FileDeletedObservable;

        static readonly IObservable<EventPattern<FileSystemEventArgs>> DirectoryChangedObservable;

        /// <summary>
        /// Checks for device additions and removals, buffers multiple events in 2 second windows. Can fire multiple times
        /// </summary>
        public static IObservable<EventPattern<FileSystemEventArgs>> DirectoryChanged => DirectoryChangedObservable;

        static DirectoryChangedEvents()
        {
            var haarcascadesDir = HAARCascadeResolver.ResolveHaarcascadesDirectory();
            if (haarcascadesDir != null)
            {
                fsw = new FileSystemWatcher(haarcascadesDir)
                {
                    EnableRaisingEvents = true
                };

                FileCreatedObservable = Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Created");
                FileCreatedObservable.Subscribe();

                FileDeletedObservable = Observable.FromEventPattern<FileSystemEventArgs>(fsw, "Deleted");
                FileDeletedObservable.Subscribe();

                DirectoryChangedObservable = Observable.Merge(FileCreatedObservable, FileDeletedObservable);
            }
            else
            {
                DirectoryChangedObservable = Observable.Never<EventPattern<FileSystemEventArgs>>();
            }
        }
    }
}