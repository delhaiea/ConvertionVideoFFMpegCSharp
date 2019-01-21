using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Converter.library;

namespace Converter.ui
{
    class Program
    {
        static void Main(string[] args)
        {
            string argBegin = null, argEnd = null;
            bool cutEnable = false;
            bool formatWebm = false, formatMp4 = false;
            string filePath = null, fileName = null;
            Regex timestampRegex = new Regex("[0-9]{2}:[0-9]{2}:[0-9]{2}$");


            if (args[0] == "--help" || args[0] == "-h")
            {
                Console.WriteLine("Help demander");
                AfficheHelp();
                Environment.Exit(0);
            }

            if (Array.IndexOf(args, "-d") != -1)
            {
                int argPosD = Array.IndexOf(args, "-d");
                argBegin = args[argPosD + 1];
                if (Array.IndexOf(args, "-f") != -1)
                {
                    int argPosF = Array.IndexOf(args, "-f");
                    argEnd = args[argPosF + 1];
                    cutEnable = true;
                    Console.WriteLine(argBegin + " est " + timestampRegex.IsMatch(argBegin));
                    Console.WriteLine(argEnd + " est " + timestampRegex.IsMatch(argEnd));
                    if (!timestampRegex.IsMatch(argBegin) || !timestampRegex.IsMatch(argEnd))
                    {
                        Console.WriteLine("Un ou des timestamp sont faux");
                        AfficheHelp();
                        Environment.Exit(0);
                    }
                    else if( !ValideurSimestamp(argBegin, argEnd) )
                    {
                        Console.WriteLine("Le second timestamp est inférieur ou égal au premier.");
                        AfficheHelp();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.WriteLine("-d specifier mais pas -f");
                    AfficheHelp();
                    Environment.Exit(0);
                }
            }


            if(Array.IndexOf(args, "--webm") != -1)
            {
                formatWebm = true;
            }else if(Array.IndexOf(args, "--mp4") != -1)
            {
                formatMp4 = true;
            }

            if(formatMp4 && formatWebm || !formatMp4 && !formatWebm)
            {
                Console.WriteLine("Double format ou pas de format");
                AfficheHelp();
                Environment.Exit(0);
            }

            fileName = args[args.Length - 1];

            if (Path.IsPathRooted(fileName))
            {
                filePath = fileName;
            }
            else
            {
                filePath = Directory.GetCurrentDirectory() + '\\' + fileName;
            };
            Console.WriteLine(filePath);

            ConvertManager cm = null;
            /*
            if (cutEnable)
                cm = new ConvertManager(filePath, Directory.GetCurrentDirectory(), argBegin, argEnd);
            else
                cm = new ConvertManager(filePath, Directory.GetCurrentDirectory());
            
            cm.PercentageChanged += PercentageChanged;
            cm.Finished += Finished;
            try
            {
                if (formatWebm)
                {
                    var res = cm.StartConvertWebM();
                    if (res == 1)
                        Console.WriteLine("Le webm existe déjà => Annulé");
                }
                else
                {
                    var res = cm.StartConvertH264();
                    if (res == 1)
                        Console.WriteLine("Le mp4 existe déjà => Annulé");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }*/

#if DEBUG
            Console.ReadKey();
#endif
        }

        public static void AfficheHelp()
        {
            Console.WriteLine("Converter --help {-d(timestamp debut) -f(timestamp fin) 00:00:00} {--webm --mp4} nom/chemin vers le fichier");
            Console.WriteLine("Ex: Converter -d 00:01:10 -f 00:21:10 --webm");
            Console.WriteLine("Par defaut, convertis toute la video.");
            Console.WriteLine("Si -d est spécifier, il faut spécifier obligatoirement -f");
            Console.WriteLine("Il est faut spécifier le format, --webm OU --mp4");
        }

        public static bool ValideurSimestamp(string argDebut, string argEnd)
        {
            string[] args = { argDebut, argEnd };

            int[] argsSecondes = { 0, 0 };

            for(int i = 0; i < 2; i++)
            {
                string[] details = args[i].Split(':');
                
                foreach(string detailsTmp in details)
                {
                    argsSecondes[i] += Int32.Parse(detailsTmp);
                }
            }

            if(argsSecondes[0] >= argsSecondes[1])
            {
                return false;
            }

            return true;
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
