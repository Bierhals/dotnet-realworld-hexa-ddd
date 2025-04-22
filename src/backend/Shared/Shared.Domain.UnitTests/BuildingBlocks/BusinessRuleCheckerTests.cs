using System.Linq;
using Conduit.Shared.Domain.BuildingBlocks;
using ErrorOr;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests.BuildingBlocks;

public class BusinessRuleCheckerTests
{
    [Fact]
    public void Check_WithAllValidRules_ReturnsSuccess()
    {
        var rules = new IBusinessRule[]
        {
            new AlwaysValidRule(),
            new AlwaysValidRule()
        };

        var result = BusinessRuleChecker.Check(rules);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeOfType<Success>();
    }

    [Fact]
    public void Check_WithOneInvalidRule_ReturnsError()
    {
        var rules = new IBusinessRule[]
        {
            new AlwaysValidRule(),
            new AlwaysInvalidRule("Rule1", "First rule failed"),
            new AlwaysValidRule()
        };

        var result = BusinessRuleChecker.Check(rules);

        result.IsError.ShouldBeTrue();
        var errors = result.Errors.ToList();
        errors.Count.ShouldBe(1);
        errors[0].Code.ShouldBe("Rule1");
        errors[0].Description.ShouldBe("First rule failed");
    }

    [Fact]
    public void Check_WithMultipleInvalidRules_ReturnsAllErrors()
    {
        var rules = new IBusinessRule[]
        {
            new AlwaysInvalidRule("Rule1", "First rule failed"),
            new AlwaysInvalidRule("Rule2", "Second rule failed")
        };

        var result = BusinessRuleChecker.Check(rules);

        result.IsError.ShouldBeTrue();
        var errors = result.Errors.ToList();
        errors.Count.ShouldBe(2);
        errors.ShouldContain(e => e.Code == "Rule1" && e.Description == "First rule failed");
        errors.ShouldContain(e => e.Code == "Rule2" && e.Description == "Second rule failed");
    }

    private class AlwaysValidRule : IBusinessRule
    {
        public ErrorOr<Success> Check() => Result.Success;
    }

    private class AlwaysInvalidRule : IBusinessRule
    {
        private readonly Error _error;

        public AlwaysInvalidRule(string code = "RuleViolated", string description = "Rule violated")
        {
            _error = Error.Validation(code, description);
        }

        public ErrorOr<Success> Check() => _error;
    }
}
