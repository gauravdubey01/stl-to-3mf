# STL to 3MF Converter (Cross-Platform)

This project has been refactored for cross-platform compatibility (macOS, Linux, Windows) using .NET 8.

## Structure
- `src/StlTo3mf.Core`: Core conversion logic (class library).
- `src/StlTo3mf.Console`: Simple command-line interface for the converter.

## How to Build on macOS
1. Install the .NET SDK from https://dotnet.microsoft.com/download.
2. Open a terminal and navigate to `mac version 1/src`.
3. Run the following command to build the project:
   ```bash
   dotnet build
   ```
4. Run the converter:
   ```bash
   dotnet run --project StlTo3mf.Console/StlTo3mf.Console.csproj -- <input.stl> <output.3mf>
   ```
