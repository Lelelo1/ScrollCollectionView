using System;
namespace ScrollCollectionViewTest
{
    public class TestControl : Xamarin.Forms.Button
    {
        [Bindable]
        public string MyText { get; set; }
        void OnMyTextChanged(string s) { this.Text = s; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class BindableAttribute : Attribute { }
}
