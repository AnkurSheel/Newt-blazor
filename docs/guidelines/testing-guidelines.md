# NetWorthTracker - Testing Guidelines for AI Agents

This document defines testing standards, patterns, and guidelines for AI agents and developers working on the **NetWorthTracker** repository. These guidelines are adapted from Ankur Sheel's testing principles (*Defining Good: Why Your Engineering Team Needs Testing Guidelines*) and tailored specifically for automated agents building C# 13, .NET 9, xUnit, bUnit, and EF Core applications.

---

## 1. Core Testing Philosophies

### 1.1 Optimize for Readability over DRY
- A test suite should read like a **Lego instruction manual**: sequential, self-contained, and completely obvious.
- Treat tests as **executable documentation**. When someone (human or AI) inspects a test to understand how a component or service behaves, its intent must be immediately clear.
- A little duplication across test methods is completely acceptable if it keeps individual tests explicit and easy to comprehend in isolation.
- Avoid excessive inheritance or overly complex base test classes that obscure the test flow.

### 1.2 Test Names Must Convey Behavior & Outcome
- Test names must clearly describe the **condition/state** being tested and the **expected outcome**.
- **Prohibited:** Vague names such as `GetUser_HappyPath`, `Test1`, `Calculate_Success`, or `AccountListTest`.
- **Recommended Patterns:**
  - `CanRetrieveAccountById`
  - `WhenAccountNotFound_ReturnsNotFoundError`
  - `CalculateNetWorth_WhenLiabilitiesExceedAssets_ReturnsNegativeValue`
  - `AddMonthlyBalance_WhenDuplicateMonthExists_ThrowsValidationException`

### 1.3 Strict Arrange → Act → Assert (AAA) Structure
- Every test must follow a single, unbroken **Arrange → Act → Assert** sequence:
  1. **Arrange:** Set up test inputs, dependencies, and state.
  2. **Act:** Execute the single action under test.
  3. **Assert:** Verify the expected outcome.
- **Rule:** A single test must not contain repeated Act/Assert chains (e.g., Arrange → Act → Assert → Act → Assert). If a scenario requires multiple actions and intermediate checks, split it into separate, distinct test cases.

```csharp
[Fact]
public void CalculateNetWorth_WithValidAssetsAndLiabilities_ReturnsCorrectDifference()
{
    // Arrange
    var assets = 150_000m;
    var liabilities = 50_000m;
    var calculator = new NetWorthCalculator();

    // Act
    var result = calculator.Calculate(assets, liabilities);

    // Assert
    Assert.Equal(100_000m, result);
}
```

### 1.4 Test Both Sides of Flags and Conditional Logic
- When feature flags, configuration toggles, or conditional branches exist, always write separate, explicit tests for both the **enabled/on** path and the **disabled/off** path.
- If testing the disabled state is difficult, document the rationale in comments and verify manually.
- **Keep flag logic out of parameterized tests (`[Theory]` / `[InlineData]`):** Avoid passing boolean flag parameters into test cases to toggle branches. Instead, write dedicated test methods (e.g., `CanExportMonthlyBalances_FlagOn` and `CanExportMonthlyBalances_FlagOff`).

### 1.5 No Branching Logic in Tests
- Do not use `if`, `switch`, `while`, or `for` loops inside test methods.
- Branching in tests introduces testing complexity, obscures failure causes, and can lead to silent false passes where assertions are skipped.
- If multiple conditions must be checked, extract shared initialization into helper methods and create distinct tests for each branch or use `[Theory]` for parameterized inputs.

### 1.6 Test Observable Behavior, Not Implementation Details
- **Test the public behavior and contract of classes, services, and components, not their internal mechanics.**
- Verify *what* the system does (inputs, outputs, state changes, and observable contract side-effects), not *how* it achieves it internally.
- Tests should be **resilient to refactoring**: extracting a private helper method, changing internal data structures, or rewriting an internal algorithm without altering observable behavior should never break existing tests.
- **Do not test private or internal methods directly.** Their behavior and edge cases should be exercised naturally through public APIs.
  - *Exception:* Complex standalone internal algorithms (e.g., specialized math or chunking algorithms) where isolating the test directly provides high diagnostic value.
- **Do not assert internal call order or private state:** Avoid inspecting private fields via reflection or writing tests that dictate the exact sequence of internal sub-routine invocations.

### 1.7 Avoid Spies and Over-Specified Mock Interactions
- Avoid test spies that merely record every internal call on a real object. Over-reliance on spies couples tests to internal implementation details and makes refactoring brittle.
- Prefer explicit interface extraction and dependency injection using mocks (e.g., `Mock<T>` / NSubstitute) or in-memory test doubles.
- Only verify mock calls that represent critical external side-effects (e.g., `_accountRepository.Verify(r => r.SaveAsync(...), Times.Once)` or publishing a domain event).
- **Do not verify intermediate query/getter calls** (e.g., verifying that `_repo.GetByIdAsync` was called exactly once when the test already asserts the final computed output).

### 1.8 Use Test Doubles (Fakes/Stubs) for Integration Tests
- In integration and component tests, prefer **fakes** or in-memory implementations (e.g., EF Core SQLite in-memory database, in-memory repository) over rigid scripted mocks.
- Scripted mocks in integration scenarios often mimic assumptions rather than real behavior, leading to tests that pass while production code fails.

### 1.9 Do Not Test Trivial Stuff (Avoid Low-Value & Brittle Tests)
- **Do not write tests for trivial, boilerplate, or pass-through code.** Writing tests for trivial code wastes development time, bloats test suites, adds zero safety, and creates high maintenance friction when modifying models.
- **What NOT to Test (Trivial & Low-Value):**
  - **Auto-properties and simple getters/setters:** Do not write tests that merely assign a property on a DTO, model, or entity and assert the value back (`dto.Name = "Test"; Assert.Equal("Test", dto.Name);`).
  - **Trivial constructors:** Do not test default parameterless constructors or constructors that simply copy arguments directly to auto-properties without validation logic.
  - **Simple 1-line pass-through / delegating methods:** Wrapper or facade methods that solely forward an argument to an underlying dependency without branching, validation, or data transformation do not need dedicated unit tests.
  - **Framework, library, and compiler features:** Do not test that .NET collections, LINQ operators, language syntax, or EF Core query engines work as documented by Microsoft.
  - **Static constants, enums, and static UI labels:** Do not write tests verifying that constant strings or enum names match their declared values, or that a component renders a fixed static label.
  - **Pure DTOs and POCOs:** Classes with no behavior or invariants do not require unit tests.
- **What to Test Instead (High-Value):**
  - Core domain logic, financial calculations, and business rules.
  - Input validation, boundary/edge conditions, and custom error/exception handling.
  - Stateful workflows, custom data transformations/mappings, and conditional branching.
  - Component user interactions, event handling, and conditional UI state rendering.

---

## 2. Test Setup & State Management

### 2.1 Keep Setup Relevant to the Assertion
- Only include setup code inside the test method body that directly influences the assertion being made.
- Move repetitive, boilerplate setup into parameterized helper methods or object builders (e.g., `CreateValidAccountDto(string id = "acc-1")`).
- Keep the unique parameter values that define the test scenario visible in the test body:

```csharp
// Good: Immediately clear what is unique about this test
[Fact]
public async Task CreateAccount_WhenNameIsDuplicate_ThrowsConflictException()
{
    // Arrange
    var existingAccount = CreateValidAccount(name: "Savings");
    await _repository.AddAsync(existingAccount);

    var newAccountDto = CreateAccountDto(name: "Savings");

    // Act & Assert
    await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAccountAsync(newAccountDto));
}
```

### 2.2 Minimal State for Database & Stateful Tests
- For tests requiring database state (e.g., SQLite / EF Core), start from the absolute minimum database state required for the system to execute.
- Construct needed state explicitly within the test arrange step rather than relying on shared pre-populated database fixtures that can introduce inter-test contamination.

---

## 3. Assertion Standards

### 3.1 Assert One Concept per Test
- Focus each test method on verifying a single logical concept or behavior.
- Asserting one concept can involve multiple `Assert` statements (e.g., inspecting several fields of a returned DTO), as long as they all validate the same conceptual outcome.
- Do not mix validation of returned data with unrelated side-effect checks (e.g., logging or analytics) within the same test unless that interaction is the core behavior being tested.

### 3.2 Assert on Properties, Not Object Equality
- Avoid asserting entire object equality (`Assert.Equal(expectedObj, actualObj)`) unless testing equality operators specifically.
- Object equality assertions rely on default or overridden `Equals` implementations that may fail to compare newly added fields or may produce unhelpful failure messages.
- Assert directly on individual relevant properties to make test intent explicit and failures easy to diagnose:

```csharp
// Avoid
Assert.Equal(expectedAccount, result);

// Preferred
Assert.Equal("Checking", result.Name);
Assert.Equal(AccountType.Asset, result.Type);
Assert.Equal(1250.00m, result.Balance);
```

- *Exception:* When explicitly validating a mapping contract (e.g., AutoMapper or manual DTO mapping), create a dedicated test named `AllFieldsAreMappedCorrectly` and intentionally assert complete structural equivalence.

### 3.3 Use `Assert.Multiple` for Multiple Assertions
- When asserting multiple properties within a single concept, wrap them in `Assert.Multiple` so that all assertion failures are reported simultaneously rather than failing on the first mismatch.

```csharp
[Fact]
public void MapToResponseDto_MapsAllCoreProperties()
{
    // Arrange
    var account = new AccountEntity
    {
        Id = Guid.NewGuid(),
        Name = "Main Checking",
        Type = AccountType.Asset,
        IsActive = true
    };

    // Act
    var dto = account.ToResponseDto();

    // Assert
    Assert.Multiple(
        () => Assert.Equal(account.Id, dto.Id),
        () => Assert.Equal("Main Checking", dto.Name),
        () => Assert.Equal(AccountType.Asset, dto.Type),
        () => Assert.True(dto.IsActive)
    );
}
```

---

## 4. Specific Guidelines for AI Agents

When AI agents generate or modify tests in this codebase, use the dedicated [Write Tests Skill](skills/write-tests.md) (`write-tests`) and ensure the following behavioral constraints are strictly observed:

1. **Never Weaken or Skip Failing Tests:**
   - Never suppress failing tests by removing assertions, reducing assertion strictness, adding `[Fact(Skip = "...")]`, or stubbing out logic with fake mocks to bypass errors.
   - Fix the underlying implementation or correctly align test expectations with requirements.

2. **Deterministic & Time-Independent Tests:**
   - Never rely on `DateTime.Now` or `DateTime.UtcNow` directly in test assertions. Use fixed dates (e.g., `new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)`) or injectable time providers (`TimeProvider`).
   - Do not make external network or file system calls in unit tests.

3. **Blazor UI Component Tests (bUnit):**
   - Use `bUnit` for Blazor component testing in `tests/NetWorthTracker.UI.Tests`.
   - Test user interactions via bUnit semantic triggers (e.g., `button.Click()`, `input.Change(...)`).
   - Assert rendered markup using semantic elements and CSS classes defined in the [UI Style Guide](ui-style-guide.md).

4. **Test Depth Based on Task Type:**
   - **Bug Fixes:** Write a reproduction test verifying the failure before applying the fix, then verify that the test passes.
   - **New Features:** Cover the core happy path, validation boundaries, and error/exception paths with clean, dedicated test cases.
   - **Refactoring:** Rely on existing unit/component test suites; add tests if edge case coverage was missing.

5. **Avoid Over-Testing Trivialities & Implementation Details:**
   - Do not generate tests for trivial auto-properties, simple DTO fields, default constructors, or 1-to-1 delegation methods.
   - Do not assert against internal implementation details, private member states, or arbitrary intermediate mock calls.
   - Reserve tests for logic that has real potential to fail: business calculations, validation, state mutations, boundary conditions, and error handling.

---

## 5. Summary Checklist for AI Agents

Before submitting code with tests, verify:
- [ ] Test names describe state and expected outcome (`CanDoX` / `WhenX_ShouldY`).
- [ ] Follows strict Arrange → Act → Assert with no multi-action chaining.
- [ ] No `if`/`switch`/loop branching inside test methods.
- [ ] No brittle tests for trivial logic (e.g., auto-properties, parameterless constructors, 1-line delegates, framework features).
- [ ] Tests verify observable public behavior and contracts, not internal implementation mechanics or private methods.
- [ ] Setup is minimal and readable; helper builders are parameterized.
- [ ] Assertions validate individual properties using `Assert.Multiple` where applicable.
- [ ] No over-specified mocks or spies; test doubles/fakes used where appropriate for integration tests.
- [ ] All tests compile with zero warnings and pass cleanly (`dotnet test`).
