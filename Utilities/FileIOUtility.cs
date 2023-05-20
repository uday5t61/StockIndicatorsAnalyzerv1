using Newtonsoft.Json;
using StockIndicatorsAnalyzer.Models;
using System.IO.Compression;

namespace StockIndicatorsAnalyzer.Utilities
{
    public class FileIOUtility
    {
        public static void UnZipFiles(string zipPath, string destFolder, bool overWrite)
        {
            ZipFile.ExtractToDirectory(
            zipPath,
            destFolder,
            overwriteFiles: overWrite
            );
        }

        public static async Task<string> CsvToJsonStringAsync(string filePath)
        {
            var csv = new List<string[]>();
            var lines = await System.IO.File.ReadAllLinesAsync(filePath); // csv file location
            // loop through all lines and add it in list as string
            foreach (string line in lines)
                csv.Add(line.Split(','));

            //split string to get first line, header line as JSON properties
            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            //loop all remaining lines, except header so starting it from 1
            // instead of 0
            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                objResult.Add(properties[j], csv[i][j]);
                listObjResult.Add(objResult);
            }
            // convert dictionary into JSON
            var jsonString = JsonConvert.SerializeObject(listObjResult);

            return jsonString;
    
        }

        public static async Task WriteToFileAsync(string data, string filePath)
        {            
            await System.IO.File.WriteAllTextAsync(filePath, data);
        }
    }
}
