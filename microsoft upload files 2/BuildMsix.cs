using System;
using System.IO;
using System.IO.Compression;
using System.Text;

class BuildMsix
{
    static string SrcDir = @"E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\microsoft upload files 2";
    static string AppName = "StlTo3mfConverter";
    static string Version = "2.5.0.0";

    static void Main()
    {
        string msixPath = Path.Combine(SrcDir, AppName + "-" + Version + ".msix");
        string uploadPath = Path.Combine(SrcDir, AppName + "-" + Version + ".msixupload");
        if (File.Exists(msixPath)) File.Delete(msixPath);

        using (var fs = new FileStream(msixPath, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            AddStoreEntry(bw, "[Content_Types].xml", Encoding.UTF8.GetBytes(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "  <Default Extension=\"exe\" ContentType=\"application/x-msdownload\" />" +
                "  <Default Extension=\"png\" ContentType=\"image/png\" />" +
                "  <Default Extension=\"xml\" ContentType=\"application/xml\" />" +
                "  <Override PartName=\"/AppxManifest.xml\" ContentType=\"application/vnd.ms-appx.manifest+xml\" />" +
                "</Types>"
            ));
            AddStoreEntryFile(bw, "AppxManifest.xml", Path.Combine(SrcDir, "AppxManifest.xml"));
            AddStoreEntryFile(bw, "StlTo3mfConverter.exe", Path.Combine(SrcDir, "StlTo3mfConverter.exe"));
            foreach (var f in Directory.GetFiles(Path.Combine(SrcDir, "assets")))
                AddStoreEntryFile(bw, "assets/" + Path.GetFileName(f), f);
            WriteCentralDir(bw);
        }

        long size = new FileInfo(msixPath).Length;
        Console.WriteLine("MSIX: " + msixPath + " (" + size + " bytes)");

        if (File.Exists(uploadPath)) File.Delete(uploadPath);
        using (var u = ZipFile.Open(uploadPath, ZipArchiveMode.Create))
            ZipFileExtensions.CreateEntryFromFile(u, msixPath, Path.GetFileName(msixPath), CompressionLevel.Optimal);
        Console.WriteLine("Upload: " + uploadPath);
    }

    static System.Collections.Generic.List<byte[]> _cd = new System.Collections.Generic.List<byte[]>();
    static int _offset = 0;

    static void AddStoreEntry(BinaryWriter bw, string name, byte[] raw)
    {
        uint crc = Crc32(raw);
        ushort nl = (ushort)Encoding.UTF8.GetByteCount(name);
        bw.Write((uint)0x04034b50); bw.Write((ushort)20); bw.Write((ushort)0);
        bw.Write((ushort)0); bw.Write((ushort)0); bw.Write((ushort)0);
        bw.Write(crc); bw.Write((uint)raw.Length); bw.Write((uint)raw.Length);
        bw.Write(nl); bw.Write((ushort)0);
        bw.Write(Encoding.UTF8.GetBytes(name)); bw.Write(raw);

        var m = new MemoryStream(); var w = new BinaryWriter(m);
        w.Write((uint)0x02014b50); w.Write((ushort)20); w.Write((ushort)20);
        w.Write((ushort)0); w.Write((ushort)0);
        w.Write((ushort)0); w.Write((ushort)0);
        w.Write(crc); w.Write((uint)raw.Length); w.Write((uint)raw.Length);
        w.Write(nl); w.Write((ushort)0); w.Write((ushort)0);
        w.Write((ushort)0); w.Write((ushort)0); w.Write((uint)0);
        w.Write((uint)_offset); w.Write(Encoding.UTF8.GetBytes(name));
        _cd.Add(m.ToArray()); w.Close();
        _offset = (int)bw.BaseStream.Position;
        Console.WriteLine("  " + name);
    }

    static void AddStoreEntryFile(BinaryWriter bw, string name, string path)
    { AddStoreEntry(bw, name, File.ReadAllBytes(path)); }

    static void WriteCentralDir(BinaryWriter bw)
    {
        long cdOffset = bw.BaseStream.Position;
        foreach (var cd in _cd) bw.Write(cd);
        long cdSize = bw.BaseStream.Position - cdOffset;
        int count = _cd.Count;
        bw.Write((uint)0x06054b50);
        bw.Write((ushort)0); bw.Write((ushort)0);
        bw.Write((ushort)count); bw.Write((ushort)count);
        bw.Write((uint)cdSize); bw.Write((uint)cdOffset);
        bw.Write((ushort)0);
    }

    static uint Crc32(byte[] d)
    { uint c = 0xFFFFFFFF; foreach (byte b in d) { c ^= b; for (int i = 0; i < 8; i++) c = (c >> 1) ^ ((c & 1) * 0xEDB88320); } return c ^ 0xFFFFFFFF; }
}
