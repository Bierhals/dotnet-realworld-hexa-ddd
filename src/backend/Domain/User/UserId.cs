using System;
using System.Collections.Generic;
using Conduit.Domain.Common;

namespace Conduit.Domain.User;

public class UserId : ValueObject
{
        public string Email
        {
            get;
        }

        public UserId(string email)
        {
            Email = email;
        }

    protected override IEnumerable<IComparable?> GetAtomicValues()
    {
        yield return Email;
    }
}
