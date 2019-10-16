﻿using CopyTranslation.ViewModels;
using ReactiveUI;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CopyTranslation.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

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
                //App.AppServiceConnected += MainPage_AppServiceConnected;
                App.AppServiceDisconnected += App_AppServiceDisconnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
        }

        private void App_AppServiceDisconnected(object sender, EventArgs e)
        {

        }

        //private async void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        //{
        //    App.Connection.RequestReceived += AppServiceConnection_RequestReceived;
        //}
    }
}
