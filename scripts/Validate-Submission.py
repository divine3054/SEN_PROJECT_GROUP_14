from pathlib import Path
import json
import hashlib
from PIL import Image, ImageOps, ImageDraw
from pypdf import PdfReader
import pypdfium2 as pdfium

root = Path(__file__).resolve().parents[1]
report = root / "docs/submission/Group14-Project-Report.pdf"
preview = root / ".tools/report-preview"
preview.mkdir(parents=True, exist_ok=True)
reader = PdfReader(report)
text = "\n".join(page.extract_text() or "" for page in reader.pages)
required = ["Project overview", "Development process", "Feasibility", "Software requirements", "Architecture", "Verification strategy", "Performance", "Deployment", "Team contribution", "Acceptance", "Use case", "Core class", "Submit and track", "Complaint lifecycle"]
for phrase in required:
    if phrase.lower() not in text.lower():
        raise RuntimeError("Missing report section: " + phrase)
document = pdfium.PdfDocument(str(report))
thumbnails = []
for index in range(len(document)):
    page = document[index]
    bitmap = page.render(scale=1.15)
    image = bitmap.to_pil()
    image.save(preview / f"page-{index+1:02d}.png")
    thumb = ImageOps.contain(image, (245, 340))
    tile = Image.new("RGB", (265, 370), "#e5e7eb")
    tile.paste(thumb, ((265-thumb.width)//2, 20))
    ImageDraw.Draw(tile).text((12, 350), f"Page {index+1}", fill="black")
    thumbnails.append(tile)
    image.close(); bitmap.close(); page.close()
for batch in range(0, len(thumbnails), 12):
    sheet = Image.new("RGB", (4*265, 3*370), "white")
    for offset, tile in enumerate(thumbnails[batch:batch+12]):
        sheet.paste(tile, ((offset % 4)*265, (offset // 4)*370))
    sheet.save(preview / f"contact-{batch//12+1}.png")
document.close()
report_data = {"pages": len(reader.pages), "sha256": hashlib.sha256(report.read_bytes()).hexdigest(), "required_sections_present": True, "all_pages_rendered": True, "status": "Review copy; administrative details and human acceptance pending"}
(preview / "validation.json").write_text(json.dumps(report_data, indent=2), encoding="utf-8")
print(json.dumps(report_data, indent=2))
