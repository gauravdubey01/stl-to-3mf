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

### Naming convention
- Folder: `v{major}.{minor}` (e.g. `v2.1`, `v2.2`, `v3.0`)
- Files: `{Name}_v{major}.{minor}.{ext}` (e.g. `StlTo3mfConverter_v2.1.cs`)
- The `version2` folder always holds the latest working copy with original filenames
