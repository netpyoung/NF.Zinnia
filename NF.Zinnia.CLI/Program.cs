using CommandLine;
using System;
using System.IO;

namespace NF.Zinnia.CLI
{
    [Verb("learn", HelpText = "learn then create model")]
    public class LearnOption
    {
        [Option('t', "train", Required = true)]
        public string TrainFpath { get; set; }

        [Option('m', "model", Required = true)]
        public string ModelFpath { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<LearnOption>(args)
                .MapResult(
                options => new Program().Learn(options),
                errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }
                    return 1;
                });
        }

        private int Learn(LearnOption opt)
        {
            // var fpath = "C:/Users/netpyoung/Desktop/NF.Zinnia/test-train.txt";
            var trainer = new Trainer();
            using (var sr = new StreamReader(File.OpenRead(opt.TrainFpath)))
            {
                while (true)
                {
                    var s = sr.ReadLine();
                    if (s == null)
                    {
                        break;
                    }

                    var c = Character.Parse(s);
                    if (!trainer.Add(c))
                    {
                        throw new InvalidOperationException();
                    }
                }

            }
            trainer.Train(opt.ModelFpath);
            return 0;
        }
    }
}
