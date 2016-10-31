using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SampleProject.Models
{
    public class TileModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public Uri ThumbnailUri { get; set; }

        public ICommand Command { get; set; }
    }
}
