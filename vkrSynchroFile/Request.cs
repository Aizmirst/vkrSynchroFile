namespace vkrSynchroFile
{
    public class Request
    {
        public int Type { get; set; }

        public string uid { get; set; }

        // путь папки на клиентсокм устройстве, для синхронизации
        public string folderPath { get; set; }

        public bool synhroMode { get; set; }
        public string profileUID { get; set; }

        public List<FileInformation> fileInformation { get; set; }
    }
}
