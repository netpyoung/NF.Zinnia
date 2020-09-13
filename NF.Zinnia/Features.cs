using System;
using System.Collections.Generic;
using System.Linq;

namespace NF.Zinnia
{
    public class Features
    {
        public sealed class NodePair
        {
            public Node First;
            public Node Last;
        }

        public sealed class NodeSet
        {
            public NodeSet(Node first, Node last)
            {
                this.First = first;
                this.Last = last;
            }

            public Node First;
            public Node Last;
            public Node Best;
        }

        private const int MAX_CHARACTER_SIZE = 50;
        private readonly List<FeatureNode> _features = new List<FeatureNode>();

        public List<FeatureNode> Get() => _features;

        public static float Distance(Node n1, Node n2)
        {
            float x = n1.X - n2.X;
            float y = n1.Y - n2.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static float Distance2(Node n1)
        {
            float x = n1.X - 0.5f;
            float y = n1.Y - 0.5f;
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static float MinimumDistance(NodeSet nodeSet)
        {
            if (nodeSet.First == nodeSet.Last)
            {
                return 0;
            }

            float a = nodeSet.Last.X - nodeSet.First.X;
            float b = nodeSet.Last.Y - nodeSet.First.Y;
            float c = nodeSet.Last.Y * nodeSet.First.X - nodeSet.Last.X * nodeSet.First.Y;

            float max = -1;
            for (Node n = nodeSet.First; n != nodeSet.Last; n = n.Next)
            {
                float dist = Math.Abs((a * n.Y) - (b * n.X) + c);
                if (dist > max)
                {
                    max = dist;
                    nodeSet.Best = n;
                }
            }
            return max * max / (a * a + b * b);
        }

        public static Features Read(Character character)
        {
            Features features = new Features();
            Node prev = null;

            // bias term
            {
                FeatureNode f = new FeatureNode
                {
                    Index = 0,
                    Value = 1.0
                };
                features._features.Add(f);
            }

            List<List<Node>> nodes = new List<List<Node>>(character.GetStrokesSize());
            for (int i = 0; i < character.GetStrokesSize(); ++i)
            {
                nodes.Add(new List<Node>());
            }

            {
                int height = character.GetHeight();
                int width = character.GetWidth();

                if (height == 0 || width == 0)
                {
                    return null;
                }

                if (character.GetStrokesSize() == 0)
                {
                    return null;
                }

                for (int i = 0; i < character.GetStrokesSize(); ++i)
                {
                    int ssize = character.GetStrokeSize(i);
                    if (ssize == 0)
                    {
                        return null;
                    }

                    for (int j = 0; j < ssize; ++j)
                    {
                        Node n = new Node(character.GetX(i, j) / width, character.GetY(i, j) / height);
                        if (j > 0)
                        {
                            nodes[i][j - 1].Next = n;
                        }
                        nodes[i].Add(n);
                    }
                }
            }


            for (int sid = 0; sid < nodes.Count; ++sid)
            {
                List<NodePair> nodePairs = new List<NodePair>();
                Node first = nodes[sid].First();
                Node last = nodes[sid].Last();

                features.GetVertex(first, last, 0, nodePairs);
                features.MakeVertexFeature(sid, nodePairs);
                if (prev != null)
                {
                    features.MakeMoveFeature(sid, prev, first);
                }
                prev = last;
            }

            features.AddFeature(2000000, nodes.Count);
            features.AddFeature(2000000 + nodes.Count, 10);

            features._features.Sort((f1, f2) => f1.Index - f2.Index);

            {
                FeatureNode f = new FeatureNode
                {
                    Index = -1,
                    Value = 0
                };
                features._features.Add(f);
            }
            return features;
        }

        private void AddFeature(int index, float value)
        {
            FeatureNode f = new FeatureNode
            {
                Index = index,
                Value = value
            };
            _features.Add(f);
        }

        private void MakeMoveFeature(int sid, Node first, Node last)
        {
            int offset = 100000 + sid * 1000;
            MakeBasicFeature(offset, first, last);
        }

        private void MakeVertexFeature(int sid, List<NodePair> nodePairs)
        {
            for (int i = 0; i < nodePairs.Count; ++i)
            {
                if (i > MAX_CHARACTER_SIZE)
                {
                    break;
                }

                Node first = nodePairs[i].First;
                Node last = nodePairs[i].Last;
                if (first == null)
                {
                    continue;
                }
                int offset = sid * 1000 + 20 * i;
                MakeBasicFeature(offset, first, last);
            }
        }

        private void MakeBasicFeature(int offset, Node first, Node last)
        {
            // distance
            AddFeature(offset + 1, 10 * Distance(first, last));

            // degree
            AddFeature(offset + 2, (float)Math.Atan2(last.Y - first.Y, last.X - first.X));

            // absolute position
            AddFeature(offset + 3, 10 * (first.X - 0.5f));
            AddFeature(offset + 4, 10 * (first.Y - 0.5f));
            AddFeature(offset + 5, 10 * (last.X - 0.5f));
            AddFeature(offset + 6, 10 * (last.Y - 0.5f));

            // absolute degree
            AddFeature(offset + 7, (float)Math.Atan2(first.Y - 0.5, first.X - 0.5));
            AddFeature(offset + 8, (float)Math.Atan2(last.Y - 0.5, last.X - 0.5));

            // absolute distance
            AddFeature(offset + 9, 10 * Distance2(first));
            AddFeature(offset + 10, 10 * Distance2(last));

            //diff
            AddFeature(offset + 11, 5 * (last.X - first.X));
            AddFeature(offset + 12, 5 * (last.Y - first.Y));
        }

        private void GetVertex(Node first, Node last, int id, List<NodePair> nodePairs)
        {
            if (nodePairs.Count <= id)
            {
                for (int size = nodePairs.Count; size <= id; ++size)
                {
                    nodePairs.Add(new NodePair());
                }
            }

            NodePair pair = nodePairs[id];
            pair.First = first;
            pair.Last = last;

            NodeSet nodeSet = new NodeSet(first, last);
            double dist = MinimumDistance(nodeSet);

            const float error = 0.001f;
            if (dist > error)
            {
                GetVertex(nodeSet.First, nodeSet.Best, id * 2 + 1, nodePairs);
                GetVertex(nodeSet.Best, nodeSet.Last, id * 2 + 2, nodePairs);
            }
        }
    }
}
