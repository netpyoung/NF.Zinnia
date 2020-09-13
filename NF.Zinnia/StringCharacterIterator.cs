using System;

namespace NF.Zinnia
{
    public sealed class StringCharacterIterator
    {
        public const char DONE = '\uffff';

        private string _str;
        private int _start;
        private int _end;
        private int _offset;

        public StringCharacterIterator(string value)
        {
            this._str = value;
            this._start = 0;
            this._end = _str.Length;
            this._offset = 0;
        }

        public StringCharacterIterator(string value, int location)
        {
            if (location < 0 || value.Length < location)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._str = value;
            this._start = 0;
            this._end = _str.Length;
            this._offset = location;
        }

        public StringCharacterIterator(string value, int start, int end, int location)
        {
            if ((start < 0 || end < start)
                || (location < start || end < location)
                || end > value.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._str = value;
            this._start = start;
            this._end = end;
            this._offset = location;
        }

        public char Current()
        {
            if (_offset == _end)
            {
                return DONE;
            }
            return _str[_offset];
        }

        public char First()
        {
            if (_start == _end)
            {
                return DONE;
            }

            _offset = _start;

            return _str[_offset];
        }

        public int GetBeginIndex()
        {
            return _start;
        }

        public int GetEndIndex()
        {
            return _end;
        }

        public int GetIndex()
        {
            return _offset;
        }

        public char Last()
        {
            if (_start == _end)
            {
                return DONE;
            }

            _offset = _end - 1;

            return _str[_offset];
        }

        public char Next()
        {
            if (_offset >= (_end - 1))
            {
                _offset = _end;
                return DONE;
            }

            _offset++;

            return _str[_offset];
        }

        public char Previous()
        {
            if (_offset == _start)
            {
                return DONE;
            }

            _offset--;

            return _str[_offset];
        }

        public char SetIndex(int location)
        {
            if (location < _start || _end < location)
            {
                throw new ArgumentOutOfRangeException();
            }

            _offset = location;

            if (_offset == _end)
            {
                return DONE;
            }

            return _str[_offset];
        }

        public void SetText(string value)
        {
            _str = value;
            _start = 0;
            _offset = 0;
            _end = _str.Length;
        }
    }
}
