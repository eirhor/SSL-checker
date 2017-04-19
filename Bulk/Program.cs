using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SSLLabsApiWrapper;
using SSLLabsApiWrapper.Models.Response;
using System.IO;
using Bulk.Models;

namespace Bulk
{
    internal class Program
    {
        private static void Main()
        {
            const string apiUrl = "https://api.ssllabs.com/api/v2";
            const string inputFileName = "sites.txt";
            const string outputFileName = "response.csv";
            const string delimiter = ";";
            var sslService = new SSLLabsApiService(apiUrl);
            var serviceStatus = sslService.Info();

            if (serviceStatus.Online)
            {
                if (File.Exists(inputFileName))
                {
                    var fileContent = File.ReadAllText(inputFileName, Encoding.UTF8);
                    var sites = fileContent.Split(delimiter.First());

                    var output = new List<string[]>();
                    foreach (var site in sites)
                    {
                        Console.WriteLine($"Started analyzing {site}");
                        var analysisResult = sslService.AutomaticAnalyze(site);
                        var response = new Response
                        {
                            SiteUrl = site,
                            Grade = analysisResult.endpoints != null && analysisResult.endpoints.Any() ? analysisResult.endpoints.First().grade : "FAILED"
                        };

                        output.Add(new[] {response.SiteUrl, response.Grade});
                        Console.WriteLine($"Finished analyzing {response.SiteUrl} with grade: {response.Grade}");
                    }

                    if (File.Exists(outputFileName))
                        File.Delete(outputFileName);

                    Console.WriteLine("Generating CSV");
                    using (TextWriter writer = File.CreateText(outputFileName))
                    {
                        foreach (var result in output)
                        {
                            writer.WriteLine(string.Join(delimiter, result));
                        }
                    }
                    Console.WriteLine("CSV generated.");
                    Console.WriteLine("Press any key to exit.");
                    System.Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("ERROR: Could not find sites.txt file.");
                    Console.WriteLine("Press any key to exit.");
                    System.Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("ERROR: SSL Labs API is unreachable.");
                Console.WriteLine("Press any key to exit.");
                System.Console.ReadKey();
            }
        }
    }
}
