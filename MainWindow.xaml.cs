using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;
using My_new_player.ViewModel;
using System.Drawing;
using MessageBox = System.Windows.Forms.MessageBox;
using My_new_player.Model;
using Newtonsoft.Json;
using TagLib.Mpeg;
using File = System.IO.File;
using System.Windows.Shapes;

namespace My_new_player
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            ////Инициализируем   библиотеку Bass с определенным состоянием инициализации InitBass и частотой дискретизации HZ 
            BassCommandLibrary.InitBass(BassCommandLibrary.HZ);
           
        }
        OpenFileDialog ofd = new OpenFileDialog(); // Создаем переменную для работы с диалоговым окном
        DispatcherTimer dtimer = new DispatcherTimer(); // Сеременная для работы и инициализации таймера

        //показать панель с файлами
        private void FunctionShowClick(object sender, RoutedEventArgs e)
        {

            if (MyGridBack.ColumnDefinitions[1].Width == new GridLength(50, GridUnitType.Star))
            {
                MyGridBack.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
                this.Width -= 50;
                LeftButton.Text = "\u25C0";
            }
            else
            {
                MyGridBack.ColumnDefinitions[1].Width = new GridLength(50, GridUnitType.Star);
                this.Width += 50;
                LeftButton.Text = "\u25B6";
            }

        }
        //показать панель с плейлистом
        private void FunctionShowClick2(object sender, RoutedEventArgs e)
        {

            if (MyGridBack.ColumnDefinitions[3].Width == new GridLength(30, GridUnitType.Star))
            {
                MyGridBack.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Star);
                this.Width -= 30;
                RigtButton.Text = "\u25B6;";
            }
            else
            {
                MyGridBack.ColumnDefinitions[3].Width = new GridLength(30, GridUnitType.Star);
                this.Width += 30;
                RigtButton.Text = "\u25C0";
            }

        }
        // проверка формата файлов
        public bool CheckFileName(string filename)
        {
            string[] ext = new string[] { ".mp3", ".m4a", ".wma", ".ogg", ".flac" };
            if (ext.Contains(System.IO.Path.GetExtension(filename)))
                return true;
            return false;
        }
        // Нажатие на кнопку "добавить файлы"
        public void btnEject_Click(object sender, RoutedEventArgs e)
        {
            ofd.Multiselect = true;
            ofd.AddExtension = true;
            ofd.DefaultExt = "*.*";
            ofd.Filter = "All files (*.mp3; *.m4a; *.wma; *.ogg; *.flac) | *.mp3; *.m4a; *.wma; *.ogg; *.flac";
            ofd.FileOk += ofd_FileOk;
            ofd.ShowDialog();
            string fn = ofd.FileName;
            ofd.Multiselect = true;
            dtimer.Tick += dtimer_Tick;
            dtimer.Start();
        }
        // Добавление названий композиций в окно плейлиста
        public void ofd_FileOk(object sender, CancelEventArgs e)
        {
            ofd.FileOk -= ofd_FileOk;
            string[] tmp = ofd.FileNames;
            if (CheckFileName(ofd.FileName))
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    PlaylistTrackLoad.Files.Add(tmp[i]);
                    TagCommandLibrary TM = new TagCommandLibrary(tmp[i]);
                    playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }

            }
            else
            {
                e.Cancel = true;
                System.Windows.MessageBox.Show("Invalid format, please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }
        // Нажатие на кнопку "Плей"
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            dtimer.Start();

            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Сейчас играет:";
                string current = PlaylistTrackLoad.Files[playlist.SelectedIndex];
                PlaylistTrackLoad.CurrentTrackNumber = playlist.SelectedIndex;
                BassCommandLibrary.Play(current, 0);
                (this.DataContext as MainWindowViewModel).OpenFile(PlaylistTrackLoad.Files[playlist.SelectedIndex]);
                labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
                slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(current);
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
            }

        }
        // Описано задание значений максимальной длины трека, минимально длины трека и значения ползунка
        private void dtimer_Tick(object sender, EventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                slTime.Minimum = 0;
                //Получаем значения посекундного измения времени трека при проигывании + вследствие этого движется ползунок
                slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                if (BassCommandLibrary.ToNextTrack() == true)
                {
                    if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
                    {
                        (this.DataContext as MainWindowViewModel).Stop();
                        BassCommandLibrary.Stop();
                        btnNext_Click(this, new RoutedEventArgs());
                        btnPrev_Click(this, new RoutedEventArgs());
                        btnPlay_Click(this, new RoutedEventArgs());
                       
                        labelNowPlaying.Content = "Сейчас играет:";
                        if (PlaylistTrackLoad.Files.Count > PlaylistTrackLoad.CurrentTrackNumber + 1)
                        {
                            (this.DataContext as MainWindowViewModel).OpenFile(PlaylistTrackLoad.Files[++PlaylistTrackLoad.CurrentTrackNumber]);
                        }
                        labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
                        labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
                        slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                        slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                        try
                        {
                            if (PlaylistTrackLoad.Files.Count >= PlaylistTrackLoad.CurrentTrackNumber + 1)
                            {
                                ++playlist.SelectedIndex;
                                labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName((PlaylistTrackLoad.Files[PlaylistTrackLoad.CurrentTrackNumber]));
                            }
                            if (PlaylistTrackLoad.CurrentTrackNumber == PlaylistTrackLoad.Files.Count)
                                labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(PlaylistTrackLoad.Files[playlist.SelectedIndex]);
                            if (PlaylistTrackLoad.CurrentTrackNumber == 0)
                            {
                                playlist.SelectedIndex = 0;
                                labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(PlaylistTrackLoad.Files[0]);
                            }
                            if (BassCommandLibrary.isStopped)
                            {
                                labelLefttime.Content = null;
                                labelRightTime.Content = null;
                                labelCurrentPlayingName.Content = null;
                            }
                        }
                        catch
                        {
                            labelCurrentPlayingName.Content = null;
                        }
                    }

                    string current = PlaylistTrackLoad.Files[playlist.SelectedIndex];
                    PlaylistTrackLoad.CurrentTrackNumber = playlist.SelectedIndex;
                    labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(current);
                    try
                    {
                        TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                        TagLib.IPicture pic = f.Tag.Pictures[0];
                        var mStream = new MemoryStream(pic.Data.Data);
                        mStream.Seek(0, SeekOrigin.Begin);
                        BitmapImage bm = new BitmapImage();
                        bm.BeginInit();
                        bm.StreamSource = mStream;
                        bm.EndInit();
                        System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                        cover.Source = bm;
                        image.Source = bm;
                    }
                    catch
                    {
                        var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                        var img = new BitmapImage(uri);
                        image.Source = img;
                    }
                }
                if (BassCommandLibrary.endPlaylist)
                {
                    btnStop_Click(this, new RoutedEventArgs());
                    playlist.SelectedIndex = PlaylistTrackLoad.CurrentTrackNumber = 0;
                    BassCommandLibrary.endPlaylist = false;
                }
            }

        }
        // Нажатие на кнопку "Стоп"
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainWindowViewModel).Stop();
            dtimer.Stop();
            BassCommandLibrary.Stop();
            slTime.Value = 0;
            labelLefttime.Content = "00:00:00";
            labelRightTime.Content = "00:00:00";
            labelNowPlaying.Content = " ";
            labelCurrentPlayingName.Content = " ";
        }
        // При перемотке песни добавить текущую поз. воспроизведения в поток
        private void slTime_Scroll(object sender, ScrollEventArgs e)
        {
            BassCommandLibrary.SetPosOfScroll(BassCommandLibrary.Stream, (int)slTime.Value);
        }

        // Нажатие на кнопку "Пауза"
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainWindowViewModel).Pause();
            BassCommandLibrary.Pause();
        }
        //Изменение позиции воспроизведения
        private void slTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
            labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
            BassCommandLibrary.SetPosOfScroll(BassCommandLibrary.Stream, (int)slTime.Value);
        }

        // Открыть папку и добавить файлы с неё
        private void btn_EjectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            dlg.ShowNewFolderButton = false;
            if (System.Windows.Forms.DialogResult.OK == result)
            {
                dtimer.Tick += new EventHandler(dtimer_Tick);
                dtimer.Start();
                foreach (string currentFile in System.IO.Directory.GetFiles(dlg.SelectedPath, "*.mp3", SearchOption.AllDirectories))
                {
                    PlaylistTrackLoad.Files.Add(currentFile);
                    TagCommandLibrary TM = new TagCommandLibrary(currentFile);
                    playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }
                foreach (string currentFile in System.IO.Directory.GetFiles(dlg.SelectedPath, "*.flac", SearchOption.AllDirectories))
                {
                    PlaylistTrackLoad.Files.Add(currentFile);
                    TagCommandLibrary TM = new TagCommandLibrary(currentFile);
                    playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }
            }

        }
        // Нажатие на кнопку "Плей"
        private void btnPlay_Click(object sender, MouseButtonEventArgs e)
        {

            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                dtimer.Start();
                labelNowPlaying.Content = "Сейчас играет:";
                string current = PlaylistTrackLoad.Files[playlist.SelectedIndex];
                PlaylistTrackLoad.CurrentTrackNumber = playlist.SelectedIndex;
                BassCommandLibrary.Play(current, 0);
                (this.DataContext as MainWindowViewModel).OpenFile(PlaylistTrackLoad.Files[playlist.SelectedIndex]);
                labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
                slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(current);
                //************************************************************************//
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img; ;
                }
                //***************************************************************************************//
            }
        }
        // Нажатие на кнопку "Следующий трек"
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Сейчас играет:";
                BassCommandLibrary.Next();
                (this.DataContext as MainWindowViewModel).OpenFile(PlaylistTrackLoad.Files[PlaylistTrackLoad.CurrentTrackNumber]);
              
                labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
                slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                try
                {
                    if (PlaylistTrackLoad.Files.Count >= PlaylistTrackLoad.CurrentTrackNumber + 1)
                    {
                        ++playlist.SelectedIndex;
                        labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName((PlaylistTrackLoad.Files[PlaylistTrackLoad.CurrentTrackNumber]));
                    }
                    else if (PlaylistTrackLoad.CurrentTrackNumber == PlaylistTrackLoad.Files.Count)
                        labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(PlaylistTrackLoad.Files[playlist.SelectedIndex]);
                    if (BassCommandLibrary.isStopped)
                    {
                        labelLefttime.Content = null;
                        labelRightTime.Content = null;
                        labelCurrentPlayingName.Content = null;
                    }
                }
                catch
                {
                    labelCurrentPlayingName.Content = null;
                }
                //***************************************************************************************//
                string current = PlaylistTrackLoad.Files[playlist.SelectedIndex];
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
                //***************************************************************************************//
            }
        }
         // Нажатие на кнопку "Предыдущий трек"
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Сейчас играет:";
                BassCommandLibrary.Prev();
                (this.DataContext as MainWindowViewModel).OpenFile(PlaylistTrackLoad.Files[PlaylistTrackLoad.CurrentTrackNumber]);
                
                labelLefttime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream)).ToString();
                slTime.Maximum = BassCommandLibrary.GetTimeOfStream(BassCommandLibrary.Stream);
                slTime.Value = BassCommandLibrary.GetPosOfStream(BassCommandLibrary.Stream);
                if ((PlaylistTrackLoad.CurrentTrackNumber - 1) >= 0)
                {
                    --playlist.SelectedIndex;
                    labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName((PlaylistTrackLoad.Files[PlaylistTrackLoad.CurrentTrackNumber]));
                    if (PlaylistTrackLoad.CurrentTrackNumber == 0)
                    {
                        playlist.SelectedIndex = 0;
                    }
                }
                else if (PlaylistTrackLoad.CurrentTrackNumber == 0)
                {
                    playlist.SelectedIndex = 0;
                    labelCurrentPlayingName.Content = PlaylistTrackLoad.GetFileName(PlaylistTrackLoad.Files[0]);
                }
                if (BassCommandLibrary.isStopped)
                {
                    labelLefttime.Content = null;
                    labelRightTime.Content = null;
                    labelCurrentPlayingName.Content = null;
                }

                //******************************************************************************************//
                string current = PlaylistTrackLoad.Files[playlist.SelectedIndex];
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
            }
            //***************************************************************************************//

        }
        // Нажатие на кнопку "Удалить трек из плейлиста"
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                (this.DataContext as MainWindowViewModel).Stop();
                BassCommandLibrary.Stop();
                PlaylistTrackLoad.CurrentTrackNumber = playlist.SelectedIndex;
                PlaylistTrackLoad.Files.Remove(PlaylistTrackLoad.Files[playlist.SelectedIndex]);
                playlist.Items.RemoveAt(playlist.SelectedIndex);
                var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                var img = new BitmapImage(uri);
                image.Source = img;
                labelLefttime.Content = "00:00:00";
                labelRightTime.Content = "00:00:00";
                labelNowPlaying.Content = null;
                labelCurrentPlayingName.Content = null;
            }
        }
        // Запрет перемещения в листбоксе (плейлисте) стрелочками
        private void listbox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
        //Выбрать изображение для плейлиста
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog(); //создание диалогового окна для выбора файла
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"; //формат загружаемого файла
            open_dialog.ShowDialog();
            {
                try
                {
                    NowPlayList.Source = new BitmapImage(new Uri(open_dialog.FileName));
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //Сохранить плейлист
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var play = new PlayList();
            play.Name = NowNamePlayList.Text;
            play.image = NowPlayList.Source;
            play.sounds = PlaylistTrackLoad.Files;
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PlayList Files(*.PLT)|*.PLT;";
            save.ShowDialog();
            using (StreamWriter file = File.CreateText(save.FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, play);
            }
        }
        //загрузить плейлист
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "PlayList Files(*.PLT)|*.PLT;";
            open.ShowDialog();
            JsonSerializer serializer = new JsonSerializer();
            string str = new StreamReader(open.FileName).ReadToEnd();
            PlayList play = JsonConvert.DeserializeObject<PlayList>(str);
            NowNamePlayList.Text = play.Name;
            NowPlayList.Source = play.image;
            (this.DataContext as MainWindowViewModel).Stop();
            dtimer.Stop();
            BassCommandLibrary.Stop();
            PlaylistTrackLoad.Files.Clear();
            foreach (var sound in play.sounds)
            {
                if (File.Exists(sound))
                {
                    PlaylistTrackLoad.Files.Add(sound);
                    TagCommandLibrary TM = new TagCommandLibrary(sound);
                    playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }
            }
        }
        //перемотка
        private void slTime_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as MainWindowViewModel).SetPosition((int)slTime.Value);
        }
    }
}
  
