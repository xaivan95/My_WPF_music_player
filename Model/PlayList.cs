using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace My_new_player.Model
{
    public class PlayList
    {
        public string Name { get; set; }
        public ImageSource image { get; set; }
        public List<string> sounds { get; set; }
    }
}
