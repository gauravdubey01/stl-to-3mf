# STL to 3MF Converter — Session Record

## Project Location
`E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\version2\`

## Files

| File | Purpose |
|---|---|
| `StlTo3mfConverter.cs` | Main WinForms UI (Form + Program entry point) |
| `StlParser.cs` | STL file parser (auto-detects ASCII/Binary, vertex deduplication) |
| `ThreeMfWriter.cs` | 3MF file writer (ZIP + OPC XML) |
| `StlTo3mfConverter.exe` | Compiled binary (compile target) |
| `StlTo3mfConverter_Setup.exe` | Inno Setup installer |
| `installer.iss` | Inno Setup script |
| `SESSION.md` | This file |

## Compile Command
```
csc /target:winexe /out:StlTo3mfConverter.exe
    /reference:System.Windows.Forms.dll
    /reference:System.Drawing.dll
    /reference:System.Xml.Linq.dll
    /reference:System.IO.Compression.dll
    StlTo3mfConverter.cs StlParser.cs ThreeMfWriter.cs
```

## Build Installer
```
"C:\Users\Gaurav\AppData\Local\Programs\Inno Setup 6\ISCC.exe" installer.iss
```

## Key Implementation Details

### STL Parser (`StlParser.cs`)
- **Format detection**: reads uint32 at offset 80, checks if file size = `84 + count × 50` → binary; otherwise ASCII
- **ASCII**: line-by-line scan for `vertex x y z` lines inside `facet` blocks
- **Binary**: reads 80-byte header, uint32 count, then 50-byte facets (12 normal + 36 vertices + 2 attribute)
- **Vertex dedup**: rounds to 6 decimal places, deduplicates via `Dictionary<string, int>`
- `StlResult` class: `Vertices[]` + `Triangles[][]` (index-based)

### 3MF Writer (`ThreeMfWriter.cs`)
- Creates ZIP archive with `System.IO.Compression.ZipArchive`
- Writes `[Content_Types].xml`, `_rels/.rels`, `3D/3dmodel.model`
- LINQ to XML auto-generates `xmlns="..."` declaration
- Output namespace: `http://schemas.microsoft.com/3dmanufacturing/core/2015/02`

### Main Form (`StlTo3mfConverter.cs`)
- **Dark/Light toggle**: button top-right, switches all colors
- **Resizable**: min 800×560, `Sizable` border, maximizable
- **App icon**: loads `3MF.png` from `E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\`
- **Drag & drop**: accept folder drops onto form
- **Custom output**: separate output folder textbox + Browse + reset (↺)
- **Async**: `BackgroundWorker` — UI stays responsive during batch processing
- **ETA**: status bar shows `File X/Y | filename.stl | ETA: Ns`
- **Hover effects**: flat buttons with accent/hover color changes

### Dark Theme Colors
| Element | Dark | Light |
|---|---|---|
| Background | `#1c1c1c` | `#f0f0f0` |
| Control bg | `#2d2d30` | White |
| Text | `#e0e0e0` | `#1e1e1e` |
| Accent | `#0078d4` | `#0078d4` |
| Button bg | `#3e3e42` | `#e0e0e0` |
| List bg | `#1e1e1e` | White |
| Status text | `#50b4ff` | `#0064b4` |
| Header bg | `#141414` | `#dcdcdc` |

## Features Checklist
- [x] ASCII STL parsing
- [x] Binary STL parsing
- [x] Auto-detect format
- [x] Vertex deduplication
- [x] 3MF output (valid ZIP/XML)
- [x] Batch folder conversion (recursive)
- [x] Source folder structure preservation in output
- [x] Dark/Light theme toggle
- [x] Resizable window with min size
- [x] App icon from logo PNG
- [x] Drag & drop folder support
- [x] Custom output folder
- [x] Async processing (BackgroundWorker)
- [x] Per-file ETA in status bar
- [x] Modern File Explorer-style folder picker (IFileOpenDialog + FOS_PICKFOLDERS, falls back to FolderBrowserDialog)
- [x] Installer with logo icon
- [x] Start Menu + Desktop shortcuts

## Versioning System
Every change is saved as a new version with incremental version number.

### How to save a new version
Run the helper script from PowerShell:
```
.\Save-Version.ps1 -Version "v2.2"
```

The script will:
1. Create a folder `E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\v2.2\`
2. Copy all source files with versioned names: `StlTo3mfConverter_v2.2.cs`, etc.
3. Include compiled `.exe`, installer, logo, and session record

### Version history
| Version | Folder | Changes |
|---|---|---|
| v2.0 | `version2` | Initial UI redesign, dark/light toggle, async, drag-drop, custom output, ETA, installer |
| v2.1 | `v2.1` | Modern File Explorer-style folder picker (OpenFileDialog trick), version in header |
| v2.2 | `v2.2` | Rounded corners on buttons/listbox, refined color palette, Windows 11 form corners via DWM, "Open Output" button in status bar |
| v2.3 | `v2.3` | Renamed to "STL to 3MF Batch Converter", MSIX v2.3.0.0 for Store submission |
| v2.5 | `v2.5` | Added Ko-fi support link, tutorial, close dialog, theme toggle text, version bump to 2.5 |

### Naming convention
- Folder: `v{major}.{minor}` (e.g. `v2.1`, `v2.2`, `v3.0`)
- Files: `{Name}_v{major}.{minor}.{ext}` (e.g. `StlTo3mfConverter_v2.1.cs`)
- The `version2` folder always holds the latest working copy with original filenames

---

## Microsoft Store Upload (June 2026)

### Folder
`E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\microsoft upload files\`

### Files
| File | Purpose |
|---|---|
| `AppxManifest.xml` | Package identity, Publisher `CN=13cc54b3-80ca-40d0-909c-b669cb0d2ba2`, Version `2.3.0.0`, `runFullTrust` |
| `StlTo3mfConverter.exe` | Compiled binary (v2.3) |
| `PrivacyPolicy.html` | No-data-collection policy (must host publicly) |
| `StlTo3mfConverter-2.3.0.0.msix` | MSIX package (ZIP with manifest + assets + exe) |
| `StlTo3mfConverter-2.3.0.0.msixupload` | Store upload file (ZIP containing .msix) |
| `build-msix.ps1` | Regeneration script (falls back to .NET ZipArchive if no Windows SDK) |
| `assets\StoreLogo.png` | 1024×1024 logo |
| `assets\Square44x44Logo.png` | 44×44 |
| `assets\Square71x71Logo.png` | 71×71 |
| `assets\Square150x150Logo.png` | 150×150 |
| `assets\Square310x310Logo.png` | 310×310 |
| `assets\Wide310x150Logo.png` | 310×150 |
| `assets\SplashScreen.png` | 620×300 |

### Publisher ID
`13cc54b3-80ca-40d0-909c-b669cb0d2ba2` (GUID from Partner Center Product Identity)

### Constraints
- No Windows SDK installed on this machine — `resources.pri` not generated
- If Store certification complains about missing `resources.pri`, install Windows SDK from https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/ and re-run `build-msix.ps1`
- Compiler: `C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe` (.NET 4.8, C# 5)
- Logo source: `E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\3MF.png` (1024×1024)

### Next Steps (Updated for v2.3)
1. Host `PrivacyPolicy.html` at a public URL (GitHub Pages, etc.)
2. Go to https://partner.microsoft.com/dashboard
3. Upload `StlTo3mfConverter-2.3.0.0.msixupload` in the new MSIX listing's Packages tab
4. Fill in listing: description, screenshots (`Screenshot1.png`), logos (`BoxArt.png`, `PosterArt.png`)
5. Enter privacy policy URL
6. Submit for certification

---

## Session: Microsoft Store Submission Attempt (June 30, 2026)

### Problem
The EXE/MSI product type in Partner Center does not accept file uploads — only a URL.
The GitHub raw URL was rejected because it redirects (302 → CDN SAS URL).
The binary was also unsigned, violating policy 10.2.9.

### Solution
Create a **new product** in Partner Center with **MSIX** as the product type (not EXE/MSI).
MSIX products have a **Packages** tab with an **Upload** button, and the Store signs them automatically.

### GitHub Repo Cleanup
- Removed `dist/` folder (contained installer .exe for raw URL hosting)
- Removed `docs/` folder (contained return code docs — only needed for EXE type)
- Release v2.2 assets kept: `StlTo3mfConverter-2.2.0.0.msix` and `StlTo3mfConverter-2.2.0.0.msixupload`
- Existing `StlTo3mfConverter_Setup_v2.2.exe` and `StlTo3mfConverter_v2.2.exe` also remain on release (for direct downloads)

### Store Listing Images Created
| Image | Size | Purpose |
|---|---|---|
| `Screenshot1.png` | 1366×768 | App screenshot (dark theme, files queued) |
| `BoxArt.png` | 1080×1080 | 1:1 Box art / Store logo |
| `PosterArt.png` | 720×1080 | 2:3 Poster art (recommended layout) |

All generated programmatically via `GenerateScreenshots.cs` (C# 5, .NET 4.8, GDI+).

### Key Lessons for Next Time
- Start with **MSIX** product type — it has file upload + automatic signing
- EXE/MSI type requires: code signing certificate ($200–400/yr) + non-redirecting URL + return code docs
- GitHub release URLs always redirect (302) — not suitable as direct download URLs
- `raw.githubusercontent.com` does not redirect but only serves committed repo files, not release assets

---

## Session: v2.3 Release (June 30, 2026)

### Changes Made
- Renamed app/product to **"STL to 3MF Batch Converter"**
- Updated `AppxManifest.xml`: DisplayName, Description, Version `2.3.0.0`
- Updated window title and header label in source code
- Recompiled `StlTo3mfConverter.exe`
- Rebuilt MSIX packages: `StlTo3mfConverter-2.3.0.0.msix` + `.msixupload`
- Created **GitHub Release v2.3**: https://github.com/gauravdubey01/stl-to-3mf/releases/tag/v2.3
  - Assets: `StlTo3mfConverter-2.3.0.0.msixupload`, `StlTo3mfConverter_v2.3.exe`
- Version snapshot saved to `v2.3\` folder

### Store Package SID
`S-1-15-2-1186650230-888448994-2132929646-1804302065-2572911844-142802650-1165083831`

### Next
- Upload `.msixupload` to new MSIX product listing in Partner Center

---

## Session: Ko-fi Support Footer Added (July 7, 2026)

### Changes Made
- Added Ko-fi donation link (`https://ko-fi.com/gauravdubeypro`) in the bottom footer of the app
- Embedded Ko-fi logo (PNG) as base64 resource beside the link — no external file dependency
- LinkLabel styled with blue accent color, adapts to dark/light theme
- Clicking the logo or "Support on Ko-fi" text opens the URL in the default browser
- Updated header label to "STL to 3MF Batch Converter v2.3"
- Updated version string in window title
- Recompiled `StlTo3mfConverter.exe`
- Rebuilt MSIX packages in `microsoft upload files 2\` folder
- Folder `microsoft upload files 2\` is a copy of original with updated EXE + MSIX packages
- Version bumped from 2.3.0.0 to 2.5.0.0

### Microsoft Store Policy Compliance
- Ko-fi link is a **voluntary donation** with no in-app benefits — compliant under policy 10.8.2
- The app remains free with no features gated behind donations
