﻿namespace Diffusion.Database;

public class QueryOptions
{
    public string Query { get; set; }
    public IReadOnlyCollection<int> FolderIds { get; set; }
    public IReadOnlyCollection<int> AlbumIds { get; set; }
    public SearchView SearchView { get; set; }
    public bool HideDeleted { get; set; }
    public bool HideUnavailable { get; set; }
    public bool HideNSFW { get; set; }
    public bool SearchNodes { get; set; }
    public ComfyQueryOptions ComfyQueryOptions { get; set; }
    public string? Folder { get; set; }
}