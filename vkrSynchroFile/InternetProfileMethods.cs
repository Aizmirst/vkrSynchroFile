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


        private const string FirstRunFlagFileName = "firstrun.marker";

        public static string myUserUID()
        {
            string uniqueId = "";
            // Загрузка уникального идентификатора из файла
            uniqueId = File.ReadAllText(FirstRunFlagFileName);

            return uniqueId;
        }
    }
}
