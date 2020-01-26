using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Namespace;
namespace ScrollCollectionViewTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
            var scroll = new Namespace.ScrollCollectionView();
            scroll.ItemsSource = (IList<object>)(BindingContext as ViewModel).TestList;
            scroll.ItemTemplate = new DataTemplate(() =>
            {
                return new Label() { Text = "yoo" };
            });
        }
    }
}
