using CopyTranslation.ViewModels;
using ReactiveUI;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using vm = Windows.UI.ViewManagement;

namespace CopyTranslation.Views
{
    public sealed partial class MainPage : Page, IViewFor<MainPageViewModel>
    {
        public MainPage()
        {
            InitializeComponent();

            ResizeWindowForVeryFirstLaunch();

            StatusButton.Events()
                .Tapped
                .InvokeCommand(ViewModel.StatusTappedCommand);

            this.WhenActivated(d => { });
        }

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();
        MainPageViewModel IViewFor<MainPageViewModel>.ViewModel { get => ViewModel; set => throw new System.NotImplementedException(); }
        object IViewFor.ViewModel { get => ViewModel; set => throw new System.NotImplementedException(); }

        private void ResizeWindowForVeryFirstLaunch()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["launchedWithPrefSize"] == null)
            {
                vm.ApplicationView.PreferredLaunchViewSize = new Size(480, 800);
                vm.ApplicationView.PreferredLaunchWindowingMode = vm.ApplicationViewWindowingMode.PreferredLaunchViewSize;
                localSettings.Values["launchedWithPrefSize"] = true;
            }
            else
            {
                vm.ApplicationView.PreferredLaunchWindowingMode = vm.ApplicationViewWindowingMode.Auto;
            }
        }
    }
}
