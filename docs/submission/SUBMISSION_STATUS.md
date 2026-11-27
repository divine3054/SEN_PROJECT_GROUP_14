# Submission pack status - 27 November 2026

## Prepared and verified

- All three members' existing contributions are preserved on shared main. Backend reliability commit c21b8e9 and integrated verification commit 9fef1a8 are published.
- Application baseline: 34 xUnit, 20 backend reliability and 18 native UI checks pass (72 total); solution builds with zero warnings/errors.
- Group14-Project-Report.pdf: 33-page review copy, including contents, requirements, feasibility, design diagrams, implementation, executed test records, screenshots, user guide and individual contribution evidence. Every page was rendered for visual inspection.
- ../demo/Group14-Demonstration.mp4: approximately 65 seconds of the actual application using synthetic data and automated demonstration dialogs. The complete recording decodes successfully. It demonstrates the persisted Customer/Agent workflow; it is not human acceptance.
- Editable diagrams and nine PNG exports are in ../phase-2/. The demonstration helper builds with zero warnings/errors.

## Information and human actions still required

1. Supply institution, confirmed course code/title, lecturer, all three matriculation numbers, deadline and submission destination. Confirm the spelling of the three member names.
2. All members review the scope, including deferring the optional separate administrator tools and the single-process data-folder assumption.
3. Actual Customer and Support Agent reviewers complete ACCEPTANCE.md, including names, date, machine, inputs, expected/actual outcomes and decision. Complete the manual recovery/usability scenarios in the checklist. Existing automated results cannot provide these signatures.
4. Regenerate the report with the confirmed details and genuine acceptance record, then have all members review it. Submit through the confirmed course channel and retain the receipt.

The PDF is visibly labelled REVIEW COPY. No final submission or new reviewer approval has been claimed.

## Regenerating the report

Use Python 3.12+ with reportlab, Pillow, pypdf and pypdfium2 installed. From the repository root run:

```powershell
python scripts/Build-Submission.py
python scripts/Validate-Submission.py
```

The generator uses report-metadata.private.json if present, otherwise report-metadata.json. The private file is ignored by Git; use it for student identifiers if they should appear only in the submitted report. Check the generated PDF before sharing it. Validation renders every page to .tools/report-preview and records the page count and SHA-256. Regenerating requires the test TRX at TestResults/backend-tests.trx; run scripts/Test-Backend.ps1 first on a new checkout.
