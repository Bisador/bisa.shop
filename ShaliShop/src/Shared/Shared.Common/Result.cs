using System.Collections.Immutable;

namespace Shared.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ImmutableList<Error>? Errors { get; }
    protected Result(ImmutableList<Error> errors) => Errors = errors;
    protected Result() => IsSuccess = true;

    public Error? Error => Errors?.FirstOrDefault();

    public static Result Success() => new();
    public static Result<T> Success<T>(T value) => new(value);

    public static Result Failure(List<Error> error) => new(ImmutableList.Create(error.ToArray()));
    public static Result Failure(Error error) => new(ImmutableList.Create(error));
    public static Result Failure(string errorCode, string errorMessage) => Failure(new Error(errorCode, errorMessage));
    public static Result Failure(string errorMessage) => Failure(new Error(string.Empty, errorMessage));

    public static Result<T> Failure<T>(List<Error> error) => new(ImmutableList.Create(error.ToArray()));
    public static Result<T> Failure<T>(Error? error) => new(error is null ? [] : ImmutableList.Create(error));

    public static Result Failure<T>(string errorCode, string errorMessage) =>
        Failure<T>(new Error(errorCode, errorMessage));

    public static Result Failure<T>(string errorMessage) => Failure<T>(new Error(string.Empty, errorMessage));
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T? Value
    {
        get
        {
            if (!IsSuccess)
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            return _value;
        }
    }

    internal Result(ImmutableList<Error> errors) : base(errors)
    {
    }

    internal Result(T value)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}

public record Error(string Code, string Message);