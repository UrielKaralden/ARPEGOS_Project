﻿using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Themes;
using ARPEGOS.ViewModels;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThemeSelectionView : ContentPage
    {
        RadioButton lastChecked;
        public ThemeSelectionView()
        {
            InitializeComponent();
            this.BindingContext = DependencyHelper.Container.Resolve<ThemeSelectionViewModel>();
        }
        public void OnCheckedChanged(Object sender, EventArgs e)
        {
            var activeRadioButton = sender as RadioButton;
            if (lastChecked != null)
                lastChecked.TextColor = Color.LightGreen;
            activeRadioButton.TextColor = Color.Black;
            if (activeRadioButton != lastChecked && lastChecked != null)
                lastChecked.IsChecked = false;
            lastChecked = activeRadioButton.IsChecked ? activeRadioButton : null;
            var themeItem = activeRadioButton.BindingContext as ThemeItem;
            var option = themeItem.Theme;            
            Application.Current.Resources.MergedDictionaries.Clear();
            switch (option)
            {
                case "Día": App.Current.Resources.MergedDictionaries.Add(new LightTheme()); break;
                case "Bosque": App.Current.Resources.MergedDictionaries.Add(new ForestTheme()); break;
                case "Desierto": App.Current.Resources.MergedDictionaries.Add(new DesertTheme()); break;
                case "Tundra": App.Current.Resources.MergedDictionaries.Add(new TundraTheme()); break;
                case "Valle": App.Current.Resources.MergedDictionaries.Add(new ValleyTheme()); break;
                case "Oceano": App.Current.Resources.MergedDictionaries.Add(new OceanTheme()); break;
                default: App.Current.Resources.MergedDictionaries.Add(new DarkTheme()); break;
            }
            var themeHelper = DependencyHelper.CurrentContext.Themes;
            themeHelper.SetCurrentTheme(option);
            themeHelper.SetBackground(option);
            themeHelper.SetAddImage(option);
            themeHelper.SetRemoveImage(option);
        }
    }
}