using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NF.Zinnia
{
    public class Trainer
    {
        private const int DIC_VERSION = 1;
        private const double THRESHOLD = 1.0e-3;

        private List<(string, List<FeatureNode>)> _x = new List<(string, List<FeatureNode>)>();
        private int _maxDim;

        static FeatureNode[] CopyFeatureNode(List<FeatureNode> fns, out int outMaxDim)
        {
            int d = 0;
            outMaxDim = 0;

            Debug.Assert(fns[0].Index == 0);
            Debug.Assert(fns[0].Value == 1);

            foreach (var f in fns)
            {
                if (f.Index < 0)
                {
                    break;
                }
                outMaxDim = Math.Max(f.Index, outMaxDim);
                d++;
            }

            FeatureNode[] x = new FeatureNode[d + 1];

            int i = 0;
            foreach (var f in fns)
            {
                if (f.Index < 0)
                {
                    break;
                }
                x[i].Index = f.Index;
                x[i].Value = f.Value;
                ++i;
            }

            x[i].Index = -1;
            x[i].Value = 0;

            return x;
        }

        static bool MakeExample(string key, List<(string, List<FeatureNode>)> x, List<double> y, List<List<FeatureNode>> copyX)
        {
            int posNum = 0;
            int negNum = 0;
            y.Clear();
            copyX.Clear();
            for (int i = 0; i < x.Count; ++i)
            {
                if (key == x[i].Item1)
                {
                    y.Add(1);
                    ++posNum;
                }
                else
                {
                    y.Add(-1);
                    ++negNum;
                }
                copyX.Add(x[i].Item2);
            }
            return (posNum > 0 && negNum > 0);
        }

        public bool Add(Character c)
        {
            string y = c.GetValue();
            Features features = Features.Read(c);
            List<FeatureNode> fn = features.Get();
            if (fn == null)
            {
                return false;
            }

            _x.Add((y, fn));
            foreach (FeatureNode node in fn)
            {
                _maxDim = Math.Max(node.Index, _maxDim);
            }

            return true;
        }

        public bool Train(string filename)
        {
            HashSet<string> dicSet = new HashSet<string>();
            for (int i = 0; i < _x.Count; ++i)
            {
                dicSet.Add(_x[i].Item1);
            }

            double[] w = new double[_maxDim + 1];
            List<double> y = new List<double>();
            List<List<FeatureNode>> xCopy = new List<List<FeatureNode>>();

            using (StreamWriter file = new StreamWriter($"{filename}.txt"))
            {
                List<string> dic = new List<string>(dicSet);
                for (int i = 0; i < dic.Count; ++i)
                {
                    if (!MakeExample(dic[i], _x, y, xCopy))
                    {
                        Console.Error.WriteLine("cannot make training data");
                    }

                    Console.Write($"learning: ({i}/{dic.Count}) {dic[i]} ");

                    SVM.Train(y.Count, w.Length, y, xCopy, 1.0, w);
                    file.Write(dic[i]);
                    file.Write(" ");
                    file.Write(((float)w[0]).ToString());

                    for (int j = 1; j < w.Length; ++j)
                    {
                        if (Math.Abs(w[j]) >= THRESHOLD)
                        {
                            file.Write(" ");
                            file.Write(j.ToString());
                            file.Write(":");
                            file.Write(((float)w[j]).ToString());
                        }
                    }
                    file.WriteLine();
                }
            }

            Convert($"{filename}.txt", filename, 0);
            return false;
        }

        public static bool MakeHeader(string textFilename, string headerFilename, string name, double compressionThreshold)
        {
            Recognizer r = new Recognizer();
            bool is_binary = r.Open(textFilename);

            string binary = textFilename;
            if (!is_binary)
            {
                binary = $"{headerFilename}.tmp";
                if (!Convert(textFilename, binary, compressionThreshold))
                {
                    return false;
                }
            }


            using (StreamWriter sw = new StreamWriter(File.Open(headerFilename, FileMode.Create)))
            {
                sw.WriteLine($"static const char {name}[] =");

                using (BinaryReader br = new BinaryReader(File.OpenRead(binary)))
                {
                    while (true)
                    {
                        int curr = br.ReadInt32();
                        if (curr == -1)
                        {
                            break;
                        }

                        int hi = (curr & 0xF0) >> 4;
                        int lo = (curr & 0x0F);

                        sw.Write("\\x");

                        if (hi >= 10)
                        {
                            sw.Write(hi - 10 + 'A');
                        }
                        else
                        {
                            sw.Write(hi + '0');
                        }

                        if (lo >= 10)
                        {
                            sw.Write(lo - 10 + 'A');
                        }
                        else
                        {
                            sw.Write(lo + '0');
                        }
                    }
                }
                sw.WriteLine("\";");
            }

            return true;
        }


        public static bool Convert(string textFilename, string binaryFilename, double compressionThreshold)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(binaryFilename, FileMode.Create)))
            {
                int magic = 0;
                int version = DIC_VERSION;
                int msize = 0;

                writer.Write(magic);
                writer.Write(version);
                writer.Write(msize);

                using (StreamReader file = new StreamReader(textFilename))
                {
                    while (true)
                    {
                        string line = file.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        string[] col = Regex.Split(line, @"[ \\t:]");
                        if (col.Length < 5)
                        {
                            return false;
                        }
                        if (col.Length % 2 != 0)
                        {
                            return false;
                        }

                        float bias = float.Parse(col[1]);
                        byte[] character = new byte[16];


                        var bytes = Encoding.UTF8.GetBytes(col[0]);
                        Array.Copy(bytes, character, bytes.Length);

                        writer.Write(character, 0, 16);
                        writer.Write(bias);

                        for (int i = 2; i < col.Length; i += 2)
                        {
                            int index = int.Parse(col[i]);
                            float value = float.Parse(col[i + 1]);
                            if (Math.Abs(value) > compressionThreshold)
                            {
                                writer.Write(index);
                                writer.Write(value);
                            }
                        }
                        writer.Write(-1);
                        writer.Write(0f);
                        ++msize;
                    }
                }

                magic = (int)writer.BaseStream.Position;
                magic ^= 0xef71821;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(magic);
                writer.Write(version);
                writer.Write(msize);
            }
            return true;
        }
    }
}