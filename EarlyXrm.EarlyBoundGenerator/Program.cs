using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace EarlyXrm.EarlyBoundGenerator
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = new FileInfo("earlybound.json");

            if (!configFile.Exists)
                throw new ApplicationException("Cannot find config file (\"earlybound.json\")!");

            var settings = File.ReadAllText(configFile.FullName);

            var earlyBoundConfig = JsonConvert.DeserializeObject<EarlyBoundConfig>(settings);

            var parameters = new Dictionary<string,string>();

            var solutions = string.Join(";", earlyBoundConfig.Solutions);

            if (earlyBoundConfig.ConnectionString != null)
            {
                parameters.Add($"connectionstring", $"\"{earlyBoundConfig.ConnectionString}\"");
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

                parameters.Add($"interactivelogin", null);
            }
            
            parameters.Add("solutionname", solutions);

            parameters.Add("namespace", earlyBoundConfig.Namespace);

            var extra = earlyBoundConfig.Include;

            if (extra != null)
                parameters.Add("extra", string.Join(";", extra.Select(x => x.Key + (x.Value?.Any() == true ? ":" + string.Join(",", x.Value) : ""))));
            else
                parameters.Add("extra", null);

            var skip = earlyBoundConfig.Exclude;
            if (skip != null)
                parameters.Add("skip", string.Join(";", skip.Select(x => x.Key + (x.Value?.Any() == true ? ":" + string.Join(",", x.Value) : ""))));
            else
                parameters.Add("skip", null);

            parameters.Add("usedisplaynames", earlyBoundConfig.UseDisplayNames.ToString().ToLower());
            parameters.Add("debugMode", earlyBoundConfig.DebugMode.ToString().ToLower());
            parameters.Add("instrument", earlyBoundConfig.Instrument.ToString().ToLower());
            parameters.Add("addsetters", earlyBoundConfig.AddSetters.ToString().ToLower());

            earlyBoundConfig.Out = @"..\..\" + earlyBoundConfig.Out;

            var outDir = Path.GetDirectoryName(earlyBoundConfig.Out);
            if (Directory.Exists(outDir) == false)
                Directory.CreateDirectory(outDir);

            parameters.Add("out", earlyBoundConfig.Out);

            parameters.Add("nestnonglobalenums", earlyBoundConfig.NestNonGlobalEnums.ToString().ToLower());
            parameters.Add("generateconstants", earlyBoundConfig.GenerateConstants.ToString().ToLower());

            parameters.Add("codewriterfilter", $"{typeof(CodeFilteringService).AssemblyQualifiedName}");
            parameters.Add("codecustomization", $"{typeof(CodeCustomistationService).AssemblyQualifiedName}");
            parameters.Add("namingservice", $"{typeof(CodeNamingService).AssemblyQualifiedName}");

            var arguments = parameters.Select(x => $"/{x.Key}{(x.Value == null ? string.Empty : $":\"{x.Value}\"")}");

            var path = configFile.DirectoryName.EndsWith(@"\bin\Debug") ? "" : @"..\bin\coretools\";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = path,
                    FileName = path + "CrmSvcUtil.exe",
                    Arguments = string.Join(" ", arguments),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
            process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            Console.WriteLine($"Start {DateTime.Now}!");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            Console.WriteLine($"Complete {DateTime.Now}!");

            Console.Read();
        }
    }
}