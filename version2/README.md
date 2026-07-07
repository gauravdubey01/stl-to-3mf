# STL to 3MF Converter

A fast, self-contained Windows desktop app that converts **STL** files (ASCII and Binary) to the modern **3MF** format — no external tools, no dependencies.

## Features

### 🔄 Batch Conversion
- Convert entire folders recursively — preserves subfolder structure
- Process hundreds of files in one click
- Async background processing keeps the UI responsive
- Real-time progress bar with per-file status and ETA

### 📐 Smart STL Parsing
- Automatic detection of ASCII vs Binary STL formats
- Robust binary parser with proper byte-level reading
- ASCII parser handles standard and non-standard formatting
- Vertex deduplication — shared vertices reduce output file size

### 🎨 Modern UI
- Dark and Light themes with one-click toggle
- Rounded corners on all controls
- Windows 11 native rounded window corners (DWM)
- Resizable window with minimum size 800×560
- Flat, modern buttons with hover effects

### 📁 Convenience
- **Drag & drop** — drag any folder onto the app
- **Modern folder picker** — native Windows File Explorer-style dialog
- **Custom output folder** — choose where 3MF files are saved
- **Auto path** — output defaults to `{source}/3mf_output`
- **Open Output** button — opens the converted files folder in Explorer

### 📦 Output
- Standard-compliant 3MF files (ZIP + OPC XML)
- Valid namespace: `http://schemas.microsoft.com/3dmanufacturing/core/2015/02`
- Ready for 3D printing slicers and CAD software

### 🚀 No Dependencies
- Pure .NET Windows Forms application
- No PrusaSlicer, no Python, no external runtimes
- Single portable `.exe` or install via the setup wizard
- Compiled for .NET Framework 4.8 (included with Windows 10/11)

## Downloads

| File | Description |
|---|---|
| `StlTo3mfConverter_v2.2.exe` | Portable executable — download and run |
| `StlTo3mfConverter_Setup_v2.2.exe` | Inno Setup installer — adds Start Menu/Desktop shortcuts |

## Usage

1. **Select STL folder** — browse or drag-drop a folder containing `.stl` files
2. **Choose output** — defaults to `{source}/3mf_output`, change if needed
3. **Convert All** — starts batch conversion with live progress
4. **Open Output** — click to open the output folder once done

## Build from Source

```
csc /target:winexe /out:StlTo3mfConverter.exe ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Xml.Linq.dll ^
    /reference:System.IO.Compression.dll ^
    StlTo3mfConverter.cs StlParser.cs ThreeMfWriter.cs
```

Requires: .NET Framework 4.8 SDK / Visual Studio Build Tools
