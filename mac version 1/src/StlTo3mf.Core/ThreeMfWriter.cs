using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace StlTo3mf
{
    public class ThreeMfWriter
    {
        private static readonly XNamespace ModelNs = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
        private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/package/2006/relationships";
        private static readonly XNamespace ContentTypesNs = "http://schemas.openxmlformats.org/package/2006/content-types";

        public void Write(string outputPath, Vector3[] vertices, int[][] triangles)
        {
            using (var stream = File.Create(outputPath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                WriteContentTypes(archive);
                WriteRels(archive);
                WriteModel(archive, vertices, triangles);
            }
        }

        private void WriteContentTypes(ZipArchive archive)
        {
            var entry = archive.CreateEntry("[Content_Types].xml");
            using (var w = new StreamWriter(entry.Open()))
            {
                new XDocument(
                    new XElement(ContentTypesNs + "Types",
                        new XElement(ContentTypesNs + "Default",
                            new XAttribute("Extension", "rels"),
                            new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                        new XElement(ContentTypesNs + "Default",
                            new XAttribute("Extension", "model"),
                            new XAttribute("ContentType", "application/vnd.ms-package.3dmanufacturing-3dmodel+xml"))
                    )
                ).Save(w);
            }
        }

        private void WriteRels(ZipArchive archive)
        {
            var entry = archive.CreateEntry("_rels/.rels");
            using (var w = new StreamWriter(entry.Open()))
            {
                new XDocument(
                    new XElement(RelNs + "Relationships",
                        new XElement(RelNs + "Relationship",
                            new XAttribute("Target", "/3D/3dmodel.model"),
                            new XAttribute("Id", "rel0"),
                            new XAttribute("Type", "http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel"))
                    )
                ).Save(w);
            }
        }

        private void WriteModel(ZipArchive archive, Vector3[] vertices, int[][] triangles)
        {
            var entry = archive.CreateEntry("3D/3dmodel.model");
            using (var w = new StreamWriter(entry.Open()))
            {
                var vertElements = new XElement[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertElements[i] = new XElement(ModelNs + "vertex",
                        new XAttribute("x", FormatFloat(vertices[i].X)),
                        new XAttribute("y", FormatFloat(vertices[i].Y)),
                        new XAttribute("z", FormatFloat(vertices[i].Z)));
                }

                var triElements = new XElement[triangles.Length];
                for (int i = 0; i < triangles.Length; i++)
                {
                    triElements[i] = new XElement(ModelNs + "triangle",
                        new XAttribute("v1", triangles[i][0]),
                        new XAttribute("v2", triangles[i][1]),
                        new XAttribute("v3", triangles[i][2]));
                }

                new XDocument(
                    new XElement(ModelNs + "model",
                        new XAttribute("unit", "millimeter"),
                        new XElement(ModelNs + "resources",
                            new XElement(ModelNs + "object",
                                new XAttribute("id", "1"),
                                new XAttribute("type", "model"),
                                new XElement(ModelNs + "mesh",
                                    new XElement(ModelNs + "vertices", vertElements),
                                    new XElement(ModelNs + "triangles", triElements)))),
                        new XElement(ModelNs + "build",
                            new XElement(ModelNs + "item",
                                new XAttribute("objectid", "1")))
                    )
                ).Save(w);
            }
        }

        private string FormatFloat(float value)
        {
            return value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
