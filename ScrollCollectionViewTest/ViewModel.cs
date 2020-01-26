using System;
using System.Collections.Generic;
using NObservable;
using ScrollCollectionView;
using ScrollCollectionViewTest;
namespace ScrollCollectionViewTest
{
    public class ViewModel
    {

        public List<string> TestList { get; set; } = new List<string>() { "One", "Two", "Three", "Four", "Five" };

        [Observable]
        public string _initObservble { get; set; }
        public ViewModel()
        {
            // new Bindable(this);
        }
    }
    
    
    
}
