﻿using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Events;
using D4Companion.Interfaces;
using D4Companion.Services;
using D4Companion.ViewModels.Dialogs;
using D4Companion.Views.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace D4Companion.ViewModels
{
    public class AffixViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly IAffixManager _affixManager;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ISettingsManager _settingsManager;

        private ObservableCollection<AffixInfo> _affixes = new ObservableCollection<AffixInfo>();
        private ObservableCollection<AffixPresetV2> _affixPresets = new ObservableCollection<AffixPresetV2>();
        private ObservableCollection<AspectInfo> _aspects = new ObservableCollection<AspectInfo>();
        private ObservableCollection<ItemAffixV2> _selectedAffixes = new ObservableCollection<ItemAffixV2>();
        private ObservableCollection<ItemAffixV2> _selectedAspects = new ObservableCollection<ItemAffixV2>();
        private ObservableCollection<ItemAffixV2> _selectedSigils = new ObservableCollection<ItemAffixV2>();

        private string _affixPresetName = string.Empty;
        private string _affixTextFilter = string.Empty;
        private int? _badgeCount = null;
        private bool _isAffixOverlayEnabled = false;
        private AffixPresetV2 _selectedAffixPreset = new AffixPresetV2();
        private int _selectedTabIndex = 0;
        private bool _toggleCore = true;
        private bool _toggleBarbarian = false;
        private bool _toggleDruid = false;
        private bool _toggleNecromancer = false;
        private bool _toggleRogue = false;
        private bool _toggleSorcerer = false;
        private bool _toggleLocations = false;

        // Start of Constructors region

        #region Constructors

        public AffixViewModel(IEventAggregator eventAggregator, ILogger<AffixViewModel> logger, IAffixManager affixManager, IDialogCoordinator dialogCoordinator, ISettingsManager settingsManager)
        {
            // Init IEventAggregator
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AffixPresetAddedEvent>().Subscribe(HandleAffixPresetAddedEvent);
            _eventAggregator.GetEvent<AffixPresetRemovedEvent>().Subscribe(HandleAffixPresetRemovedEvent);
            _eventAggregator.GetEvent<ApplicationLoadedEvent>().Subscribe(HandleApplicationLoadedEvent);
            _eventAggregator.GetEvent<SelectedAffixesChangedEvent>().Subscribe(HandleSelectedAffixesChangedEvent);
            _eventAggregator.GetEvent<SelectedAspectsChangedEvent>().Subscribe(HandleSelectedAspectsChangedEvent);
            _eventAggregator.GetEvent<ToggleOverlayEvent>().Subscribe(HandleToggleOverlayEvent);
            _eventAggregator.GetEvent<ToggleOverlayKeyBindingEvent>().Subscribe(HandleToggleOverlayKeyBindingEvent);

            // Init logger
            _logger = logger;

            // Init services
            _affixManager = affixManager;
            _dialogCoordinator = dialogCoordinator;
            _settingsManager = settingsManager;

            // Init View commands
            AddAffixPresetNameCommand = new DelegateCommand(AddAffixPresetNameExecute, CanAddAffixPresetNameExecute);
            RemoveAffixPresetNameCommand = new DelegateCommand(RemoveAffixPresetNameExecute, CanRemoveAffixPresetNameExecute);
            RemoveAffixCommand = new DelegateCommand<ItemAffixV2>(RemoveAffixExecute);
            RemoveAspectCommand = new DelegateCommand<ItemAffixV2>(RemoveAspectExecute);
            SetAffixCommand = new DelegateCommand<AffixInfo>(SetAffixExecute);
            SetAffixColorCommand = new DelegateCommand<ItemAffixV2>(SetAffixColorExecute);
            SetAspectCommand = new DelegateCommand<AspectInfo>(SetAspectExecute);

            // Init filter views
            CreateItemAffixesFilteredView();
            CreateItemAspectsFilteredView();
            CreateSelectedAffixesHelmFilteredView();
            CreateSelectedAffixesChestFilteredView();
            CreateSelectedAffixesGlovesFilteredView();
            CreateSelectedAffixesPantsFilteredView();
            CreateSelectedAffixesBootsFilteredView();
            CreateSelectedAffixesAmuletFilteredView();
            CreateSelectedAffixesRingFilteredView();
            CreateSelectedAffixesWeaponFilteredView();
            CreateSelectedAffixesRangedFilteredView();
            CreateSelectedAffixesOffhandFilteredView();
            CreateSelectedAspectsFilteredView();
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public ObservableCollection<AffixInfo> Affixes { get => _affixes; set => _affixes = value; }
        public ObservableCollection<AffixPresetV2> AffixPresets { get => _affixPresets; set => _affixPresets = value; }
        public ObservableCollection<AspectInfo> Aspects { get => _aspects; set => _aspects = value; }
        public ObservableCollection<ItemAffixV2> SelectedAffixes { get => _selectedAffixes; set => _selectedAffixes = value; }
        public ObservableCollection<ItemAffixV2> SelectedAspects { get => _selectedAspects; set => _selectedAspects = value; }
        public ObservableCollection<ItemAffixV2> SelectedSigils { get => _selectedSigils; set => _selectedSigils = value; }
        public ListCollectionView? AffixesFiltered { get; private set; }
        public ListCollectionView? AspectsFiltered { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredHelm { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredChest { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredGloves { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredPants { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredBoots { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredAmulet { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredRing { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredWeapon { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredRanged { get; private set; }
        public ListCollectionView? SelectedAffixesFilteredOffhand { get; private set; }
        public ListCollectionView? SelectedAspectsFiltered { get; private set; }

        public DelegateCommand AddAffixPresetNameCommand { get; }
        public DelegateCommand RemoveAffixPresetNameCommand { get; }
        public DelegateCommand<ItemAffixV2> RemoveAffixCommand { get; }
        public DelegateCommand<ItemAffixV2> RemoveAspectCommand { get; }
        public DelegateCommand<AffixInfo> SetAffixCommand { get; }
        public DelegateCommand<ItemAffixV2> SetAffixColorCommand { get; }
        public DelegateCommand<AspectInfo> SetAspectCommand { get; }

        public string AffixPresetName
        {
            get => _affixPresetName;
            set
            {
                SetProperty(ref _affixPresetName, value, () => { RaisePropertyChanged(nameof(AffixPresetName)); });
                AddAffixPresetNameCommand?.RaiseCanExecuteChanged();
            }
        }

        public string AffixTextFilter
        {
            get => _affixTextFilter;
            set
            {
                SetProperty(ref _affixTextFilter, value, () => { RaisePropertyChanged(nameof(AffixTextFilter)); });
                AffixesFiltered?.Refresh();
                AspectsFiltered?.Refresh();
            }
        }
        public int? BadgeCount { get => _badgeCount; set => _badgeCount = value; }

        public bool IsAffixOverlayEnabled
        {
            get => _isAffixOverlayEnabled;
            set
            {
                _isAffixOverlayEnabled = value;
                RaisePropertyChanged(nameof(IsAffixOverlayEnabled));
                _eventAggregator.GetEvent<ToggleOverlayFromGUIEvent>().Publish(new ToggleOverlayFromGUIEventParams { IsEnabled = value });
            }
        }

        public bool IsAffixPresetSelected
        {
            get
            {
                return SelectedAffixPreset != null && !string.IsNullOrWhiteSpace(SelectedAffixPreset.Name);
            }
        }

        public bool IsAffixesTabActive
        {
            get => SelectedTabIndex == 0;
        }

        public bool IsAspectsTabActive
        {
            get => SelectedTabIndex == 1;
        }

        public bool IsSigilsTabActive
        {
            get => SelectedTabIndex == 2;
        }

        public AffixPresetV2 SelectedAffixPreset
        {
            get => _selectedAffixPreset;
            set
            {
                _selectedAffixPreset = value;
                RaisePropertyChanged(nameof(SelectedAffixPreset));
                RaisePropertyChanged(nameof(IsAffixPresetSelected));
                RemoveAffixPresetNameCommand?.RaiseCanExecuteChanged();
                if (value != null)
                {
                    _settingsManager.Settings.SelectedAffixName = value.Name;
                    _settingsManager.SaveSettings();
                }
                else
                {
                    _selectedAffixPreset = new AffixPresetV2();
                }
                UpdateSelectedAffixes();
                UpdateSelectedAspects();
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                RaisePropertyChanged();

                RaisePropertyChanged(nameof(IsAffixesTabActive));
                RaisePropertyChanged(nameof(IsAspectsTabActive));
                RaisePropertyChanged(nameof(IsSigilsTabActive));
            }
        }

        public bool ToggleCore
        {
            get => _toggleCore; set
            {
                _toggleCore = value;

                if (value) 
                {
                    ToggleBarbarian = false;
                    ToggleDruid = false;
                    ToggleNecromancer = false;
                    ToggleRogue = false;
                    ToggleSorcerer = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleCore));
            }
        }

        /// <summary>
        /// Reset filter when all category toggles are false.
        /// </summary>
        private void CheckResetAffixFilter()
        {
            if (!ToggleCore && !ToggleBarbarian && !ToggleDruid && !ToggleNecromancer && !ToggleRogue && !ToggleSorcerer) 
            {
                AffixesFiltered?.Refresh();
                AspectsFiltered?.Refresh();
            }
        }

        public bool ToggleBarbarian
        {
            get => _toggleBarbarian;
            set
            {
                _toggleBarbarian = value;

                if (value)
                {
                    ToggleCore = false;
                    ToggleDruid = false;
                    ToggleNecromancer = false;
                    ToggleRogue = false;
                    ToggleSorcerer = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleBarbarian));
            }
        }

        public bool ToggleDruid
        {
            get => _toggleDruid; set
            {
                _toggleDruid = value;

                if (value)
                {
                    ToggleCore = false;
                    ToggleBarbarian = false;
                    ToggleNecromancer = false;
                    ToggleRogue = false;
                    ToggleSorcerer = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleDruid));
            }
        }

        public bool ToggleNecromancer
        {
            get => _toggleNecromancer;
            set
            {
                _toggleNecromancer = value;

                if (value)
                {
                    ToggleCore = false;
                    ToggleBarbarian = false;
                    ToggleDruid = false;
                    ToggleRogue = false;
                    ToggleSorcerer = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleNecromancer));
            }
        }

        public bool ToggleRogue
        {
            get => _toggleRogue;
            set
            {
                _toggleRogue = value;

                if (value)
                {
                    ToggleCore = false;
                    ToggleBarbarian = false;
                    ToggleDruid = false;
                    ToggleNecromancer = false;
                    ToggleSorcerer = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleRogue));
            }
        }

        public bool ToggleSorcerer
        {
            get => _toggleSorcerer;
            set
            {
                _toggleSorcerer = value;

                if (value)
                {
                    ToggleCore = false;
                    ToggleBarbarian = false;
                    ToggleDruid = false;
                    ToggleNecromancer = false;
                    ToggleRogue = false;

                    AffixesFiltered?.Refresh();
                    AspectsFiltered?.Refresh();
                }

                CheckResetAffixFilter();
                RaisePropertyChanged(nameof(ToggleSorcerer));
            }
        }

        public bool ToggleLocations
        {
            get => _toggleLocations;
            set
            {
                _toggleLocations = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        private void HandleAffixPresetAddedEvent()
        {
            UpdateAffixPresets();

            // Select added preset
            var preset = _affixPresets.FirstOrDefault(preset => preset.Name.Equals(AffixPresetName));
            if (preset != null)
            {
                SelectedAffixPreset = preset;
            }

            // Clear preset name
            AffixPresetName = string.Empty;
        }

        private void HandleAffixPresetRemovedEvent()
        {
            UpdateAffixPresets();

            // Select first preset
            if (AffixPresets.Count > 0)
            {
                SelectedAffixPreset = AffixPresets[0];
            }
        }

        private void HandleApplicationLoadedEvent()
        {
            // Load affix and aspect gamedata
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Affixes.Clear();
                Affixes.AddRange(_affixManager.Affixes);

                Aspects.Clear();
                Aspects.AddRange(_affixManager.Aspects);
            });

            // Load affix presets
            UpdateAffixPresets();

            // Load selected affixes
            UpdateSelectedAffixes();

            // Load selected aspects
            UpdateSelectedAspects();
        }

        private void HandleSelectedAffixesChangedEvent()
        {
            UpdateSelectedAffixes();
        }

        private void HandleSelectedAspectsChangedEvent()
        {
            UpdateSelectedAspects();
        }

        private void HandleToggleOverlayEvent(ToggleOverlayEventParams toggleOverlayEventParams)
        {
            IsAffixOverlayEnabled = toggleOverlayEventParams.IsEnabled;
        }

        private void HandleToggleOverlayKeyBindingEvent()
        {
            IsAffixOverlayEnabled = !IsAffixOverlayEnabled;
        }

        private void RemoveAffixExecute(ItemAffixV2 itemAffix)
        {
            if (itemAffix != null)
            {
                _affixManager.RemoveAffix(itemAffix);
            }
        }

        private void RemoveAspectExecute(ItemAffixV2 itemAffix)
        {
            if (itemAffix != null)
            {
                _affixManager.RemoveAspect(itemAffix);
            }
        }

        private async void SetAffixExecute(AffixInfo affixInfo)
        {
            if (affixInfo != null)
            {
                var setAffixDialog = new CustomDialog() { Title = "Set affix" };
                var dataContext = new SetAffixViewModel(async instance =>
                {
                    await setAffixDialog.WaitUntilUnloadedAsync();
                }, affixInfo);
                setAffixDialog.Content = new SetAffixView() { DataContext = dataContext };
                await _dialogCoordinator.ShowMetroDialogAsync(this, setAffixDialog);
                await setAffixDialog.WaitUntilUnloadedAsync();
            }
        }

        private async void SetAffixColorExecute(ItemAffixV2 itemAffix)
        {
            if (itemAffix != null)
            {
                var setAffixColorDialog = new CustomDialog() { Title = "Set affix color" };
                var dataContext = new SetAffixColorViewModel(async instance =>
                {
                    await setAffixColorDialog.WaitUntilUnloadedAsync();
                }, itemAffix);
                setAffixColorDialog.Content = new SetAffixColorView() { DataContext = dataContext };
                await _dialogCoordinator.ShowMetroDialogAsync(this, setAffixColorDialog);
                await setAffixColorDialog.WaitUntilUnloadedAsync();
            }
        }

        private void SetAspectExecute(AspectInfo aspectInfo)
        {
            if (aspectInfo != null)
            {
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Helm);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Chest);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Gloves);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Pants);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Boots);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Amulet);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Ring);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Ranged);
                _affixManager.AddAspect(aspectInfo, ItemTypeConstants.Offhand);
            }
        }

        #endregion

        // Start of Methods region

        #region Methods

        private bool CanAddAffixPresetNameExecute()
        {
            return !string.IsNullOrWhiteSpace(AffixPresetName) &&
                !_affixPresets.Any(preset => preset.Name.Equals(AffixPresetName));
        }

        private void AddAffixPresetNameExecute()
        {
            _affixManager.AddAffixPreset(new AffixPresetV2
            {
                Name = AffixPresetName
            });
        }

        private void CreateItemAffixesFilteredView()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                AffixesFiltered = new ListCollectionView(Affixes)
                {
                    Filter = FilterAffixes
                };
            });
        }

        private bool FilterAffixes(object affixObj)
        {
            var allowed = true;
            if (affixObj == null) return false;

            AffixInfo affixInfo = (AffixInfo)affixObj;

            if (!affixInfo.Description.ToLower().Contains(AffixTextFilter.ToLower()) && !string.IsNullOrWhiteSpace(AffixTextFilter))
            {
                return false;
            }

            if (ToggleCore)
            {
                allowed = affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleBarbarian)
            {
                allowed = affixInfo.AllowedForPlayerClass[2] == 1 && !affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleDruid)
            {
                allowed = affixInfo.AllowedForPlayerClass[1] == 1 && !affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleNecromancer)
            {
                allowed = affixInfo.AllowedForPlayerClass[4] == 1 && !affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleRogue)
            {
                allowed = affixInfo.AllowedForPlayerClass[3] == 1 && !affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleSorcerer)
            {
                allowed = affixInfo.AllowedForPlayerClass[0] == 1 && !affixInfo.AllowedForPlayerClass.All(c => c == 1);
            }

            return allowed;
        }

        private void CreateItemAspectsFilteredView()
        {
            // As the view is accessed by the UI it will need to be created on the UI thread
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                AspectsFiltered = new ListCollectionView(Aspects)
                {
                    Filter = FilterAspects
                };
            });
        }

        private bool FilterAspects(object aspectObj)
        {
            var allowed = true;
            if (aspectObj == null) return false;

            AspectInfo aspectInfo = (AspectInfo)aspectObj;

            if (!aspectInfo.Description.ToLower().Contains(AffixTextFilter.ToLower()) && !aspectInfo.Name.ToLower().Contains(AffixTextFilter.ToLower()) && !string.IsNullOrWhiteSpace(AffixTextFilter))
            {
                return false;
            }

            if (ToggleCore)
            {
                allowed = aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleBarbarian)
            {
                allowed = aspectInfo.AllowedForPlayerClass[2] == 1 && !aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleDruid)
            {
                allowed = aspectInfo.AllowedForPlayerClass[1] == 1 && !aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleNecromancer)
            {
                allowed = aspectInfo.AllowedForPlayerClass[4] == 1 && !aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleRogue)
            {
                allowed = aspectInfo.AllowedForPlayerClass[3] == 1 && !aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }
            else if (ToggleSorcerer)
            {
                allowed = aspectInfo.AllowedForPlayerClass[0] == 1 && !aspectInfo.AllowedForPlayerClass.All(c => c == 1);
            }

            return allowed;
        }

        private void CreateSelectedAffixesHelmFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredHelm = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesHelm
                };
            });
        }

        private bool FilterSelectedAffixesHelm(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Helm);
        }

        private void CreateSelectedAffixesChestFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredChest = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesChest
                };
            });
        }

        private bool FilterSelectedAffixesChest(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Chest);
        }

        private void CreateSelectedAffixesGlovesFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredGloves = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesGloves
                };
            });
        }

        private bool FilterSelectedAffixesGloves(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Gloves);
        }

        private void CreateSelectedAffixesPantsFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredPants = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesPants
                };
            });
        }

        private bool FilterSelectedAffixesPants(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Pants);
        }

        private void CreateSelectedAffixesBootsFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredBoots = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesBoots
                };
            });
        }

        private bool FilterSelectedAffixesBoots(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Boots);
        }

        private void CreateSelectedAffixesAmuletFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredAmulet = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesAmulet
                };
            });
        }

        private bool FilterSelectedAffixesAmulet(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Amulet);
        }

        private void CreateSelectedAffixesRingFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredRing = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesRing
                };
            });
        }

        private bool FilterSelectedAffixesRing(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Ring);
        }

        private void CreateSelectedAffixesWeaponFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredWeapon = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesWeapon
                };
            });
        }

        private bool FilterSelectedAffixesWeapon(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Weapon);
        }

        private void CreateSelectedAffixesRangedFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredRanged = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesRanged
                };
            });
        }

        private bool FilterSelectedAffixesRanged(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Ranged);
        }

        private void CreateSelectedAffixesOffhandFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAffixesFilteredOffhand = new ListCollectionView(SelectedAffixes)
                {
                    Filter = FilterSelectedAffixesOffhand
                };
            });
        }

        private bool FilterSelectedAffixesOffhand(object selectedAffixObj)
        {
            if (selectedAffixObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAffixObj;

            return itemAffix.Type.Equals(ItemTypeConstants.Offhand);
        }

        private void CreateSelectedAspectsFilteredView()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                SelectedAspectsFiltered = new ListCollectionView(SelectedAspects)
                {
                    Filter = FilterSelectedAspects
                };
            });
        }

        private bool FilterSelectedAspects(object selectedAspectObj)
        {
            if (selectedAspectObj == null) return false;

            ItemAffixV2 itemAffix = (ItemAffixV2)selectedAspectObj;

            return !SelectedAspectsFiltered?.Cast<ItemAffixV2>().Any(a => a.Id.Equals(itemAffix.Id)) ?? false;
        }

        private void UpdateAffixPresets()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                AffixPresets.Clear();
                AffixPresets.AddRange(_affixManager.AffixPresets);
                if (AffixPresets.Any())
                {
                    // Load settings
                    var preset = _affixPresets.FirstOrDefault(preset => preset.Name.Equals(_settingsManager.Settings.SelectedAffixName));
                    if (preset != null)
                    {
                        SelectedAffixPreset = preset;
                    }
                }
            });
            AddAffixPresetNameCommand?.RaiseCanExecuteChanged();
        }

        private void UpdateSelectedAffixes()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                SelectedAffixes.Clear();
                if (SelectedAffixPreset != null)
                {
                    SelectedAffixes.AddRange(SelectedAffixPreset.ItemAffixes);
                }
            });
        }

        private void UpdateSelectedAspects()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                SelectedAspects.Clear();
                if (SelectedAffixPreset != null)
                {
                    SelectedAspects.AddRange(SelectedAffixPreset.ItemAspects);
                }
            });
        }

        private bool CanRemoveAffixPresetNameExecute()
        {
            return SelectedAffixPreset != null && !string.IsNullOrWhiteSpace(SelectedAffixPreset.Name);
        }

        private void RemoveAffixPresetNameExecute()
        {
            _dialogCoordinator.ShowMessageAsync(this, $"Delete", $"Are you sure you want to delete preset \"{SelectedAffixPreset.Name}\"", MessageDialogStyle.AffirmativeAndNegative).ContinueWith(t =>
            {
                if (t.Result == MessageDialogResult.Affirmative)
                {
                    _logger.LogInformation($"Deleted preset \"{SelectedAffixPreset.Name}\"");
                    _affixManager.RemoveAffixPreset(SelectedAffixPreset);
                }
            });
        }

        #endregion
    }
}
