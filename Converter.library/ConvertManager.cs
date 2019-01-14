using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Converter.library
{
    public class ConvertManager
    {
        private readonly string _input;
        private readonly string _output;
        private readonly double _totalTime;

        public ConvertManager(string inputfile, string outputfolder) :
            this(inputfile, outputfolder, String.Format("{0}",Path.GetFileNameWithoutExtension(inputfile)), true)
        { }
        public ConvertManager(string inputfile, string outputfolder, string filename) : 
            this(inputfile, outputfolder, filename, false) { }

        public ConvertManager(string inputfile, string outputfolder, string filename, bool multithread)
        {
            if (!File.Exists(inputfile))
                throw new FileNotFoundException(String.Format("Le fichier {0} n'existe pas", inputfile));
            if (!Directory.Exists(outputfolder))
                throw new DirectoryNotFoundException(String.Format("Le dossier {0} n'existe pas", outputfolder));

            _input = inputfile;
            _output = string.Format("{0}{1}{2}", outputfolder, Path.DirectorySeparatorChar, filename);
            _totalTime = GetDuration(inputfile);
        }

        private double GetDuration(string inputfile)
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = @".\ffmpeg\bin\ffmpeg.exe",
                Arguments = String.Format("-i {0} -hide_banner", inputfile),
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(processInfo);
            StreamReader reader = process.StandardError;
            string output = reader.ReadToEnd();
            Regex r = new Regex("Duration: [0-9][0-9]:[0-9][0-9]:[0-9][0-9].[0-9][0-9]");
            Match matched = r.Match(output);
            string duration = matched.Value.Remove(0, 9);
            double res = TimeSpan.Parse(duration).TotalSeconds;
            process.WaitForExit();

            return res;
        }

        public byte StartConvertWebM()
        {
            if (File.Exists(String.Format("{0}.webm", _output)))
                return 1;
            Convert(VideoCodec.WebM, AudioCodec.Vorbis, "webm");

            return 0;
        }

        public byte StartConvertH264()
        {
            if (File.Exists(String.Format("{0}.mp4", _output)))
                return 1;
            Convert(VideoCodec.Mp4, AudioCodec.AAC, "mp4");
            return 0;
        }

        private void Convert(string codecVideo, string codecAudio, string extension)
        {
            string filename = String.Format("{0}.{1}", Path.GetFileName(_output), extension);
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = @".\ffmpeg\bin\ffmpeg.exe",
                Arguments = String.Format("-i {0} -c:v {1} -crf 10 -b:v 1M -c:a {2} {3}.{4}", _input, codecVideo, codecAudio, _output, extension),
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(processInfo);

            StreamReader sr = process.StandardError;
            while (!sr.EndOfStream)
            {
                OutputDataReceived(sr.ReadLine(), filename);
            }

            PrepareFinishHandler(filename);
            process.Close();
        }

        private void OutputDataReceived(string data, string filename)
        {
            try
            {
                Regex r = new Regex("time=[0-9][0-9]:[0-9][0-9]:[0-9][0-9].[0-9][0-9]");
                Match matched = r.Match(data);
                string duration = matched.Value.Remove(0, 5);
                double renderedTime = TimeSpan.Parse(duration).TotalSeconds;
                double percentage = Math.Round(renderedTime / _totalTime * 100, 2);
                /*if (percentage > 100)
                    percentage = 100.00;*/
                ChangePercentage(percentage, filename);
            }
            catch { }
        }

        private void ChangePercentage(double p, string filename)
        {
            PercentageChangedEventArgs u = new PercentageChangedEventArgs
            {
                Percentage = p,
                File = filename
            };
            PercentageChanged?.Invoke(this, u);
        }

        private void PrepareFinishHandler(string filename)
        {
            ConvertionFinishedEventArgs u = new ConvertionFinishedEventArgs
            {
                File = filename
            };

            Finished?.Invoke(this, u);
        }

        public event EventHandler<PercentageChangedEventArgs> PercentageChanged;
        public event EventHandler<ConvertionFinishedEventArgs> Finished;
    }
}
