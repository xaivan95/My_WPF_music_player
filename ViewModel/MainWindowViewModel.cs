using My_new_player.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using NAudio.Extras;
using NAudio.Wave;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

namespace My_new_player.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private AudioFileReader reader;
        private IWavePlayer player;
        private Equalizer equalizer;
        private string selectedFile;
        private readonly EqualizerBand[] bands;

        public MainWindowViewModel()
        {
            bands = new EqualizerBand[]
                    {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = 0},
                    };
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            equalizer?.Update();
        }

        public float MinimumGain => -30;

        public float MaximumGain => 30;

        public float Band1
        {
            get => bands[0].Gain;
            set
            {
                if (bands[0].Gain != value)
                {
                    bands[0].Gain = value;
                    OnPropertyChanged("Band1");
                }
            }
        }

        public float Band2
        {
            get => bands[1].Gain;
            set
            {
                if (bands[1].Gain != value)
                {
                    bands[1].Gain = value;
                    OnPropertyChanged("Band2");
                }
            }
        }

        public float Band3
        {
            get => bands[2].Gain;
            set
            {
                if (bands[2].Gain != value)
                {
                    bands[2].Gain = value;
                    OnPropertyChanged("Band3");
                }
            }
        }

        public float Band4
        {
            get => bands[3].Gain;
            set
            {
                if (bands[3].Gain != value)
                {
                    bands[3].Gain = value;
                    OnPropertyChanged("Band4");
                }
            }
        }

        public float Band5
        {
            get => bands[4].Gain;
            set
            {
                if (bands[4].Gain != value)
                {
                    bands[4].Gain = value;
                    OnPropertyChanged("Band5");
                }
            }
        }

        public float Band6
        {
            get => bands[5].Gain;
            set
            {
                if (bands[5].Gain != value)
                {
                    bands[5].Gain = value;
                    OnPropertyChanged("Band6");
                }
            }
        }


        public float Band7
        {
            get => bands[6].Gain;
            set
            {
                if (bands[6].Gain != value)
                {
                    bands[6].Gain = value;
                    OnPropertyChanged("Band7");
                }
            }
        }

        public float Band8
        {
            get => bands[7].Gain;
            set
            {
                if (bands[7].Gain != value)
                {
                    bands[7].Gain = value;
                    OnPropertyChanged("Band7");
                }
            }
        }

        public void OpenFile(string FileName)
        {
            player?.Stop();
            reader = new AudioFileReader(FileName);
                equalizer = new Equalizer(reader, bands);
                player = new WaveOutEvent();
                player.Init(equalizer);
                player.Play();
        }

        public void SetPosition(int time)
        {
            reader.CurrentTime = new TimeSpan(0, 0, time);

        }
        public void Pause()
        {
            player?.Pause();
        }

        public void Stop()
        {
            player?.Stop();
        }

        public void Dispose()
        {
            player?.Dispose();
            reader?.Dispose();
        }
    }
}
