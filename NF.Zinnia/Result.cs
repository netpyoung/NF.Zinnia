using System.Collections.Generic;

namespace NF.Zinnia
{
    public class Result
    {
        public List<(float score, string value)> Results { get; } = new List<(float, string)>();

        public int Size => Results.Count;

        public void Add(string character, float score)
        {
            Results.Add((score, character));
        }

        public void Clear()
        {
            Results.Clear();
        }

        public string GetValue(int v)
        {
            if (v >= Results.Count)
            {
                return null;
            }
            return Results[v].value;
        }

        public float GetScore(int v)
        {
            if (v >= Results.Count)
            {
                return -1;
            }
            return Results[v].score;
        }
    }
}
