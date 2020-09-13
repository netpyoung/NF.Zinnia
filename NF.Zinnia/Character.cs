using System.Collections.Generic;
using System.Text;

namespace NF.Zinnia
{
    public class Character
    {
        public sealed class Dot
        {
            public int X;
            public int Y;

            public Dot(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private int _width;
        private int _height;
        private string _value;
        private readonly List<List<Dot>> _strokes = new List<List<Dot>>();
        private readonly Sexp _sexp = new Sexp();

        public Character()
        {
        }

        public Character(int width, int height)
        {
            this._width = width;
            this._height = height;
        }

        public string GetValue() => _value;
        public void SetValue(string value) => this._value = value;
        public void Clear() => _strokes.Clear();

        public int GetStrokesSize() => _strokes.Count;
        public int GetStrokeSize(int id)
        {
            if (_strokes.Count <= id)
            {
                return -1;
            }
            return _strokes[id].Count;
        }

        public int GetHeight() => _height;
        public void SetHeight(int height) => this._height = height;
        public int GetWidth() => _width;
        public void SetWidth(int width) => this._width = width;

        public float GetX(int id, int i)
        {
            if (id >= _strokes.Count)
            {
                return -1;
            }
            if (i >= _strokes[id].Count)
            {
                return -1;
            }
            return _strokes[id][i].X;
        }

        public float GetY(int id, int i)
        {
            if (id >= _strokes.Count)
            {
                return -1;
            }
            if (i >= _strokes[id].Count)
            {
                return -1;
            }
            return _strokes[id][i].Y;
        }

        public void Add(int id, int x, int y)
        {
            Dot d = new Dot(x, y);
            for (int size = _strokes.Count; size <= id; ++size)
            {
                _strokes.Add(new List<Dot>());
            }
            _strokes[id].Add(d);
        }

        public static Character Parse(string str)
        {
            Character c = new Character();
            Sexp.Cell rootCell = c._sexp.Read(str);
            if (rootCell == null)
            {
                return null;
            }

            Sexp.Cell ccell = rootCell.GetCar();
            if (!ccell.IsAtom())
            {
                return null;
            }

            if (ccell.GetAtom() != "character")
            {
                return null;
            }

            for (Sexp.Cell it = rootCell.GetCdr(); it != null; it = it.GetCdr())
            {
                Sexp.Cell cell = it.GetCar();
                if (cell.GetCar() != null
                    && cell.GetCar().IsAtom()
                    && cell.GetCdr() != null
                    && cell.GetCdr().GetCar() != null
                    && cell.GetCdr().GetCar().IsAtom())
                {
                    string name = cell.GetCar().GetAtom();
                    string value = cell.GetCdr().GetCar().GetAtom();

                    switch (name)
                    {
                        case "value":
                            c.SetValue(value);
                            break;
                        case "width":
                            c.SetWidth(int.Parse(value));
                            break;
                        case "height":
                            c.SetHeight(int.Parse(value));
                            break;
                        default:
                            break;
                    }
                }
                if (cell.GetCar() != null
                    && cell.GetCar().IsAtom()
                    && cell.GetCdr() != null
                    && cell.GetCdr().GetCar() != null
                    && cell.GetCdr().GetCar().IsCons())
                {
                    int id = 0;
                    for (Sexp.Cell st = cell.GetCdr(); st != null; st = st.GetCdr())
                    {
                        for (Sexp.Cell dot = st.GetCar(); dot != null; dot = dot.GetCdr())
                        {
                            if (dot.GetCar() != null
                                && dot.GetCar().GetCar() != null
                                && dot.GetCar().GetCar().IsAtom()
                                && dot.GetCar().GetCdr() != null
                                && dot.GetCar().GetCdr().GetCar() != null
                                && dot.GetCar().GetCdr().GetCar().IsAtom())
                            {
                                int x = int.Parse(dot.GetCar().GetCar().GetAtom());
                                int y = int.Parse(dot.GetCar().GetCdr().GetCar().GetAtom());
                                c.Add(id, x, y);
                            }
                        }
                        ++id;
                    }
                }
            }
            return c;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.Append($"(character ");
            {
                sb.Append($"(value {GetValue()})");
                sb.Append($"(width {GetWidth()})");
                sb.Append($"(height {GetHeight()})");
                sb.Append($"(strokes ");
                for (int id = 0; id < GetStrokesSize(); ++id)
                {
                    sb.Append("(");
                    for (int s = 0; s < GetStrokeSize(id); ++s)
                    {
                        sb.Append($"({GetX(id, s)} {GetY(id, s)})");
                    }
                    sb.Append(")");
                }
                sb.Append($")");
            }
            sb.Append($")");
            return sb.ToString();
        }
    }
}
