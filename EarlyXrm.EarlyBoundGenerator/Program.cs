using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace EarlyXrm.EarlyBoundGenerator
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = new FileInfo("earlybound.json");

            if (!configFile.Exists)
                throw new ApplicationException("Cannot find config file!");

            var settings = File.ReadAllText(configFile.FullName);

            var earlyBoundConfig = JsonConvert.DeserializeObject<EarlyBoundConfig>(settings);

            var parameters = new List<string>();

            var solutions = string.Join(";", earlyBoundConfig.Solutions);

            if (earlyBoundConfig.ConnectionString != null)
            {
                parameters.Add($"/connectionstring:\"{earlyBoundConfig.ConnectionString}\"");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(solutions))
                {
                    Console.WriteLine($"To filter generation results by one or more solutions, the \"ConnectionString\" setting in the \"earlybound.json\" file must be populated:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($@"
    {{
        ...
        ""Solutions"": [""{string.Join("\", \"", earlyBoundConfig.Solutions)}""],
        ""ConnectionString"": ""<!--POPULATE-->"",
        ...
    }}");
                    Console.Read();
                    return;
                }

                parameters.Add($"/interactivelogin");
            }
            
            parameters.Add(solutions);

            parameters.Add(earlyBoundConfig.Namespace);

            var extra = earlyBoundConfig.Include;

            if (extra != null)
                parameters.Add(string.Join(";", extra.Select(x => x.Key + (x.Value?.Any() == true ? ":" + string.Join(",", x.Value) : ""))));
            else
                parameters.Add(null);

            var skip = earlyBoundConfig.Exclude;
            if (skip != null)
                parameters.Add(string.Join(";", skip.Select(x => x.Key + (x.Value?.Any() == true ? ":" + string.Join(",", x.Value) : ""))));
            else
                parameters.Add(null);

            parameters.Add(earlyBoundConfig.UseDisplayNames.ToString().ToLower());
            parameters.Add(earlyBoundConfig.DebugMode.ToString().ToLower());
            parameters.Add(earlyBoundConfig.Instrument.ToString().ToLower());
            parameters.Add(earlyBoundConfig.AddSetters.ToString().ToLower());

            if (configFile.DirectoryName.EndsWith(@"\bin\Debug"))
            {
                earlyBoundConfig.Out = @"..\..\" + earlyBoundConfig.Out;
            }

            var outDir = Path.GetDirectoryName(earlyBoundConfig.Out);
            if (Directory.Exists(outDir) == false)
                Directory.CreateDirectory(outDir);

            parameters.Add(earlyBoundConfig.Out);

            parameters = parameters.Select(x => x.StartsWith("/") ? x : $"\"{x}\"").ToList();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "generate.bat",
                    Arguments = string.Join(" ", parameters),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Console.WriteLine("Complete!");

            Console.Read();
        }
    }
}