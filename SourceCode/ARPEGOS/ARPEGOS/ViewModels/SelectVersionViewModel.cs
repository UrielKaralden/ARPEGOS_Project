﻿using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.Services.Interfaces;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class SelectVersionViewModel: BaseViewModel
    {
        private SelectionStatus _selectionStatus;
        private IDialogService dialogService;
        private string selectedGame;
        private bool deleteMode;
        public SelectionStatus CurrentStatus
        {
            get => this._selectionStatus;
            set
            {
                this.SetProperty(ref this._selectionStatus, value);
                this.OnPropertyChanged(nameof(this.Title));
            }
        }
        public bool DeleteMode 
        { 
            get => this.deleteMode; 
            set => this.SetProperty(ref this.deleteMode, value); 
        }


        public ICommand AddButtonCommand { get; set; }
        public ICommand DeleteButtonCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand ReturnCommand { get; }
        public ObservableCollection<string> SelectableElements { get; }

        public SelectVersionViewModel (string title)
        {
            dialogService = DependencyHelper.CurrentContext.Dialog;
            this.Title = title;
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingVersion;
            this.dialogService = DependencyHelper.CurrentContext.Dialog;
            this.selectedGame = string.Empty;
            this.AddButtonCommand = new Command(async () => await App.Navigation.PushAsync(new AddGameView()));
            this.DeleteButtonCommand = new Command(() =>
            {
                this.CurrentStatus = SelectionStatus.DeletingVersion;
                this.Load(this.CurrentStatus);
            });

            this.ReturnCommand = new Command(async() => await Device.InvokeOnMainThreadAsync(async() => await App.Navigation.PopToRootAsync()));
            this.Load(this.CurrentStatus);
        }

    
        private async Task SelectItem (string item)
        {
            if (this.IsBusy)
                return;

            this.IsBusy = true;
            switch (this.CurrentStatus)
            {
                case SelectionStatus.SelectingVersion:
                    var previousViewModel = App.Navigation.NavigationStack.First().BindingContext as MainViewModel;
                    selectedGame = previousViewModel.SelectedGame;
                    DependencyHelper.CurrentContext.CurrentGame = await Task.Run(async () => await OntologyService.LoadGame(selectedGame, item));
                    previousViewModel.LoadNewStateCommand.Execute(MainViewModel.SelectionStatus.SelectingCharacter);
                    this.CurrentStatus = SelectionStatus.Done;
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.DeletingVersion:
                    var confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if (confirmation == true)
                        await Device.InvokeOnMainThreadAsync(() => FileService.DeleteGameVersion(selectedGame, item));
                    this.CurrentStatus = SelectionStatus.SelectingVersion;
                    this.Load(CurrentStatus);
                    break;

                default:
                    this.CurrentStatus = SelectionStatus.SelectingVersion;
                    this.Load(this.CurrentStatus);
                    break;
            }

            this.IsBusy = false;
        }

        private void Load (SelectionStatus status)
        {
            IEnumerable<string> items;
            switch (status)
            {
                case SelectionStatus.SelectingVersion:
                    DeleteMode = false;
                    var previousViewModel = App.Navigation.NavigationStack[App.Navigation.NavigationStack.Count - 1].BindingContext as MainViewModel;
                    selectedGame = previousViewModel.SelectedGame;
                    items = FileService.ListVersions(selectedGame);
                    break;

                case SelectionStatus.DeletingVersion:
                    DeleteMode = true;
                    items = FileService.ListVersions(selectedGame);
                    break;

                default:
                    ReturnCommand.Execute(null);
                    return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                if (items != null)
                {
                    this.SelectableElements.Clear();
                    this.SelectableElements.AddRange(items);
                }
            });
        }

        public enum SelectionStatus
        {
            SelectingVersion,
            DeletingVersion,
            Done
        }

        
    }
}
