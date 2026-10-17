# Test-driven development examples

Two business rules were implemented using a **Red → Green → Refactor** sequence with xUnit. The failing results below are actual observed assertion failures, rather than build or environment errors. No claim is made that every feature was written test-first.

## Example 1: Resolution requires notes

**Tests:** Resolve_WithoutNotes_IsRejected, using null, empty, and whitespace-only resolution notes.

- **Red:** three assertions failed because the validation errors did not contain a ResolutionNotes entry.
- **Green:** the rule was implemented to return a field-specific error when attempting to resolve without meaningful notes.
- **Verification:** the same three test cases passed.

## Example 2: Overdue calculation

**Test:** PastDue_Unresolved_Unarchived_Order_IsOverdue.

- **Red:** expected true for an overdue order, but the placeholder rule returned false.
- **Green:** the rule was implemented to require a due date before today and exclude resolved or archived orders.
- **Refactor:** the rule remained a pure function receiving the current facility date explicitly, with added boundary cases.
- **Verification:** the relevant rule tests passed, followed by the full 45-test suite (27 unit and 18 integration tests).

To reproduce the suite:

~~~sh
./scripts/test.sh
~~~

Related HTTP integration tests also verify that rejected writes do not alter persisted orders or audit history.
