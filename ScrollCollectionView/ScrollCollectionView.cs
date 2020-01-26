using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Specialized;
using System.Reflection;
using NObservable;
using ScrollCollectionView;

namespace Namespace
{
    public class ScrollCollectionView : ScrollView
    {

        public ScrollCollectionView()
        {
            
            Console.WriteLine("contructor");
            new Bindable(this);
            Console.WriteLine("made bindable");
            Content = Container;
            Margin = 0;

            Observe.Autorun(() =>
            {
                if(_ItemsSource == null || _ItemsSource.Count == 0)
                {
                    Console.WriteLine("clear");
                    //Container.Children.Clear();
                }
                else
                {
                    foreach (var item in _ItemsSource)
                    {
                        Console.WriteLine("listening on object: " + item);
                        Listen(item);
                    }
                }

            });

        }
        public StackLayout Container { get; set; } = new StackLayout() { Margin = 0 };
        void Listen(object item)
        {
            if(item == null)
            {
                Container.Children.RemoveAt(_ItemsSource.IndexOf(item));
            }

            DataTemplate render = null;
            if (Build == null && ItemTemplate == null)
            {
                Console.WriteLine("You forgot to provide either an ItemTemplate or a Build function to ScrollCollectionView");
                return;
            }

            render = Build != null ? Build(item) : ItemTemplate;

            var view = (View)render.CreateContent();
            if (!Container.Children.Contains(view))
            {
                Container.Children.Insert(_ItemsSource.IndexOf(item), view);
            }
        }
        // https://github.com/nicolo-ottaviani/Xamarin.BindableProperty.Fody/issues
        // [Observable]
        public string Text { get; set; }
        // bindable makes the properties discoverable in xaml for the contol


        [Observable]
        IList<object> _ItemsSource { get; set; }
        [Bindable]
        public IList<object> ItemsSource { get; set; }
        void OnItemsSourceChanged(IList<object> itemsSource)
        {
            Console.WriteLine("Itemssource changed");
           // _ItemsSource = itemsSource;
        }

        public DataTemplate ItemTemplate { get; set; }
        public Func<object, DataTemplate> Build { get; set; }

        public int MaxItemsShown { get; set; } = 0;
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class BindableAttribute : Attribute { }
}
