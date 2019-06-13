using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using System.Collections.Specialized;
using System.Reflection;

namespace ScrollListView
{

    public partial class ScrollListView : StackLayout
    {

        public StackLayout Container
        {
            get { return container; }
        }

        public static readonly BindableProperty EntryProperty =
            BindableProperty.Create("Entry",
                typeof(Entry),
                typeof(ScrollListView),
                default(Entry),
                propertyChanged: EntryPropertyChanged);

        public Entry Entry
        {
            get { return (Entry)GetValue(EntryProperty); }
            set { SetValue(EntryProperty, value); }
        }

        private static void EntryPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("bindable: " + bindable);
            Console.WriteLine("newValue: " + newValue);
        }
        /* might support
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(propertyName: "ItemTemplate",
                 returnType: typeof(DataTemplate),
                 declaringType: typeof(AutocompleteCollectionView),
                 defaultValue: default(DataTemplate),
                 propertyChanged: ItemTemplatePropertyChanged);
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        private static void ItemTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("newValue: " + newValue);
            (bindable as AutocompleteCollectionView).listView.ItemTemplate = newValue as DataTemplate;

        }
        */
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(
                propertyName: "Text",
                returnType: typeof(string),
                declaringType: typeof(ScrollListView),
                defaultValue: default(string),
                propertyChanged: TextPropertyChanged);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("Text changed: " + newValue);
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                propertyName: "ItemsSource",
                returnType: typeof(IEnumerable),
                declaringType: typeof(ScrollListView),
                defaultValue: default(IEnumerable),
                propertyChanged: ItemsSourcePropertyChanged);

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { Console.WriteLine("set agg: " + value); SetValue(ItemsSourceProperty, value); }
        }

        // require a observablecollection to react on changes in the list. just like ListView
        private static void ItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // (bindable as AutocompleteCollectionView).listView.ItemsSource = newValue as IEnumerable;
            // https://github.com/xamarin/Xamarin.Forms/blob/master/Xamarin.Forms.Core/ItemsView.cs
            // https://github.com/HoussemDellai/Xamarin-Forms-RepeaterView/blob/master/Repeater/Repeater/RepeaterView.cs
            // can probably improve this rendering...
            Console.WriteLine("instance bindingContext is: " + ((ScrollListView)bindable).BindingContext);
            Console.WriteLine("ItemsSource changed: " + newValue);

            var instance = bindable as ScrollListView;

            if (instance == null) return;

            if (oldValue != null)
            {
                if (oldValue is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged -= instance.CollectionChanged;
                }
            }

            if (newValue != null)
            {
                if(instance.Build != null)
                {
                    instance.BuildAll();
                }

                if (instance.ItemsSource is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged += instance.CollectionChanged;
                }
            }

            /* For using mobx this has to be fixed https://github.com/kekekeks/NObservable/issues/3
            NObservable.Observe.Autorun(() =>
            {
                var l = instance.list;
                Console.WriteLine("list changed: " + l);
                var children = instance.container.Children as List<View>;
                var b = children.Select((View view) => view.BindingContext);
                Console.WriteLine("bindingContexts was: ");
                foreach(var x in b)
                {
                    Console.WriteLine(x);
                }
                var changes = b.Except(l);
                Console.WriteLine("Changes where: ");
                foreach(var c in changes)
                {
                    Console.WriteLine(c);
                }

            });
            */
        }

        // https://github.com/DottorPagliaccius/Xamarin-Custom-Controls/blob/master/src/Xamarin.CustomControls.Repeater/RepeaterView.cs
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            /*
            Console.WriteLine("oldItems.. ");
            foreach(var item in e.OldItems)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("newItems.. ");
            foreach (var item in e.NewItems)
            {
                Console.WriteLine(item);
            }
            */
            var items = ItemsSource.Cast<object>().ToList();
            var futureHeight = new List<Task<double>>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    var index = e.NewStartingIndex;
                    Console.WriteLine("Add");

                    foreach (var newItem in e.NewItems)
                    {
                        container.Children.Insert(index++, AddFutureHeight(futureHeight, newItem));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    Console.WriteLine("Move");
                    var moveItem = items[e.OldStartingIndex];

                    container.Children.RemoveAt(e.OldStartingIndex);
                    container.Children.Insert(e.NewStartingIndex, ViewFor(moveItem));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    container.Children.RemoveAt(e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    Console.WriteLine("Replace. oldIndex: " + e.OldStartingIndex + " and newIndex: " + e.NewStartingIndex);
                    container.Children.RemoveAt(e.OldStartingIndex);
                    container.Children.Insert(e.NewStartingIndex, AddFutureHeight(futureHeight, items[e.NewStartingIndex]));
                    break;

                case NotifyCollectionChangedAction.Reset:

                    BuildAll();
                    break;
            }
            
            if (container.Children.Count >= MaxItemsShown) // is correct
            {
                Task.WhenAll(futureHeight).ContinueWith((args) =>
                {
                    double maxHeight = 0;
                    for (int i = 0; i < MaxItemsShown; i++)
                    {
                        maxHeight += container.Children[i].Height;
                    }
                    scrollView.HeightRequest = maxHeight;
                });
            }
            
        }
        private View AddFutureHeight(List<Task<double>> futureHeight, object newItem)
        {
            var view = ViewFor(newItem);
            var getSize = new AsyncEventListener();
            view.SizeChanged += getSize.Listen;
            futureHeight.Add(getSize.Successfully);
            return view;
        }
        // using DataTemplate instead of View until issue is fixed: https://github.com/xamarin/Xamarin.Forms/issues/6476
        public static readonly BindableProperty BuildProperty =
           BindableProperty.Create(
                propertyName: "Build",
                returnType: typeof(Func<object, DataTemplate>),
                declaringType: typeof(ScrollListView),
                defaultValue: default(Func<object, DataTemplate>), // define a default: When ItemSource is missing throw exception, when having itemsource - might build a default rows
                propertyChanged: BuildPropertyChanged);

        public Func<object, DataTemplate> Build
        {
            get { return (Func<object, DataTemplate>)GetValue(BuildProperty); }
            set { Console.WriteLine("set build: " + value); SetValue(BuildProperty, value); }
        }
        private static void BuildPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine("build changed");
            var instance = bindable as ScrollListView;

            if (instance == null) return;

            if (instance.ItemsSource != null)
            {
                instance.BuildAll();
            }
        }

        // can't us ListView/CollectionView https://github.com/xamarin/Xamarin.Forms/issues/5942
        // https://stackoverflow.com/questions/56405488/cant-create-content-with-datatemplate-in-code-behind-with-control-defined-in-xa
        // clone is neccesery to to geneate new ids
        // check is View or is Layout

        private void BuildAll() // keep in mind when setting a height - SizeChanged is never fired - so MaxItemsShown is inactive
        {
            container.Children.Clear();
            int index = 0;
            List<Task<double>> setMaxHeight = new List<Task<double>>();
            foreach (var value in ItemsSource)
            {
                var futureHeight = Render(value);
                if (index < MaxItemsShown)
                {
                    setMaxHeight.Add(futureHeight);
                }
                index++;
            }
            Task.WhenAll(setMaxHeight).ContinueWith(async (arg) =>
            {
                double[] d = await arg;
                scrollView.HeightRequest = d.Sum();
            });
        }
        
        public Task<double> Render(object value)
        {
            
            var build = ViewFor(value);
            AsyncEventListener getSize = new AsyncEventListener();
            build.SizeChanged += getSize.Listen;
            container.Children.Add(build);
            return getSize.Successfully; // or width if supporting horizontal and used like list/carousel
        }

        public View ViewFor(object value)
        {
            var dataTemplate = Build(value);
            var build = dataTemplate.CreateContent() as View;
            build.BindingContext = value;

            return build;
        }
        /*
        // when cloning with: https://github.com/AlenToma/FastDeepCloner it does not clone everything sucesfully - including _id
        // asnwer at https://stackoverflow.com/questions/28316495/clone-tree-data-structure-with-back-references
        public View Clone(View original) // need to clone commands and gesturerecognizers as well
        {
            try
            {
                var originalClone = FastDeepCloner.DeepCloner.Clone(original); // new id created

                if(originalClone is Label)
                {
                    Console.WriteLine("label: " + ((Label)original).BindingContext);
                    Console.WriteLine("labelClone: " + ((Label)originalClone).BindingContext);

                }

                foreach(var g in original.GestureRecognizers)
                {
                    var gClone = Force.DeepCloner.DeepClonerExtensions.ShallowClone(g); // creates exact clone
                    Console.WriteLine("added : " + gClone);
                    originalClone.GestureRecognizers.Add(gClone);
                }

                Layout<View> layout = original as Layout<View>;
                if (layout != null)
                {
                    foreach (View child in layout.Children)
                    {
                        (originalClone as Layout<View>).Children.Add(Clone(child));
                    }
                }
                return originalClone;
            }
            catch (Exception exc)
            {
                // many exceptions are now thrown leading to the actual exception (Xamarin.Forms.Button) // might want to try and limit it to only the actual exception
                // https://github.com/AlenToma/FastDeepCloner/issues/4
                throw new Exception("FastDeepCloner was unable to clone: " + original + ". With following Exception thrown: " + exc);
            }
            // DeepCopy.ObjectCloner.Clone


        }
        
        /*      About cloning:
                Console.WriteLine("old id: " + original.Id);
                // var originalClone = Force.DeepCloner.DeepClonerExtensions.ShallowClone(original); // does not generate new id
                // originalClone = Force.DeepCloner.DeepClonerExtensions.DeepClone(originalClone); (( does not create new id and breakes debug -page won't display
                var originalClone = CloneExtensions.CloneFactory.GetClone(new Label() { Text = "teeeext" }); 

                Console.WriteLine("new id: " + originalClone.Id);
                // Needed for var: FastDeepCloner.DeepCloner.Clone(original);  // https://github.com/AlenToma/FastDeepCloner // generates new id

                var settings = new FastDeepCloner.FastDeepClonerSettings() I different settin sdoes not hel
                {
                    FieldType = FastDeepCloner.FieldType.PropertyInfo
                };

                */

        // both listview and collectionview expand - having empty space within

        // max height can be given by amount of item. display items. And device height should limit it too
        public static BindableProperty MaxItemsShownProperty =
            BindableProperty.Create(
                propertyName: "MaxItemsShownProperty",
                returnType: typeof(int),
                declaringType: typeof(ScrollListView),
                defaultValue: 5, // or what is the max suggestions given by googleplacesautocomplete?
                propertyChanged: MaxItemsShownPropertyChanged);

        public int MaxItemsShown
        {
            get { return (int)GetValue(MaxItemsShownProperty); }
            set { SetValue(MaxItemsShownProperty, value); }
        }
        public static void MaxItemsShownPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = bindable as ScrollListView;
        }
        public ScrollListView()
        {
            InitializeComponent();

            // listView.HasUnevenRows = true; // fix withcollectioview as well
            
        }
        private void OnBindingContextChanged(object sender, EventArgs e)
        {

        }
        private void setHeight()
        {
            // collectionView.ItemTemplate.
        }
        private void testOut(out string h)
        {
            h = "HELLOOO";
        }
        /*
        BindableProperty ListTypeProperty =
             BindableProperty.Create(propertyName: "ListType",
                 returnType: typeof(Type),
                 declaringType: typeof(AutocompleteCollectionView),
                 defaultValue: Type.CollectionView);
        public Type ListType
        {
            get { return (Type)GetValue(ListTypeProperty); }
            set { SetValue(ListTypeProperty, value); }
        }
        public enum Type
        {
            CollectionView,
            ListView
        }
        */
    }
    // https://stackoverflow.com/questions/12858501/is-it-possible-to-await-an-event-instead-of-another-async-method by Anders Skovborg
    // used to get bounds after views are displayed - to set maxHeight with MaxItemsShown
    public class AsyncEventListener
    {
        private object sender;
        private EventArgs eventArgs;
        public AsyncEventListener()
        {
            Func<double> func = () =>
            {
                if (sender is View view) // sufficiently supporting all controls?
                {
                    Console.WriteLine("Got height");
                    view.SizeChanged -= this.Listen;
                    return view.Height;
                }
                throw new Exception("AsyncEventListener could not listen for SizeChanged on " + sender + " as it was not a View");
            };
            Successfully = new Task<double>(func);
        }

        public void Listen(object sender, EventArgs eventArgs)
        {
            this.sender = sender;
            this.eventArgs = eventArgs;
            if (!Successfully.IsCompleted)
            {
                Console.WriteLine("running synchornisously");
                Successfully.RunSynchronously();

            }
        }

        public Task<double> Successfully { get; }
    }
    public class O : Object
    {
        void doo()
        {
            this.MemberwiseClone();
        }
    }
    // When new row is added - might scroll down to it by defalt
    // When scolling on slider the scroll view won't scroll. It does not help wrapping content in a DataTemplate. tested with CollectionView and same problem
}
