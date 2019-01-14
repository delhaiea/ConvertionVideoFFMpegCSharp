using System;
using Converter.library;

namespace Converter.ui
{
    class Program
    {
        static void Main(string[] args)
        {
            ConvertManager cm = new ConvertManager(@"C:\foo\rr.mov", @"C:\foo");
            cm.PercentageChanged += PercentageChanged;
            cm.Finished += Finished;
            try
            {
                var res = cm.StartConvertWebM();
                if (res == 1)
                    Console.WriteLine("Le webm existe déjà => Annulé");
                res = cm.StartConvertH264();
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
            Console.Write("Conversion...\t[{0}]\t{1}    \r", args.Percentage, args.File);
        }

        public static void Finished(object obj, ConvertionFinishedEventArgs args)
        {
            Console.WriteLine("Terminé \t[100]\t{0}", args.File);
        }
    }
}
