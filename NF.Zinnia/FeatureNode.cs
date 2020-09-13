using System;
using System.Collections.Generic;

namespace NF.Zinnia
{
    public class FeatureNode : IEquatable<FeatureNode>, IComparable<FeatureNode>
    {
        public int Index;
        public double Value;

        public int CompareTo(FeatureNode other)
        {
            return this.Index - other.Index;
        }

        public bool Equals(FeatureNode other)
        {
            return this.Index == other.Index;
        }

        public static double Dot(List<FeatureNode> x1s, List<FeatureNode> x2s)
        {
            double sum = 0;

            var x1Iter = x1s.GetEnumerator();
            var x2Iter = x2s.GetEnumerator();

            x1Iter.MoveNext();
            x2Iter.MoveNext();

            FeatureNode x1 = x1Iter.Current;
            FeatureNode x2 = x2Iter.Current;
            

            while (x1.Index >= 0 && x2.Index >= 0)
            {
                if (x1.Index == x2.Index)
                {
                    sum += (x1.Value * x2.Value);

                    x1Iter.MoveNext();
                    x2Iter.MoveNext();
                    x1 = x1Iter.Current;
                    x2 = x2Iter.Current;
                }
                else if (x1.Index < x2.Index)
                {
                    x1Iter.MoveNext();
                    x1 = x1Iter.Current;
                }
                else
                {
                    x2Iter.MoveNext();
                    x2 = x2Iter.Current;
                }
            }
            return sum;
        }
    }
}
