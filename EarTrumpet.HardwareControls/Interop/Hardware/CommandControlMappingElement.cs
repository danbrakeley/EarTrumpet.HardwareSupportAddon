namespace EarTrumpet.HardwareControls.Interop.Hardware
{
    public class CommandControlMappingElement
    {
        public enum Command
        {
            SystemVolume,
            SystemMute,
            ApplicationVolume,
            ApplicationMute,
            SetDefaultDevice,
            CycleDefaultDevice,
            None
        };

        public enum Mode
        {
            Indexed,
            ApplicationSelection,
            ApplicationFocus,
            None
        };
        
        public string audioDevice { get; set; }
        public Command command { get; set; }
        public Mode mode { get; set; }
        public string appId { get; set; }
        public string appDisplayName{ get; set; }
        public int index { get; set; }
        public HardwareConfiguration config { get; set; }

        // Constructors

        // TODO: if this isn't present, then HardwareManager.Current is null and crashes
        public CommandControlMappingElement() { }

        public CommandControlMappingElement(
            string audioDevice,
            Command command,
            Mode mode, 
            string appId,
            string appDisplayName,
            int index,
            HardwareConfiguration config)
        {
            this.audioDevice = audioDevice;
            this.command = command;
            this.mode = mode;
            this.appId = appId;
            this.appDisplayName = appDisplayName;
            this.index = index;
            this.config = config;
        }
    }
}