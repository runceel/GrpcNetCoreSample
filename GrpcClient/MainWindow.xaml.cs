using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GrpcSample;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;

namespace GrpcClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] Scopes { get; } = new[] { "api://サーバー側のアプリのアプリID/Call.API" }; // 自分で作ったスコープの名前です
        private readonly IPublicClientApplication _app;
        public MainWindow()
        {
            InitializeComponent();

            // 本当は設定(appsettings.json とか)から読む
            var options = new PublicClientApplicationOptions
            {
                ClientId = "クライアントアプリのアプリID",
                RedirectUri = "http://localhost",
                TenantId = "テナント ID",
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

        private async void CallGrpcServiceForAdminButton_Click(object sender, RoutedEventArgs e)
        {
            var accessToken = await GetAccessTokenAsync();

            var jwt = new JwtSecurityToken(accessToken);
            //var groupId = jwt.Claims.FirstOrDefault(x => x.Type == "groups")?.Value;
            //if (groupId != "admins グループオブジェクト ID")
            //{
            //    MessageBox.Show("You are not a member of admins group, right? Please do not click this button.");
            //    return;
            //}

            var roleName = jwt.Claims.FirstOrDefault(x => x.Type == "roles")?.Value;
            if (roleName != "Admins")
            {
                MessageBox.Show("You are not a member of admins group, right? Please do not click this button.");
                return;
            }

            using (var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001")
            })
            {
                var greetServices = Grpc.Net.Client.GrpcClient.Create<Greeter.GreeterClient>(client);
                var response = await greetServices.GreetForAdminAsync(new GreetRequest
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
    }
}
