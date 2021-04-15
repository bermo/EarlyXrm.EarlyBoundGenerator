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
                throw new ApplicationException("Cannot find config file!");

            var settings = File.ReadAllText(configFile.FullName);

            var earlyBoundConfig = JsonConvert.DeserializeObject<EarlyBoundConfig>(settings);

            var parameters = new List<string>();
            parameters.Add(earlyBoundConfig.ConnectionString);
            parameters.Add(string.Join("; ", earlyBoundConfig.Solutions));
            parameters.Add(earlyBoundConfig.Namespace);

            var extra = earlyBoundConfig.Include;

            if (extra != null)
                parameters.Add(string.Join("; ", extra.Select(x => x.Key + (x.Value?.Any() == true ? ": " + string.Join(", ", x.Value) : ""))));
            else
                parameters.Add("");

            var skip = earlyBoundConfig.Exclude;
            if (skip != null)
                parameters.Add(string.Join("; ", skip.Select(x => x.Key + (x.Value?.Any() == true ? ": " + string.Join(", ", x.Value) : ""))));
            else
                parameters.Add("");

            parameters.Add(earlyBoundConfig.UseDisplayNames.ToString());
            parameters.Add(earlyBoundConfig.DebugMode.ToString());
            parameters.Add(earlyBoundConfig.Instrument.ToString());
            parameters.Add(earlyBoundConfig.AddSetters.ToString());

            if (configFile.DirectoryName.EndsWith(@"\bin\Debug"))
            {
                earlyBoundConfig.EntitiesOut = @"..\..\" + earlyBoundConfig.EntitiesOut;
                earlyBoundConfig.OptionSetsOut = @"..\..\" + earlyBoundConfig.OptionSetsOut;
            }

            parameters.Add(earlyBoundConfig.EntitiesOut);
            parameters.Add(earlyBoundConfig.OptionSetsOut);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "generate.bat",
                    Arguments = "\"" + string.Join("\";\"", parameters) + "\"",
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