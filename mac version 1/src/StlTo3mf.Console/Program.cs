using System;
using StlTo3mf;

namespace StlTo3mf.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: StlTo3mf.Console <input.stl> <output.3mf>");
                return;
            }

            string inputPath = args[0];
            string outputPath = args[1];

            try
            {
                System.Console.WriteLine($"Converting {inputPath} to {outputPath}...");
                
                var parser = new StlParser();
                var result = parser.Parse(inputPath);
                
                var writer = new ThreeMfWriter();
                writer.Write(outputPath, result.Vertices, result.Triangles);
                
                System.Console.WriteLine("Conversion successful!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
