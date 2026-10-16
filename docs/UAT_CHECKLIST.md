# Human UAT — maintenance coordinator

Persona is fictional. **Human acceptance: pending / not approved.** Agent-controlled browser checks and automated tests are recorded separately in VERIFICATION.md.

Preconditions for all scenarios: start the local app, use fictional data, and note the dashboard counts. Use one new order throughout.

| Scenario | Steps | Expected result | Human actual result | Human pass/fail |
|---|---|---|---|---|
| Create high-priority work | New work order → fill required information → High → Create | Reference generated; details persisted; Open count increases | Not run by user | Pending |
| Assign technician | Open created order → Edit → select technician → Save | Assignment appears in details and history | Not run by user | Pending |
| Start work | Select In progress → Save status | Badge/history update without page reload; dashboard reflects change | Not run by user | Pending |
| Reject invalid resolution | Select Resolved; leave notes blank → Save status | Clear resolution-note error; stored status/history unchanged | Not run by user | Pending |
| Complete resolution | Enter repair/verification notes → Save status → Overview | Resolved badge and audit entry; resolved count increases; overdue excludes order | Not run by user | Pending |

Record your actual observations and acceptance decision after personally performing these steps. No external stakeholder acceptance has been claimed.
