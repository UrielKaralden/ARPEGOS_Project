﻿
namespace ARPEGOS.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Services.Interfaces;
    using ARPEGOS.ViewModels.Base;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MainViewModel : BaseViewModel
    {
        private SelectionStatus _selectionStatus,_previousStatus;

        private string selectedGame;

        private IDialogService dialogService;

        private bool _cancelEnabled;

        public bool CancelEnabled
        { 
            get => _cancelEnabled;
            set => this.SetProperty(ref this._cancelEnabled, value);
        }

        public ICommand AddButtonCommand { get; set; }
        public ICommand ClearCheckCommand { get; }
        public ICommand DeleteButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand PushSkillViewCommand { get; }

        public ICommand LoadNewStateCommand { get; set; }

        public string SelectedGame
        {
            get => this.selectedGame;
            set => this.SetProperty(ref this.selectedGame, value);
        }

        public ObservableCollection<string> SelectableElements { get; }

        public SelectionStatus CurrentStatus
        {
            get => this._selectionStatus;
            set
            {
                this.SetProperty(ref this._selectionStatus, value);
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public SelectionStatus PreviousStatus
        {
            get => this._previousStatus;
            set
            {
                this.SetProperty(ref this._previousStatus, value);
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public new string Title
        {
            get
            {
                return this.CurrentStatus switch
                    {
                        SelectionStatus.SelectingGame => "Selecciona el juego",
                        SelectionStatus.SelectingCharacter => "Selecciona el personaje",
                        SelectionStatus.DeletingGame => "Selecciona el juego",
                        _ => "Carga completada!"
                    };
            }
        }

        public MainViewModel (IDialogService dialogService)
        {
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingGame;
            this.PreviousStatus = this.CurrentStatus;
            this.dialogService = dialogService;
            this.CancelEnabled = false;
            this.AddButtonCommand = new Command(async() => 
            {
                switch(this.CurrentStatus)
                {
                    case SelectionStatus.SelectingGame:
                        await App.Navigation.PushAsync(new AddGameView());
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.SelectingCharacter:
                        var item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                            await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new NavigationPage(new CreationRootView())));
                        }
                        this.Load(this.CurrentStatus);
                        break;
                }
            });

            this.DeleteButtonCommand = new Command(() => 
            {
                this.PreviousStatus = this.CurrentStatus;
                switch(this.PreviousStatus)
                {
                    case SelectionStatus.SelectingGame: this.CurrentStatus = SelectionStatus.DeletingGame; break;
                    case SelectionStatus.SelectingCharacter: this.CurrentStatus = SelectionStatus.DeletingCharacter; break;
                    case SelectionStatus.DeletingGame: this.CurrentStatus = SelectionStatus.SelectingGame; break;
                    case SelectionStatus.DeletingCharacter: this.CurrentStatus = SelectionStatus.SelectingCharacter; break;
                }
                this.CancelEnabled = !this.CancelEnabled;
                this.Load(CurrentStatus); 
            });

            this.CancelButtonCommand = new Command(() => 
            {
                this.CancelEnabled = false;
                this.CurrentStatus = this.PreviousStatus;  
                this.Load(CurrentStatus); 
            });

            this.LoadNewStateCommand = new Command<SelectionStatus>(status => {this.PreviousStatus = this.CurrentStatus; this.CurrentStatus = status;  this.Load(status); });
        }

        public async Task Init()
        {
            if (!base.initialized)
            {
                this.IsBusy = true;
                try
                {
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                }
                finally
                {
                    this.IsBusy = false;
                    this.initialized = true;
                }
            }
        }

        private async Task SelectItem(string item)
        {
            if (this.IsBusy)
                return;

            this.IsBusy = true;
            switch (this.CurrentStatus)
            {
                case SelectionStatus.SelectingGame:
                    this.SelectedGame = item;
                    MainThread.BeginInvokeOnMainThread(async() => await App.Navigation.PushAsync(new SelectVersionView(dialogService)));
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.SelectingCharacter:
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (string.IsNullOrWhiteSpace(item))
                            break;
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                    }
                    else
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.LoadCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                    await MainThread.InvokeOnMainThreadAsync(async() => await App.Navigation.PushAsync(new OptionsView()));
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.Done;
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.DeletingGame:
                    this.CancelEnabled = true;
                    var confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if (confirmation == true)
                        await MainThread.InvokeOnMainThreadAsync(()=> FileService.DeleteGame(this.selectedGame));
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(CurrentStatus);
                    break;                    

                case SelectionStatus.DeletingCharacter:
                    this.CancelEnabled = true;
                    confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if(confirmation == true)
                        await MainThread.InvokeOnMainThreadAsync(() => FileService.DeleteCharacter(item, this.selectedGame));                    
                    this.CurrentStatus = SelectionStatus.SelectingCharacter;
                    this.Load(this.CurrentStatus);
                    break;

                default:
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                    break;
            }

            this.IsBusy = false;
        }

        private void Load(SelectionStatus status)
        {
            IEnumerable<string> items;
            switch (status)
            {
                case SelectionStatus.SelectingGame:
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.SelectingCharacter:
                    items = FileService.ListCharacters(this.SelectedGame);
                    break;
                case SelectionStatus.Done:
                    items = new string[0];
                    #if DEBUG
                    items = new List<string> { "Volver a empezar" };
                    #endif
                    break;
                default:
                    return;
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if(items != null)
                {
                    this.SelectableElements.Clear();
                    this.CancelEnabled = false;
                    this.SelectableElements.AddRange(items);
                    if (status == SelectionStatus.SelectingCharacter)
                        this.SelectableElements.Add(string.Empty);
                }
            });
        }

        public enum SelectionStatus
        {
            AddGame,
            AddCharacter,
            SelectingGame,
            SelectingVersion,
            SelectingCharacter,
            DeletingGame,
            DeletingCharacter,
            Done
        }
    }
}
