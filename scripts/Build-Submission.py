"""Build the single-PDF project report from repository evidence and team metadata.

Dependencies: reportlab, Pillow, pypdf, pypdfium2. Run from any directory.
Unknown administrative details and human acceptance stay visibly pending.
"""
from __future__ import annotations

import html
import json
import re
import subprocess
from pathlib import Path
from datetime import datetime, timezone
import xml.etree.ElementTree as ET

from PIL import Image as PILImage
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate, PageTemplate, Frame, Paragraph, Spacer, Table, TableStyle,
    PageBreak, NextPageTemplate, Image, KeepTogether,
)
from reportlab.platypus.tableofcontents import TableOfContents

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / "docs" / "submission"
DEST.mkdir(parents=True, exist_ok=True)
metadata_path = DEST / "report-metadata.private.json"
if not metadata_path.exists():
    metadata_path = DEST / "report-metadata.json"
META = json.loads(metadata_path.read_text(encoding="utf-8"))
PDF_PATH = DEST / "Group14-Project-Report.pdf"

font_dir = Path(r"C:\Windows\Fonts")
font_candidates = [("calibri.ttf", "calibrib.ttf"), ("arial.ttf", "arialbd.ttf"), ("segoeui.ttf", "segoeuib.ttf")]
FONT, BOLD = "Helvetica", "Helvetica-Bold"
for regular, bold in font_candidates:
    if (font_dir / regular).exists() and (font_dir / bold).exists():
        pdfmetrics.registerFont(TTFont("ReportBody", str(font_dir / regular)))
        pdfmetrics.registerFont(TTFont("ReportBold", str(font_dir / bold)))
        pdfmetrics.registerFontFamily("ReportBody", normal="ReportBody", bold="ReportBold", italic="ReportBody", boldItalic="ReportBold")
        FONT, BOLD = "ReportBody", "ReportBold"
        break

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name="BodyText2", fontName=FONT, fontSize=10, leading=14, spaceAfter=7, splitLongWords=True))
styles.add(ParagraphStyle(name="Title2", fontName=BOLD, fontSize=27, leading=33, textColor=colors.HexColor("#16324F"), spaceAfter=16))
styles.add(ParagraphStyle(name="Chapter2", fontName=BOLD, fontSize=18, leading=23, textColor=colors.HexColor("#16324F"), spaceBefore=15, spaceAfter=10, keepWithNext=True))
styles.add(ParagraphStyle(name="Section2", fontName=BOLD, fontSize=12, leading=16, spaceBefore=10, spaceAfter=6, keepWithNext=True))
styles.add(ParagraphStyle(name="Small2", fontName=FONT, fontSize=8, leading=10.5, spaceAfter=5, splitLongWords=True))
styles.add(ParagraphStyle(name="Cell2", fontName=FONT, fontSize=8, leading=10.5, splitLongWords=True))
styles.add(ParagraphStyle(name="Cover2", fontName=FONT, fontSize=12, leading=18, spaceAfter=10))
styles.add(ParagraphStyle(name="Code2", fontName="Courier", fontSize=8, leading=11, spaceAfter=5, splitLongWords=True))

ADMIN_KEYS = ["institution", "course_code", "lecturer", "confirmed_deadline", "submission_destination"]
missing = [key for key in ADMIN_KEYS if not META.get(key)]
missing += ["matriculation number: " + member["name"] for member in META["members"] if not member.get("matriculation_number")]
review_copy = bool(missing) or "pending" in META.get("stakeholder_acceptance", "Pending").lower()
try:
    REV = subprocess.check_output(["git", "-c", f"safe.directory={ROOT.as_posix()}", "-C", str(ROOT), "rev-parse", "--short", "HEAD"], text=True).strip()
except Exception:
    REV = "unavailable"

def esc(value: object) -> str:
    return html.escape(str(value)).replace("\u2011", "-").replace("\u2013", "-").replace("\u2014", "-")

def markup(value: str) -> str:
    value = esc(value)
    value = re.sub(r"\*\*(.+?)\*\*", r"<b>\1</b>", value)
    value = re.sub(r"`([^`]+)`", r"<font name='Courier'>\1</font>", value)
    value = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r"\1", value)
    return value

STORY: list = []
def p(text: str, style="BodyText2"):
    formatted = markup(text)
    formatted = re.sub(r"(https?://[^\s<]+)", r'<link href="\1" color="#16324F">\1</link>', formatted)
    STORY.append(Paragraph(formatted, styles[style]))

def chapter(title: str):
    STORY.append(PageBreak())
    p(title, "Chapter2")

def table(rows: list[list[str]], widths: list[float] | None = None, header=True):
    cells = [[Paragraph(markup(str(cell)), styles["Cell2"]) for cell in row] for row in rows]
    t = Table(cells, colWidths=widths, repeatRows=1 if header else 0, hAlign="LEFT")
    commands = [
        ("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6), ("TOPPADDING", (0, 0), (-1, -1), 6),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 6), ("LINEBELOW", (0, 0), (-1, 0), 0.7, colors.HexColor("#64748B")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F1F5F9")]),
    ]
    if header: commands.append(("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#DFE8F1")))
    t.setStyle(TableStyle(commands)); STORY.append(t); STORY.append(Spacer(1, 10))

def include_markdown(path: Path, skip_title=True):
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    index = 0; fenced = False
    while index < len(lines):
        line = lines[index].strip(); index += 1
        if not line: continue
        if line.startswith("```"):
            fenced = not fenced; continue
        if fenced:
            if line.startswith(("powershell", "dotnet", "git ")): p(line, "Code2")
            continue
        if line.startswith("| "):
            rows = [line.strip("|").split("|")]
            while index < len(lines) and lines[index].strip().startswith("|"):
                candidate = lines[index].strip().strip("|").split("|"); index += 1
                if all(re.fullmatch(r"[\s:-]+", cell) for cell in candidate): continue
                rows.append(candidate)
            rows = [[cell.strip() for cell in row] for row in rows]
            table(rows, [505 / len(rows[0])] * len(rows[0]))
        elif line.startswith("# "):
            if not skip_title: p(line[2:], "Section2")
        elif line.startswith("## "): p(line[3:], "Section2")
        elif line.startswith("### "): p(line[4:], "Section2")
        elif line.startswith("- "): p("- " + line[2:])
        else: p(line)

def page_decoration(canvas, document):
    width, height = document.pageTemplate.pagesize
    canvas.setPageSize((width, height))
    canvas.saveState(); canvas.setFont(FONT, 8)
    canvas.setFillColor(colors.HexColor("#475569"))
    canvas.drawString(45, height - 28, "GROUP 14  |  COMPLAINT AND FEEDBACK MANAGEMENT SYSTEM")
    canvas.setStrokeColor(colors.HexColor("#CBD5E1")); canvas.line(45, height - 36, width - 45, height - 36)
    canvas.drawString(45, 28, ("REVIEW COPY - details / acceptance pending" if review_copy else "Team-reviewed report") + f"  |  Source base {REV}")
    canvas.drawRightString(width - 45, 28, f"Page {document.page}"); canvas.restoreState()

class ReportDocument(BaseDocTemplate):
    def afterFlowable(self, flowable):
        if isinstance(flowable, Paragraph) and flowable.style.name == "Chapter2":
            title = flowable.getPlainText()
            key = f"chapter-{self.page}-{abs(hash(title))}"
            self.canv.bookmarkPage(key)
            self.notify("TOCEntry", (0, title, self.page, key))

doc = ReportDocument(str(PDF_PATH), pagesize=A4, title=META["project_title"], author="Group 14", leftMargin=45, rightMargin=45, topMargin=55, bottomMargin=50)
body_frame = Frame(45, 50, 505, A4[1] - 110, leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)
doc.addPageTemplates(PageTemplate(id="Body", frames=[body_frame], onPage=page_decoration, pagesize=A4))

STORY.append(Spacer(1, 55)); p("SOFTWARE ENGINEERING PROJECT REPORT", "Cover2")
p(META["project_title"], "Title2"); p("Group 14 | Windows Forms | C# / .NET 10 | Waterfall", "Cover2")
p("REVIEW COPY - administrative details and final human acceptance pending" if review_copy else "Submission copy", "Section2")
for label, key in [("Institution", "institution"), ("Course", "course_title"), ("Course code", "course_code"), ("Lecturer", "lecturer"), ("Confirmed deadline", "confirmed_deadline")]:
    p(f"**{label}:** {META.get(key) or 'Awaiting team confirmation'}", "Cover2")
STORY.append(Spacer(1, 14))
table([["Member", "Matriculation number", "Primary responsibility"]] + [[m["name"], m.get("matriculation_number") or "Awaiting team details", m["role"]] for m in META["members"]], [145, 130, 230])
p("Prepared from repository source, executed test evidence and recorded team contributions on 27 November 2026. Missing details are not invented; no stakeholder acceptance is inferred.", "Small2")

STORY.append(PageBreak()); p("Contents", "Title2")
contents = TableOfContents()
contents.levelStyles = [ParagraphStyle(name="ContentsLevel", fontName=FONT, fontSize=10, leading=17, spaceBefore=5, leftIndent=0, firstLineIndent=0)]
STORY.append(contents)

chapter("1. Project overview and objectives")
p("The system provides a local desktop workflow for recording complaints and general feedback, categorising issues, assigning a reference and tracking resolution. Customers submit and track tickets; Support Agents filter and update them. The four core features match the Group 14 topic in the lecturer's brief.")
p("Objectives: provide traceable complaint records; reduce lost or informal requests; give consistent category/status information; preserve data across restarts; separate UI, business logic and storage; and provide testable, attributable C# source.")
p("The first acceptance baseline is one Windows application process per data directory, with demonstration accounts and JSON persistence. Separate administrator account-management/summary screens are not currently implemented and remain a team scope decision. The existing Admin login opens the support workspace.")
p("Repository: " + META["repository_url"])
p("Demonstration link: " + META["video_url"])
p("The demonstration is an automated screen recording of actual WinForms controls with synthetic data and auto-closing demo feedback. It is distinct from final human acceptance.")

chapter("2. Development process and team organisation")
p("Waterfall organises this project into planning/feasibility, requirements, system modelling/design, implementation, verification/validation and delivery. Phase records separate technical completion from real reviewer approval.")
table([["Stage", "Repository evidence", "Current status"],
       ["Planning", "Feasibility, schedule and risk draft", "Prepared; team dates/capacity need confirmation"],
       ["Requirements", "SRS with FR-01..FR-08 and NFR-01..NFR-06", "Documented; remaining team scope decisions pending"],
       ["Design", "Architecture/use case/class/sequence/activity and wireframes", "Editable source + 9 PNG exports"],
       ["Implementation", "Forms, Models, Data and Services", "Integrated and committed; tested core features"],
       ["Verification", "xUnit, reliability and native UI checks", "72 passing automated checks; human acceptance pending"],
       ["Delivery", "This report, demo, repository, acceptance record", "Review pack prepared; submission details pending"]], [85, 220, 200])
p("Chronology note: some planning and requirements documents were formalised after initial implementation. This report presents the lifecycle structure without backdating reviews or claiming that every document preceded all coding. Commit history and the project log preserve the actual order.")
p("Member 3's Phase 2 design approval and Phase 3 authorisation are recorded in the project log for 27 November 2026. This is one member's recorded approval, not an invented final team or stakeholder signature.")

chapter("3. Feasibility, schedule and risks")
include_markdown(ROOT / "docs/phase-2/FEASIBILITY-SCHEDULE-RISKS.md")

chapter("4. Software requirements specification")
include_markdown(ROOT / "docs/requirements/SRS.md")

chapter("5. Architecture, data design and implementation")
p("Presentation (Forms) depends on ComplaintService (Services), which uses the domain objects and StorageService (Models/Data). Forms collect input and display results; they do not directly edit JSON. Program creates a shared service and routes the authenticated role.")
table([["Component", "Responsibility"], ["LoginForm", "Masked credentials, blank/invalid input feedback and role return"], ["CustomerForm", "Submission, saved-reference confirmation, tracking and read-only details"], ["AgentForm", "Six explicit read-only columns, filters, single selection and updates"], ["ComplaintService", "Validation, authentication, references, snapshots and staged persistence"], ["StorageService", "JSON validation, atomic replacement, previous-version backups and explicit recovery"]], [130, 375])
p("User stores Id, Username, PasswordHash and Role. Complaint stores ReferenceNumber, SubmittedBy, Title, Category, Description, Status, AgentNotes, CreatedAt and UpdatedAt. Enums define roles, four complaint categories and Pending/InProgress/Resolved/Closed states.")
p("Atomic persistence: serialize to a unique temporary file, flush it, replace the existing primary atomically and retain its previous version as .bak. Only a successful persistent write publishes the candidate records to the service cache. A failed write preserves the prior cache and file.")
p("Reference allocation uses reference-sequence.json with one watermark per year, reconciled with the largest existing CMP reference. A reserved number is saved before the complaint; failures can create gaps, but healthy retained counters prevent reuse after deletion. The in-process lock is not a multi-process/database transaction.")
p("Recovery is explicit: validate complaints/users backup, preserve the displaced primary as .corrupt-*, restore and reopen the app. Missing primaries with backups are not silently replaced with empty records. Counter history is not automatically rewound.")
p("Authentication retains the supplied SHA-256/Base64 demonstration scheme and seeded accounts. Role routing and the implemented local classroom workflow are documented; a production password system, actor-level authorisation, customer ownership enforcement and strict state transitions are not claimed.")
p("Separation and OOP: User/Complaint model entities, enums, encapsulated service/storage classes, injectable storage for deterministic failure tests, and an IFormFeedback interface that lets UI checks record messages while normal app launches use real MessageBox feedback.")

chapter("6. Verification strategy and executed test records")
p("Testing is separated into isolated unit checks, integration/persistence checks, native-control system checks and human acceptance. Most service tests use real JSON I/O and are integration tests. Every fixture uses its own sample-data directory; no real complaint records are used.")
p("The final integrated baseline passes 34 xUnit cases, 20 reliability scenarios and 18 native UI checks: 72 automated checks total. The complete solution builds with zero warnings and errors. The known stale success-reference defect was reproduced by UI-14, fixed by clearing the label before validation, and verified by rerunning the expanded suite.")
p("Test case table: theory inputs are included in the test name where applicable. A Passed outcome means the expected assertions were met in the executed run; it is not an independently signed acceptance result.")

catalog = {
 "AUTH01": ("Seeded Customer/Agent/Admin credentials", "Matching authenticated role"),
 "AUTH02": ("Uppercase customer username", "Valid user returned"),
 "AUTH03": ("Wrong/unknown/empty credentials", "Null authentication result"),
 "SUB01": ("Padded Billing title/details", "Trimmed persisted Pending complaint"),
 "SUB02": ("Blank/whitespace required field", "ArgumentException; no ticket created"),
 "CAT01": ("Each defined category", "Selected category round-trips"),
 "REF01": ("Three submissions with service restart", "Distinct references 0001, 0002, 0003"),
 "TRACK01": ("Lowercase/padded saved reference", "Correct complaint found"),
 "TRACK02": ("Unknown/blank reference", "No match"),
 "FILTER01": ("Mixed categories and statuses", "Correct all/single/combined filters"),
 "FILTER02": ("Two distinct creation dates", "Newest ticket first"),
 "UPDATE01": ("Valid reference/category/status/notes", "Updated fields persist; creation time retained"),
 "UPDATE02": ("Whitespace replacement notes", "Existing notes preserved"),
 "UPDATE03": ("Unknown update reference", "False result; JSON unchanged"),
 "STORE01": ("Missing complaints file", "Empty list for fresh installation"),
 "STORE02": ("Unicode, quotes and multiline details", "All complaint fields round-trip"),
 "STORE03": ("Missing users, followed by reload", "Three stable hashed demonstration accounts"),
 "STORE04": ("One saved custom agent", "Custom users retained; no reseeding"),
 "STORE05": ("Malformed complaint JSON", "JsonException; source preserved"),
 "UNIT01": ("UTF-8 string abc", "Known SHA-256/Base64 digest"),
 "PERF01": ("1,000 synthetic complaint records", "Correct counts/lookup within 2 seconds")
}
trx = ET.parse(ROOT / "TestResults/backend-tests.trx")
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
case_rows = [["ID", "Inputs", "Expected result", "Actual evidence", "Outcome"]]
for result in sorted(trx.findall(".//t:UnitTestResult", ns), key=lambda item: item.attrib["testName"]):
    name = result.attrib["testName"]; match = re.search(r"(AUTH\d+|SUB\d+|CAT\d+|REF\d+|TRACK\d+|FILTER\d+|UPDATE\d+|STORE\d+|UNIT\d+|PERF\d+)", name)
    key = match.group(1) if match else "CASE"
    inputs, expected = catalog.get(key, (name, "Assertions satisfied"))
    if "(" in name: inputs += ": " + name.split("(", 1)[1].rstrip(")")
    outcome = result.attrib["outcome"]
    case_rows.append([key, inputs, expected, "Assertions met; " + result.attrib.get("duration", "") if outcome == "Passed" else "See failure in TRX", outcome])
table(case_rows, [48, 135, 145, 127, 50])

for title, path, key in [
    ("Reliability / recovery scenarios", ROOT / "docs/review/backend-reliability-results.json", "Results"),
    ("Native WinForms scenarios", ROOT / "docs/review/ui-check-results.json", "Results")
]:
    p(title, "Section2"); data = json.loads(path.read_text(encoding="utf-8"))
    rows = [["ID", "Inputs / scenario", "Expected result", "Actual evidence", "Outcome"]]
    for item in data[key]:
        ident, description = item["Name"].split(" ", 1)
        rows.append([ident, "Isolated sample data; " + description, description, item["Message"], "Passed" if item["Passed"] else "Failed"])
    table(rows, [48, 135, 145, 127, 50])

chapter("7. Performance, defects and remaining validation")
ui_data = json.loads((ROOT / "docs/review/ui-check-results.json").read_text(encoding="utf-8"))
for measure in ui_data.get("Measurements", []):
    table([["Operation on 1,000 rows", "Measured time", "Proposed target"], ["Native grid load/bind", f"{measure['GridLoadMs']:.1f} ms", "Under 2,000 ms"], ["Filter and redisplay", f"{measure['FilterMs']:.1f} ms", "Under 2,000 ms"], ["Customer tracking display", f"{measure['SearchMs']:.1f} ms", "Under 2,000 ms"]], [245, 130, 130])
    p(f"Environment: {measure['OS']}, {measure['Runtime']}, {measure['LogicalCpus']} logical CPUs, {measure['Dpi']} DPI. This is the tested host, not proof of performance on every computer.")
p("Resolved defects: missing/unclear validation feedback; stale prior success reference after a rejected submission; overly broad agent grid columns and unsafe selection state; cached changes after failed saves; collision-prone Count + 1 references; and missing explicit backup restoration. Evidence links and residual limits are recorded in docs/INTEGRATION_REVIEW.md.")
p("Remaining validation: actual modal-popup usability, a named Customer and Support Agent acceptance session, target-PC confirmation if different, and final team scope/report approval. A reviewer must record real input, expected/actual result, date, machine and decision. No signed acceptance is invented.")

chapter("8. Deployment and user guide")
p("Install the .NET 10 SDK on Windows, clone/download the shared repository, open its root folder and run the PowerShell Run-Project helper. It uses the workspace-local SDK where available, otherwise dotnet on PATH. Do not use Live Preview to run a desktop application.")
for command in ["powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Build -Configuration Release", "powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Run"]: p(command, "Code2")
p("Customer: sign in as customer1/pass123, enter title/category/description, submit and retain the reference. On Track Status, enter that reference; blank/unknown input is handled explicitly. Support Agent: sign in as agent1/agent123, filter/reset, select a row, edit status/category/notes and save. Admin demo account admin1/admin123 currently opens the same support workspace.")
p("The data folder is the process working directory; the run helper uses the repository root. Keep users.json, complaints.json and reference-sequence.json together with their backups. Run one app process per directory. Close the app before explicit recovery, inspect the backup and use tools/StorageRecovery with --restore only when you intend to replace the primary.")

chapter("9. Team contribution evidence and references")
table([["Contributor", "Evidence", "Responsibility"], ["Eregha Tarilah", "8e4b9d5; 2fea1c6", "UI, validation, native-control checks"], ["Kailota Igwe", "bfb18e4; bd30094; c21b8e9; 9fef1a8", "Backend, storage, reliability, integration/design fixes"], ["Chukwuemeka Okoye", "e659b7f; 406036b", "SRS, xUnit tests, QA evidence, planning/activity material"]], [135, 165, 205])
p("Individual contribution records preserve each original commit author. Every member remains responsible for reviewing and understanding their submitted work. Final matriculation details must be supplied and verified by the team.")
for reference in ["Group Topics.pdf: Group 14, pages 2-3 (provided course topic brief).", "Group Project Questions.pdf: objective, lifecycle and report requirements, pages 1-2 (provided assignment brief).", META["repository_url"], "Repository SRS, design pack, test plan, TRX-derived results and integration review.", "Microsoft .NET/xUnit testing guide: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit", "xUnit parallel-test documentation: https://xunit.net/docs/running-tests-in-parallel"]: p(reference)

chapter("10. Acceptance and submission readiness")
include_markdown(DEST / "ACCEPTANCE.md")
if missing:
    p("Administrative details still needed", "Section2")
    for item in missing: p("- " + item)
p("Scope: " + META.get("scope_confirmation", "Pending"))
p("Stakeholder acceptance: " + META.get("stakeholder_acceptance", "Pending"))
p("The team must review the final PDF, verify accessible repository/video links, submit through the confirmed course channel and retain its receipt. This review copy is not a submission receipt.")

chapter("Appendix A. Running application evidence")
p("Screenshots are from the recorded automated walkthrough of the actual application, using isolated synthetic data. Demo messages auto-close so the recording can proceed unattended; the normal application still uses its standard message boxes.")
screens = ROOT / "docs/demo/screenshots"
for name, title in [("01-login-validation", "Invalid login feedback and masked password"), ("02-submission-input", "Customer complaint input"), ("03-submitted-reference", "Persisted reference returned after submission"), ("05-agent-workspace", "Agent grid and filtering workspace"), ("06-agent-update", "Selected ticket category/status/notes update"), ("07-persisted-resolution", "Customer sees the persisted resolution after reopening")]:
    image_path = screens / (name + ".png")
    if not image_path.exists(): continue
    p(title, "Section2")
    width, height = PILImage.open(image_path).size; scale = min(505 / width, 420 / height)
    STORY.append(Image(str(image_path), width=width * scale, height=height * scale)); STORY.append(Spacer(1, 12))

chapter("Appendix B. System and software design plates")
p("The following full-resolution plates use larger digital page sizes so labels remain readable when zoomed. Editable source and standalone PNGs are in docs/phase-2. Optional administrator features remain explicitly marked for a team scope decision.")
figures = [("01-architecture", "Three-tier architecture"), ("02-use-cases", "Use case diagram"), ("03-classes", "Core class diagram"), ("04-sequence", "Submit and track sequence"), ("09-activity", "Complaint lifecycle activity flow")]
for index, (name, title) in enumerate(figures):
    image_path = ROOT / "docs/phase-2/exports" / (name + ".png")
    width, height = PILImage.open(image_path).size; draw_width = 910; draw_height = height * draw_width / width
    plate_size = (1000, max(740, draw_height + 200)); template_id = f"Plate{index}"
    frame = Frame(45, 50, 910, plate_size[1] - 110, leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)
    doc.addPageTemplates(PageTemplate(id=template_id, frames=[frame], onPage=page_decoration, pagesize=plate_size))
    STORY.extend([NextPageTemplate(template_id), PageBreak()])
    p(title, "Chapter2"); p("Group 14 | Integrated design | Editable source: docs/phase-2/design.drawio", "Small2")
    STORY.append(Image(str(image_path), width=draw_width, height=draw_height))

doc.multiBuild(STORY)
print(f"Built {PDF_PATH}")
print("Report status: " + ("REVIEW COPY" if review_copy else "Metadata complete; verify human acceptance separately"))
