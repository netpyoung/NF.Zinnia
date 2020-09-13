using NF.Zinnia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StrokeStorage
{
    public DrawableTexture2D DrawableTexture2D;

    private Vector2 dragStart = Vector2.zero;
    private Vector2 dragEnd = Vector2.zero;
    private Vector2 dragPre = Vector2.zero;
    private int strokeIndex = -1;
    private Dictionary<int, List<Vector2>> dic = new Dictionary<int, List<Vector2>>();
    Recognizer r;
    Character s;
    public bool IsDragable(Vector2 p1) => (Vector2.Distance(p1, dragEnd) > 5);
    public bool IsDragStarted() => dragStart != Vector2.zero;

    // |
    // |
    // |
    // +------

    public void Init()
    {
        r = new Recognizer();
        var textAsset = Resources.Load<TextAsset>("handwriting-ja-light.model");
        //var textAsset = Resources.Load<TextAsset>("handwriting-ja.model");

        using (var ms = new MemoryStream(textAsset.bytes))
        {
            using (var br = new BinaryReader(ms))
            {
                r.Open(br);
            }
        }
        s = new Character(320, 240);
    }

    public void DragStarted(Vector2 p)
    {
        strokeIndex++;
        dic[strokeIndex] = new List<Vector2>();
        dragStart = p;
    }

    public void Drag(Vector2 p1)
    {
        dragEnd = p1;
        dragEnd.x = (float)Math.Round(dragEnd.x, 2);
        dragEnd.y = (float)Math.Round(dragEnd.y, 2);

        if (dragPre == Vector2.zero)
        {
            dragPre = dragEnd;
        }

        DrawableTexture2D.PaintLine(dragEnd, dragPre, 3, Color.red, 3);

        dragPre = dragEnd;

        DrawableTexture2D.Apply();

        dic[strokeIndex].Add(dragEnd);
    }

    public void DragEnded()
    {
        dragStart = Vector2.zero;
        dragEnd = Vector2.zero;
        dragPre = Vector2.zero;
    }

    public List<(float, string)> Print()
    {
        var ret = new List<(float, string)>();

        if (!dic.ContainsKey(strokeIndex))
        {
            return ret;
        }

        Debug.Log(string.Join(", ", dic[strokeIndex].Select(x => x.ToString()).ToArray()));

        s.Clear();
        int height = DrawableTexture2D._texture.height;
        foreach (var kv in dic.OrderBy(x => x.Key))
        {
            foreach (var p in kv.Value)
            {
                s.Add(kv.Key, (int)p.x, height - (int)p.y);
            }
        }


        var result = r.Classify(s, 10);
        for (int i = 0; i < result.Size; ++i)
        {
            Debug.Log($"{i} {result.GetValue(i)} \t {result.GetScore(i)}");
            ret.Add((result.GetScore(i), result.GetValue(i)));
        }
        return ret;
    }

    public void Clear()
    {
        Debug.Log("clear");
        dic.Clear();
        strokeIndex = -1;

        DrawableTexture2D.Clear();
        DrawableTexture2D.Apply();

        dragStart = Vector2.zero;
        dragEnd = Vector2.zero;
        dragPre = Vector2.zero;
    }

    public void Undo()
    {
        if (strokeIndex < 0)
        {
            return;
        }
        Debug.Log("undo");

        dic.Remove(strokeIndex);
        strokeIndex--;

        if (strokeIndex < 0)
        {
            DrawableTexture2D.Clear();
            DrawableTexture2D.Apply();
        }
        else
        {
            DrawableTexture2D.Clear();

            foreach (var kv in dic.OrderBy(x => x.Key))
            {
                dragPre = Vector2.zero;

                foreach (var p in kv.Value)
                {
                    dragEnd = p;
                    dragEnd.x = (float)Math.Round(dragEnd.x, 2);
                    dragEnd.y = (float)Math.Round(dragEnd.y, 2);

                    if (dragPre == Vector2.zero)
                    {
                        dragPre = dragEnd;
                    }

                    DrawableTexture2D.PaintLine(dragEnd, dragPre, 3, Color.red, 3);
                    dragPre = dragEnd;
                }
            }

            DrawableTexture2D.Apply();
        }

        dragStart = Vector2.zero;
        dragEnd = Vector2.zero;
        dragPre = Vector2.zero;
    }
}
