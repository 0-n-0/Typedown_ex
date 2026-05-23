using Typedown.Core.Models;
using Typedown.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Typedown.Core.Controls
{
    public sealed partial class DocumentTabBar : UserControl
    {
        public AppViewModel ViewModel => DataContext as AppViewModel;

        public DocumentTabBar()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Bindings?.Update();
        }

        private async void OnTabItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is DocumentTab tab)
            {
                await ViewModel.DocumentTabsViewModel.SelectTab(tab);
            }
        }

        private async void OnCloseTabTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if ((sender as FrameworkElement)?.Tag is DocumentTab tab)
            {
                await ViewModel.DocumentTabsViewModel.CloseTab(tab);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Bindings?.StopTracking();
        }
    }
}
