namespace Ludere.Utils.Parser;

using System.Numerics;
using Pollus.Mathematics;

public record struct ParserError : IError
{
    public enum Type
    {
        Fail,
        Panic,
    }

    public string Inner { get; }
    public Type ErrorType { get; }

    public ParserError(Type ErrorType, string inner)
    {
        Inner = inner;
        this.ErrorType = ErrorType;
    }

    public static ParserError Panic(IParser parser, string msg, in ParserState state)
    {
        return Panic($"{parser.GetType().Name}: {msg}", state);
    }

    public static ParserError Panic(string msg, in ParserState state)
    {
        int start = state.Original[..state.Cursor].LastIndexOf('\n') switch
        {
            -1 => 0,
            var idx => idx,
        };

        int cursor = state.Cursor - start;

        int end = state.Original[state.Cursor..].IndexOf('\r') switch
        {
            -1 => state.Original[state.Cursor..].IndexOf('\n') switch
            {
                -1 => state.Original.Length,
                var idx => state.Cursor + idx,
            },
            var idx => state.Cursor + idx,
        };

        string section = state.Original[start..end].ToString();

        if (start != 0)
        {
            section = "..." + section;
            cursor += 3;
        }

        if (end != state.Original.Length)
        {
            section += "...";
        }

        string marker = new('-', int.Max(cursor, 0));

        (int line, int pos) = FindPos(state);

        return new ParserError(Type.Panic, $"Parser Panic on {line}:{pos}\n{section}\n{marker}^\n{msg}");
    }

    static (int, int) FindPos(in ParserState state)
    {
        int totCounter = 0;
        int lineCounter = 0;
        foreach (ReadOnlySpan<char> line in state.Original.EnumerateLines())
        {
            totCounter += line.Length;
            if (totCounter >= state.Cursor)
            {
                return (lineCounter, totCounter - (totCounter - state.Cursor));
            }

            lineCounter += 1;
        }

        return (-1, -1);
    }

    public static ParserError Fail()
    {
        return new ParserError(Type.Fail, $"");
    }
}

public struct Parser
{
    public struct Output
    {
        Range range;
    }

    readonly string input;

    public Parser(string input)
    {
        this.input = input;
    }
}

public ref struct ParserState
{
    readonly ReadOnlySpan<char> original;
    readonly List<Range> ranges;
    readonly List<object> objects;
    int cursor;

    public ReadOnlySpan<char> Input { get; private set; }
    internal ReadOnlySpan<char> Original => original;
    public List<Range> Ranges => ranges!;
    public List<object> Objects => objects!;
    public int Cursor => cursor;
    public int RangesCursor => ranges.Count;
    public int LocalCursor => cursor - (original.Length - Input.Length);
    public bool ReachedEnd => cursor >= original.Length;

    public ParserState(string input) : this(input.AsSpan())
    {
    }

    public ParserState(ReadOnlySpan<char> input)
    {
        this.cursor = 0;
        this.original = input;
        this.Input = input;
        this.ranges = new();
        this.objects = new();
    }

    public Result<Empty, ParserError> Check()
    {
        if (ReachedEnd)
        {
            return ParserError.Panic("Reached end of file", this);
        }

        return Empty.Default;
    }

    public void SetCursor(int pos)
    {
        cursor = Math.Clamp(pos, 0, original.Length);
        Input = original[cursor..];
    }

    public void Step()
    {
        cursor = int.Min(cursor + 1, original.Length);
        Input = Input[1..];
    }

    public void StepBy(int step)
    {
        cursor = int.Min(cursor + step, original.Length);
        Input = Input[step..];
    }

    public void AppendRange(Range range)
    {
        ranges.Add(range);
    }

    public void AppendObject(object obj)
    {
        objects.Add(obj);
    }

    public void BacktrackRanges(int pos)
    {
        if (ranges.Count == 0) return;

        while (ranges.Count > pos)
        {
            ranges.RemoveAt(ranges.Count - 1);
        }
    }
}

public interface IParser
{
    Result<Empty, ParserError> Attempt(ref ParserState state);

    Result<Empty, ParserError> Parse(string input, out ParserState state)
    {
        state = new ParserState(input);
        var attempt = Attempt(ref state);
        return attempt;
    }

    Result<Empty, ParserError> Parse(ReadOnlySpan<char> input, out ParserState state)
    {
        state = new ParserState(input);
        var attempt = Attempt(ref state);
        return attempt;
    }

    IParser Optional()
    {
        return new OptionalParser(this);
    }

    IParser Not()
    {
        return new NotParser(this);
    }

    IParser Until(IParser stop)
    {
        return new UntilParser(this, stop);
    }

    IParser And(IParser next)
    {
        return new CombinatorParser(this, next);
    }

    IParser Or(IParser other)
    {
        return new EitherParser(this, other);
    }

    IParser Many()
    {
        return new ManyParser(this);
    }

    IParser Many1()
    {
        return new Many1Parser(this);
    }

    IParser Capture()
    {
        return new CaptureParser(this);
    }

    IParser As<T>(AsParser<T>.Predicate predicate)
    {
        return new AsParser<T>(this, predicate);
    }
}

public struct CombinatorParser : IParser
{
    readonly IParser current;
    readonly IParser? next;

    public CombinatorParser(IParser current, IParser? next)
    {
        this.current = current;
        this.next = next;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        var currentAttempt = current.Attempt(ref state);
        if (currentAttempt.IsErr())
        {
            return currentAttempt;
        }

        if (next is not null)
        {
            return next.Attempt(ref state);
        }

        return currentAttempt;
    }

    public override string ToString()
    {
        return $"Combinator [ {current} & {next} ]";
    }
}

public struct EitherParser : IParser
{
    readonly IParser first;
    readonly IParser second;

    public EitherParser(IParser first, IParser second)
    {
        this.first = first;
        this.second = second;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        var firstAttempt = first.Attempt(ref state);
        if (firstAttempt.IsOk())
        {
            return firstAttempt;
        }

        return second.Attempt(ref state);
    }

    public override string ToString()
    {
        return $"Either [ {first} | {second} ]";
    }
}

public struct OptionalParser : IParser
{
    readonly IParser parser;

    public OptionalParser(IParser parser)
    {
        this.parser = parser;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int cursor = state.Cursor;
        int rangesCursor = state.RangesCursor;

        var attempt = parser.Attempt(ref state);
        if (attempt.IsErr())
        {
            state.SetCursor(cursor);
            state.BacktrackRanges(rangesCursor);
        }

        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Optional [ {parser} ]";
    }
}

public struct UntilParser : IParser
{
    readonly IParser parser;
    readonly IParser stop;

    public UntilParser(IParser parser, IParser stop)
    {
        this.parser = parser;
        this.stop = stop;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int prevCursor = state.Cursor;
        while (true)
        {
            var stopAttempt = stop.Attempt(ref state);
            if (stopAttempt.IsOk())
            {
                break;
            }

            var parseAttempt = parser.Attempt(ref state);
            if (parseAttempt.IsErr())
            {
                return parseAttempt;
            }

            if (prevCursor == state.Cursor)
            {
                return ParserError.Panic(this, "Infinite loop, no progress detected", state);
            }

            prevCursor = state.Cursor;
        }

        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Until [ Parser: {parser}, Stop: {stop} ]";
    }
}

public struct AsParser<T> : IParser
{
    public delegate T Predicate(Range range, ReadOnlySpan<char> span);

    readonly IParser parser;
    readonly Predicate pred;

    public AsParser(IParser parser, Predicate pred)
    {
        this.parser = parser;
        this.pred = pred;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int startCursor = state.Cursor;
        var attempt = parser.Attempt(ref state);
        if (attempt.IsOk())
        {
            state.AppendRange(startCursor..state.Cursor);
            state.AppendObject(pred.Invoke(startCursor..state.Cursor, state.Original)!);
            return Empty.Default;
        }

        return attempt;
    }

    public override string ToString()
    {
        return $"As [ {parser} ][ {typeof(T)} ]";
    }
}

public struct ManyParser : IParser
{
    readonly IParser parser;

    public ManyParser(IParser parser)
    {
        this.parser = parser;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        while (true)
        {
            int cursor = state.Cursor;
            int rangesCursor = state.RangesCursor;

            var attempt = parser.Attempt(ref state);
            if (attempt.IsErr())
            {
                state.BacktrackRanges(rangesCursor);
                state.SetCursor(cursor);
                break;
            }

            if (cursor == state.Cursor)
            {
                return ParserError.Panic(this, "Infinite loop detected, no progress made", state);
            }
        }

        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Many [ {parser} ]";
    }
}

public struct Many1Parser : IParser
{
    readonly IParser parser;

    public Many1Parser(IParser parser)
    {
        this.parser = parser;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int rangesCursor = state.RangesCursor;
        var attempt = parser.Attempt(ref state);

        if (attempt.IsErr())
        {
            state.BacktrackRanges(rangesCursor);
            return ParserError.Panic($"Expected at least one {parser}", state);
        }

        while (attempt.IsOk())
        {
            attempt = parser.Attempt(ref state);
        }

        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Many1 [ {parser} ]";
    }
}

public struct NotParser : IParser
{
    readonly IParser parser;

    public NotParser(IParser parser)
    {
        this.parser = parser;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int rangesCursor = state.RangesCursor;
        var attempt = parser.Attempt(ref state);

        if (attempt.IsOk())
        {
            state.BacktrackRanges(rangesCursor);
            return ParserError.Panic($"Expected to not parse {parser}", state);
        }

        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Not [ {parser} ]";
    }
}

public struct CaptureParser : IParser
{
    readonly IParser parser;

    public CaptureParser(IParser parser)
    {
        this.parser = parser;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        int startCursor = state.Cursor;
        var attempt = parser.Attempt(ref state);
        if (attempt.IsOk())
        {
            state.AppendRange(startCursor..state.Cursor);
            return Empty.Default;
        }

        return attempt;
    }

    public override string ToString()
    {
        return $"Capture [ {parser} ]";
    }
}

public struct StringParser : IParser
{
    readonly string expected;

    public StringParser(string expected)
    {
        this.expected = expected;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        if (state.Cursor + expected.Length > state.Input.Length)
        {
            return ParserError.Panic("Not enough characters in stream", state);
        }

        if (state.Input[..expected.Length].SequenceEqual(expected.AsSpan()))
        {
            state.StepBy(expected.Length);
            return Empty.Default;
        }

        return ParserError.Panic($"Expected string {expected}", state);
    }

    public override string ToString()
    {
        return $"String {{ {expected} }}";
    }
}

public struct CharParser : IParser
{
    readonly string accepted;

    public CharParser(string accepted)
    {
        this.accepted = accepted;
    }

    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        var stateCheck = state.Check();
        if (stateCheck.IsErr()) return stateCheck;

        if (!accepted.Contains(state.Input[0]))
        {
            return ParserError.Panic($"Expected any of {accepted}", state);
        }

        state.Step();
        return Empty.Default;
    }

    public override string ToString()
    {
        return $"Char {{ {accepted} }}";
    }
}

public struct EndOfLineParser : IParser
{
    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        var stateCheck = state.Check();
        if (stateCheck.IsErr()) return stateCheck;

        if (state.Input[0] == '\r')
        {
            state.Step();
        }

        if (state.Input[0] == '\n')
        {
            state.Step();
            return Empty.Default;
        }

        return ParserError.Panic("Expected newline", state);
    }

    public override string ToString()
    {
        return $"EndOfLine";
    }
}

public struct EndOfFileParser : IParser
{
    public Result<Empty, ParserError> Attempt(ref ParserState state)
    {
        if (state.ReachedEnd)
        {
            return Empty.Default;
        }

        return ParserError.Panic("Expected end of file", state);
    }

    public override string ToString()
    {
        return $"EndOfFile";
    }
}

public static class Parsers
{
    public const string ALPHA_CAPITAL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string ALPHA_LOWER = "abcdefghijklmnopqrstuvwxyz";
    public const string ALPHA = ALPHA_LOWER + ALPHA_CAPITAL;
    public const string NUM = "0123456789";
    public const string ALPHA_NUM = NUM + ALPHA;
    public const string WHITESPACE = " ";

    public static readonly IParser AlphaNum = new CharParser(ALPHA_NUM);
    public static readonly IParser Numeric = new CharParser(NUM);
    public static readonly IParser Alpha = new CharParser(ALPHA);
    public static readonly IParser AlphaLower = new CharParser(ALPHA_LOWER);
    public static readonly IParser AlphaCapital = new CharParser(ALPHA_CAPITAL);
    public static readonly IParser Whitespace = new CharParser(WHITESPACE);
    public static readonly IParser EndOfLine = new EndOfLineParser();
    public static readonly IParser EndOfFile = new EndOfFileParser();

    public static IParser Char(string accepted)
    {
        return new CharParser(accepted);
    }

    public static IParser String(string expected)
    {
        return new StringParser(expected);
    }
}

public static class Parse
{
    public static Result<Range, ParserError> Ident(ReadOnlySpan<char> input, ReadOnlySpan<char> separators)
    {
        int sepIdx = -1;
        foreach (var sep in separators)
        {
            if ((sepIdx = input.IndexOf(sep)) != -1)
            {
                break;
            }
        }

        return sepIdx switch
        {
            -1 => new ParserError(ParserError.Type.Panic, "No separator found"),
            _ => ..sepIdx,
        };
    }

    public static Result<int, ParserError> Int(ReadOnlySpan<char> input)
    {
        if (!int.TryParse(input, out var i))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse int");
        }

        return i;
    }

    public static Result<Vec2<int>, ParserError> Int2(ReadOnlySpan<char> input, char separator)
    {
        var (fst, snd) = SplitBy2(input, separator);

        if (!int.TryParse(input[fst], out int x))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse first int");
        }

        if (!int.TryParse(input[snd], out int y))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse second int");
        }

        return new Vec2<int>(x, y);
    }

    public static Result<Vec3<int>, ParserError> Int3(ReadOnlySpan<char> input, char separator)
    {
        var split = SplitBy3(input, separator);
        if (split.IsErr()) return split.ToErr();

        var (fst, snd, trd) = split.Unwrap();

        if (!int.TryParse(input[fst], out int x))
        {
            return new ParserError(ParserError.Type.Panic, $"Failed to parse first int from {input[fst]}");
        }

        if (!int.TryParse(input[snd], out int y))
        {
            return new ParserError(ParserError.Type.Panic, $"Failed to parse second int from {input[snd]}");
        }

        if (!int.TryParse(input[trd], out int z))
        {
            return new ParserError(ParserError.Type.Panic, $"Failed to parse third int from {input[trd]}");
        }

        return new Vec3<int>(x, y, z);
    }

    public static Result<float, ParserError> Float(ReadOnlySpan<char> input)
    {
        if (!float.TryParse(input, out var f))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse float");
        }

        return f;
    }

    public static Result<Vector2, ParserError> Vector2(ReadOnlySpan<char> input, char separator)
    {
        var (fst, snd) = SplitBy2(input, separator);

        if (!float.TryParse(input[fst], out float x))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse first float");
        }

        if (!float.TryParse(input[snd], out float y))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse second float");
        }

        return new Vector2(x, y);
    }

    public static Result<Vector3, ParserError> Vector3(ReadOnlySpan<char> input, char separator)
    {
        var split = SplitBy3(input, separator);
        if (split.IsErr()) return split.ToErr();

        var (fst, snd, trd) = split.Unwrap();

        if (!float.TryParse(input[fst], out float x))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse first float");
        }

        if (!float.TryParse(input[snd], out float y))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse second float");
        }

        if (!float.TryParse(input[trd], out float z))
        {
            return new ParserError(ParserError.Type.Panic, "Failed to parse third float");
        }

        return new Vector3(x, y, z);
    }

    public static (Range, Range) SplitBy2(ReadOnlySpan<char> input, char separator)
    {
        var split = input.IndexOf(separator);
        return (..split, (split + 1)..);
    }

    public static Result<(Range, Range, Range), ParserError> SplitBy3(ReadOnlySpan<char> input, char separator)
    {
        var fst = input.IndexOf(separator);
        if (fst == -1)
        {
            return new ParserError(ParserError.Type.Panic, "Failed to find first split");
        }

        var snd = input[(fst + 1)..].IndexOf(separator);
        if (snd == -1)
        {
            return new ParserError(ParserError.Type.Panic, "Failed to find second split");
        }

        snd += fst + 1;

        return (..fst, (fst + 1)..snd, (snd + 1)..);
    }

    public static SplitByEnumerator SplitByN(this ReadOnlySpan<char> input, char separator)
    {
        return new SplitByEnumerator(input, separator);
    }

    public ref struct SplitByEnumerator
    {
        readonly ReadOnlySpan<char> input;
        readonly char separator;
        int prev;

        public Range Current { get; private set; }

        public SplitByEnumerator(ReadOnlySpan<char> input, char separator)
        {
            this.input = input;
            this.separator = separator;
            this.prev = 0;
            this.Current = 0..0;
        }

        public SplitByEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (prev >= input.Length) return false;

            int next = input[prev..].IndexOf(separator) switch
            {
                -1 => input.Length,
                var n => n + prev,
            };

            Current = prev..next;
            prev = next + 1;
            return true;
        }
    }
}