using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hurricane.WindowSkinNormal.Views
{
    class ViewManager : IValueConverter
    {
        private readonly Dictionary<Type, FrameworkElement> _cachedViews;
        private static ReadOnlyDictionary<Type, Type> _viewsViewModels; 

        public ViewManager()
        {
            _cachedViews = new Dictionary<Type, FrameworkElement>();

            if (_viewsViewModels == null)
                _viewsViewModels = new ReadOnlyDictionary<Type, Type>(new Dictionary<Type, Type>
                {
                    {typeof (ViewModel.MainView.ChartsView), typeof (ChartsView)},
                    {
                        typeof (ViewModel.MainView.CollectionView),
                        typeof (CollectionView)
                    },
                    {typeof (ViewModel.MainView.PlaylistView), typeof (PlaylistView)},
                    {typeof (ViewModel.MainView.QueueView), typeof (QueueView)},
                    {typeof (ViewModel.MainView.HistoryView), typeof (HistoryView)}
                });
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var type = value.GetType();

            if (_viewsViewModels.ContainsKey(type))
                return GetView(type, value, _viewsViewModels[type]);
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private FrameworkElement GetView(Type viewModelType, object viewModel, Type viewType)
        {
            if (_cachedViews.ContainsKey(viewModelType))
                return _cachedViews[viewModelType];

            var view = (FrameworkElement) Activator.CreateInstance(viewType);
            view.DataContext = viewModel;

            _cachedViews.Add(viewModelType, view);
            return view;
        }
    }
}
