using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ScrollCollectionViewTest.Views;

namespace ScrollCollectionViewTest
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var shell = new AppShell();
            shell.CurrentItem = new ShellContent() { ContentTemplate = new DataTemplate(() => new TestPage()) };
            MainPage = shell;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
