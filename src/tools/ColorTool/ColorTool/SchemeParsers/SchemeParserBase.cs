using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTool.SchemeParsers
{
    internal abstract class SchemeParserBase : ISchemeParser
    {
        // ISchemeParser elements

        public abstract string Name { get; }

        public abstract ColorScheme ParseScheme(string schemeName, bool reportErrors = false);

        // Common elements and helpers
        public abstract string FileExtension { get; }

        protected string ExtractSchemeName(string schemeFileName) =>
            Path.ChangeExtension(schemeFileName, null);

        protected string FindScheme(string schemeName, out string realName)
        {
            // get the real name
            realName = Path.GetFileName(schemeName);

            // if we have a full path, then try that
            if (Path.IsPathRooted(schemeName))
            {
                var testFile = string.Concat(schemeName, FileExtension);
                return File.Exists(testFile) ? testFile : null;
            }
            else
            {
                return SchemeManager.GetSearchPaths(schemeName, FileExtension).FirstOrDefault(File.Exists);
            }
        }
    }
}

