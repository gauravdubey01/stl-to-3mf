using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace StlTo3mf
{
    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
    }

    public class StlResult
    {
        public Vector3[] Vertices { get; set; } = Array.Empty<Vector3>();
        public int[][] Triangles { get; set; } = Array.Empty<int[]>();
    }

    public class StlParser
    {
        public StlResult Parse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                if (IsBinaryStl(stream))
                    return ParseBinary(stream);
                else
                    return ParseAscii(stream);
            }
        }

        private bool IsBinaryStl(Stream stream)
        {
            if (stream.Length < 84) return false;
            stream.Position = 80;
            byte[] buf = new byte[4];
            stream.Read(buf, 0, 4);
            uint count = BitConverter.ToUInt32(buf, 0);
            stream.Position = 0;
            return stream.Length == 84 + count * 50;
        }

        private StlResult ParseAscii(Stream stream)
        {
            var reader = new StreamReader(stream);
            var rawVerts = new List<float[]>();
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        rawVerts.Add(new[] {
                            ParseFloat(parts[1]),
                            ParseFloat(parts[2]),
                            ParseFloat(parts[3])
                        });
                    }
                }
            }

            return Deduplicate(rawVerts);
        }

        private StlResult ParseBinary(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                reader.ReadBytes(80);
                uint triCount = reader.ReadUInt32();
                var rawVerts = new List<float[]>((int)triCount * 3);

                for (int t = 0; t < triCount; t++)
                {
                    reader.ReadBytes(12);
                    for (int j = 0; j < 3; j++)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                        rawVerts.Add(new[] { x, y, z });
                    }
                    reader.ReadUInt16();
                }

                return Deduplicate(rawVerts);
            }
        }

        private StlResult Deduplicate(List<float[]> rawVerts)
        {
            int triCount = rawVerts.Count / 3;
            var triangles = new int[triCount][];
            var dedup = new Dictionary<string, int>(rawVerts.Count);
            var uniqueVerts = new List<Vector3>(rawVerts.Count);

            for (int t = 0; t < triCount; t++)
            {
                triangles[t] = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    float[] v = rawVerts[t * 3 + j];
                    string key = string.Format(CultureInfo.InvariantCulture, "{0:F6},{1:F6},{2:F6}", v[0], v[1], v[2]);
                    if (!dedup.TryGetValue(key, out int idx))
                    {
                        idx = uniqueVerts.Count;
                        dedup[key] = idx;
                        uniqueVerts.Add(new Vector3(v[0], v[1], v[2]));
                    }
                    triangles[t][j] = idx;
                }
            }

            return new StlResult { Vertices = uniqueVerts.ToArray(), Triangles = triangles };
        }

        private float ParseFloat(string s)
        {
            return float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
