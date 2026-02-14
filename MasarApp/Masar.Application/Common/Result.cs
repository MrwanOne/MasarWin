using System;
using System.Collections.Generic;

namespace Masar.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string message) => new(false, message);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, string message, T? value) : base(isSuccess, message)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, string.Empty, value);
    public static new Result<T> Failure(string message) => new(false, message, default);
}
