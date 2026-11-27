# Complaint lifecycle activity diagram

Status: **Draft based on the current implementation; team review pending.**
Prepared: 27 November 2026.

The diagram follows the integrated Customer and Support Agent flows. Administrators use the support workspace; account management and ticket summaries remain a scope decision. The editable activity page is included in [design.drawio](design.drawio), with a [PNG export](exports/09-activity.png). This technical refresh does not imply additional team approval.

```mermaid
flowchart TD
    Start((Start)) --> Login[Enter username and password]
    Login --> Auth{Credentials valid?}
    Auth -- No --> LoginError[Show invalid-login message]
    LoginError --> Login
    Auth -- Yes --> Role{Role}

    Role -- Customer --> CustomerAction{Choose action}
    CustomerAction -- Submit --> EnterComplaint[Enter title, category and description]
    EnterComplaint --> Required{Title and description nonblank?}
    Required -- No --> ValidationError[Show validation error]
    ValidationError --> EnterComplaint
    Required -- Yes --> CreateComplaint[Reserve persistent reference and stage Pending complaint]
    CreateComplaint --> SaveComplaint[Atomically replace complaints.json and retain backup]
    SaveComplaint --> SubmitSaved{Save successful?}
    SubmitSaved -- No --> SubmitError[Report save failure; do not show success]
    SubmitSaved -- Yes --> ShowReference[Publish cache and display reference popup and label]
    ShowReference --> CustomerAction
    SubmitError --> CustomerEnd((End))

    CustomerAction -- Track --> EnterReference[Enter reference number]
    EnterReference --> ReferenceRequired{Reference nonblank?}
    ReferenceRequired -- No --> BlankReference[Clear old details and show validation alert]
    BlankReference --> EnterReference
    ReferenceRequired -- Yes --> FindComplaint[Clear old details and look up reference]
    FindComplaint --> Found{Complaint found?}
    Found -- Yes --> ShowDetails[Display category, status, update time and notes]
    Found -- No --> NoMatch[Show no-match message]
    ShowDetails --> CustomerEnd
    NoMatch --> CustomerEnd

    Role -- Support Agent or Admin --> Review[View complaint list]
    Review --> Filter[Apply or reset category/status filters]
    Filter --> SelectComplaint[Select a complaint]
    SelectComplaint --> EditComplaint[Choose category/status and enter notes]
    EditComplaint --> SaveUpdate[Persist complaint update]
    SaveUpdate --> UpdateSaved{Save successful?}
    UpdateSaved -- Yes --> Refresh[Refresh complaint list]
    UpdateSaved -- No --> UpdateError[Show a useful save error]
    Refresh --> AgentEnd((End))
    UpdateError --> AgentEnd
```

## Review notes

- Cached changes now publish only after successful persistence. Failure-injection checks confirm that failed writes preserve the prior cache and primary file.
- Blank tracking input has its own alert. Rejected submissions clear any previous success reference.
- Successful submissions display both the returned reference label and a confirmation message. The automated UI suite records the message call; a human must still confirm actual dialog usability.
- Administrator-specific account management and summary actions remain outside the implemented flows pending a team scope decision.
