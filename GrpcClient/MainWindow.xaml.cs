using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GrpcSample;

namespace GrpcClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CallGrpcServiceButton_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001")
            })
            {
                var greetServices = Grpc.Net.Client.GrpcClient.Create<Greeter.GreeterClient>(client);
                var response = await greetServices.GreetAsync(new GreetRequest
                {
                    Name = textBoxName.Text,
                });
                MessageBox.Show(response.Message);
            }
        }
    }
}
