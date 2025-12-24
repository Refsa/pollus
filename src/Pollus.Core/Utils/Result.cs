namespace Pollus.Utils;

public interface IError
{
    static string Name { get; } = "Error";
    string Inner { get; }

    string? ToString()
    {
        return $"{Name} {{ {Inner} }}";
    }
}

public record struct Result<V, E>
    where E : IError
{
    V ok;
    E err;

    bool isOk;

    public static Result<V, E> Ok(V value)
    {
        return new Result<V, E>
        {
            ok = value,
            isOk = true,
        };
    }

    public static Result<V, E> Err(E err)
    {
        return new Result<V, E>
        {
            err = err,
            isOk = false,
        };
    }

    public static implicit operator Result<V, E>(V value)
    {
        return Result<V, E>.Ok(value);
    }

    public static implicit operator Result<V, E>(E error)
    {
        return Result<V, E>.Err(error);
    }

    public bool IsOk()
    {
        return isOk;
    }

    public bool IsErr()
    {
        return !IsOk();
    }

    public bool TryErr(out E error)
    {
        error = this.err;
        return IsErr();
    }

    public Result<U, E> ChangeOk<U>()
    {
        if (IsErr())
        {
            return Result<U, E>.Err(err);
        }

        throw new NullReferenceException($"Result<{typeof(V)}, {typeof(E)}> contains unset error");
    }

    public Result<V, EE> MapErr<EE>(Func<E, EE> predicate) where EE : IError
    {
        if (IsErr())
        {
            return Result<V, EE>.Err(predicate.Invoke(err));
        }
        else
        {
            return Result<V, EE>.Ok(ok);
        }
    }

    public Result<VV, E> MapOk<VV>(Func<V, VV> predicate)
    {
        if (IsOk())
        {
            return Result<VV, E>.Ok(predicate.Invoke(ok));
        }
        else
        {
            return Result<VV, E>.Err(err);
        }
    }

    public E ToErr()
    {
        if (IsErr())
        {
            return err;
        }

        throw new NullReferenceException($"Result<{typeof(V)}, {typeof(E)}> contains unset error");
    }

    public V Unwrap()
    {
        if (IsOk())
        {
            return ok;
        }
        else if (IsErr())
        {
            throw new NullReferenceException($"Result<{typeof(V)}, {typeof(E)}> contains error: {err}");
        }

        throw new NullReferenceException($"Result<{typeof(V)}, {typeof(E)}> contains unset error");
    }

    public override string ToString()
    {
        if (isOk)
        {
            return $"Ok {{ {ok} }}";
        }
        else
        {
            return $"Error {{ {err} }}";
        }
    }
}

public record struct ExceptionError : IError
{
    public static string Name => "ExceptionError";
    public string Inner => $"{exception}";
    readonly Exception exception;

    public ExceptionError(Exception exception)
    {
        this.exception = exception;
    }

    public static implicit operator ExceptionError(Exception exception)
    {
        return new ExceptionError(exception);
    }

    public static implicit operator Exception(ExceptionError error)
    {
        return error.exception;
    }
}

public record struct Empty
{
    public static readonly Empty Default = new();

    public override string ToString()
    {
        return "Empty";
    }

    public override int GetHashCode()
    {
        return 0;
    }
}