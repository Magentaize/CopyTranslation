using CopyTranslation.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using vm = Windows.UI.ViewManagement;
using cw = Windows.UI.Core.CoreWindow;
using ke = Windows.UI.Core.KeyEventArgs;
using pe = Windows.UI.Core.PointerEventArgs;
using System.Reactive.Concurrency;
using System.Reactive;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;

namespace CopyTranslation.Views
{
    public sealed partial class MainPage : Page, IViewFor<MainPageViewModel>
    {
        public Visibility CompactOverlayVisibility { get; }

        public MainPage()
        {
            if (vm.ApplicationView.GetForCurrentView().IsViewModeSupported(vm.ApplicationViewMode.CompactOverlay))
            {
                CompactOverlayVisibility = Visibility.Visible;
            }

            InitializeComponent();

            ExtendTitleBar();

            VeryFirstLaunch();

            StatusButton.Events()
                .Tapped
                .InvokeCommand(ViewModel.StatusTappedCommand);

            BuildTextblockFontSizeRecognizer();

            this.WhenActivated(d => { });
        }

        private void BuildTextblockFontSizeRecognizer()
        {
            TextblockHotkeyRecognizer();
            TextblockGestureRecognizer();
        }

        private void TextblockHotkeyRecognizer()
        {
            var ctrlDown = Observable.FromEventPattern<TypedEventHandler<cw, ke>, cw, ke>(
                h => Window.Current.CoreWindow.KeyDown += h,
                h => Window.Current.CoreWindow.KeyDown -= h)
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(x => x.EventArgs)
                .Where(x => x.VirtualKey == Windows.System.VirtualKey.Control);
            var ctrlUp = Observable.FromEventPattern<TypedEventHandler<cw, ke>, cw, ke>(
                h => Window.Current.CoreWindow.KeyUp += h,
                h => Window.Current.CoreWindow.KeyUp -= h)
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(x => x.EventArgs)
                .Where(x => x.VirtualKey == Windows.System.VirtualKey.Control);

            Observable.FromEventPattern<TypedEventHandler<cw, pe>, cw, pe>(
                h => Window.Current.CoreWindow.PointerWheelChanged += h,
                h => Window.Current.CoreWindow.PointerWheelChanged -= h)
            .SkipUntil(ctrlDown)
            .TakeUntil(ctrlUp)
            .Repeat()
            .Select(x => x.EventArgs.CurrentPoint.Properties)
            .Subscribe(ViewModel.TranslatedTextFontSizeWheel);
        }

        private void TextblockGestureRecognizer()
        {
            var recognizer = new GestureRecognizer()
            {
                GestureSettings = GestureSettings.ManipulationScale
            };

            recognizer.ManipulationStarted += (_, __) => ViewModel.TranslatedTextFontSizePitchStart.OnNext(Unit.Default);

            recognizer.ManipulationCompleted += (_, __) => ViewModel.TranslatedTextFontSizePitchCompleted.OnNext(Unit.Default);

            recognizer.ManipulationUpdated += (_, e) =>
            {
                ViewModel.TranslatedTextFontSizePitch.OnNext(e.Cumulative.Scale);
            };

            // Panel has override router event, so we must use AddHandler
            // to add our handler

            TranslatedTextViewer.AddHandler(PointerPressedEvent,
                new PointerEventHandler((_, e) =>
                {
                    TranslatedText.CapturePointer(e.Pointer);
                    recognizer.ProcessDownEvent(e.GetCurrentPoint(TranslatedTextPanel));
                }),
                true);

            TranslatedTextViewer.AddHandler(PointerMovedEvent,
                new PointerEventHandler((_, e) =>
                {
                    recognizer.ProcessMoveEvents(e.GetIntermediatePoints(TranslatedTextPanel));
                }),
                true);

            TranslatedTextViewer.AddHandler(PointerReleasedEvent,
                new PointerEventHandler((_, e) =>
                {
                    recognizer.ProcessUpEvent(e.GetCurrentPoint(TranslatedTextPanel));
                    TranslatedText.ReleasePointerCapture(e.Pointer);
                }),
                true);

            TranslatedTextViewer.AddHandler(PointerCanceledEvent,
                new PointerEventHandler((_, e) =>
                {
                    recognizer.CompleteGesture();
                    TranslatedText.ReleasePointerCapture(e.Pointer);
                }),
                true);
        }

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();
        MainPageViewModel IViewFor<MainPageViewModel>.ViewModel { get => ViewModel; set => throw new System.NotImplementedException(); }
        object IViewFor.ViewModel { get => ViewModel; set => throw new System.NotImplementedException(); }

        private void VeryFirstLaunch()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // ResizeWindow
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

        bool compactOverlayEnabled;
        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (compactOverlayEnabled)
            {
                var modeSwitched = await vm.ApplicationView.GetForCurrentView().TryEnterViewModeAsync(vm.ApplicationViewMode.Default);
                if (modeSwitched) compactOverlayEnabled = false;
            }
            else
            {
                var modeSwitched = await vm.ApplicationView.GetForCurrentView().TryEnterViewModeAsync(vm.ApplicationViewMode.CompactOverlay);
                if (modeSwitched) compactOverlayEnabled = true;
            }
        }

        private void ExtendTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);
            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            var titleBar = vm.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            AppTitleBar.Height = coreTitleBar.Height;
        }
    }
}
