namespace vkrSynchroFile
{
    public class ListItem
    {
        // Текст, отображаемый в список
        public string text { get; set; }

        // номер профиля в списке
        public int profile_number { get; set; }

        // id профиля в бд
        public int profile_id { get; set; }

        // Данные папки 1
        public int folder1id { get; set; }
        public string folder1name { get; set; }
        public string folder1path { get; set; }

        // Данные папки 2
        public int folder2id { get; set; }
        public string folder2name { get; set; }
        public string folder2path { get; set; }

        //Внутри 1 пк или по сети
        public int profType { get; set; }

        // Одно или друсторонняя синхронизация
        public bool profMode { get; set; }

        // Информация для автоматизации
        public bool auto_type { get; set; }
        public string auto_day { get; set; }
        public string auto_time { get; set; }

        // uid второго пользователя в сетевом профиле
        public string userUID { get; set; }
        public string profileUID { get; set; }

        // флаг для отметки создателя сетевого профиля
        public bool mainUser { get; set; }
    }
}
