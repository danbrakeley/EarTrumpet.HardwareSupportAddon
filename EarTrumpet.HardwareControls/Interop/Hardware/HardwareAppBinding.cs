using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EarTrumpet.DataModel.AppInformation;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.HardwareControls.ViewModels;
using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.HardwareControls.Interop.Hardware
{
    public abstract class HardwareAppBinding
    {
        public abstract string Name { get; }
        public abstract Type ConfigType { get; }

        public abstract void AddCommand(CommandControlMappingElement command);
        public abstract CommandControlMappingElement GetCommandAt(int index);
        public abstract void RemoveCommandAt(int index);
        public abstract void ModifyCommandAt(int index, CommandControlMappingElement newCommand);
        
        public abstract Window GetConfigurationWindow(HardwareSettingsViewModel hardwareSettingsViewModel, 
            HardwareConfiguration loadConfig = null);

        public abstract int CalculateVolume(int value, int minValue, int maxValue, float scalingValue);

        protected IAudioDeviceManager _audioDeviceManager;
        protected DeviceCollectionViewModel _deviceCollectionViewModel;
        private ISettingsBag _settings;

        protected List<CommandControlMappingElement> _commandControlMappings;

        public List<CommandControlMappingElement> GetCommandControlMappings()
        {
            return _commandControlMappings;
        }
        
        protected HardwareAppBinding(DeviceCollectionViewModel deviceViewModel, IAudioDeviceManager audioDeviceManager)
        {
            _deviceCollectionViewModel = deviceViewModel;
            _audioDeviceManager = audioDeviceManager;
            _commandControlMappings = new List<CommandControlMappingElement>();
            
            _settings = StorageFactory.GetSettings();
            
        }
        
        protected List<DeviceViewModel> GetDevicesByName(string name)
        {
            var result = new List<DeviceViewModel>();
            foreach (var device in _deviceCollectionViewModel.AllDevices)
            {
                if (device.DisplayName == name || name == Properties.Resources.AllAudioDevicesSelectionText)
                {
                    result.Add(device);
                }
            }

            return result;
        }
        
        protected List<IAppItemViewModel> GetAppsByAppId(string deviceName, string appId)
        {
            var appIdLowerCase = appId.ToLower();
            var apps = new List<IAppItemViewModel>();

            foreach (var device in GetDevicesByName(deviceName))
            {
                apps.AddRange(device.Apps.Where(app => app.AppId.ToLower() == appIdLowerCase));
            }

            return apps;
        }

        protected List<IAppItemViewModel> GetAppsByIndex(string deviceName, int index)
        {
            var apps = new List<IAppItemViewModel>();
            
            foreach (var device in GetDevicesByName(deviceName))
            {
                if (index < device.Apps.Count())
                {
                    apps.Add(device.Apps[index]);
                }
            }
            
            return apps;
        }

        protected List<IAppItemViewModel> GetFocusedApps(string deviceName)
        {
            var activatedHandle = User32.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return null;
            }

            int activeProcId;
            // Note: the return value of GetWindowThreadProcessId() is the thread id, and is safe to ignore.
            User32.GetWindowThreadProcessId(activatedHandle, out activeProcId);

            // Ask EarTrumpet to collate info about this app
            var appinfo = AppInformationFactory.CreateForProcess(activeProcId);
            if (appinfo == null)
            {
                return null;
            }

            // TODO: is the appId always the package install path?
            var appId = appinfo.PackageInstallPath;

            return GetAppsByAppId(deviceName, appId);
        }

        protected void LoadSettings(string key)
        {
            _commandControlMappings = _settings.Get(key, new List<CommandControlMappingElement>());
        }

        protected void SaveSettings(string key)
        {
            _settings.Set(key, _commandControlMappings);
        }
    }
}