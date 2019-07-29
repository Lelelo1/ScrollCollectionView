using System;
using System.Collections.ObjectModel;

namespace ScrollCollectionViewTest.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public ObservableCollection<string> TestObservableCollection { get; set; }
            = new ObservableCollection<string>()
                { "one", "two", "three", "four",
                    "five", "six", "seven", "eight" };
    }
}
