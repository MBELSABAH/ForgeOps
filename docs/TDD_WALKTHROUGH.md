# Observed TDD walkthrough

The initial rules had empty validation / a false overdue result as explicit implementation placeholders. The tests were written and executed before their corresponding rules were implemented. These are the two test-first rules; other features are not claimed as TDD.

Command pattern (from repository root):

    dotnet test tests/ForgeOps.Tests/ForgeOps.Tests.csproj --filter FullyQualifiedName~TEST_NAME

## 1. Resolution requires notes
- Test: Resolve_WithoutNotes_IsRejected, with null, empty and whitespace input.
- Red observed: 3 failed assertions. Assert.Contains reported an empty error collection; ResolutionNotes was missing.
- Green fix: return a ResolutionNotes error when the target is Resolved and notes are blank.
- Green observed: 3 passed, 0 failed.

## 2. Past due and unresolved means overdue
- Test: PastDue_Unresolved_Unarchived_Order_IsOverdue.
- Red observed: 1 failed assertion, Expected True / Actual False.
- Green fix: require a date before today, an unresolved status and an unarchived order.
- Refactor: retain a pure rule with an explicit today parameter. Add boundary cases for due today, future, resolved and archived orders, plus valid resolution cases.
- Green/refactor rerun observed: 12 passed, 0 failed. After integration and additional coverage, the complete suite passed 45/45 (27 unit, 18 integration).

The first sandboxed attempt could not start MSBuild IPC. Both Red results above came from actual xUnit assertion failures after rerunning with local IPC access, not from the environment failure.
