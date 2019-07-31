using System;
using System.Collections.Generic;
using Namespace;
using Xamarin.Forms;

namespace ScrollCollectionViewTest.Views
{
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();
            scrollCollectionView.Build = (model) =>
            {
                string number = model as string;
                switch(number)
                {
                    case "three":
                        return specialTemplate;
                }
                return defaultTemplate;
            };
        }
        void Handle_Add(object s, EventArgs e)
        {
            testViewModel.TestObservableCollection.Add("number");
        }
        void Handle_Remove(object s, EventArgs e)
        {

            testViewModel.TestObservableCollection.Remove("one");
        }
    }
}
