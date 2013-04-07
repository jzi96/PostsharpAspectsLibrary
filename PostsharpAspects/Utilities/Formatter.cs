using System;
using System.Reflection;
using System.Text;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// Provides formatting string representing types, methods and fields. The
    /// formatting strings may contain arguments like <c>{0}</c> 
    /// filled at runtime with generic parameters or method arguments.
    /// </summary>
    internal static class Formatter
    {
        /// <summary>
        /// Gets a formatting string representing a <see cref="Type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A formatting string representing the type
        /// where each generic type argument is represented as a
        /// formatting argument (e.g. <c>Dictionary&lt;{0},P1}&gt;</c>.
        /// </returns>
        public static string GetTypeFormatString(Type type)
        {
            var stringBuilder = new StringBuilder();

            // Build the format string for the declaring type.

            stringBuilder.Append(type.FullName);

            if (type.IsGenericTypeDefinition)
            {
                stringBuilder.Append("<");
                for (int i = 0; i < type.GetGenericArguments().Length; i++)
                {
                    if (i > 0)
                        stringBuilder.Append(", ");
                    stringBuilder.AppendFormat("{{{0}}}", i);
                }
                stringBuilder.Append(">");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the formatting strings representing a method.
        /// </summary>
        /// <param name="method">A <see cref="MethodBase"/>.</param>
        /// <returns></returns>
        public static MethodFormatStrings GetMethodFormatStrings(MethodBase method)
        {
            bool methodIsGeneric;

            var stringBuilder = new StringBuilder();

            string typeFormat = GetTypeFormatString(method.DeclaringType);
            bool typeIsGeneric = method.DeclaringType.IsGenericTypeDefinition;

            // Build the format string for the method name.
            stringBuilder.Length = 0;
            stringBuilder.Append("::");
            stringBuilder.Append(method.Name);
            if (method.IsGenericMethodDefinition)
            {
                methodIsGeneric = true;
                stringBuilder.Append("<");
                for (int i = 0; i < method.GetGenericArguments().Length; i++)
                {
                    if (i > 0)
                        stringBuilder.Append(", ");
                    stringBuilder.AppendFormat("{{{0}}}", i);
                }
                stringBuilder.Append(">");
            }
            else
            {
                methodIsGeneric = false;
            }
            string methodFormat = stringBuilder.ToString();

            // Build the format string for parameters.
            stringBuilder.Length = 0;
            ParameterInfo[] parameters = method.GetParameters();
            stringBuilder.Append("(");
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append("{{{");
                stringBuilder.Append(i);
                stringBuilder.Append("}}}");
            }
            stringBuilder.Append(")");

            string parameterFormat = stringBuilder.ToString();

            return new MethodFormatStrings(typeFormat, typeIsGeneric, methodFormat, methodIsGeneric, parameterFormat);
        }

        /// <summary>
        /// Pads a string with a space, if not empty and not yet padded.
        /// </summary>
        /// <param name="prefix">A string.</param>
        /// <returns>A padded string.</returns>
        public static string NormalizePrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return string.Empty;
            }
            if (prefix.EndsWith(" "))
            {
                return prefix;
            }
            return prefix + " ";
        }

        public static string FormatString(string format, params object[] args)
        {
            if (args == null)
                return format;
            return string.Format(format, args);
        }
    }
}