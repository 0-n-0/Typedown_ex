using PropertyChanged;
using System;
using System.ComponentModel;
using System.IO;
using Typedown.Core.Utilities;

namespace Typedown.Core.Models
{
    public partial class DocumentTab : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string FilePath { get; set; }

        public string WorkTitle { get; set; }

        public string Markdown { get; set; } = string.Empty;

        public ulong FileHash { get; set; }

        public ulong CurrentHash { get; set; }

        public bool Saved { get; set; } = true;

        public bool AutoSavedSucc { get; set; } = true;

        public bool FileLoaded { get; set; }

        public bool IsSelected { get; set; }

        public ContentHistory History { get; } = new();

        public string FileName => Path.GetFileName(FilePath);

        public string Title => string.IsNullOrEmpty(FileName) ? (WorkTitle ?? "Untitled") : FileName;

        public string DisplayTitle => Saved ? Title : $"*{Title}";

        public bool IsUntitled => string.IsNullOrEmpty(FilePath);

        public string ImageBasePath(string defaultImageBasePath) => string.IsNullOrEmpty(FilePath) ? defaultImageBasePath : Path.GetDirectoryName(FilePath);

        [SuppressPropertyChangedWarnings]
        public void RefreshComputedProperties()
        {
            PropertyChanged?.Invoke(this, new(nameof(FileName)));
            PropertyChanged?.Invoke(this, new(nameof(Title)));
            PropertyChanged?.Invoke(this, new(nameof(DisplayTitle)));
            PropertyChanged?.Invoke(this, new(nameof(IsUntitled)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
