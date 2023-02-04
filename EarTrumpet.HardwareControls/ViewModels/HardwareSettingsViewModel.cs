using System;
using System.Windows.Forms;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EarTrumpet.UI.Helpers;
using EarTrumpet.Extensions;
using EarTrumpet.HardwareControls.Interop.Hardware;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.HardwareControls.ViewModels
{
    public class AppInfo : IEquatable<AppInfo>
    {
        public string AppId { get; set; }
        public string DisplayName { get; set; }
        
        public AppInfo(string appId, string displayName)
        {
            this.AppId = appId.ToLower();
            this.DisplayName = displayName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AppInfo objAsApp = obj as AppInfo;
            if (objAsApp == null) return false;
            else return Equals(objAsApp);
        }
        public override int GetHashCode()
        {
            return this.AppId.GetHashCode();
        }
        public bool Equals(AppInfo other)
        {
            if (other == null) return false;
            return this.AppId == other.AppId;
        }
    }

    public class HardwareSettingsViewModel : BindableBase
    {
        public ICommand SaveCommandControlMappingCommand { get; }
        public ICommand SelectControlCommand { get; }
        public string SelectedControl { get; set; }
        public AppInfo SelectedApp { get; set; }
        public int SelectedAppIndex { get; set; }
        public string SelectedDevice
        {
            set
            {
                if (Properties.Resources.AllAudioDevicesSelectionText == value)
                {
                    _selectedDevice = null;
                }
                else
                {
                    foreach (var dev in _devices.AllDevices)
                    {
                        if (dev.DisplayName == value)
                        {
                            _selectedDevice = dev;
                            break;
                        }
                    }
                }

                RefreshApps();

                // If "all devices" is selected, "set as default device" command is not available.
                // If "all devices" is not selected, "cycle default devices" command is not available.
                if(Properties.Resources.AllAudioDevicesSelectionText == value)
                {
                    Commands.Remove(Properties.Resources.SetAsDefaultDevice);

                    if(!Commands.Contains(Properties.Resources.CycleDefaultDevices))
                    {
                        Commands.Add(Properties.Resources.CycleDefaultDevices);
                    }
                }
                else
                {
                    Commands.Remove(Properties.Resources.CycleDefaultDevices);

                    if (!Commands.Contains(Properties.Resources.SetAsDefaultDevice))
                    {
                        Commands.Add(Properties.Resources.SetAsDefaultDevice);
                    }
                }

                RaisePropertyChanged("Commands");
            }

            get
            {
                if (_selectedDevice != null)
                {
                    return _selectedDevice.DisplayName;
                }

                return Properties.Resources.AllAudioDevicesSelectionText;
            }
        }
        public Boolean ModeSelectionEnabled
        {
            set
            {
                _modeSelectionEnabled = value;
                RaisePropertyChanged("ModeSelectionEnabled");
            }

            get
            {
                return _modeSelectionEnabled;
            }
        }
        public string SelectedMode
        {
            set
            {
                _selectedMode = value;
                IndexSelectionEnabled = value == Properties.Resources.IndexedText;
                AppSelectionEnabled = value == Properties.Resources.ApplicationSelectionText;
                RefreshApps();
            }
            get
            {
                return _selectedMode;
            }
        }
        public string SelectedCommand
        {
            set
            {
                _selectedCommand = value;

                if (Properties.Resources.AudioDeviceVolumeText == value ||
                    Properties.Resources.AudioDeviceMuteText == value ||
                    Properties.Resources.SetAsDefaultDevice == value)
                {
                    // Audio device specific command selected.
                    // -> Disable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = false;
                    IndexSelectionEnabled = false;
                    AppSelectionEnabled = false;
                }
                else if (Properties.Resources.ApplicationVolumeText == value || 
                         Properties.Resources.ApplicationMuteText == value)
                {
                    // Application specific command selected.
                    // -> Enable Mode and Selection ComboBoxes.

                    ModeSelectionEnabled = true;
                    IndexSelectionEnabled = _selectedMode == Properties.Resources.IndexedText;
                    AppSelectionEnabled = _selectedMode == Properties.Resources.ApplicationSelectionText;
                } else
                {
                    // Invalid selection. Do nothing.
                }
            }

            get
            {
                return _selectedCommand;
            }
        }
        public Boolean IndexSelectionEnabled
        {
            get { return _indexSelectionEnabled; }
            set
            {
                _indexSelectionEnabled = value;
                RaisePropertyChanged("IndexSelectionEnabled");
            }
        }
        public Boolean AppSelectionEnabled
        {
            get { return _appSelectionEnabled; }
            set
            {
                _appSelectionEnabled = value;
                RaisePropertyChanged("AppSelectionEnabled");
            }
        }
        public ObservableCollection<string> AudioDevices
        {
            get
            {
                ObservableCollection<String> availableAudioDevices = new ObservableCollection<string>();
                var devices = _devices.AllDevices;

                availableAudioDevices.Add(Properties.Resources.AllAudioDevicesSelectionText);

                foreach (var device in devices)
                {
                    availableAudioDevices.Add(device.DisplayName);
                }

                return availableAudioDevices;
            }
        }
        public ObservableCollection<string> DeviceTypes
        {
            get
            {
                ObservableCollection<String> deviceTypes = new ObservableCollection<string>();
                deviceTypes.AddRange(HardwareManager.Current.GetDeviceTypes());

                return deviceTypes;
            }
        }
        public ObservableCollection<AppInfo> SelectableApps
        {
            get { return _apps; }
        }
        public ObservableCollection<int> SelectableIndexes
        {
            get { return _indexes; }
        }
        public ObservableCollection<string> Modes
        {
            get
            {
                ObservableCollection<String> modes = new ObservableCollection<string>();

                // In "Indexed" mode, the user can assign an application index to a control.
                modes.Add(Properties.Resources.IndexedText);
                // In "Application Selection" mode, the user can select from a list of running applications.
                modes.Add(Properties.Resources.ApplicationSelectionText);
                // In "Application Focus" mode, the focused application has its volume changed (if possible).
                modes.Add(Properties.Resources.ApplicationFocusText);

                return modes;
            }
        }
        public ObservableCollection<string> Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }
        public string SelectedDeviceType
        {
            get { return _selectedDeviceType; }
            set
            {
                _selectedDeviceType = value;
                RaisePropertyChanged("SelectedDeviceType");
            }
        }

        private DeviceCollectionViewModel _devices;
        private DeviceViewModel _selectedDevice = null;
        private Boolean _modeSelectionEnabled = false;
        private String _selectedMode;
        private String _selectedCommand;
        private string _selectedDeviceType;
        private Boolean _indexSelectionEnabled = false;
        private Boolean _appSelectionEnabled = false;
        private ObservableCollection<AppInfo> _apps = new ObservableCollection<AppInfo>();
        private ObservableCollection<int> _indexes = new ObservableCollection<int>();
        private WindowHolder _ControlWizardWindow = null;
        private HardwareConfiguration _hardwareConfiguration = null;
        private CommandControlMappingElement _commandControlMappingElement = null;
        private EarTrumpetHardwareControlsPageViewModel _hardwareControls = null;
        private ObservableCollection<String> _commands = new ObservableCollection<string>();

        public HardwareSettingsViewModel(DeviceCollectionViewModel devices, EarTrumpetHardwareControlsPageViewModel earTrumpetHardwareControlsPageViewModel)
        {
            // Set default commands.
            _commands.Add(Properties.Resources.AudioDeviceVolumeText);
            _commands.Add(Properties.Resources.AudioDeviceMuteText);
            _commands.Add(Properties.Resources.ApplicationVolumeText);
            _commands.Add(Properties.Resources.ApplicationMuteText);
            _commands.Add(Properties.Resources.CycleDefaultDevices);

            _devices = devices;
            _hardwareControls = earTrumpetHardwareControlsPageViewModel;

            SelectControlCommand = new RelayCommand(SelectControl);
            SaveCommandControlMappingCommand = new RelayCommand(SaveCommandControlMapping);

            switch (_hardwareControls.ItemModificationWay)
            {
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_EMPTY:

                    // Set default command.
                    SelectedCommand = Properties.Resources.AudioDeviceVolumeText;

                    // Set default device type.
                    SelectedDeviceType = "MIDI";

                    // Set default selection.
                    SelectedControl = Properties.Resources.NoControlSelectedMessage;

                    break;
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.EDIT_EXISTING:
                case EarTrumpetHardwareControlsPageViewModel.ItemModificationWays.NEW_FROM_EXISTING:
                    var selectedMappingElement = HardwareManager.Current.GetCommandControlMappings()[_hardwareControls.SelectedIndex];

                    FillForm(selectedMappingElement);
                    break;

                default:
                    // Do not fill widgets.
                    break;
            }
        }

        public void SelectControl()
        {
            _ControlWizardWindow = HardwareManager.Current.GetHardwareWizard(SelectedDeviceType, this,
                _hardwareConfiguration);

            if (_ControlWizardWindow != null)
            {
                _ControlWizardWindow.OpenOrBringToFront();
            }
            else
            {
                MessageBox.Show(Properties.Resources.UnknownDeviceTypeSelectedMessageText, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SaveCommandControlMapping()
        {
            if (_hardwareConfiguration == null ||
                string.IsNullOrEmpty(SelectedDevice) ||
                string.IsNullOrEmpty(SelectedCommand) ||
                (ModeSelectionEnabled && string.IsNullOrEmpty(SelectedMode)) ||
                (IndexSelectionEnabled && SelectedAppIndex < 0) ||
                (AppSelectionEnabled && SelectedApp == null) ||
                string.IsNullOrEmpty(SelectedDeviceType))
            {
                // Do nothing if the settings were not done yet.
                MessageBox.Show(Properties.Resources.IncompleteDeviceConfigurationMessage, "EarTrumpet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CommandControlMappingElement.Command command = CommandControlMappingElement.Command.None;
            CommandControlMappingElement.Mode mode = CommandControlMappingElement.Mode.None;

            if (SelectedCommand == Properties.Resources.AudioDeviceVolumeText)
            {
                command = CommandControlMappingElement.Command.SystemVolume;
            }
            else if (SelectedCommand == Properties.Resources.AudioDeviceMuteText)
            {
                command = CommandControlMappingElement.Command.SystemMute;
            }
            else if (SelectedCommand == Properties.Resources.ApplicationVolumeText)
            {
                command = CommandControlMappingElement.Command.ApplicationVolume;
            }
            else if (SelectedCommand == Properties.Resources.ApplicationMuteText)
            {
                command = CommandControlMappingElement.Command.ApplicationMute;
            }
            else if (SelectedCommand == Properties.Resources.SetAsDefaultDevice)
            {
                command = CommandControlMappingElement.Command.SetDefaultDevice;
            }
            else if (SelectedCommand == Properties.Resources.CycleDefaultDevices)
            {
                command = CommandControlMappingElement.Command.CycleDefaultDevice;
            }
            
            if (SelectedMode == Properties.Resources.IndexedText)
            {
                mode = CommandControlMappingElement.Mode.Indexed;
            }
            else if (SelectedMode == Properties.Resources.ApplicationSelectionText)
            {
                mode = CommandControlMappingElement.Mode.ApplicationSelection;
            }
            else if (SelectedMode == Properties.Resources.ApplicationFocusText)
            {
                mode = CommandControlMappingElement.Mode.ApplicationFocus;
            }

            string appId = null, displayName = null;
            if (SelectedApp != null)
            {
                appId = SelectedApp.AppId;
                displayName = SelectedApp.DisplayName;
            }

            _commandControlMappingElement = new CommandControlMappingElement(
                SelectedDevice,
                command,
                mode,
                appId,
                displayName,
                SelectedAppIndex,
                _hardwareConfiguration);

            // Notify the hardware controls page about the new assignment.
            _hardwareControls.ControlCommandMappingSelectedCallback(_commandControlMappingElement);
        }

        public void ControlSelectedCallback(HardwareConfiguration hardwareConfiguration)
        {
            _hardwareConfiguration = hardwareConfiguration;

            _ControlWizardWindow.OpenOrClose();

            SelectedControl = _hardwareConfiguration.ToString();
            RaisePropertyChanged("SelectedControl");
        }

        private void RefreshApps()
        {
            _apps.Clear();
            _indexes.Clear();

            if (Properties.Resources.ApplicationSelectionText == SelectedMode)
            {
                if (Properties.Resources.AllAudioDevicesSelectionText == SelectedDevice)
                {
                    foreach (var dev in _devices.AllDevices)
                    {
                        foreach(var app in dev.Apps)
                        {
                            var appInfo = new AppInfo(app.AppId, app.DisplayName);
                            if (!_apps.Contains(appInfo))
                            {
                                _apps.Add(appInfo);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var app in _selectedDevice?.Apps)
                    {
                        _apps.Add(new AppInfo(app.AppId, app.DisplayName));
                    }
                }
            }
            else if (Properties.Resources.IndexedText == SelectedMode)
            {
                // We do not expect more than 20 applications to be addressed.
                for (var i = 0; i < 20; i++)
                {
                    _indexes.Add(i);
                }
            }
            else
            {
                // Leave app/index names list empty.
            }
        }

        private void FillForm(CommandControlMappingElement data)
        {
            void FillApplication()
            {
                switch (data.mode)
                {
                    case CommandControlMappingElement.Mode.Indexed:
                        SelectedMode = Properties.Resources.IndexedText;
                        SelectedAppIndex = data.index;
                        break;
                    case CommandControlMappingElement.Mode.ApplicationSelection:
                        SelectedMode = Properties.Resources.ApplicationSelectionText;
                        SelectedApp = new AppInfo(data.appId, data.appDisplayName);
                        break;
                    case CommandControlMappingElement.Mode.ApplicationFocus:
                        SelectedMode = Properties.Resources.ApplicationFocusText;
                        break;
                }
            }

            SelectedDevice = data.audioDevice;

            switch (data.command)
            {
                case CommandControlMappingElement.Command.ApplicationMute:
                    SelectedCommand = Properties.Resources.ApplicationMuteText;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.ApplicationVolume:
                    SelectedCommand = Properties.Resources.ApplicationVolumeText;
                    FillApplication();
                    break;
                case CommandControlMappingElement.Command.SystemMute:
                    SelectedCommand = Properties.Resources.AudioDeviceMuteText;
                    break;
                case CommandControlMappingElement.Command.SystemVolume:
                    SelectedCommand = Properties.Resources.AudioDeviceVolumeText;
                    break;
                case CommandControlMappingElement.Command.SetDefaultDevice:
                    SelectedCommand = Properties.Resources.SetAsDefaultDevice;
                    break;
                case CommandControlMappingElement.Command.CycleDefaultDevice:
                    SelectedCommand = Properties.Resources.CycleDefaultDevices;
                    break;
            }

            SelectedDeviceType = HardwareManager.Current.GetConfigType(data);
            SelectedControl = data.config.ToString();
            _hardwareConfiguration = data.config;
        }
    }
}