using LyricsScraperApi.Models.Errors;

namespace LyricsScraperApi.ResultPattern;

/// <summary>
/// A value or an error.
/// </summary>
public sealed class Result<TValue>
{
    private readonly TValue? _value;

    private Result(TValue? value, BaseError? error)
    {
        _value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;
    public BaseError? Error { get; }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value when Result is a failure");

    public static Result<TValue> Success(TValue value) => new(value, null);

    public static Result<TValue> Failure(BaseError error) => new(default, error);
}
