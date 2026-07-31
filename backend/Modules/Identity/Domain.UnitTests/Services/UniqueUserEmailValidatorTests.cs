using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.UnitTests.TestDoubles;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests.Services;

public class UniqueUserEmailValidatorTests
{
    [Fact]
    public async Task An_email_used_by_no_one_is_unique()
    {
        var validator = new UniqueUserEmailValidator(new FakeUsersRepository());

        var result = await validator.IsUniqueAsync(UserEmail.Create("new@example.com").Value, ct: CancellationToken.None);

        result.IsError.ShouldBeFalse();
    }

    [Fact]
    public async Task An_email_already_used_by_another_user_is_not_unique()
    {
        var repository = new FakeUsersRepository();
        repository.Add(User.RegisterNewUser(UserEmail.Create("taken@example.com").Value, Username.Create("taken").Value, "hash"));
        var validator = new UniqueUserEmailValidator(repository);

        var result = await validator.IsUniqueAsync(UserEmail.Create("taken@example.com").Value, ct: CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueEmail");
    }

    [Fact]
    public async Task A_user_keeping_their_own_email_is_not_considered_a_duplicate()
    {
        var repository = new FakeUsersRepository();
        var existingUser = User.RegisterNewUser(UserEmail.Create("me@example.com").Value, Username.Create("me").Value, "hash");
        repository.Add(existingUser);
        var validator = new UniqueUserEmailValidator(repository);

        var result = await validator.IsUniqueAsync(UserEmail.Create("me@example.com").Value, existingUser.Id, ct: CancellationToken.None);

        result.IsError.ShouldBeFalse();
    }

    [Fact]
    public async Task An_email_used_by_a_different_user_is_still_a_duplicate_when_excluding_someone_else()
    {
        var repository = new FakeUsersRepository();
        var otherUser = User.RegisterNewUser(UserEmail.Create("taken@example.com").Value, Username.Create("taken").Value, "hash");
        repository.Add(otherUser);
        var validator = new UniqueUserEmailValidator(repository);

        var result = await validator.IsUniqueAsync(UserEmail.Create("taken@example.com").Value, UserId.New(), ct: CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueEmail");
    }
}
