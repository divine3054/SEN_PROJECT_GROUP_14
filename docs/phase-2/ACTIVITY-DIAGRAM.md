# Complaint lifecycle activity diagram

Status: **Draft based on the current implementation; team review pending.**
Prepared: 27 November 2026.

The diagram follows the implemented Customer and Support Agent flows. Administrators are routed to the support workspace, but account management and ticket summaries are not implemented. Persistence-error branches describe the required response; the current UI does not handle every storage error gracefully.

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
    Required -- Yes --> CreateComplaint[Create Pending complaint and reference]
    CreateComplaint --> SaveComplaint[Write complaints.json]
    SaveComplaint --> SubmitSaved{Save successful?}
    SubmitSaved -- No --> SubmitError[Report save failure; do not show success]
    SubmitSaved -- Yes --> ShowReference[Display reference number]
    ShowReference --> CustomerAction
    SubmitError --> CustomerEnd((End))

    CustomerAction -- Track --> EnterReference[Enter reference number]
    EnterReference --> FindComplaint[Look up reference]
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

- The diagram records the intended save-failure outcome separately from demonstrated behavior. The backend mutates cached records before attempting to save; a failed write can leave in-memory state changed.
- Blank tracking input currently follows the no-match path rather than showing a dedicated validation alert.
- A successful customer submission currently displays the reference in the form; a separate confirmation dialog is not shown.
- Administrator-specific account management and summary actions remain outside the implemented flows pending a team scope decision.
