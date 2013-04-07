using System;
using System.Reflection;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// Set of 3 formatting string that, at runtime, represent a method and its
    /// parameters.
    /// </summary>
    [Serializable]
    internal class MethodFormatStrings
    {
        private readonly string _typeFormat;
        private readonly string _methodFormat;
        private readonly string _parameterFormat;
        private readonly bool _typeIsGeneric;
        private readonly bool _methodIsGeneric;

        /// <summary>
        /// Initializes a new <see cref="MethodFormatStrings"/>.
        /// </summary>
        /// <param name="typeFormat">
        /// The formatting string representing the type
        /// where each generic type argument is represented as a
        /// formatting argument (e.g. <c>Dictionary&lt;{0},P1}&gt;</c>.
        /// </param>
        /// <param name="methodFormat">
        /// The formatting string representing the method (but not the declaring type).
        /// where each generic method argument is represented as a
        /// formatting argument (e.g. <c>ToArray&lt;{0}&gt;</c>. 
        /// </param>
        /// <param name="parameterFormat">
        /// The formatting string representing the list of parameters, where each
        /// parameter is representing as a formatting argument 
        /// (e.g. <c>{0}, {1}</c>).        
        /// </param>
        /// <param name="methodIsGeneric">Indicates whether the method is generic.</param>
        /// <param name="typeIsGeneric">Indicates whether the type declaring the method is generic.</param>
        internal MethodFormatStrings(string typeFormat, bool typeIsGeneric,
                                     string methodFormat,
                                     bool methodIsGeneric,
                                     string parameterFormat)
        {
            _typeFormat = typeFormat;
            _methodFormat = methodFormat;
            _parameterFormat = parameterFormat;
            _typeIsGeneric = typeIsGeneric;
            _methodIsGeneric = methodIsGeneric;
        }


        /// <summary>
        /// Gets a string representing a concrete method invocation.
        /// </summary>
        /// <param name="instance">Instance on which the method is invoked.</param>
        /// <param name="method">Invoked method.</param>
        /// <param name="invocationParameters">Concrete invocation parameters.</param>
        /// <returns>A representation of the method invocation.</returns>
        public string Format(
            object instance,
            MethodBase method,
            object[] invocationParameters)
        {
            var parts = new[]
                            {
                                _typeIsGeneric
                                    ? Formatter.FormatString( _typeFormat, method.DeclaringType.GetGenericArguments() )
                                    : _typeFormat,
                                _methodIsGeneric
                                    ? Formatter.FormatString( _methodFormat, method.GetGenericArguments() )
                                    : _methodFormat,
                                instance == null ? String.Empty : string.Format( "{{{0}}}", instance ),
                                Formatter.FormatString( _parameterFormat, invocationParameters )
                            };

            return string.Concat(parts);
        }
    }
}