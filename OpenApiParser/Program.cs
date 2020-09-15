using Microsoft.OpenApi.Readers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenApiParser
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            const string OpenApiSpecBasePath = @"C:\OpenAPI\";
            const string OutFileName = "outfile.csv";

            var specFiles = Directory.GetFiles(OpenApiSpecBasePath, "*", SearchOption.AllDirectories)
                .Where(f => (new[] { ".json", ".yaml" }).Contains(Path.GetExtension(f)));

            using var csvFile = File.CreateText(Path.Combine(OpenApiSpecBasePath, OutFileName));

            WriteHeaderRow(csvFile);

            foreach (var file in specFiles)
            {
                WriteRows(csvFile, file);
            }
        }

        private static void WriteRows(StreamWriter csvFile, string file)
        {
            var fileInfo = new FileInfo(file);

            using var stream = fileInfo.OpenRead();

            var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

            var apiInfo = openApiDocument.Paths
                .Select(path => new
                {
                    ProductName = fileInfo.Directory.Name,
                    SpecFile = fileInfo.Name,
                    Path = path.Key,
                })
                .Distinct();

            foreach (var api in apiInfo)
            {
                csvFile.Write($"{api.ProductName},");
                csvFile.Write($"{api.SpecFile},");
                csvFile.Write($"{api.Path},");
                csvFile.Write($"{openApiDocument.Info.Title},");
                csvFile.Write($"\"{openApiDocument.Info.Description}\",");
                csvFile.Write($"{openApiDocument.Info.Version},");
                csvFile.WriteLine();
            }
        }

        private static void WriteHeaderRow(StreamWriter csvFile)
        {
            csvFile.Write("Product Name,");
            csvFile.Write("Spec File,");
            csvFile.Write("Path,");
            csvFile.Write("Title,");
            csvFile.Write("Description,");
            csvFile.Write("Version,");
            csvFile.WriteLine();
        }
    }
}
