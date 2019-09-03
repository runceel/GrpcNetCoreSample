using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GrpcSample;
using Microsoft.Identity.Client;

namespace GrpcClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] Scopes { get; } = new[] { "api://サーバーのアプリID/Call.API" }; // 自分で作ったスコープの名前です
        private readonly IPublicClientApplication _app;
        public MainWindow()
        {
            InitializeComponent();

            // 本当は設定(appsettings.json とか)から読む
            var options = new PublicClientApplicationOptions
            {
                ClientId = "クライアントアプリID",
                RedirectUri = "http://localhost",
                TenantId = "テナントID",
            };
            _app = PublicClientApplicationBuilder.CreateWithApplicationOptions(options).Build();
        }

        private async void CallGrpcServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var accessToken = await GetAccessTokenAsync();

            using (var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001")
            })
            {
                var greetServices = Grpc.Net.Client.GrpcClient.Create<Greeter.GreeterClient>(client);
                var response = await greetServices.GreetAsync(new GreetRequest
                {
                    Name = textBoxName.Text,
                },
                new Grpc.Core.Metadata
                {
                    { "Authorization", $"Bearer {accessToken}" },                
                });
                MessageBox.Show(response.Message);
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            AuthenticationResult r;
            try
            {
                var account = (await _app.GetAccountsAsync())?.FirstOrDefault();
                r = await _app.AcquireTokenSilent(Scopes, account).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                r = await _app.AcquireTokenInteractive(Scopes)
                    .WithSystemWebViewOptions(new SystemWebViewOptions
                    {
                        OpenBrowserAsync = SystemWebViewOptions.OpenWithChromeEdgeBrowserAsync,
                    })
                    .ExecuteAsync();
            }

            return r.AccessToken;
        }
    }
}
