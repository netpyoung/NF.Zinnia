using System.Text;

namespace NF.Zinnia
{
    public class Sexp
    {
        private enum Stat
        {
            CONS,
            ATOM
        };

        public sealed class Cons
        {
            public Cell car;
            public Cell cdr;
        }

        public sealed class Cell
        {
            private Stat stat;
            private Cons cons;
            private string atom;

            public bool IsCons() => stat == Stat.CONS;
            public bool IsAtom() => stat == Stat.ATOM;

            public void SetCdr(Cell cell)
            {
                stat = Stat.CONS;
                if (cons == null)
                {
                    cons = new Cons();
                }
                cons.cdr = cell;
            }

            public void SetCar(Cell cell)
            {
                stat = Stat.CONS;
                if (cons == null)
                {
                    cons = new Cons();
                }
                cons.car = cell;
            }

            public void SetAtom(string atom)
            {
                stat = Stat.ATOM;
                this.atom = atom;
            }

            public string GetAtom() => atom;

            public Cell GetCar()
            {
                if (cons == null)
                {
                    return null;
                }

                return cons.car;
            }

            public Cell GetCdr()
            {
                if (cons == null)
                {
                    return null;
                }

                return cons.cdr;
            }
        }

        public Cell Read(string str)
        {
            return Read(new StringCharacterIterator($" {str}"));
        }

        public Cell Read(StringCharacterIterator sexp)
        {
            Comment(sexp);
            int r = NextToken(sexp, '(');
            if (r == 1)
            {
                return ReadCar(sexp);
            }

            if (r == 0)
            {
                return ReadAtom(sexp);
            }
            return null;
        }

        private Cell ReadCar(StringCharacterIterator sexp)
        {
            Comment(sexp);
            int r = NextToken(sexp, ')');
            if (r != 0)
            {
                return null;
            }

            Cell cell = new Cell();
            cell.SetCar(Read(sexp));
            cell.SetCdr(ReadCdr(sexp));
            return cell;
        }

        private Cell ReadCdr(StringCharacterIterator sexp)
        {
            Comment(sexp);
            int r = NextToken(sexp, ')');
            if (r != 0)
            {
                return null;
            }
            return ReadCar(sexp);
        }

        private int NextToken(StringCharacterIterator sexp, char n)
        {
            char c = sexp.Next();
            for (; c == ' '; c = sexp.Next())
            {
                if (c == StringCharacterIterator.DONE)
                {
                    return -1;
                }
            }

            if (c == n)
            {
                return 1;
            }

            sexp.Previous();
            return 0;
        }

        private void Comment(StringCharacterIterator sexp)
        {
            int r = NextToken(sexp, ';');
            if (r == 1)
            {
                for (char c = sexp.Next(); c != StringCharacterIterator.DONE; c = sexp.Next())
                {
                    if (c == '\r')
                    {
                        break;
                    }
                    if (c == '\n')
                    {
                        break;
                    }
                }
                Comment(sexp);
            }
        }

        private Cell ReadAtom(StringCharacterIterator sexp)
        {
            Comment(sexp);
            char c = sexp.Next();

            if (c == ' ')
            {
                return null;
            }

            if (c == '(')
            {
                return null;
            }

            if (c == ')')
            {
                return null;
            }

            if (c == StringCharacterIterator.DONE)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(c);
            while (true)
            {
                c = sexp.Next();
                sb.Append(c);
                if (c == ' ' || c == '(' || c == ')' || c == StringCharacterIterator.DONE)
                {
                    sexp.Previous();
                    sb.Remove(sb.Length - 1, 1);

                    Cell cell = new Cell();
                    cell.SetAtom(sb.ToString());
                    return cell;
                }
            }
        }
    }
}
