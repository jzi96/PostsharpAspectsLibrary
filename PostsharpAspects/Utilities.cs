using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zieschang.Net.Projects.PostsharpAspects
{
    public static class Utilities
    {
        public static string GetContractNamespace(Type t)
        {
            var namespaceParts = t.Namespace.Split('.');
            var sb = new StringBuilder();
            var loopStart = 0;
            if (namespaceParts.Length > 2)
            {
                loopStart = 2;
                sb.Append("contracts://");
                sb.Append(namespaceParts[1].ToLowerInvariant())
                    .Append(".")
                    .Append(namespaceParts[0].ToLowerInvariant())
                    .Append("/");
            }
            for (int idx = loopStart; idx < namespaceParts.Length; idx++)
            {
                sb.Append(namespaceParts[idx])
                    .Append("/");
            }
            var version = t.Assembly.GetName().Version.ToString();
            if (version == "0.0.0.0")
            {
                var versionAttribute = t.Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
                if (versionAttribute != null && versionAttribute.Length > 0)
                {
                    var v = (AssemblyVersionAttribute)versionAttribute[0];
                    version = v.Version;
                }
            }
            version = version.Substring(0, version.IndexOf('.', version.IndexOf('.') + 1));
            sb.Append(version)
                .Append("/");
            var ns = sb.ToString();
            return ns;
        }
    }
}
