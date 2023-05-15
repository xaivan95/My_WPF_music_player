using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass;
using TagLib;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;

namespace My_new_player
{
    /// <summary>
    /// Класс тегов
    /// </summary>
    public class TagCommandLibrary
    {
        public string Artist;
        public string Title;
        /// <summary>
        ///Теги для аудиофайла
        /// </summary>
        /// <param name="file"></param>
        public TagCommandLibrary(string file)
        {            
            TAG_INFO tagInfo = new TAG_INFO();
              tagInfo = BassTags.BASS_TAG_GetFromFile(file);
            Artist = tagInfo.artist;
            if (tagInfo.title == "")
                Title = PlaylistTrackLoad.GetFileName(file);
            else
                Title = tagInfo.title; 
    }
    }
}
