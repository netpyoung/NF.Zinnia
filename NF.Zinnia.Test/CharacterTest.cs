using Xunit;

namespace NF.Zinnia.Test
{
    public class CharacterTest
    {
        [Fact]
        public void Test1()
        {
            // (character
            //    (width canvas width)
            //    (height canvas height)
            //    (strokes
            //      ((0 - th - stroke 0 - th - strokey)... (0 - th - stroke 0 - th - strokey))
            //      ((1 - th - stroke 0 - th - strokey) ... (1 - th - stroke 1 - th - strokey))
            //      ((2 - th - stroke 2 - th - strokey) ... (2 - th - stroke 2 - th - strokey))
            //      ...)
            // )

            var str = "(character (value кв) (strokes ((1 1)(2 2))))";
            var character = Character.Parse(str);
            Assert.Equal("кв", character.GetValue());
            Assert.Equal(1, character.GetStrokesSize());
            Assert.Equal(2, character.GetStrokeSize(0));
        }
    }
}
