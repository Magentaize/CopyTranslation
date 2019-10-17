using CopyTranslation.ViewModels;
using ReactiveUI;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using vm = Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace CopyTranslation.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ResizeWindowForVeryFirstLaunch();

            StatusButton.Events()
                .Tapped
                .InvokeCommand(ViewModel.StatusTappedCommand);
        }

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                App.AppServiceConnected += App_AppServiceConnected;
                App.AppServiceDisconnected += App_AppServiceDisconnected;
                await LaunchFullTrustAsync();
            }
        }

        private void App_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            ViewModel.SubTranslate();
        }

        private void App_AppServiceDisconnected(object sender, EventArgs e)
        {

        }

        private IAsyncAction LaunchFullTrustAsync()
        {
            return FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

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
