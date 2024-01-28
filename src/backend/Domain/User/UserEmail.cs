using System;
using System.Collections.Generic;
using Conduit.Domain.Common;
using Conduit.Domain.User.Rules;
using CSharpFunctionalExtensions;

namespace Conduit.Domain.User;

public class UserEmail : ValueObject
{
    public string Value { get; }

    protected UserEmail(string email)
    {
        Value = email;
    }

    public static Result<UserEmail, Error> Create(string email)
    {
        string emailLowerCase = email.ToLower();
        UnitResult<Error> checkResult = UserRules.EmailIsValidRule(emailLowerCase);

        if (checkResult.IsFailure)
        {
            return Result.Failure<UserEmail, Error>(checkResult.Error);
        }

        return new UserEmail(emailLowerCase);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
