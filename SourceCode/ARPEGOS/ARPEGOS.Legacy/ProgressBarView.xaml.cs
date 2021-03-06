﻿using ARPEGOS.ViewModels;
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
    public partial class ProgressBarView : ContentView
    {
        public ProgressBarView()
        {
            InitializeComponent();
            this.BindingContext = App.Container.Resolve<ProgressBarViewModel>();
        }
    }
}