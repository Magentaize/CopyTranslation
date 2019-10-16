using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace CopyTranslation.Native
{
    internal sealed class AppService
    {
        private AppServiceConnection connection = null;
        private AutoResetEvent are;

        public async Task RunAsync()
        {
            await InitializeAppServiceConnectionAsync();
            are = new AutoResetEvent(false);
            await Task.Run(() => are.WaitOne());
        }

        private string GetClipboard()
        {
            try
            {
                return TextCopy.Clipboard.GetText() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var op = args.Request.Message["Op"] as string;
            var res = new ValueSet();

            if (op == "Clipboard")
            {
                res.Add("Res", await RunAsync(GetClipboard));
            }
            else
            {
                res.Add("Res", string.Empty);
            }

            Debug.WriteLine($"Clipboard: {res["Res"].ToString()}");
            var status = await args.Request.SendResponseAsync(res);
            deferral.Complete();
            Debug.WriteLine($"SendStatus: {status.ToString()}");
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            are.Set();
        }

        private async Task InitializeAppServiceConnectionAsync()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "ClipboardInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            var status = await connection.OpenAsync();
            Debug.WriteLine($"AppService Status: {status.ToString()}");
        }

        private static Task<TResult> RunAsync<TResult>(Func<TResult> func, ApartmentState state = ApartmentState.STA)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    var result = func();

                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            thread.SetApartmentState(state);
            thread.Start();

            return tcs.Task;
        }
    }
}
