﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Pages;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    public class PageChangedEventArgs
    {
        public int Page { get; set; }
        public CursorPosition CursorPosition { get; set; }
        public Action? OnCompleted { get; set; }
    }

    public partial class ThumbnailView
    {
        public void GoFirstPage(Action? onCompleted)
        {
            Model.Page = 1;

            var args = new PageChangedEventArgs()
            {
                Page = Model.Page,
                OnCompleted = onCompleted

            };

            PageChangedCommand?.Execute(args);
        }

        public void GoLastPage(Action? onCompleted)
        {
            Model.Page = Model.Pages;

            var args = new PageChangedEventArgs()
            {
                Page = Model.Page,
                OnCompleted = onCompleted
            };

            PageChangedCommand?.Execute(args);
        }

        public bool GoPrevPage(Action? onCompleted, bool gotoEnd = false)
        {
            if (Model.Page > 1)
            {
                Model.Page--;
                //currentItemIndex = Model.PageSize - 1;

                var args = new PageChangedEventArgs()
                {
                    Page = Model.Page,
                    CursorPosition = gotoEnd ? CursorPosition.End : CursorPosition.Start,
                    OnCompleted = onCompleted
                };

                PageChangedCommand?.Execute(args);
                return true;
            }

            return false;
        }

        public bool GoNextPage(Action? onCompleted)
        {
            if (Model.Page < Model.Pages)
            {
                Model.Page++;
                //currentItemIndex = 0;

                var args = new PageChangedEventArgs()
                {
                    Page = Model.Page,
                    CursorPosition = CursorPosition.Start,
                    OnCompleted = onCompleted
                };

                PageChangedCommand?.Execute(args);
                return true;
            }
            return false;
        }

        public void SetPagingEnabled()
        {
            Model.SetPagingEnabled(Model.Page);
        }

        public void ClearSelection()
        {
            ThumbnailListView.SelectedItems.Clear();
        }

        public void ReloadThumbnailsView()
        {
            var wrapPanel = GetChildOfType<WrapPanel>(this);

            if (wrapPanel == null || wrapPanel.Children.Count == 0)
                return; // 🔐 Protects against out-of-range indexing here

            var scrollViewer = GetChildOfType<ScrollViewer>(this);
            if (scrollViewer == null)
                return;

            var offset = scrollViewer.VerticalOffset;
            var height = scrollViewer.ViewportHeight;

            var item = wrapPanel.Children[0] as ListViewItem;

            // 🛡️ Additional verification
            if (item == null)
                return;

            var preloadSize = item.ActualHeight * 2;

            double top = 0;
            double left = 0;
            var maxHeight = item.ActualHeight;

            for (var i = 0; i < wrapPanel.Children.Count; i++)
            {
                item = wrapPanel.Children[i] as ListViewItem;

                if (item == null)
                    continue; // 🛡️ Additional verification

                if (top + item.ActualHeight >= (offset - preloadSize) && top <= (offset + height + preloadSize))
                {
                    if (item?.DataContext is ImageEntry { LoadState: LoadState.Unloaded } imageEntry)
                    {
                        ServiceLocator.ThumbnailService.QueueImage(imageEntry);
                    }
                }

                if (item.ActualHeight > maxHeight)
                {
                    maxHeight = item.ActualHeight;
                }

                left += item.ActualWidth;

                if (left + item.ActualWidth > wrapPanel.ActualWidth)
                {
                    top += maxHeight;
                    maxHeight = item.ActualHeight;
                    left = 0;
                }
            }
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _debounceRedrawThumbnails();
        }

        private void RemoveRootFolder_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ImageEntry { EntryType: EntryType.RootFolder } rootFolder })
            {
                ServiceLocator.FolderService.ShowRemoveRootFolderDialog(new FolderViewModel()
                {
                    Id = rootFolder.Id,
                    Path = rootFolder.Path,
                    Name = rootFolder.Name
                });
            }
        }
    }

}
