using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zieschang.Net.Projects.PostsharpAspects.Aspects
{
    internal class ValidationHelper
    {
        public const string HandleExceptionWrapExceptionTypeError = "PJZI001";
        public const string ParameterNullCheckParameterNotDefined = "JZI001";
        public const string ParameterNullCheckParameterNotFound = "JZI004";
        public const string ParameterNullCheckParameterNotFoundIndex = "JZI002";
        public const string ParameterNullCheckParameterNotFoundName = "JZI003";
        public const string ParameterNullCheckParameterValueType = "JZI005";
        public const string PerformanceCounterAttributeCategoryNameMissing = "JZI006";
        public const string PerformanceCounterAttributeCounterNameMissing = "JZI007";
    }
}
