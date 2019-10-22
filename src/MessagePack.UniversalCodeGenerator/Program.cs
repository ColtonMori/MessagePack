﻿// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MessagePack.CodeGenerator.Generator;
using Mono.Options;

namespace MessagePack.CodeGenerator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var cmdArgs = new CommandlineArguments(args);
                if (!cmdArgs.IsParsed)
                {
                    return 0;
                }

                // Generator Start...
                var sw = Stopwatch.StartNew();
                Console.WriteLine("Project Compilation Start:" + cmdArgs.InputPath);

                var collector = new TypeCollector(cmdArgs.InputPath, cmdArgs.ConditionalSymbols, true, cmdArgs.IsUseMap);

                Console.WriteLine("Project Compilation Complete:" + sw.Elapsed.ToString());
                Console.WriteLine();

                sw.Restart();
                Console.WriteLine("Method Collect Start");

                (ObjectSerializationInfo[] objectInfo, EnumSerializationInfo[] enumInfo, GenericSerializationInfo[] genericInfo, UnionSerializationInfo[] unionInfo) = collector.Collect();

                Console.WriteLine("Method Collect Complete:" + sw.Elapsed.ToString());

                Console.WriteLine("Output Generation Start");
                sw.Restart();

                FormatterTemplate[] objectFormatterTemplates = objectInfo
                    .GroupBy(x => x.Namespace)
                    .Select(x => new FormatterTemplate()
                    {
                        Namespace = cmdArgs.GetNamespaceDot() + "Formatters" + ((x.Key == null) ? string.Empty : "." + x.Key),
                        ObjectSerializationInfos = x.ToArray(),
                    })
                    .ToArray();

                EnumTemplate[] enumFormatterTemplates = enumInfo
                    .GroupBy(x => x.Namespace)
                    .Select(x => new EnumTemplate()
                    {
                        Namespace = cmdArgs.GetNamespaceDot() + "Formatters" + ((x.Key == null) ? string.Empty : "." + x.Key),
                        EnumSerializationInfos = x.ToArray(),
                    })
                    .ToArray();

                UnionTemplate[] unionFormatterTemplates = unionInfo
                    .GroupBy(x => x.Namespace)
                    .Select(x => new UnionTemplate()
                    {
                        Namespace = cmdArgs.GetNamespaceDot() + "Formatters" + ((x.Key == null) ? string.Empty : "." + x.Key),
                        UnionSerializationInfos = x.ToArray(),
                    })
                    .ToArray();

                var resolverTemplate = new ResolverTemplate()
                {
                    Namespace = cmdArgs.GetNamespaceDot() + "Resolvers",
                    FormatterNamespace = cmdArgs.GetNamespaceDot() + "Formatters",
                    ResolverName = cmdArgs.ResolverName,
                    RegisterInfos = genericInfo.Cast<IResolverRegisterInfo>().Concat(enumInfo).Concat(unionInfo).Concat(objectInfo).ToArray(),
                };

                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated />");
                sb.AppendLine();
                sb.AppendLine(resolverTemplate.TransformText());

                foreach (EnumTemplate item in enumFormatterTemplates)
                {
                    var text = item.TransformText();
                    sb.AppendLine(text);
                }

                foreach (UnionTemplate item in unionFormatterTemplates)
                {
                    var text = item.TransformText();
                    sb.AppendLine(text);
                }

                foreach (FormatterTemplate item in objectFormatterTemplates)
                {
                    var text = item.TransformText();
                    sb.AppendLine(text);
                }

                Output(cmdArgs.OutputPath, sb.ToString());

                Console.WriteLine("String Generation Complete:" + sw.Elapsed.ToString());
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Error:" + ex);
                return 1;
            }
        }

        private static void Output(string path, string text)
        {
            path = path.Replace("global::", string.Empty);

            const string prefix = "[Out]";
            Console.WriteLine(prefix + path);

            var fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            System.IO.File.WriteAllText(path, text, Encoding.UTF8);
        }

        internal class CommandlineArguments
        {
            public string InputPath { get; private set; }

            public string OutputPath { get; private set; }

            public List<string> ConditionalSymbols { get; private set; }

            public string ResolverName { get; private set; }

            public string NamespaceRoot { get; private set; }

            public bool IsUseMap { get; private set; }

            public bool IsParsed { get; set; }

            public CommandlineArguments(string[] args)
            {
                this.ConditionalSymbols = new List<string>();
                this.NamespaceRoot = "MessagePack";
                this.ResolverName = "GeneratedResolver";
                this.IsUseMap = false;

                var option = new OptionSet()
            {
                { "i|input=", "[required]Input path of analyze csproj", x => { this.InputPath = x; } },
                { "o|output=", "[required]Output file path", x => { this.OutputPath = x; } },
                { "c|conditionalsymbol=", "[optional, default=empty]conditional compiler symbol", x => { this.ConditionalSymbols.AddRange(x.Split(',')); } },
                { "r|resolvername=", "[optional, default=GeneratedResolver]Set resolver name", x => { this.ResolverName = x; } },
                { "n|namespace=", "[optional, default=MessagePack]Set namespace root name", x => { this.NamespaceRoot = x; } },
                { "m|usemapmode", "[optional, default=false]Force use map mode serialization", x => { this.IsUseMap = true; } },
            };
                if (args.Length == 0)
                {
                    goto SHOW_HELP;
                }
                else
                {
                    option.Parse(args);

                    if (this.InputPath == null || this.OutputPath == null)
                    {
                        Console.WriteLine("Invalid Argument:" + string.Join(" ", args));
                        Console.WriteLine();
                        goto SHOW_HELP;
                    }

                    this.IsParsed = true;
                    return;
                }

SHOW_HELP:
                Console.WriteLine("mpc arguments help:");
                option.WriteOptionDescriptions(Console.Out);
                this.IsParsed = false;
            }

            public string GetNamespaceDot()
            {
                return string.IsNullOrWhiteSpace(this.NamespaceRoot) ? string.Empty : this.NamespaceRoot + ".";
            }
        }
    }
}
