﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Diffusion.Common;
using Diffusion.Database;
using Diffusion.Database.Models;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Thumbnails;
using ArchivedStatus = Diffusion.Toolkit.Models.ArchivedStatus;

namespace Diffusion.Toolkit.Services
{
    public class PathComparer : IEqualityComparer<FolderViewModel>
    {
        public bool Equals(FolderViewModel? x, FolderViewModel? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Path == y.Path;
        }

        public int GetHashCode(FolderViewModel obj)
        {
            return obj.Path.GetHashCode();
        }
    }

    public class FolderService
    {
        private Settings _settings => ServiceLocator.Settings!;

        private DataStore _dataStore => ServiceLocator.DataStore!;

        private Dispatcher _dispatcher => ServiceLocator.Dispatcher;

        public FolderService()
        {
            ServiceLocator.DataStore.DataChanged += DataChangedEventHandler;
        }

        private void DataChangedEventHandler(object? sender, DataChangedEventArgs e)
        {
            if (e.EntityType == EntityType.Folder)
            {
                switch (e.SourceType)
                {
                    case SourceType.Item:
                        if (e.Property is "Excluded")
                        {
                            _isExcludedFoldersDirty = true;
                        }
                        else if (e.Property is "Archived")
                        {
                            _isArchivedFoldersDirty = true;
                        }
                        break;
                    case SourceType.Collection:
                        _isExcludedFoldersDirty = true;
                        _isArchivedFoldersDirty = true;
                        _isFoldersDirty = true;
                        break;
                }
            }
        }

        private void ApplyDBFolderChanges(IEnumerable<FolderChange> folderChanges)
        {
            var removed = 0;
            var updated = 0;
            var added = 0;

            foreach (var folderChange in folderChanges.Where(d => d.FolderType == FolderType.Watched))
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    added += _dataStore.AddRootFolder(folderChange.Path, folderChange.Recursive);
                }
                else if (folderChange.ChangeType == ChangeType.Remove)
                {
                    removed += _dataStore.RemoveFolder(folderChange.Path);
                }
                else if (folderChange.ChangeType == ChangeType.ChangePath)
                {
                    updated += _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
                }
            }


            foreach (var folderChange in folderChanges.Where(d => d.FolderType == FolderType.Excluded))
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    _dataStore.SetFolderExcluded(folderChange.Path, true, false);
                }
                else if (folderChange.ChangeType == ChangeType.Remove)
                {
                    _dataStore.SetFolderExcluded(folderChange.Path, true, false);
                }
                //else if (folderChange.ChangeType == ChangeType.ChangePath)
                //{
                //    updated += _dataStore.ChangeFolderPath(folderChange.Path, folderChange.NewPath);
                //}
            }
        }

        public bool HasRootFolders
        {
            get
            {
                return RootFolders.Any();
            }
        }

        private bool _isFoldersDirty = true;
        private bool _isArchivedFoldersDirty = true;

        private IReadOnlyCollection<Folder> _rootFolders;
        private IReadOnlyCollection<Folder> _allFolders;
        
        private IDictionary<int, bool> _archivedStatus;
        private IReadOnlyCollection<Folder> _excludedFolders;
        private IReadOnlyCollection<Folder> _archivedFolders;
        private HashSet<string> _excludedOrArchivedFolderPaths;

        private void RefreshData()
        {
            _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.FolderId, d => d.Complete);
            _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
            _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
            _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
        }


        public IDictionary<int, bool> ArchivedStatus
        {
            get
            {
                if (_isArchivedFoldersDirty)
                {
                    _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.FolderId, d => d.Complete);
                    _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
                    _isArchivedFoldersDirty = false;
                }

                return _archivedStatus;
            }
        }

        public IReadOnlyCollection<Folder> RootFolders
        {
            get
            {
                if (_isFoldersDirty)
                {
                    _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
                    _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
                    _isFoldersDirty = false;
                }
                return _rootFolders;
            }
        }

        public IReadOnlyCollection<Folder> AllFolders
        {
            get
            {
                if (_isFoldersDirty)
                {
                    _rootFolders = ServiceLocator.DataStore.GetRootFolders().ToList();
                    _allFolders = ServiceLocator.DataStore.GetFolders().ToList();
                    _isFoldersDirty = false;
                }
                return _allFolders;
            }
        }


        private bool _isExcludedFoldersDirty = true;


        public IReadOnlyCollection<Folder> ExcludedFolders
        {
            get
            {
                if (_isExcludedFoldersDirty)
                {
                    _excludedFolders = ServiceLocator.DataStore.GetExcludedFolders().ToList();
                    _excludedOrArchivedFolderPaths = ServiceLocator.DataStore.GetExcludedFolders().Concat(ServiceLocator.DataStore.GetArchivedFolders()).Select(d => d.Path).ToHashSet();

                    _isExcludedFoldersDirty = false;
                }

                return _excludedFolders;
            }
        }

        public IReadOnlyCollection<Folder> ArchivedFolders
        {
            get
            {
                if (_isArchivedFoldersDirty)
                {
                    _archivedStatus = ServiceLocator.DataStore.GetArchivedStatus().ToDictionary(d => d.FolderId, d => d.Complete);
                    _archivedFolders = ServiceLocator.DataStore.GetArchivedFolders().ToList();
                    _isArchivedFoldersDirty = false;
                }

                return _archivedFolders;
            }
        }

        public HashSet<string> ExcludedOrArchivedFolderPaths
        {
            get
            {
                if (_isExcludedFoldersDirty || _isArchivedFoldersDirty || _isFoldersDirty)
                {
                    _excludedOrArchivedFolderPaths = ExcludedFolders.Concat(ArchivedFolders).Select(d => d.Path).ToHashSet();
                }

                return _excludedOrArchivedFolderPaths;
            }
        }

        private bool IsExcludedOrArchivedFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            return ExcludedOrArchivedFolderPaths.Contains(directory);
        }

        private bool IsExcludedOrArchivedFolder(string path)
        {
            return ExcludedOrArchivedFolderPaths.Contains(path);
        }

        private ArchivedStatus GetArchivedStatus(Folder folder)
        {
            ArchivedStatus archivedStatus = Models.ArchivedStatus.Unarchived;

            if (folder.Archived)
            {
                archivedStatus = Models.ArchivedStatus.Archived;

                if (ArchivedStatus.TryGetValue(folder.Id, out var complete))
                {
                    archivedStatus = complete ? archivedStatus : Models.ArchivedStatus.PartiallyArchived;
                }
            }

            return archivedStatus;
        }


        private void UpdateFolder(FolderViewModel folderView, Dictionary<string, FolderView> lookup)
        {
            if (lookup.TryGetValue(folderView.Path, out var folder))
            {
                folderView.Id = folder.Id;
                folderView.HasChildren = folder.HasChildren;
                folderView.IsArchived = folder.Archived;
                folderView.IsUnavailable = folder.Unavailable;
                folderView.IsExcluded = folder.Excluded;
                folderView.IsScanned = true;
                folderView.ArchivedStatus = GetArchivedStatus(folder);
            }
        }

        public async Task LoadFolders()
        {
            var folders = ServiceLocator.DataStore.GetFoldersView().ToList();

            _dispatcher.Invoke(() =>
                {
                    if (ServiceLocator.MainModel.Folders == null || ServiceLocator.MainModel.Folders.Count == 0)
                    {

                        ServiceLocator.MainModel.Folders = new ObservableCollection<FolderViewModel>(folders.Where(d => d.IsRoot).Select(folder => new FolderViewModel()
                        {
                            Id = folder.Id,
                            HasChildren = folder.HasChildren,
                            Visible = true,
                            Depth = 0,
                            Name = Path.GetFileName(folder.Path),
                            Path = folder.Path,
                            IsArchived = folder.Archived,
                            IsExcluded = folder.Excluded,
                            ArchivedStatus = GetArchivedStatus(folder),
                            IsUnavailable = !Directory.Exists(folder.Path),
                            IsScanned = true
                        }));
                    }
                    else
                    {
                        var comparer = new PathComparer();

                        var lookup = folders.ToDictionary(d => d.Path);

                        foreach (var folder in ServiceLocator.MainModel.Folders.ToList())
                        {
                            UpdateFolder(folder, lookup);

                            if (folder.State == FolderState.Expanded && Directory.Exists(folder.Path))
                            {
                                var driveFolders = GetDriveSubFolders(folder);

                                if (folder.Children != null)
                                {
                                    driveFolders = driveFolders.Except(folder.Children, comparer);
                                }

                                if (driveFolders.Any())
                                {
                                    if (folder.Children == null)
                                    {
                                        folder.Children = new ObservableCollection<FolderViewModel>();
                                    }

                                    foreach (var subFolder in driveFolders.Reverse())
                                    {
                                        if (folder.Children.FirstOrDefault(d => d.Path == subFolder.Path) == null)
                                        {
                                            var insertPoint = ServiceLocator.MainModel.Folders.IndexOf(folder) + 1;
                                            var targetFolder = ServiceLocator.MainModel.Folders[insertPoint];

                                            while (subFolder.Path.CompareTo(targetFolder.Path) > 0 && targetFolder.Depth == subFolder.Depth)
                                            {
                                                insertPoint++;
                                                targetFolder = ServiceLocator.MainModel.Folders[insertPoint];
                                            }
                                            ServiceLocator.MainModel.Folders.Insert(insertPoint, subFolder);
                                            folder.Children.Add(subFolder);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

            await ServiceLocator.ScanningService.CheckUnavailableFolders();

        }

        public IEnumerable<FolderViewModel> GetDriveSubFolders(FolderViewModel folder)
        {
            try
            {
                return Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true
                }).Select(sub => new FolderViewModel()
                {
                    Parent = folder,
                    HasChildren = true,
                    Visible = true,
                    Depth = folder.Depth + 1,
                    Name = Path.GetFileName(sub),
                    Path = sub,
                });
            }
            catch (DirectoryNotFoundException ex)
            {
                return Enumerable.Empty<FolderViewModel>();
            }
        }

        public ObservableCollection<FolderViewModel> GetSubFolders(FolderViewModel folder)
        {
            if (!Directory.Exists(folder.Path))
            {
                return new ObservableCollection<FolderViewModel>();
            }

            var subfolders = ServiceLocator.DataStore.GetSubFoldersView(folder.Id);
            
            var subViews = subfolders.Select(sub => new FolderViewModel()
            {
                Id = sub.Id,
                Parent = folder,
                HasChildren = sub.HasChildren,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub.Path),
                Path = sub.Path,
                IsArchived = sub.Archived,
                ArchivedStatus = GetArchivedStatus(sub),
                IsUnavailable = !Directory.Exists(sub.Path),
                IsExcluded = sub.Excluded,
                IsScanned = true,
            }).ToList();

            var lookup = subViews.Select(p => p.Path).ToHashSet();

            var directories = Directory.GetDirectories(folder.Path, "*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            }).Where(path => !lookup.Contains(path)).Select(sub => new FolderViewModel()
            {
                Parent = folder,
                HasChildren = Directory.GetDirectories(sub).Length > 0,
                Visible = true,
                Depth = folder.Depth + 1,
                Name = Path.GetFileName(sub),
                Path = sub,
            });

            return new ObservableCollection<FolderViewModel>(subViews.Concat(directories).OrderBy(d => d.Name));
        }

        public async Task ApplyFolderChanges(IEnumerable<FolderChange> folderChanges, bool confirmScan = false)
        {
            var addedFolders = new List<string>();

            ApplyDBFolderChanges(folderChanges);

            foreach (var folderChange in folderChanges)
            {
                if (folderChange.ChangeType == ChangeType.Add)
                {
                    addedFolders.Add(folderChange.Path);
                }
            }

            RemoveWatchers();

            // If any folders changed
            if (_settings.WatchFolders)
            {
                CreateWatchers();
            }

            await LoadFolders();

            // if excluded paths changed?
            // CleanExcludedPaths(_settings.ExcludePaths);

            // possible Write contention with OnSettingsChanged

            if (addedFolders.Any())
            {
                PopupResult result = PopupResult.Yes;

                if (confirmScan)
                {
                    result = await ServiceLocator.MessageService.Show("Do you want to scan your folders now?", "Settings Updated", PopupButtons.YesNo);
                }

                if (result == PopupResult.Yes)
                {
                    var filesToScan = new List<string>();

                    if (await ServiceLocator.ProgressService.TryStartTask())
                    {
                        var cancellationToken = ServiceLocator.ProgressService.CancellationToken;

                        foreach (var folder in addedFolders)
                        {
                            filesToScan.AddRange(await ServiceLocator.ScanningService.GetFilesToScan(folder, new HashSet<string>(), cancellationToken));
                        }

                        await ServiceLocator.MetadataScannerService.QueueBatchAsync(filesToScan, null, cancellationToken);
                    }
                }

            }
        }

        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        public void CreateWatchers()
        {
            foreach (var path in ServiceLocator.FolderService.RootFolders.Select(d => d.Path))
            {
                if (Directory.Exists(path))
                {
                    var watcher = new FileSystemWatcher(path)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = _settings.RecurseFolders.GetValueOrDefault(true)
                    };
                    watcher.Created += WatcherOnCreated;
                    watcher.Renamed += WatcherOnRenamed;
                    watcher.Changed += WatcherOnChanged;
                    watcher.Deleted += WatcherOnDeleted;
                    _watchers.Add(watcher);
                }
            }
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Path.GetFileName(e.FullPath) == "dt_thumbnails.db-journal")
                {
                    return;
                }

                FileAttributes attr = File.GetAttributes(e.FullPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    //ServiceLocator.DataStore.ChangeFolderPath(e.OldFullPath, e.FullPath);
                }
                else
                {
                    if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        if (!ServiceLocator.FolderService.IsExcludedOrArchivedFile(e.FullPath))
                        {
                            ServiceLocator.ProgressService.StartTask().ContinueWith(t =>
                            {
                                _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken);
                            });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message + " " + exception.StackTrace);
            }

        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(e.FullPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if (!IsExcludedOrArchivedFolder(e.FullPath))
                    {
                        ServiceLocator.DataStore.ChangeFolderPath(e.OldFullPath, e.FullPath);
                        // ServiceLocator.ProgressService.StartTask().ContinueWith(t => { _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken); });
                    }
                }
                else
                {
                    if (_settings.FileExtensions.IndexOf(Path.GetExtension(e.FullPath), StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        if (!IsExcludedOrArchivedFile(e.FullPath))
                        {
                            ServiceLocator.ProgressService.StartTask().ContinueWith(t => { _ = ServiceLocator.MetadataScannerService.QueueAsync(e.FullPath, ServiceLocator.ProgressService.CancellationToken); });
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.Message + " " + exception.StackTrace);
            }
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == "dt_thumbnails.db-journal")
            {
                return;
            }

            var x = e;
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            var x = e;
        }

        public void DisableWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        public void EnableWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        private void RemoveWatchers()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }

        public void SetFoldersDirty()
        {
            _isArchivedFoldersDirty = true;
            _isExcludedFoldersDirty = true;
            _isFoldersDirty = true;
        }

        public void Delete(string path)
        {
            if (ServiceLocator.Settings.PermanentlyDelete)
            {
                Directory.Delete(path);
            }
            else
            {
                Win32FileAPI.Recycle(path);
            }
        }
    }
}
