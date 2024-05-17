namespace vkrSynchroFile
{
    public class Request
    {
        public int Type { get; set; }

        public string uid { get; set; }

        // путь папки на клиентсокм устройстве, для синхронизации
        public string folderPath { get; set; }

        // Информация для автоматизации
        public bool auto_type { get; set; }
        public string auto_day { get; set; }
        public string auto_time { get; set; }

        public bool synhroMode { get; set; }
        public string profileUID { get; set; }

        public List<FileInformation> fileInformation { get; set; }
    }
}
