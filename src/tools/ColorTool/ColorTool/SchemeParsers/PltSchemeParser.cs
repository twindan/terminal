//
// Copyright (C) Microsoft.  All rights reserved.
// Licensed under the terms described in the LICENSE file in the root of this project.
//

using System;
using System.IO;
using System.Linq;

namespace ColorTool.SchemeParsers
{
    class PltSchemeParser : SchemeParserBase
    {
        public override string Name { get; } = "PLT File Parser";

        public override string FileExtension => ".plt";

        private static uint ReadSingle(FileStream stream)
        {
            var colorFromFile = (uint)stream.ReadByte();

            // the files should have 6 bits (0..0x3f), but some files have out of bounds data
            colorFromFile = Math.Min(colorFromFile, 0x3f);

            // scale it from 6 bits to 8 bits
            colorFromFile = (colorFromFile << 2) | ((colorFromFile >> 4) & 0x3);

            // done
            return colorFromFile;
        }

        public override ColorScheme ParseScheme(string schemeName, SchemeParseOptions options, bool reportErrors = false)
        {
            var filename = FindScheme(schemeName, out schemeName);
            if (filename == null) return null;

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                const int ColorTableSize = 16;

                // the file must be exactly 64 bytes (16 x 4 bytes)
                const int ExpectedFileSize = ColorTableSize * 4;
                if (fileStream.Length != ExpectedFileSize)
                {
                    if ( reportErrors )
                        Console.WriteLine("Invalid PLT file");
                    return null;
                }

                // read the data from the file
                var colorTable = new uint[ColorTableSize];
                for (var index = 0; index < colorTable.Length; ++index)
                {
                    // read the raw colors
                    var red = ReadSingle(fileStream);
                    var green = ReadSingle(fileStream);
                    var blue = ReadSingle(fileStream);
                    fileStream.ReadByte();      // ignore the alpha

                    // construct the entry
                    colorTable[index] = red | (green << 8) | (blue << 16);
                }

                // set the default attributes
                Console.WriteLine("attributes: {0}/{1}", options.BackgroundColorIndex, options.ForegroundColorIndex);
                var attributes = new ConsoleAttributes(colorTable[options.BackgroundColorIndex],
                                                       colorTable[options.ForegroundColorIndex],
                                                       null, null);

                // construct the scheme
                return new ColorScheme(schemeName, colorTable, attributes);
            }
        }
    }
}
