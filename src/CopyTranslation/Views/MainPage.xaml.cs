using CopyTranslation.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace CopyTranslation.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();
    }
}
