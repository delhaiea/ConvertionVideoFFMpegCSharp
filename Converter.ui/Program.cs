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
            if(args.Length == 0)
            {
                Console.WriteLine("Aucun argument specifier");
                AfficheHelp();
                Environment.Exit(0);
            }

            string argBeginUser = null, argEndUser = null, argBeginFinal = null, argEndFinal = null;
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
                argBeginUser = args[argPosD + 1];
                if (Array.IndexOf(args, "-f") != -1)
                {
                    int argPosF = Array.IndexOf(args, "-f");
                    argEndUser = args[argPosF + 1];
                    cutEnable = true;
                    Console.WriteLine(argBeginUser + " est " + timestampRegex.IsMatch(argBeginUser));
                    Console.WriteLine(argEndUser + " est " + timestampRegex.IsMatch(argEndUser));
                    if (!timestampRegex.IsMatch(argBeginUser) || !timestampRegex.IsMatch(argEndUser))
                    {
                        Console.WriteLine("Un ou des timestamp sont faux");
                        AfficheHelp();
                        Environment.Exit(0);
                    }
                    else if( !ValideurSimestamp(argBeginUser, argEndUser) )
                    {
                        Console.WriteLine("Le second timestamp est inférieur ou égal au premier.");
                        AfficheHelp();
                        Environment.Exit(0);
                    }
                    argBeginFinal = argBeginUser;
                    argEndFinal = CalculTimestampEnd(argBeginUser, argEndUser);

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
           
            if (cutEnable)
                cm = new ConvertManager(filePath, Directory.GetCurrentDirectory(), argBeginFinal, argEndFinal);
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
            }

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
            //
            Console.WriteLine("Usage: Converter [OPTIONS]... [FICHIER]...");
            Console.WriteLine("-d {timestamp} : Spécifie le timestamp a partir duquel la video sera découper. (-f obligatoire)");
            Console.WriteLine("-f {timestamp} : Spécifie le timestamp jusqu'où la video sera découper. (-d obligatoire)");
            Console.WriteLine("--webm : spécifie le format de sortie en webm.");
            Console.WriteLine("--mp4 : spécifie le format de sortie en mp4.");
            Console.WriteLine("--all : Utiliser pour convertir la vidéo en webm ET mp4. (par défaut)");
            Console.WriteLine("{timestamp} Format : hh:mm:ss");
            Console.WriteLine("FICHIER Le fichier peut être un chemin relatif ou absolue.");

        }

        public static bool ValideurSimestamp(string argDebut, string argEnd)
        {
            int[] timeSpec = { 3600, 60, 0 };
            string[] args = { argDebut, argEnd };

            int[] argsSecondes = { 0, 0 };

            for(int i = 0; i < 2; i++)
            {
                string[] details = args[i].Split(':');

                for (int y = 0; y < details.Length; y++)
                {
                    argsSecondes[i] += Int32.Parse(details[y]) * timeSpec[y];
                }
            }

            if(argsSecondes[0] >= argsSecondes[1])
            {
                return false;
            }

            return true;
        }

        static string CalculTimestampEnd(string argBeginUser, string argEndUser)
        {
            int[] timeSpec = { 3600, 60, 0 };
            string[] args = { argBeginUser, argEndUser };

            int[] argsSecondes = { 0, 0 };

            for (int i = 0; i < 2; i++)
            {
                string[] details = args[i].Split(':');

                for(int y = 0; y < details.Length; y++)
                {
                    argsSecondes[i] += Int32.Parse(details[y]) * timeSpec[y]; 
                }
            }

            int argEndFinalSeconds = argsSecondes[1] - argsSecondes[0];
            int[] argEndFinalDetails = { 0, 0, 0 };
            string argEndFinal = "";
            Console.WriteLine(argEndFinalSeconds);
            while(argEndFinalSeconds >= 3600)
            {
                argEndFinalDetails[0] += 1;
                argEndFinalSeconds -= 3600;
            }

            while (argEndFinalSeconds >= 60)
            {
                argEndFinalDetails[1] += 1;
                argEndFinalSeconds -= 60;
            }

            argEndFinalDetails[2] = argEndFinalSeconds;

            if (argEndFinalDetails[0] < 10)
                argEndFinal += '0' + argEndFinalDetails[0].ToString();
            else
                argEndFinal += '0' + argEndFinalDetails[0].ToString();

            argEndFinal += ':';

            if (argEndFinalDetails[1] < 10)
                argEndFinal += '0' + argEndFinalDetails[1].ToString();
            else
                argEndFinal += argEndFinalDetails[1].ToString();

            argEndFinal += ':';

            if (argEndFinalDetails[2] < 10)
                argEndFinal += '0' + argEndFinalDetails[2].ToString();
            else
                argEndFinal += argEndFinalDetails[1].ToString();
#if DEBUG
            Console.WriteLine("debut {0}, fin {1}, calculer {2} ", argBeginUser, argEndUser, argEndFinal);
#endif
            return argEndFinal;
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
