using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkrSynchroFile
{
    internal class InternetProfileMethods
    {
        public static string[] GetFolderInfo(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            string[] result = new string[2];
            result[0] = directoryInfo.Name;
            result[1] = directoryInfo.FullName;

            return result;
        }
    }
}
