using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Typedown.Core.Models;
using Typedown.Core.Utilities;

namespace Typedown.Core.ViewModels
{
    public sealed partial class DocumentTabsViewModel : INotifyPropertyChanged, IDisposable
    {
        public IServiceProvider ServiceProvider { get; }

        public FileViewModel FileViewModel => ServiceProvider.GetService<FileViewModel>();

        public ObservableCollection<DocumentTab> Tabs { get; } = new();

        public DocumentTab CurrentTab { get; private set; }

        public bool HasMultipleTabs => Tabs.Count > 1;

        private int untitledIndex = 1;

        public Command<DocumentTab> SelectTabCommand { get; } = new();

        public Command<DocumentTab> CloseTabCommand { get; } = new();

        public Command<Unit> NewTabCommand { get; } = new();

        private readonly CompositeDisposable disposables = new();

        public DocumentTabsViewModel(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            SelectTabCommand.OnExecute.Subscribe(async tab => await SelectTab(tab));
            CloseTabCommand.OnExecute.Subscribe(async tab => await CloseTab(tab));
            NewTabCommand.OnExecute.Subscribe(async _ => await FileViewModel.NewFile());
            Tabs.CollectionChanged += (_, __) => NotifyTabListChanged();
        }

        public DocumentTab CreateTab(bool select = true)
        {
            var tab = new DocumentTab() { WorkTitle = GetUntitledTitle() };
            Tabs.Add(tab);
            if (select)
            {
                SetCurrentTab(tab);
            }
            return tab;
        }

        private string GetUntitledTitle()
        {
            var title = untitledIndex == 1 ? "Untitled" : $"Untitled {untitledIndex}";
            untitledIndex++;
            return title;
        }

        public async Task<bool> SelectTab(DocumentTab tab, bool postMessage = true)
        {
            if (tab == null || tab == CurrentTab)
            {
                return true;
            }
            if (!await FileViewModel.PersistCurrentTabState())
            {
                return false;
            }
            SetCurrentTab(tab);
            FileViewModel.ApplyTabToEditor(tab, postMessage);
            return true;
        }

        public async Task<bool> CloseTab(DocumentTab tab)
        {
            tab ??= CurrentTab;
            if (tab == null)
            {
                return false;
            }
            var wasCurrent = tab == CurrentTab;
            if (wasCurrent && !await FileViewModel.PersistCurrentTabState())
            {
                return false;
            }
            if (wasCurrent && !await FileViewModel.AskToSave(tab))
            {
                return false;
            }
            if (!wasCurrent)
            {
                SetCurrentTab(tab);
                FileViewModel.ApplyTabToEditor(tab);
                if (!await FileViewModel.AskToSave(tab))
                {
                    return false;
                }
                wasCurrent = true;
            }
            var index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);
            if (!Tabs.Any())
            {
                await FileViewModel.NewFile();
                return true;
            }
            if (wasCurrent)
            {
                SetCurrentTab(Tabs[Math.Max(0, Math.Min(index, Tabs.Count - 1))]);
                FileViewModel.ApplyTabToEditor(CurrentTab);
            }
            return true;
        }

        public async Task<bool> CloseAllTabs()
        {
            var originalTab = CurrentTab;
            if (!await FileViewModel.PersistCurrentTabState())
            {
                return false;
            }
            foreach (var tab in Tabs.ToList())
            {
                SetCurrentTab(tab);
                FileViewModel.ApplyTabToEditor(tab);
                if (!await FileViewModel.AskToSave(tab))
                {
                    if (originalTab != null && Tabs.Contains(originalTab))
                    {
                        SetCurrentTab(originalTab);
                        FileViewModel.ApplyTabToEditor(originalTab);
                    }
                    return false;
                }
            }
            return true;
        }

        public DocumentTab FindTab(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            return Tabs.FirstOrDefault(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFileOpen(string filePath) => FindTab(filePath) != null;

        public void SetCurrentTab(DocumentTab tab)
        {
            if (CurrentTab != null)
            {
                CurrentTab.IsSelected = false;
            }
            CurrentTab = tab;
            if (CurrentTab != null)
            {
                CurrentTab.IsSelected = true;
            }
            NotifyCurrentTabChanged();
        }

        public void NotifyCurrentTabChanged()
        {
            PropertyChanged?.Invoke(this, new(nameof(CurrentTab)));
            FileViewModel.NotifyCurrentFileChanged();
        }

        private void NotifyTabListChanged()
        {
            PropertyChanged?.Invoke(this, new(nameof(HasMultipleTabs)));
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
