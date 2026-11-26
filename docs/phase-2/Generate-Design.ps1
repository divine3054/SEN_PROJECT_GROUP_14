$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$outDir = Join-Path $PSScriptRoot 'exports'
[void][IO.Directory]::CreateDirectory($outDir)
$script:doc = New-Object System.Xml.XmlDocument
$script:mxfile = $doc.CreateElement('mxfile')
$mxfile.SetAttribute('host', 'app.diagrams.net')
[void]$doc.AppendChild($mxfile)
$script:pen = New-Object Drawing.Pen([Drawing.Color]::FromArgb(63,75,90), 1.4)
$script:ink = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(28,40,55))
$script:paper = New-Object Drawing.SolidBrush([Drawing.Color]::White)
$script:fill = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(241,245,249))
$script:font = New-Object Drawing.Font('Arial', 15, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
$script:small = New-Object Drawing.Font('Arial', 13, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
$script:heading = New-Object Drawing.Font('Arial', 25, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)

function Page($name, $slug, $width, $height) {
    $script:slug = $slug
    $script:bmp = New-Object Drawing.Bitmap(($width * 2), ($height * 2))
    $bmp.SetResolution(192,192)
    $script:g = [Drawing.Graphics]::FromImage($bmp)
    $g.ScaleTransform(2,2)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'
    $g.Clear([Drawing.Color]::White)
    $diagram = $doc.CreateElement('diagram'); $diagram.SetAttribute('name',$name); $diagram.SetAttribute('id',$slug)
    [void]$mxfile.AppendChild($diagram)
    $model = $doc.CreateElement('mxGraphModel'); $model.SetAttribute('pageWidth',"$width"); $model.SetAttribute('pageHeight',"$height"); $model.SetAttribute('page','1')
    [void]$diagram.AppendChild($model)
    $script:root = $doc.CreateElement('root'); [void]$model.AppendChild($root)
    foreach ($id in @('0','1')) { $cell = $doc.CreateElement('mxCell'); $cell.SetAttribute('id',$id); if ($id -eq '1') {$cell.SetAttribute('parent','0')}; [void]$root.AppendChild($cell) }
    $script:counter = 1
    Text $name 35 20 ($width-70) 40 $heading
    Text 'Complaint and Feedback Management System | Phase 2 review baseline | 26 November 2026' 35 65 ($width-70) 30 $small
}
function Vertex($text,$x,$y,$w,$h,$style) {
    $script:counter++
    $cell = $doc.CreateElement('mxCell'); $cell.SetAttribute('id',"n$counter"); $cell.SetAttribute('value',$text)
    $cell.SetAttribute('style',$style); $cell.SetAttribute('vertex','1'); $cell.SetAttribute('parent','1')
    $geo = $doc.CreateElement('mxGeometry'); foreach($a in @{x=$x;y=$y;width=$w;height=$h;as='geometry'}.GetEnumerator()) {$geo.SetAttribute($a.Key,"$($a.Value)")}
    [void]$cell.AppendChild($geo); [void]$root.AppendChild($cell)
}
function Text($text,$x,$y,$w,$h,$f=$font) {
    $g.DrawString($text,$f,$ink,[Drawing.RectangleF]::new($x,$y,$w,$h))
    Vertex $text $x $y $w $h ("text;html=0;align=left;verticalAlign=top;whiteSpace=wrap;fontFamily=Arial;fontSize=" + $f.Size + ';')
}
function Box($text,$x,$y,$w,$h,$ellipse=$false) {
    if($ellipse) { $g.FillEllipse($fill,$x,$y,$w,$h); $g.DrawEllipse($pen,$x,$y,$w,$h); $style='ellipse;' }
    else { $g.FillRectangle($fill,$x,$y,$w,$h); $g.DrawRectangle($pen,$x,$y,$w,$h); $style='' }
    $align='align=left;verticalAlign=top;'
    if($ellipse) {
        $format=New-Object Drawing.StringFormat
        $format.Alignment='Center'; $format.LineAlignment='Center'
        $g.DrawString($text,$font,$ink,[Drawing.RectangleF]::new(($x+25),($y+5),($w-50),($h-10)),$format)
        $format.Dispose(); $align='align=center;verticalAlign=middle;'
    } else {
        $g.DrawString($text,$font,$ink,[Drawing.RectangleF]::new(($x+12),($y+10),($w-24),($h-18)))
    }
    Vertex $text $x $y $w $h ($style+'html=0;whiteSpace=wrap;'+$align+'spacing=12;fillColor=#F1F5F9;strokeColor=#3F4B5A;fontFamily=Arial;fontSize=15;')
}
function Line($x1,$y1,$x2,$y2,$arrow=$true,$dashed=$false) {
    $p = $pen.Clone(); if($dashed){$p.DashStyle='Dash'}
    $g.DrawLine($p,$x1,$y1,$x2,$y2)
    if($arrow) {
        $angle=[Math]::Atan2(($y2-$y1),($x2-$x1)); $size=10
        $g.DrawLine($pen,$x2,$y2,($x2-$size*[Math]::Cos($angle-0.45)),($y2-$size*[Math]::Sin($angle-0.45)))
        $g.DrawLine($pen,$x2,$y2,($x2-$size*[Math]::Cos($angle+0.45)),($y2-$size*[Math]::Sin($angle+0.45)))
    }
    $p.Dispose(); $script:counter++
    $cell=$doc.CreateElement('mxCell'); $cell.SetAttribute('id',"n$counter"); $cell.SetAttribute('edge','1'); $cell.SetAttribute('parent','1')
    $end=if($arrow){'open'}else{'none'}; $dash=if($dashed){'1'}else{'0'}
    $cell.SetAttribute('style',"endArrow=$end;dashed=$dash;html=0;strokeColor=#3F4B5A;")
    $geo=$doc.CreateElement('mxGeometry'); $geo.SetAttribute('relative','1'); $geo.SetAttribute('as','geometry')
    foreach($pt in @(@($x1,$y1,'sourcePoint'),@($x2,$y2,'targetPoint'))) {
        $point=$doc.CreateElement('mxPoint'); $point.SetAttribute('x',"$($pt[0])");$point.SetAttribute('y',"$($pt[1])");$point.SetAttribute('as',$pt[2]);[void]$geo.AppendChild($point)
    }
    [void]$cell.AppendChild($geo);[void]$root.AppendChild($cell)
}
function Actor($name,$x,$y) {
    $g.DrawEllipse($pen,($x+26),$y,28,28)
    $g.DrawLine($pen,($x+40),($y+28),($x+40),($y+75)); $g.DrawLine($pen,($x+10),($y+45),($x+70),($y+45))
    $g.DrawLine($pen,($x+40),($y+75),($x+15),($y+105)); $g.DrawLine($pen,($x+40),($y+75),($x+65),($y+105))
    Vertex '' $x $y 80 105 'shape=umlActor;'
    Text $name ($x-35) ($y+112) 165 45
}
function SavePage { $bmp.Save((Join-Path $outDir "$slug.png"),[Drawing.Imaging.ImageFormat]::Png);$g.Dispose();$bmp.Dispose() }

Page '01 / Three-tier architecture' '01-architecture' 1200 900
Box "PRESENTATION | Forms/`nLoginForm / CustomerForm / AgentForm`nOptional: AdminSummaryForm`nInput, dropdowns, validation alerts, confirmation messages, ticket grids" 170 130 860 140
Line 600 270 600 330
Box "BUSINESS LOGIC | Services/ComplaintService`nAuthenticate; validate title and description; generate references`nSubmit and track complaints; filter category/status; update category/status/notes" 170 330 860 130
Line 600 460 600 520
Box "DATA ACCESS & DOMAIN | Data/ and Models/`nStorageService / User / Complaint / Enums`nJSON serialization and deserialization; SHA-256 password hashing" 170 520 860 130
Line 600 650 600 710
Box "LOCAL RECORDS`ncomplaints.json / users.json (process working directory)" 310 710 580 80
Text 'Arrows show dependencies/calls. Forms also use domain types for binding. Forms do not access JSON directly.' 170 810 860 55
SavePage

Page '02 / Use cases' '02-use-cases' 1500 1240
# System boundary is unfilled, so nested elements remain visible in draw.io.
$g.DrawRectangle($pen,300,120,1110,1050)
Vertex '' 300 120 1110 1050 'fillColor=none;strokeColor=#3F4B5A;'
Text 'Complaint and Feedback Management System' 560 130 650 35
Actor 'Customer' 100 300
Actor 'Support Agent' 100 610
Actor 'Administrator' 100 930
Box 'Log In' 390 190 260 60 $true
Box "Submit Complaint/Feedback" 390 300 300 75 $true
Box "Receive Reference Number" 900 300 310 75 $true
Box 'Track Complaint Status' 390 410 300 70 $true
Box 'View & Filter Complaints' 390 530 300 70 $true
Box 'Re-categorise Issue' 390 640 350 70 $true
Box "Update Resolution Status & Notes" 390 750 420 80 $true
Box 'Manage Agent Accounts' 390 890 350 70 $true
Box 'View System Ticket Summary' 390 1000 420 70 $true
Line 180 345 390 220 $false; Line 180 345 390 337 $false; Line 180 345 390 445 $false
Line 180 655 390 220 $false; Line 180 655 390 565 $false; Line 180 655 390 675 $false; Line 180 655 390 790 $false
Line 180 975 390 220 $false; Line 180 975 390 925 $false; Line 180 975 390 1035 $false
Line 690 337 900 337 $true $true; Text '<<include>>' 730 307 155 25
Text 'Administrator account management and ticket summary are requirements awaiting implementation/scope decision.' 330 1110 1000 40
SavePage

Page '03 / Backend class diagram' '03-classes' 1600 1180
Box "User`n--------------------------------------`n+ Id: string`n+ Username: string`n+ PasswordHash: string`n+ Role: UserRole" 40 130 370 200
Box "Complaint`n--------------------------------------`n+ ReferenceNumber: string`n+ SubmittedBy: string`n+ Title: string`n+ Category: ComplaintCategory`n+ Description: string`n+ Status: ComplaintStatus`n+ AgentNotes: string`n+ CreatedAt: DateTime`n+ UpdatedAt: DateTime" 40 420 370 310
Box "ComplaintService`n-------------------------------------------------------------------`n- _storage: StorageService`n- _complaints: List<Complaint>`n- _users: List<User>`n+ ComplaintService()`n+ Authenticate(username: string, password: string): User?`n+ SubmitComplaint(username: string, title: string,`n    category: ComplaintCategory, description: string): Complaint`n+ TrackByReference(referenceNumber: string): Complaint?`n+ GetAllComplaints(categoryFilter: ComplaintCategory? = null,`n    statusFilter: ComplaintStatus? = null): List<Complaint>`n+ UpdateComplaintStatus(referenceNumber: string,`n    newStatus: ComplaintStatus, newCategory: ComplaintCategory,`n    agentNotes: string): bool" 510 130 980 390
Box "StorageService`n-------------------------------------------------------------------`n- _complaintsFile: string`n- _usersFile: string`n+ LoadComplaints(): List<Complaint>`n+ SaveComplaints(complaints: List<Complaint>): void`n+ LoadUsers(): List<User>`n+ SaveUsers(users: List<User>): void`n+ HashPassword(password: string): string {static}" 620 650 720 270
Line 510 230 410 230; Text 'manages 0..*' 415 195 105 35 $small
Line 510 470 410 560; Text 'manages 0..*' 414 545 120 35 $small
Line 1000 520 1000 650; Text 'owns / uses 1' 1020 565 240 30
Box "<<enumeration>> UserRole`nCustomer / SupportAgent / Admin" 40 810 390 90
Box "<<enumeration>> ComplaintCategory`nBilling / Technical / Service / GeneralFeedback" 40 940 570 90
Box "<<enumeration>> ComplaintStatus`nPending / InProgress / Resolved / Closed" 700 970 640 90
Text 'Public model properties use +. SubmittedBy is a username string. Collection links describe the private lists; no User navigation property exists.' 40 1100 1470 50
SavePage

Page '04 / Submit and track sequence' '04-sequence' 1500 1330
$xs=@(100,400,850,1320)
$names=@('Customer','CustomerForm','ComplaintService','StorageService')
for($i=0;$i -lt 4;$i++){Box $names[$i] ($xs[$i]-80) 130 165 55;Line $xs[$i] 185 $xs[$i] 1230 $false $true}
function Message($from,$to,$y,$label,$reply=$false) {Line $xs[$from] $y $xs[$to] $y $true $reply;Text $label ([Math]::Min($xs[$from],$xs[$to])+12) ($y-35) ([Math]::Abs($xs[$to]-$xs[$from])-20) 34 $small}
Message 0 1 245 'Enter title/category/description; click Submit'
Message 1 2 335 'SubmitComplaint(user.Username, title, category, desc)'
Box "Validate title/description`nGenerate CMP-{year}-{Count + 1:D4}`nCreate Complaint; append to cached list" 750 370 490 95
Message 2 3 535 'SaveComplaints(_complaints)'
Box "Serialize and write`ncomplaints.json" 1200 570 255 75
Message 3 2 685 'Save completes' $true
Message 2 1 755 'Return new Complaint' $true
Message 1 0 825 'Confirmation popup + lblGeneratedRef' $true
Message 0 1 915 'Enter reference; click Search'
Message 1 2 985 'TrackByReference(referenceNumber)'
Message 2 1 1055 'Complaint? (lookup in cached list)' $true
Message 1 0 1125 'Display Status, Category, UpdatedAt, AgentNotes' $true
Text 'Alternatives: missing fields -> ArgumentException; JSON/file failure -> exception; missing reference -> null. UI shows an alert/no-match message. Success confirmation requires a successful save. Failed saves can leave cached changes in memory.' 160 1240 1200 75
SavePage

Page '05 / LoginForm wireframe' '05-login' 1000 760
Box 'Complaint Management System - Login' 170 140 660 490
Text 'Username' 230 225 200 25
Box 'txtUsername [TextBox]' 230 260 540 55
Text 'Password' 230 340 200 25
Box 'txtPassword [UseSystemPasswordChar = true]' 230 375 540 55
Box 'btnLogin | Log In' 570 475 200 55
Text 'Authenticate -> Customer: CustomerForm; SupportAgent / Admin: AgentForm. Invalid credentials: alert and remain here.' 170 660 660 65
SavePage

Page '06 / CustomerForm - Submit tab' '06-customer-submit' 1150 900
Box 'CustomerForm' 60 120 1030 670
Box 'Submit Complaint [active tab]' 90 180 390 50
Box 'Track Status' 480 180 260 50
Text 'Title' 110 270 150 25; Box 'txtTitle' 110 305 930 50
Text 'Category' 110 385 180 25; Box 'cmbCategory [ComplaintCategory dropdown]' 110 420 540 50
Text 'Description' 110 500 180 25; Box "txtDescription`n[Multiline TextBox]" 110 535 930 120
Box 'btnSubmit | Submit' 800 685 240 55
Text 'lblGeneratedRef [returned reference after successful save]' 110 700 650 40
Text 'Success: confirmation popup containing the reference. Required fields and persistence errors: validation alert.' 60 820 1030 55
SavePage

Page '07 / CustomerForm - Track tab' '07-customer-track' 1150 900
Box 'CustomerForm' 60 120 1030 670
Box 'Submit Complaint' 90 180 390 50; Box 'Track Status [active tab]' 480 180 360 50
Box 'txtSearchRef [reference number]' 110 275 660 55; Box 'btnSearch | Search' 800 275 240 55
Box 'Status: lblTrackedStatus [read-only]' 110 385 930 55
Box 'Category: lblTrackedCategory [read-only]' 110 460 930 55
Box 'UpdatedAt: lblTrackedUpdatedAt [read-only]' 110 535 930 55
Box "AgentNotes: txtTrackedAgentNotes`n[ReadOnly = true; Multiline = true]" 110 610 930 110
Text 'Blank reference: validation alert. No match: clear previous results and show a not-found message.' 60 820 1030 55
SavePage

Page '08 / AgentForm wireframe' '08-agent' 1550 1050
Box 'AgentForm | Support Agent / Administrator' 45 120 1460 800
Box 'cmbFilterCategory [All / enum]' 75 180 420 50
Box 'cmbFilterStatus [All / enum]' 515 180 420 50
Box 'btnFilter' 960 180 190 50; Box 'btnRefresh' 1170 180 230 50
Box "dgvComplaints [read-only; full-row single selection]`n`nReferenceNumber     | Title            | Category       | Status         | SubmittedBy      | CreatedAt`n--------------------------------------------------------------------------------------------------------------`n[Complaint records from GetAllComplaints; explicit bound columns]" 75 280 1380 290
Text 'Selected complaint' 75 610 350 30
Box 'cmbUpdateStatus [ComplaintStatus]' 75 655 440 50
Box 'cmbUpdateCategory [ComplaintCategory]' 540 655 550 50
Box "txtAgentNotes`n[Multiline TextBox]" 75 735 1015 120
Box 'btnSaveUpdate' 1130 790 325 60
Text 'Save disabled without selection. Populate action controls from the selected row; update by ReferenceNumber. Refresh clears filters. Admin account management and summary screens remain undecided scope.' 45 950 1460 70
SavePage

$doc.Save((Join-Path $PSScriptRoot 'design.drawio'))
foreach($item in @($pen,$ink,$paper,$fill,$font,$small,$heading)){$item.Dispose()}
Write-Output 'Generated 8 editable draw.io pages and 8 PNG exports at 2x resolution.'
