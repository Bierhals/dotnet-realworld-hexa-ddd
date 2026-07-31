using System;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests;

public class UserFollowIdTests
{
    [Fact]
    public void A_new_user_follow_id_is_not_empty()
    {
        var id = UserFollowId.New();

        id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Two_new_user_follow_ids_are_different()
    {
        var first = UserFollowId.New();
        var second = UserFollowId.New();

        first.ShouldNotBe(second);
    }

    [Fact]
    public void Rehydrating_a_user_follow_id_restores_the_original_value()
    {
        var original = UserFollowId.New();

        var rehydrated = UserFollowId.Rehydrate(original.Value.ToString());

        rehydrated.ShouldBe(original);
    }
}
