using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TPIH.Gecco.WPF.Core;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyChangedEventArgs> _eventArgsCache;

        protected ViewModelBase()
        {
            _eventArgsCache = new Dictionary<string, PropertyChangedEventArgs>();
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {                        
            PropertyChangedEventArgs args;
            if (!_eventArgsCache.TryGetValue(propertyName, out args))
            {
                args = new PropertyChangedEventArgs(propertyName);
                _eventArgsCache.Add(propertyName, args);
            }
            OnPropertyChanged(args);            
        }
        protected virtual void OnPropertyChanged(Expression<Func<object>> expression)
        {
            this.PropertyChanged.Raise(this, expression);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        public void OnError(Exception exp)
        {
            GlobalCommands.ShowError.Execute(exp);
        }

        #endregion
    }
}
