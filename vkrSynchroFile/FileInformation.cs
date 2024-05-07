using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkrSynchroFile
{
    public class FileInformation
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastModified { get; set; }
        public string HashCode { get; set; }
        public bool IsDirectory { get; set; }
        public bool ForSynchro { get; set; }
        public bool ForCopy { get; set; }
        public byte[] FileData { get; set; }
    }
}
