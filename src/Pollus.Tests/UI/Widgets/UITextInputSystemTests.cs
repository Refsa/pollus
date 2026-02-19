namespace Pollus.Tests.UI.Widgets;

using Pollus.UI;

public class UITextInputSystemTests
{
    [Fact]
    public void GetPrevOffset_SingleWord_JumpsToStart()
    {
        Assert.Equal(5, UITextInputSystem.GetPrevOffset("hello", 5));
    }

    [Fact]
    public void GetPrevOffset_TwoWords_JumpsToWordStart()
    {
        // "hello world" cursor at end (11) -> jumps back 5 ("world")
        Assert.Equal(5, UITextInputSystem.GetPrevOffset("hello world", 11));
    }

    [Fact]
    public void GetPrevOffset_TwoWords_CursorAtSecondWordStart()
    {
        // "hello world" cursor at 6 (start of "world") -> jumps back 6
        Assert.Equal(6, UITextInputSystem.GetPrevOffset("hello world", 6));
    }

    [Fact]
    public void GetPrevOffset_CursorAfterSpaces_SkipsSpaces()
    {
        // "hello   world" cursor at end (13) -> jumps back 5 ("world")
        Assert.Equal(5, UITextInputSystem.GetPrevOffset("hello   world", 13));
    }

    [Fact]
    public void GetPrevOffset_CursorAtOne_ReturnsOne()
    {
        Assert.Equal(1, UITextInputSystem.GetPrevOffset("hello", 1));
    }

    [Fact]
    public void GetPrevOffset_TrailingSpaces_SkipsSpacesToWord()
    {
        // "hello   " cursor at 8 -> skips 3 spaces + 5 chars = 8
        Assert.Equal(8, UITextInputSystem.GetPrevOffset("hello   ", 8));
    }

    [Fact]
    public void GetPrevOffset_MultipleWords_JumpsOneWord()
    {
        // "one two three" cursor at end (13) -> jumps back 5 ("three")
        Assert.Equal(5, UITextInputSystem.GetPrevOffset("one two three", 13));
    }

    [Fact]
    public void GetPrevOffset_CursorInMiddleOfWord()
    {
        // "hello world" cursor at 8 (on 'r') -> jumps back to 'w' at 6 = 2
        Assert.Equal(2, UITextInputSystem.GetPrevOffset("hello world", 8));
    }

    [Fact]
    public void GetPrevOffset_OnlySpaces_JumpsToStart()
    {
        Assert.Equal(3, UITextInputSystem.GetPrevOffset("   ", 3));
    }

    // GetNextOffset - returns how many chars to jump forward (word boundary)

    [Fact]
    public void GetNextOffset_SingleWord_JumpsToEnd()
    {
        Assert.Equal(5, UITextInputSystem.GetNextOffset("hello", 0));
    }

    [Fact]
    public void GetNextOffset_TwoWords_JumpsToFirstWordEnd()
    {
        // "hello world" cursor at 0 -> jumps 5 ("hello")
        Assert.Equal(5, UITextInputSystem.GetNextOffset("hello world", 0));
    }

    [Fact]
    public void GetNextOffset_CursorAtSpace_SkipsSpaceThenWord()
    {
        // "hello world" cursor at 5 (on space) -> skips space + "world" = 6
        Assert.Equal(6, UITextInputSystem.GetNextOffset("hello world", 5));
    }

    [Fact]
    public void GetNextOffset_MultipleSpaces_SkipsAllSpaces()
    {
        // "hello   world" cursor at 5 -> skips 3 spaces + "world" = 8
        Assert.Equal(8, UITextInputSystem.GetNextOffset("hello   world", 5));
    }

    [Fact]
    public void GetNextOffset_CursorInMiddleOfWord()
    {
        // "hello world" cursor at 2 -> jumps to end of "hello" = 3
        Assert.Equal(3, UITextInputSystem.GetNextOffset("hello world", 2));
    }

    [Fact]
    public void GetNextOffset_CursorNearEnd_JumpsToEnd()
    {
        // "hello" cursor at 4 -> 1
        Assert.Equal(1, UITextInputSystem.GetNextOffset("hello", 4));
    }

    [Fact]
    public void GetNextOffset_MultipleWords_JumpsOneWord()
    {
        // "one two three" cursor at 0 -> jumps 3 ("one")
        Assert.Equal(3, UITextInputSystem.GetNextOffset("one two three", 0));
    }

    [Fact]
    public void GetNextOffset_OnlySpaces_JumpsToEnd()
    {
        Assert.Equal(3, UITextInputSystem.GetNextOffset("   ", 0));
    }

    [Fact]
    public void GetNextOffset_LeadingSpaces()
    {
        // "  hello" cursor at 0 -> skips 2 spaces + "hello" = 7
        Assert.Equal(7, UITextInputSystem.GetNextOffset("  hello", 0));
    }

    // PassesFilter - character validation

    [Theory]
    [InlineData('a', true)]
    [InlineData('Z', true)]
    [InlineData('0', true)]
    [InlineData(' ', true)]
    [InlineData('.', true)]
    [InlineData('\n', false)]
    [InlineData('\t', false)]
    [InlineData('\0', false)]
    public void PassesFilter_Any(char ch, bool expected)
    {
        Assert.Equal(expected, UITextInputSystem.PassesFilter(ch, UIInputFilterType.Any, "", 0));
    }

    [Theory]
    [InlineData('0', "", 0, true)]
    [InlineData('9', "", 0, true)]
    [InlineData('a', "", 0, false)]
    [InlineData('.', "", 0, false)]
    [InlineData('-', "", 0, true)]       // minus at position 0, no existing minus
    [InlineData('-', "5", 1, false)]     // minus not at position 0
    [InlineData('-', "-5", 0, false)]    // already has minus
    public void PassesFilter_Integer(char ch, string text, int cursorPos, bool expected)
    {
        Assert.Equal(expected, UITextInputSystem.PassesFilter(ch, UIInputFilterType.Integer, text, cursorPos));
    }

    [Theory]
    [InlineData('5', "", 0, true)]
    [InlineData('.', "3", 1, true)]      // first dot
    [InlineData('.', "3.1", 3, false)]   // second dot
    [InlineData('-', "", 0, true)]       // minus at start
    [InlineData('-', "3", 1, false)]     // minus not at start
    [InlineData('-', "-3", 0, false)]    // duplicate minus
    [InlineData('a', "", 0, false)]
    public void PassesFilter_Decimal(char ch, string text, int cursorPos, bool expected)
    {
        Assert.Equal(expected, UITextInputSystem.PassesFilter(ch, UIInputFilterType.Decimal, text, cursorPos));
    }

    [Theory]
    [InlineData('a', true)]
    [InlineData('Z', true)]
    [InlineData('5', true)]
    [InlineData(' ', false)]
    [InlineData('.', false)]
    [InlineData('-', false)]
    public void PassesFilter_Alphanumeric(char ch, bool expected)
    {
        Assert.Equal(expected, UITextInputSystem.PassesFilter(ch, UIInputFilterType.Alphanumeric, "", 0));
    }
}
