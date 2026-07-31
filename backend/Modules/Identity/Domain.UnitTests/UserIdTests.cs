using System;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests;

public class UserIdTests
{
    [Fact]
    public void A_new_user_id_is_not_empty()
    {
        var id = UserId.New();

        id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Two_new_user_ids_are_different()
    {
        var first = UserId.New();
        var second = UserId.New();

        first.ShouldNotBe(second);
    }

    [Fact]
    public void Rehydrating_a_user_id_restores_the_original_value()
    {
        var original = UserId.New();

        var rehydrated = UserId.Rehydrate(original.Value.ToString());

        rehydrated.ShouldBe(original);
    }
}
