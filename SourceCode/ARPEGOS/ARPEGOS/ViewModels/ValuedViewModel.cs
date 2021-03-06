﻿using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using FFImageLoading.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class ValuedViewModel: BaseViewModel
    {
        #region Properties
        #region Private
        private DialogService dialogService = new DialogService();
        private ObservableCollection<Item> data;
        private Stage currentStage;
        private string stageName, stageLimitProperty;
        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private double currentLimit;
        private double? elementLimit;
        private bool hasGeneralLimit, hasStageLimit, showDescription;
        
        #endregion

        #region Public
        public ObservableCollection<Item> Data
        {
            get => this.data;
            set => SetProperty(ref this.data, value);
        }
        public Stage CurrentStage
        {
            get => this.currentStage;
            set => SetProperty(ref this.currentStage, value);
        }

        public string StageName
        {
            get => this.stageName;
            set => SetProperty(ref this.stageName, value);
        }

        public double StageLimit
        {
            get => this.stageLimit;
            set => SetProperty(ref this.stageLimit, value);
        }

        public string StageLimitProperty
        {
            get => this.stageLimitProperty;
            set => SetProperty(ref this.stageLimitProperty, value);
        }

        public double StageProgress
        {
            get => this.stageProgress;
            set => SetProperty(ref this.stageProgress, value);
        }

        public double StageProgressLabel
        {
            get => this.stageProgressLabel;
            set => SetProperty(ref this.stageProgressLabel, value);
        }

        public double GeneralLimit
        {
            get => this.generalLimit;
            set => SetProperty(ref this.generalLimit, value);
        }

        public double GeneralProgress
        {
            get => this.generalProgress;
            set => SetProperty(ref this.generalProgress, value);
        }

        public double GeneralProgressLabel
        {
            get => this.generalProgressLabel;
            set => SetProperty(ref this.generalProgressLabel, value);
        }

        public bool HasGeneralLimit
        {
            get => this.hasGeneralLimit;
            set => SetProperty(ref this.hasGeneralLimit, value);
        }
        public bool HasStageLimit
        {
            get => this.hasStageLimit;
            set => SetProperty(ref this.hasStageLimit, value);
        }
        public double CurrentLimit
        {
            get => this.currentLimit;
            set => SetProperty(ref this.currentLimit, value);
        }

        public double? ElementLimit
        {
            get => this.elementLimit;
            set => SetProperty(ref this.elementLimit, value);
        }

        public bool ShowDescription
        {
            get => this.showDescription;
            set => SetProperty(ref this.showDescription, value);
        }


        #endregion
        #endregion

        #region Commands
        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        #endregion

        #region Constructor
        public ValuedViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;

            var assertionIndex = 0;
            foreach (var assertion in character.Ontology.Data.Relations.Assertions)
            {
                Debug.WriteLine($"------------------{assertionIndex}------------------");
                Debug.WriteLine($"Subject = {assertion.TaxonomySubject.ToString().Split('#').Last()}");
                Debug.WriteLine($"Predicate = {assertion.TaxonomyPredicate.ToString().Split('#').Last()}");
                Debug.WriteLine($"Object = {assertion.TaxonomyObject.ToString().Split('#').Last()}");
                ++assertionIndex;
            }

            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            var stageString = this.CurrentStage.FullName;
            this.StageName = FileService.FormatName(stageString.Split('#').Last());
            this.StageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last(), false, this.CurrentStage.EditGeneralLimit);
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.CurrentLimit = this.StageLimit;
            this.StageProgressLabel = this.StageLimit;
            this.StageProgress = (this.StageProgressLabel > 0 )? 1 : 0;
            this.ShowDescription = true;
            this.HasGeneralLimit = this.CurrentStage.EditGeneralLimit;
            this.hasStageLimit = true;
            this.ElementLimit = null;
            this.Data = new ObservableCollection<Item>();

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(stageString, true);
                StageViewModel.GeneralMaximum = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralLimit = StageViewModel.GeneralMaximum;
                StageViewModel.GeneralProgress = 1;
            }

            if (this.HasGeneralLimit == true)
            {
                this.HasGeneralLimit = true;
                this.GeneralLimit = StageViewModel.GeneralLimit;
                this.GeneralProgress = StageViewModel.GeneralProgress;
                this.GeneralProgressLabel = this.GeneralLimit;
                this.CurrentLimit = Math.Min(this.GeneralLimit, this.StageLimit);

                // To maintain stage progress bar state
                var stageProperty = game.Ontology.Model.PropertyModel.SelectProperty(character.GetFullString(this.stageLimitProperty));
                var stagePropertyDefinedAnnotations = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(stageProperty);
                var definition = stagePropertyDefinedAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                var stageMax = Convert.ToDouble(character.GetValue(definition));
                //this.StageProgress = this.StageProgressLabel / stageMax;       
                this.StageProgress = this.StageProgressLabel > 0 ? 1 : 0;
            }
            else
                this.CurrentLimit = this.StageLimit;            

            if(character.CheckClass(this.CurrentStage.FullName, false))
            {
                var subjectClass = game.Ontology.Model.ClassModel.SelectClass(this.CurrentStage.FullName);
                var subjectClassAnnotationEntries = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(subjectClass);
                var elementLimitAnnotationEntries = subjectClassAnnotationEntries.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ElementLimit"));
                if(elementLimitAnnotationEntries.Count() > 0)
                {
                    var definition = elementLimitAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                    this.ElementLimit = character.GetValue(definition);
                    this.CurrentLimit = Convert.ToDouble(this.ElementLimit);
                }
            }


            if (character.CheckDatatypeProperty(this.CurrentStage.FullName, StageViewModel.ApplyOnCharacter))
            {
                this.ShowDescription = false;
                var itemStep = character.GetStep(this.StageName, stageString.Split('#').Last());
                if (this.CurrentLimit > itemStep * 100)
                    this.CurrentLimit = itemStep * 100;

                List<Item> DataList = new List<Item>();
                var newItem = new Item(this.CurrentStage.FullName, string.Empty, string.Empty, itemStep, this.CurrentLimit);
                newItem.IsEnabled = true;
                Data.Add(newItem);
            }
            else
            {
                var Items = character.GetIndividuals(this.CurrentStage.FullName);
                List<Item> DataList = new List<Item>();

                foreach (var item in Items)
                {
                    var itemStep = character.GetStep(item.FullName.Split('#').Last(), stageString.Split('#').Last());

                    if (this.CurrentLimit > item.Step * 100)
                        this.CurrentLimit = item.Step * 100;

                    var itemClassName = string.Empty;
                    var itemFact = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data.SelectFact(item.FullName);
                    var itemClassAssertion = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(itemFact);
                    if (itemClassAssertion.EntriesCount > 0)
                        itemClassName = itemClassAssertion.Single().TaxonomyObject.ToString().Split('#').Last();

                    var newItem = new Item(item.FullName, item.Description, itemClassName, itemStep, this.CurrentLimit);
                    Data.Add(newItem);
                }
            }


            this.NextCommand = new Command(async () => await Task.Run(()=>Next()));

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });
        }
        #endregion    
        
        private async Task Next()
        {
            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            foreach (var item in Data)
            {
                var itemName = item.FullName.Split('#').Last();
                var characterItemString = $"{character.Context}{itemName}";
                var itemValue = item.Value.ToString();

                if (!character.CheckDatatypeProperty(item.FullName, false))
                {
                    var itemProperties = game.Ontology.Model.PropertyModel.Where(property => property.ToString().Contains(itemName));
                    if (itemProperties.Count() > 0)
                    {
                        var itemTotalPropertyEntries = itemProperties.Where(property => property.ToString().Contains("Total"));
                        if (itemTotalPropertyEntries.Count() > 0)
                        {
                            var itemPropertyName = string.Empty;
                            if (itemTotalPropertyEntries.Count() > 1)
                            {
                                var name = $"Per_{itemName}_Total";
                                itemPropertyName = itemTotalPropertyEntries.Where(entry => entry.ToString().Split('#').Last() == name).Single().ToString().Split('#').Last();
                            }
                            else
                                itemPropertyName = itemTotalPropertyEntries.Single().ToString().Split('#').Last();
                            characterItemString = $"{character.Context}{itemPropertyName}";
                            character.UpdateDatatypeAssertion(characterItemString, itemValue);
                        }
                    }
                }
                else
                    character.UpdateDatatypeAssertion(characterItemString, itemValue);
            }

            if (this.CurrentStage.EditStageLimit)
            {
                var characterStageLimitProperty = $"{character.Context}{this.stageLimitProperty}";
                character.UpdateDatatypeAssertion(characterStageLimitProperty, Convert.ToString(Convert.ToInt32(this.StageProgressLabel)));
                var characterAssertions = character.GetCharacterProperties();
                var assertionFound = characterAssertions.TryGetValue(characterStageLimitProperty, out var valueList);
                if (assertionFound == true)
                {
                    var valueString = valueList.Single().Split('^').First();
                    var currentValue = Convert.ToDouble(valueString);
                    while (currentValue != StageProgressLabel)
                    {
                        character.UpdateDatatypeAssertion(characterStageLimitProperty, this.StageProgressLabel.ToString());
                        characterAssertions = character.GetCharacterProperties();
                        assertionFound = characterAssertions.TryGetValue(characterStageLimitProperty, out valueList);
                        if (assertionFound == true)
                        {
                            valueString = valueList.Single().Split('^').First();
                            currentValue = Convert.ToDouble(valueString);
                        }
                    }
                }
            }

            if (this.CurrentStage.EditGeneralLimit)
            {
                var characterStageLimitProperty = $"{character.Context}{StageViewModel.GeneralLimitProperty}";
                character.UpdateDatatypeAssertion(characterStageLimitProperty, Convert.ToString(Convert.ToInt32(this.GeneralProgressLabel)));
                StageViewModel.GeneralLimit = this.GeneralProgressLabel;
                StageViewModel.GeneralProgress = this.GeneralProgressLabel/StageViewModel.GeneralMaximum;
            }

            ++StageViewModel.CurrentStep;
            try
            {
                if (StageViewModel.CurrentStep < StageViewModel.CreationScheme.Count())
                {
                    var nextStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
                    if (nextStage.IsGrouped)
                    {
                        switch (nextStage.Type)
                        {
                            case Stage.StageType.SingleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceGroupView())); break;
                            case Stage.StageType.MultipleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceGroupView())); break;
                            default: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedGroupView())); break;
                        }
                    }
                    else
                    {
                        switch (nextStage.Type)
                        {
                            case Stage.StageType.SingleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceView())); break;
                            case Stage.StageType.MultipleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceView())); break;
                            default: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedView())); break;
                        }
                    }
                }
                else
                {
                    await dialogService.DisplayAlert("Nota informativa", "Proceso de creación finalizado correctamente");
                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        App.Navigation = DependencyHelper.CurrentContext.AppMainView?.Detail?.Navigation;
                        App.Current.MainPage = DependencyHelper.CurrentContext.AppMainView;
                    });
                }
            }
            catch (Exception e)
            {
                await dialogService.DisplayAlert(this.StageName, e.Message);
                --StageViewModel.CurrentStep;
            }
            finally
            {
                await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
            }

        }
    }
}