using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using NObservable;
using Xamarin.Forms;
using System.Reflection.Emit;

namespace ScrollCollectionView
{

    // https://github.com/kekekeks/NObservable/issues/3
    public class Bindable : INotifyPropertyChanged
    {
        public Bindable(object self = null) // <-- when creating custom forms control 
        {
            var instance = self != null ? self : this;

            var properties = instance.GetType().GetRuntimeProperties();
            foreach (var p in properties)
            {
                Observe.Autorun(() =>
                {
                    var run = p.GetValue(instance);
                    OnPropertyChanged(p.Name);
                });
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
