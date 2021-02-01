//
// Copyright (C) Microsoft.  All rights reserved.
// Licensed under the terms described in the LICENSE file in the root of this project.
//

using ColorTool.ConsoleTargets;
using ColorTool.SchemeWriters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorTool
{
    static class Program
    {
        private static bool quietMode = false;
        private static bool reportErrors = false;
        private static bool setDefaults = false;
        private static bool setProperties = true;
        private static bool setUnixStyle = false;
        private static bool setTerminalStyle = false;
        private static SchemeParsers.SchemeParseOptions schemeParserOptions = new SchemeParsers.SchemeParseOptions();

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            string schemeName = null;
            int i = 0;
            while(i < args.Length)
            {
                string arg = args[i++];
                switch (arg)
                {
                    case "-?":
                    case "--help":
                        Usage();
                        return;
                    case "-c":
                    case "--current":
                        ColorTable.PrintTable();
                        return;
                    case "-l":
                    case "--location":
                        SchemeManager.PrintSchemesDirectory();
                        return;
                    case "-o":
                    case "--output":
                        if (i < args.Length)
                        {
                            new IniSchemeWriter().ExportCurrentAsIni(args[i++]);
                        }
                        else
                        {
                            OutputUsage();
                        }
                        return;
                    case "-s":
                    case "--schemes":
                        SchemeManager.PrintSchemes();
                        return;
                    case "-v":
                    case "--version":
                        Version();
                        return;
                    case "-e":
                    case "--errors":
                        reportErrors     = true;
                        break;
                    case "-q":
                    case "--quiet":
                        quietMode        = true;
                        break;
                    case "-d":
                    case "--defaults":
                        setDefaults      = true;
                        setProperties    = false;
                        break;
                    case "-b":
                    case "--both":
                        setDefaults      = true;
                        setProperties    = true;
                        break;
                    case "-x":
                    case "--xterm":
                        setUnixStyle     = true;
                        setProperties    = true;
                        break;
                    case "-t":
                    case "--terminal":
                        setTerminalStyle = true;
                        setProperties    = true;
                        break;
                    case "-f":
                    case "--foreground":
                        if (i < args.Length)
                        {
                            schemeParserOptions.ForegroundColorIndex = int.Parse(args[i++]);
                        }
                        else
                        {
                            OutputUsage();
                            return;
                        }
                        break;
                    case "-k":
                    case "--background":
                        if (i < args.Length)
                        {
                            schemeParserOptions.BackgroundColorIndex = int.Parse(args[i++]);
                        }
                        else
                        {
                            OutputUsage();
                            return;
                        }
                        break;
                    case "--hex-color":
                        if (i < args.Length)
                        {
                            var color = int.Parse(args[i++], System.Globalization.NumberStyles.HexNumber);
                            schemeParserOptions.BackgroundColorIndex = (color >> 4) & 0xf;
                            schemeParserOptions.ForegroundColorIndex = color & 0xf;
                        }
                        else
                        {
                            OutputUsage();
                            return;
                        }
                        break;

                    default:
                        if (arg.StartsWith("-") || schemeName != null)
                        {
                            OutputUsage();
                            return;
                        }
                        else
                        {
                            schemeName = arg;
                        }
                        break;
                }
            }

            if (schemeName == null)
            {
                OutputUsage();
                return;
            }

            ColorScheme colorScheme = SchemeManager.GetScheme(schemeName, schemeParserOptions, reportErrors);

            if (colorScheme == null)
            {
                Console.WriteLine(string.Format(Resources.SchemeNotFound, schemeName));
                return;
            }

            foreach (var target in GetConsoleTargets())
            {
                target.ApplyColorScheme(colorScheme, quietMode);
            }
        }

        private static void Usage()
        {
            Console.WriteLine(Resources.Usage,
                string.Join($"{Environment.NewLine}  ", SchemeManager.GetParsers().Select(p => p.Name)));
        }

        private static void OutputUsage()
        {
            Console.WriteLine(Resources.OutputUsage);
        }

        private static void Version()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine($"ColorTool v{info.FileVersion}");
        }

        /// <summary>
        /// Returns an enumerable of consoles that we want to apply the color scheme to.
        /// The contents of this enumerable depends on the user's provided command line flags.
        /// </summary>
        private static IEnumerable<IConsoleTarget> GetConsoleTargets()
        {
            if (setDefaults)
            {
                yield return new DefaultConsoleTarget();
            }
            if (setProperties)
            {
                if (setUnixStyle)
                {
                    yield return new VirtualTerminalConsoleTarget();
                }
                else if (setTerminalStyle)
                {
                    yield return new TerminalSchemeConsoleTarget();
                }
                else
                {
                    yield return new CurrentConsoleTarget();
                }
            }
        }
    }
}
