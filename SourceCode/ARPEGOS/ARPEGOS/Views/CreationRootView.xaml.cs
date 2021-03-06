﻿using ARPEGOS.Helpers;
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
    public partial class CreationRootView: ContentPage
    {
        RadioButton lastChecked;
        public CreationRootView ()
        {
            InitializeComponent();
            this.BindingContext = new CreationRootViewModel();
        }
        void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var activeRadioButton = sender as RadioButton;
            if(lastChecked != null)
                lastChecked.TextColor = Color.LightGreen;
            activeRadioButton.TextColor = Color.Black;
            if (activeRadioButton != lastChecked && lastChecked != null)
                lastChecked.IsChecked = false;
            lastChecked = activeRadioButton.IsChecked ? activeRadioButton : null;
            var viewModel = this.BindingContext as CreationRootViewModel;
            viewModel.SelectedItem = activeRadioButton.BindingContext as Item;
            if (lastChecked != null)
                viewModel.Continue = true;
            else
                viewModel.Continue = false;
        }
    }
}