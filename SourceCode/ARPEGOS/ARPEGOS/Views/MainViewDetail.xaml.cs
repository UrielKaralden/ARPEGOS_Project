﻿
namespace ARPEGOS.Views
{
    using ARPEGOS.ViewModels;

    using Autofac;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewDetail : ContentPage
    {
        public MainViewDetail()
        {
            this.InitializeComponent();
            this.BindingContext = App.Container.Resolve<MainViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (this.BindingContext is MainViewModel vm)
            {
                await vm.Init();
            }
        }
    }
}