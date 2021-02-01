﻿//
// Copyright (C) Microsoft.  All rights reserved.
// Licensed under the terms described in the LICENSE file in the root of this project.
//

using ColorTool.SchemeParsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ColorTool.ConsoleAPI;

namespace ColorTool
{
    static class SchemeManager
    {
        public static IEnumerable<string> GetSearchPaths(string schemeName, string extension)
        {
            // Search order, for argument "name", where 'exe' is the dir of the exe.
            //  1. ./name
            //  2. ./name.ext
            //  3. ./schemes/name
            //  4. ./schemes/name.ext
            //  5. exe/schemes/name
            //  6. exe/schemes/name.ext
            //  7. name (as an absolute path)

            string cwd = "./";
            yield return cwd + schemeName;

            string filename = schemeName + extension;
            yield return cwd + filename;

            string cwdSchemes = "./schemes/";
            yield return cwdSchemes + schemeName;
            yield return cwdSchemes + filename;

            string exeDir = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
            string exeSchemes = exeDir + "/schemes/";
            yield return exeSchemes + schemeName;
            yield return exeSchemes + filename;
            yield return schemeName;
        }

        public static void PrintSchemesDirectory()
        {
            string schemeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schemes");
            Console.WriteLine(schemeDirectory);
        }

        public static void PrintSchemes()
        {
            var schemeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schemes");

            if (Directory.Exists(schemeDirectory))
            {
                IntPtr handle = GetStdOutputHandle();
                GetConsoleMode(handle, out var mode);
                SetConsoleMode(handle, mode | 0x4);

                int consoleWidth = Console.WindowWidth;
                string fgText = " gYw ";
                foreach (string schemeName in Directory.GetFiles(schemeDirectory).Select(Path.GetFileName))
                {
                    ColorScheme colorScheme = GetScheme(schemeName, new SchemeParseOptions(), false);
                    if (colorScheme != null)
                    {
                        string colors = string.Empty;
                        for (var index = 0; index < 8; index++)
                        {
                            var color = colorScheme[index];
                            // Set the background color to the color in the scheme, plus some text to show how it looks
                            colors += $"\x1b[48;2;{color.R};{color.G};{color.B}m{fgText}";
                        }
                        // Align scheme colors right, or on newline if it doesn't fit
                        int schemeTextLength = fgText.Length * 8;
                        int bufferLength = consoleWidth - (schemeName.Length + schemeTextLength);

                        string bufferString = bufferLength >= 0
                            ? new string(' ', bufferLength)
                            : "\n" + new string(' ', consoleWidth - schemeTextLength);

                        string outputString = schemeName + bufferString + colors;
                        Console.WriteLine(outputString);
                        Console.ResetColor();
                    }
                }
            }
        }

        public static ColorScheme GetScheme(string schemeName, SchemeParseOptions options, bool reportErrors = false)
        {
            IEnumerable<ISchemeParser> parsers = GetParsers();

            // if the user provided an extension, remove it and filter it
            var extension = Path.GetExtension(schemeName);
            if (!string.IsNullOrEmpty(extension))
            {
                // filter it to valid parsers
                parsers = parsers.Where(test => string.Compare(test.FileExtension, extension, true) == 0);

                // the extension is always at the end, so just remove it
                schemeName = schemeName.Substring(0, schemeName.Length - extension.Length);
            }

            return GetParsers()
                .Select(parser => parser.ParseScheme(schemeName, options, reportErrors))
                .FirstOrDefault(x => x != null);
        }

        public static IEnumerable<ISchemeParser> GetParsers()
        {
            return typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(ISchemeParser).IsAssignableFrom(t))
                .Select(t => (ISchemeParser)Activator.CreateInstance(t));
        }
    }
}
