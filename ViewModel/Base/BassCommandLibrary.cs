using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;
using Un4seen.Bass.AddOn.Mpc;
using Un4seen.Bass.AddOn;
using System.IO;
using System.ComponentModel;
using System.Timers;
using TagLib;

namespace My_new_player
{
    /// <summary>
    /// Логика работы муз. плеера с использованием библиотеки
    /// </summary>
    public static class BassCommandLibrary
    {
        //Частота дискретизации
        public static int HZ = 44100; 
        //Состояние инициализации
        public static bool InitDefaultDevice; 
        //Поток
        public static int Stream;
        //Громкость
        public static int Volume = 100;
        //Канал остановлен "руками" (нажатие на стоп, например
        public static bool isStopped = true;
        //Будет верно, когда плейлист будет полностью проигран
        public static bool endPlaylist;
        //Инициализация Bass.Dll с такой-то частотой дискретизации
        public static bool InitBass(int hz)
        {
             if (!InitDefaultDevice)
              {
                  InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
              }
            Bass.BASS_PluginLoad("bassflac.dll");
            return InitDefaultDevice;
        }
        /// <summary>
        /// Логика кнопки "Плей"
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="vol"></param>
        public static void Play(string filename, int vol)
        {
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED)
            {
                Stop();//Остановили канал, если он играл
                if (InitBass(HZ))
                {
                    Stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_DEFAULT);
                    if (Stream != 0)
                    {
                        Volume = vol;
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100);
                        Bass.BASS_ChannelPlay(Stream, false);
                    }
                }
            }
            else Bass.BASS_ChannelPlay(Stream, false);
            isStopped = false;           
        }
        /// <summary>
        /// Логика кнопки "Стоп"
        /// </summary>
        public static void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            isStopped = true;
        }
        /// <summary>
        /// Логика кнопки "Пауза"
        /// </summary>
        public static void Pause()
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
                Bass.BASS_ChannelPause(Stream);
        }
        /// <summary>
        /// Получить длину трека в секундах
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetTimeOfStream(int stream)
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(stream, TimeBytes);
            return (int)Time; 
        }
        /// <summary>
        /// Получить текущую позицию в треке
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static long GetPosOfStream(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            var posSec = (int)Bass.BASS_ChannelBytes2Seconds(stream, pos);    
            return posSec;
        }
        /// <summary>
        /// Перемотка трека
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pos"></param>
        public static void SetPosOfScroll(int stream, int pos)
        {   
           Bass.BASS_ChannelSetPosition(stream, (double)pos); //double ili int?
        }
        /// <summary>
        /// Изменение громкости
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vol"></param>
        public static void SetVolumeToStream(int stream, int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(stream,BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }
        /// <summary>
        /// Логика автопереключения треков
        /// </summary>
        /// <returns></returns>
        public static bool ToNextTrack()
        {
            if ((Bass.BASS_ChannelIsActive (Stream) == BASSActive.BASS_ACTIVE_STOPPED) && (!isStopped))
            {
                if (PlaylistTrackLoad.Files.Count > PlaylistTrackLoad.CurrentTrackNumber + 1)
                {
                   
                //    Play(Vars.Files[++Vars.CurrentTrackNumber], Volume); //++ ubral
                    endPlaylist = false;
                       return true;
                    
                }
                else endPlaylist = true;
            }
            return false;
        }
        /// <summary>
        /// Логика кнопки "Следующий трек"
        /// </summary>
        public static void Next()
        {
            if ((Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING) && (!isStopped))
            {
                if (PlaylistTrackLoad.Files.Count > PlaylistTrackLoad.CurrentTrackNumber + 1)
                {
                   
                   Play(PlaylistTrackLoad.Files[++PlaylistTrackLoad.CurrentTrackNumber], 100);
                    endPlaylist = false;
                }
                   else endPlaylist = true;   
            }         
        }
        /// <summary>
        /// Логика кнопки "Предыдущий трек"
        /// </summary>
        public static void Prev()
        {
            if ((Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING) && (!isStopped))
            {
                if ((PlaylistTrackLoad.CurrentTrackNumber - 1) >= 0)
                {
                   
                    Play(PlaylistTrackLoad.Files[--PlaylistTrackLoad.CurrentTrackNumber], 100);
                    endPlaylist = false;
                }
                else
                { 
                    Stop();
                }
            }
        }
    }
}
