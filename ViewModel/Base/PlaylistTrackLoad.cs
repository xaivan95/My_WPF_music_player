using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_new_player
{
    public static class PlaylistTrackLoad
    {      
        //Путь к исполняемому файлу
        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        //Список полных имён файлов
        public static List<string> Files = new List<string>();
        //Текущая позиция трека в плейлисте
        public static int CurrentTrackNumber;
        //Получаем имя файла
        public static string GetFileName(string file)
        {
            string[] tmp = file.Split('\\');
            return tmp[tmp.Length - 1];
        }       
    }
}
