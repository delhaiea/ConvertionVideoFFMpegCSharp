using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter.library
{
    public class PercentageChangedEventArgs : EventArgs
    {
        public double Percentage { get; set; }
        public string File { get; set; }
    }
}
