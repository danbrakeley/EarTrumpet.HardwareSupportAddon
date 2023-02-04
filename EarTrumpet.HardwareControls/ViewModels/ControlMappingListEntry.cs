using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.HardwareControls.ViewModels
{
    public class ControlMappingListEntry
    {
        // Name of the specific audio device (or a standard string for "all/any")
        public string DeviceName { get; set; }

        // Type is a short text string that maps 1-1 to CommandControlMappingElement.Command
        public string Type { get; set; }

        // Context depends on the Command, but for System commands, it is empty, and for App commands, it is the app name, index, or "focused app"
        public string Context { get; set; }

        // Control is a short string describing what hardware control is assigned to this mapping
        public string Control { get; set; }
    }
}
