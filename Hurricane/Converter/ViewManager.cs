using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Hurricane.ViewModel.MainView;
using CollectionView = Hurricane.ViewModel.MainView.CollectionView;

namespace Hurricane.Converter
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
                    {typeof (ChartsView), typeof (WindowSkinNormal.Views.ChartsView)},
                    {
                        typeof (CollectionView),
                        typeof (WindowSkinNormal.Views.CollectionView)
                    },
                    {typeof (PlaylistView), typeof (WindowSkinNormal.Views.PlaylistView)},
                    {typeof (QueueView), typeof (WindowSkinNormal.Views.QueueView)},
                    {typeof (HistoryView), typeof (WindowSkinNormal.Views.HistoryView)}
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

            var view = (FrameworkElement)Activator.CreateInstance(viewType);
            view.DataContext = viewModel;

            _cachedViews.Add(viewModelType, view);
            return view;
        }
    }
}
