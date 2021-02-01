//
// Copyright (C) Microsoft.  All rights reserved.
// Licensed under the terms described in the LICENSE file in the root of this project.
//

namespace ColorTool.SchemeParsers
{
    public class SchemeParseOptions
    {
        public SchemeParseOptions()
        {
            // empty
        }

        public int BackgroundColorIndex { get; set; } = 0;
        public int ForegroundColorIndex { get; set; } = 7;

    }
}
