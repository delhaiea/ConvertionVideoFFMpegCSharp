using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Converter.library;

namespace Converter.ui
{
    class Program
    {
        static void Main(string[] args)
        {
            ConvertManager cm = new ConvertManager(@"C:\foo\test.avi", @"C:\foo");
            cm.PercentageChanged += PercentageChanged;
            cm.Finished += Finished;
            try
            {
                /*var res = cm.StartConvertWebM();
                if (res == 1)
                    Console.WriteLine("Le webm existe déjà => Annulé");*/
                var res = cm.StartConvertH264();
                if (res == 1)
                    Console.WriteLine("Le mp4 existe déjà => Annulé");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        public static void PercentageChanged(object obj, PercentageChangedEventArgs args)
        {
            Console.Write("Conversion... [{0}] {1}    \r", args.Percentage, args.File);
        }

        public static void Finished(object obj, ConvertionFinishedEventArgs args)
        {
            Console.WriteLine("\n{0} => Terminé", args.File);
        }
    }
}
