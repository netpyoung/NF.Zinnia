using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NF.Zinnia
{
    public class Recognizer
    {
        private sealed class Model
        {
            public string Character;
            public float Bias;
            public List<FeatureNode> X;
        }

        private readonly List<Model> _models = new List<Model>();

        public string GetValue(int i)
        {
            if (i >= _models.Count)
            {
                return null;
            }
            return _models[i].Character;
        }

        public bool Open(string filepath)
        {
            using (var reader = new BinaryReader(File.Open(filepath, FileMode.Open)))
            {
                return Open(reader);
            }
        }
        
        public bool Open(BinaryReader reader)
        {
            // ;; File Format
            // uint32 magic
            // uint32 version
            // uint32 size
            //
            // List<Model>
            // {
            //      byte[16] character-utf8
            //      float bias
            //
            //      List<FeatureNode>
            //      {
            //          int32 index
            //          float value
            //      }
            // }

            int magic = reader.ReadInt32();
            uint version = reader.ReadUInt32();

            int size = reader.ReadInt32();

            _models.Capacity = size;

            while (reader.PeekChar() != -1)
            {
                byte[] b = reader.ReadBytes(16);
                float bias = reader.ReadSingle();

                Model m = new Model();
                m.X = new List<FeatureNode>();
                m.Character = Encoding.UTF8.GetString(b);
                m.Character = m.Character.Trim();
                m.Bias = bias;

                while (true)
                {
                    FeatureNode f = new FeatureNode();
                    f.Index = reader.ReadInt32();
                    f.Value = reader.ReadSingle();
                    m.X.Add(f);
                    if (f.Index == -1)
                    {
                        break;
                    }
                }
                _models.Add(m);
            }
            return true;
        }

        public Result Classify(Character character, int nbest)
        {
            if (_models.Count == 0 || nbest <= 0)
            {
                return null;
            }

            Features feature = Features.Read(character);
            if (feature == null)
            {
                return null;
            }

            List<FeatureNode> x = feature.Get();
            List<(float first, string second)> results = new List<(float, string)>(_models.Count);

            for (int i = 0; i < _models.Count; ++i)
            {
                results.Add((_models[i].Bias + (float)FeatureNode.Dot(_models[i].X, x), _models[i].Character));
            }

            results.Sort((p1, p2) => Math.Sign(p2.first - p1.first));

            Result result = new Result();

            nbest = Math.Min(nbest, results.Count);

            for (int i = 0; result.Size < nbest; ++i)
            {
                result.Add(results[i].second, results[i].first);
            }
            return result;
        }
    }
}
