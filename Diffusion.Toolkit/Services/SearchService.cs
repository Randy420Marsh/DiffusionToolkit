﻿using System;
using Diffusion.Database;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Controls;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class SearchService
{
    public event EventHandler<string> SortBy;
    public event EventHandler<string> SortOrder;
    public event EventHandler<SearchFilter> SearchFilter;
    public event EventHandler<FolderViewModel> OpenFolder;
    public event EventHandler<string> OpenPath;
    public event EventHandler Search;
    public event EventHandler Refresh;
    public event EventHandler<SearchView> View;

    public void SetSortBy(string value)
    {
        SortBy?.Invoke(this, value);
    }

    public void SetSortOrder(string value)
    {
        SortOrder?.Invoke(this, value);
    }

    public void SetFilter(SearchFilter value)
    {
        SearchFilter?.Invoke(this, value);
    }

    public SearchService(FilterControlModel filter, SearchSettings searchSettings)
    {
        Filter = filter;
        SearchSettings = searchSettings;
    }

    public FilterControlModel Filter { get; }

    public SearchSettings SearchSettings { get; }

    public ViewMode CurrentViewMode { get; set; }

    public void ExecuteSearch()
    {
        Search?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshResults()
    {
        Refresh?.Invoke(this, EventArgs.Empty);
    }

    public void SetView(SearchView view)
    {
        View?.Invoke(this, view);
    }

    public void AddNodeFilter(string property, string value)
    {
        Filter.AddNodeFilterEquals(property, value);
    }

    public void AddDefaultSearchProperty(string property)
    {
        SearchSettings.AddDefaultSearchProperty(property);
    }

    public void ExecuteOpenFolder(FolderViewModel folder)
    {
        OpenFolder?.Invoke(this, folder);
    }

    public void ExecuteOpenPath(string path)
    {
        OpenPath?.Invoke(this, path);
    }
}