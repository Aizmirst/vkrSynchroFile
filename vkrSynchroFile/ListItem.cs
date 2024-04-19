using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkrSynchroFile
{
    public class ListItem
    {
        // Пример скрытых переменных, которые вы хотите добавить к элементам списка
        public int profile_id { get; set; }
        public int folder1id { get; set; }
        public string folder1name { get; set; }
        public string folder1path { get; set; }
        public int folder2id { get; set; }
        public string folder2name { get; set; }
        public string folder2path { get; set; }
        public string text { get; set; }
        public int profType { get; set; }
        public bool profMode { get; set; }
    }
}
