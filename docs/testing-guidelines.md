# Testing Guidelines: Unit and Integration Tests (Classic / Chicago Style)

This guideline describes how we write and organize Unit and Integration tests in this repository. It follows the Classic (Chicago) school of testing and is heavily inspired by Vladimir Khorikov's *Unit Testing: Principles, Practices, and Patterns*.

---

## 1. Classic (Chicago) vs. London (Mockist) Style

There are two main schools of unit testing:

| | Classic / Chicago | London / Mockist |
|---|---|---|
| Isolation refers to | Test cases (tests run independently) | Units under test (each class in isolation) |
| What is mocked? | Only unmanaged dependencies (external systems) | Most collaborators |
| Tests | Behavior spanning multiple classes (in-memory) | A single class in strict isolation |
| Coupling to implementation | Low | High |

We follow the Classic approach: a "unit" is a unit of behavior, not necessarily a single class. Multiple collaborating classes may be tested together as long as everything stays in-memory and deterministic.

## 2. The Four Pillars of a Good Test (Khorikov)

Every test should be evaluated along four criteria:

1. Protection against regressions — does the test catch real bugs?
2. Resistance to refactoring — does the test remain green when implementation changes but behavior does not?
3. Fast feedback — does the test run fast enough to be run frequently?
4. Maintainability — is the test easy to read and maintain?

No single test will be perfect on all four dimensions; trade-offs are necessary. Prioritise resistance to refactoring to avoid brittle tests (a common pitfall from over-mocking).

## 3. What to mock — and what not to mock

Distinguish between managed vs. unmanaged dependencies; this drives how dependencies should be replaced in tests. Important: persistence / database access does not belong in a Unit Test — that is the purpose of an Integration Test.

### In Unit Tests

- Managed dependencies (our own DB, file system, etc.): Do not call the real implementation. Replace repository interfaces (e.g. `IOrderRepository`) with a simple in-memory fake implementation. Do not use `Mock<T>.Verify(...)` to assert internal implementation details.
- Unmanaged dependencies (external APIs, payment providers, email services): mock or stub these to avoid side effects and flakiness.
- Domain objects and value objects: never replace them — use real objects in-memory, deterministic and fast.
- Mock only across system boundaries, not between collaborating classes inside the same domain.

### In Integration Tests

- Managed dependencies (our DB): test against a real instance (for example with Testcontainers). Integration tests verify that mappings, constraints and queries actually work.
- Unmanaged dependencies: still stub/mock them (e.g., a local WireMock) rather than calling the external real service.

Rule of thumb: if you need a mock to verify internal implementation details ("was method X called?"), the test is probably overly coupled to implementation and brittle.

### Test Doubles — terminology

| Term | Meaning |
|---|---|
| Dummy | Passed in but never used |
| Stub | Returns predefined responses (input for SUT) |
| Spy | Like a stub but records calls |
| Mock | Verifies interactions (asserts that SUT called collaborator) |
| Fake | A working, simplified implementation (e.g. in-memory repository) |

Mocks and Spies belong only at the system boundary (unmanaged dependencies). Fakes replace managed dependencies in Unit Tests. Avoid verifying internal behavior with mocks.

## 4. Structure of a Test: AAA (Arrange–Act–Assert)

Every test should follow Arrange, Act, Assert:

```csharp
[Fact]
public void Sum_of_two_numbers()
{
    // Arrange
    var calculator = new Calculator();

    // Act
    var result = calculator.Sum(2, 3);

    // Assert
    Assert.Equal(5, result);
}
```

Rules:
- One Act block per test.
- No conditionals or loops inside tests — keep them linear and easy to read.
- For complex setup, use factories, Object Mother, or Test Data Builders.
- Group related assertions rather than many unrelated asserts.

## 5. What makes a good test name?

Avoid `MethodName_Scenario_ExpectedResult`. Prefer behaviour-descriptive names in plain language that a domain expert can understand. The test name should describe the scenario and the expected outcome, not implementation details.

Bad:

- `ApplyDiscount_PremiumCustomer_Returns10Percent()`
- `CalculateOrderTotal_WhenCalled_ReturnsCorrectSum()`

Good:

- `Premium_customers_get_a_10_percent_discount_on_orders_above_100_euros`
- `An_order_cannot_be_shipped_if_it_contains_no_items`
- `Cancelling_an_already_shipped_order_throws_a_domain_exception`

Characteristics of good names:
- Do not mention class or method names from the implementation — those break on renames.
- Describe a business rule or scenario + expected result.
- Use underscores for long, sentence-like names.

## 6. Four kinds of code — not everything needs a Unit Test

| Type | Business logic? | Many collaborators? | Recommended testing |
|---|---:|---:|---|
| Domain model / algorithms | Yes | Few | Unit tests — high ROI |
| Trivial code (getters/setters) | No | Few | Do not test |
| Controllers / coordinators | No | Many | Integration tests |
| Overly complex code (anti-pattern) | Yes | Many | Refactor to isolate business logic |

Goal: separate business logic from infrastructure (Humble Object Pattern) so most logic is testable with fast Unit Tests.

## 7. Unit Test vs. Integration Test — when to use which

Typical layering:

```
Api → Application → Domain → Infrastructure
```

Guidelines by layer:

- Domain — Unit Tests only: pure business logic without external dependencies.
- Application — mainly Unit Tests with fakes/stubs for managed dependencies; Integration Tests when verifying end-to-end persistence or interaction with real infrastructure.
- Infrastructure — Integration Tests: test adapters against real technologies (SQL mapping, serialization). Use Testcontainers for databases when possible.
- Api — Integration Tests: exercise routing, model binding, auth and serialization by running requests through the full stack (e.g., WebApplicationFactory / TestServer). Unit tests on controllers are rarely necessary and often indicate misplaced logic.

Short overview:

| Layer | Test type | Replace/mock dependencies? |
|---|---|---|
| Domain | Unit Test | No |
| Application | Unit Test (+ occasional Integration) | Replace unmanaged ports with stubs; repositories can be faked in Unit Tests |
| Infrastructure | Integration Test | No |
| Api | Integration Test | Usually no |

## 8. When to mock, when not — concrete examples

Decision depends on whether the dependency crosses a system boundary (unmanaged) or is managed by us.

Do not mock; use fakes or real objects in tests when:

- `IOrderRepository` (our DB): in Application Unit Tests use a simple in-memory fake; in Integration Tests use a real DB via Testcontainers. Avoid `Verify()` on repository calls.
- Domain objects interacting with each other: never mock — test the actual objects in-memory.

Mock or stub when:

- `IPaymentGateway`, `IEmailSender`, `ISmsProvider` (external services): stub/mock to create deterministic scenarios and avoid side effects. Example: stub `IPaymentGateway` to return failure and assert no email was sent.
- `IClock`/time source: stub to control "now" for deterministic tests.
- External HTTP APIs in infrastructure tests: prefer a local stub server (WireMock) if calling the real service is undesirable.

Faustregel decision tree:

1. Does the call cross a system boundary to an unmanaged system? → Yes: stub/mock.
2. Is it our own infrastructure (DB, file system)? → Fake in Unit Tests, real instance in Integration Tests.
3. Is it pure domain or application orchestration without side effects? → Use real objects in Unit Tests.

## 9. Integration Tests

- Integration tests verify code against real managed dependencies (databases, file systems, message brokers). Use Testcontainers or dedicated test instances.
- Unmanaged dependencies should still be stubbed or mocked (e.g., a local HTTP stub) rather than calling a real third-party service.
- Focus integration tests on the happy path and the most important edge cases — they are slower and more expensive than unit tests.
- Follow the test pyramid: many fast unit tests (domain), fewer integration tests (application/infrastructure), and very few end-to-end tests.

## 10. Avoid these anti-patterns

- Overspecification / Overmocking: too many mocks that verify implementation details.
- Testing private methods: only test public behaviour.
- Multiple Acts per test: one Act per test.
- Leaking domain knowledge to tests (re-implementing logic in tests).
- Fragile tests that fail on pure refactoring.
- Mystery Guest: hidden test data outside the test itself.

## 11. Pre-merge checklist

- Test name describes behaviour in plain language
- AAA structure followed; only one Act block
- Only unmanaged dependencies are mocked
- No verification of internal implementation details
- Tests remain green under refactoring (no brittle asserts)
- Business logic separated from infrastructure where possible
- Integration tests cover the happy path and relevant edge cases

---

## Further reading

- Vladimir Khorikov — *Unit Testing: Principles, Practices, and Patterns* (Manning, 2020)
- Martin Fowler — "Mocks Aren't Stubs" (https://martinfowler.com/articles/mocksArentStubs.html)
- Martin Fowler — "Test Pyramid" (https://martinfowler.com/bliki/TestPyramid.html)
