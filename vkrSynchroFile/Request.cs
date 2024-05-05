using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkrSynchroFile
{
    public class Request
    {
        public int Type { get; set; }

        public string uid { get; set; }
        public string Message { get; set; }
        public bool synhroMode { get; set; }
        public string profileUID { get; set; }
        public byte[] FileData { get; set; }
    }
}
