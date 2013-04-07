using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// Set of helper methods for various problems
    /// to make live easier
    /// </summary>
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public static class Utilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
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



        public static string GetStringValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return null;
            }
            return Convert.ToString(value);
        }
        public static string GetStringValue(this object value, string defaultValue)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return defaultValue;
            }
            return Convert.ToString(value);
        }
        public static int GetIntegerValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return 0;
            }
            return Convert.ToInt32(value);
        }
        public static long GetLongValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return 0;
            }
            return Convert.ToInt64(value);
        }
        public static long? GetNullableLongValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return null;
            }
            return Convert.ToInt64(value);
        }
        public static Nullable<DateTime> GetDateTimeAsUtcValue(this object value)
        {
            var tmp = GetDateTimeAsUtcValue(value, TimeZoneInfo.Local).Value;
            tmp = tmp.AssumeIsLocalTime();
            return tmp;
        }
        public static Nullable<DateTime> GetDateTimeAsUtcValue(this object value, TimeZoneInfo tz)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return null;
            }
            DateTime dt = Convert.ToDateTime(value);
            if (tz == TimeZoneInfo.Utc)
            {
                dt = dt.AssumeIsUtc(); //new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
            }
            else
            {
                dt = TimeZoneInfo.ConvertTimeFromUtc(dt, tz);
                dt = dt.AssumeIsLocalTime();
            }
            return dt;

        }
        public static Nullable<DateTime> GetDateTimeValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return null;
            }
            DateTime dt = Convert.ToDateTime(value);
            return dt;
        }
        public static double GetDoubleValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return 0;
            }
            return Convert.ToDouble(value);
        }
        public static decimal GetDecimalValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return 0;
            }
            return Convert.ToDecimal(value);
        }
        public static decimal? GetNullableDecimalValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return null;
            }
            return Convert.ToDecimal(value);
        }
        public static bool GetBooleanValue(this object value)
        {
            if ((value == null || Convert.IsDBNull(value)))
            {
                return false;
            }
            return Convert.ToBoolean(value);
        }
        public static short GetDBDataValue(this bool value)
        {
            return (short)(value ? 1 : 0);
        }
        /// <summary>
        /// Check if the current date is today
        /// </summary>
        /// <param name="cmpDate"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsToday(this DateTime cmpDate)
        {
            DateTime current = DateTime.Now;
            return (current.Year == cmpDate.Year && current.Month == cmpDate.Month && current.Day == cmpDate.Day);
        }
        public static DateTime GetDateLocalForSpecificDateTimeZone(this DateTime cmpDate, TimeZoneInfo tzInfo)
        {
            DateTime tmp = cmpDate.AssumeIsLocalTime();
            DateTime savingTmp = System.TimeZoneInfo.ConvertTime(tmp, tzInfo);
            TimeSpan diff = default(TimeSpan);
            if ((savingTmp.Day == tmp.Day))
            {
                diff = new TimeSpan(savingTmp.Hour, savingTmp.Minute, savingTmp.Second);
            }
            else
            {
                if ((savingTmp.Month == tmp.Month))
                {
                    if ((savingTmp.Day > tmp.Day))
                    {
                        diff = new TimeSpan(24 - savingTmp.Hour, savingTmp.Minute, savingTmp.Second);
                    }
                    else
                    {
                        diff = new TimeSpan(savingTmp.Hour, savingTmp.Minute, savingTmp.Second);
                    }
                }
                else
                {
                    if ((savingTmp.Day > tmp.Day))
                    {
                        diff = new TimeSpan(24 - savingTmp.Hour, savingTmp.Minute, savingTmp.Second);
                    }
                    else
                    {
                        diff = new TimeSpan(savingTmp.Hour, savingTmp.Minute, savingTmp.Second);
                    }
                }
            }
            tmp = tmp.Add(diff);
            return tmp;
        }
        public static DateTime GetLastWeekdayOfMonth(this DateTime date, DayOfWeek day)
        {
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            int wantedDay = Convert.ToInt32(day);
            int lastDay = Convert.ToInt32(lastDayOfMonth.DayOfWeek);
            return lastDayOfMonth.AddDays(lastDay >= wantedDay ? wantedDay - lastDay : wantedDay - lastDay - 7);
        }
        /// <summary>
        /// Check if both dates are at the same Day
        /// </summary>
        /// <remarks><para>There is no conversion of time zones.</para></remarks>
        /// <param name="baseDate"></param>
        /// <param name="cmpDate"></param>
        /// <returns></returns>
        public static bool IsSameDay(this DateTime baseDate, DateTime cmpDate)
        {
            return baseDate.Day == cmpDate.Day && baseDate.Month == cmpDate.Month && baseDate.Year == cmpDate.Year;
        }


        /// <summary>
        /// Checks if <paramref name="value"/> is between
        /// <paramref name="lowerLimit"/> and <paramref name="upperLimit"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <returns><see langword="True"/>, if the <paramref name="value"/> is between both values;
        /// otherwise <see langword="false"/></returns>
        /// <remarks><para>The check does not include the range borders.</para></remarks>
        public static bool IsBetween(this decimal value, decimal lowerLimit, decimal upperLimit)
        {
            return value > lowerLimit && value < upperLimit;
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is between
        /// <paramref name="lowerLimit"/> and <paramref name="upperLimit"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <returns><see langword="True"/>, if the <paramref name="value"/> is between both values or equal to one;
        /// otherwise <see langword="false"/></returns>
        /// <remarks><para>The check includes the range borders.</para></remarks>
        public static bool IsBetweenOrEqual(this decimal value, decimal lowerLimit, decimal upperLimit)
        {
            return value >= lowerLimit && value <= upperLimit;
        }


        public static object GetNullableTypeValue<T>(this Nullable<T> value) where T : struct
        {
            if (value.HasValue)
            {
                return value.Value;
            }
            return null;
        }


        /// <summary>
        /// Returns the <see cref="DateTime"/> object
        /// with <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Local"/>
        /// </summary>
        /// <param name="date"></param>
        /// <returns>new <see cref="DateTime"/> object</returns>
        public static DateTime AssumeIsLocalTime(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Local);
        }

        /// <summary>
        /// Returns the <see cref="DateTime"/> object
        /// with <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Utc"/>
        /// </summary>
        /// <param name="date"></param>
        /// <returns>new <see cref="DateTime"/> object</returns>
        public static DateTime AssumeIsUtc(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
        /// <summary>
        /// Generate a  <see cref="System.Data.DataTable"/> structure
        /// for the properties of the passed <see cref="Type"/>.
        /// </summary>
        /// <param name="objType">... to analyze.</param>
        /// <exception cref="ArgumentNullException"> thrown
        /// if <paramref name="objType"/> is <see langword="null"/>.</exception>
        /// <returns>Table with structure.</returns>
        public static System.Data.DataTable CreateTableStructure(this Type objType)
        {
            if (objType == null)
                throw new ArgumentNullException("objType");
            DataTable tb = new DataTable(objType.Name);
            var props = objType.GetProperties();
            for (int p = 0; p < props.Length; p++)
            {
                var property = props[p];
                if (property.CanRead)
                    tb.Columns.Add(property.Name, property.PropertyType);
            }
            return tb;
        }
        /// <summary>
        /// Converts the collection to a <see cref="System.Data.DataTable"/>
        /// matching the property structure.
        /// </summary>
        /// <typeparam name="T">The list type to analyze.</typeparam>
        /// <param name="outputValues">values to ouptut</param>
        /// <returns>The converted table. If <paramref name="outputValues"/>
        /// is <see langword="null"/> or empty, the result is <see langword="null"/></returns>
        public static System.Data.DataTable ConvertToTable<T>(this IList<T> outputValues)
        {
            if (outputValues == null || outputValues.Count == 0)
                return null;
            Type objType = typeof(T);
            DataTable tb = CreateTableStructure(objType);
            var props = objType.GetProperties();
            //create structure;
            for (int i = 0; i < outputValues.Count; i++)
            {
                var current = outputValues[i];
                var row = tb.NewRow();
                for (int p = 0; p < props.Length; p++)
                {
                    var property = props[p];
                    if (property.CanRead)
                        row[p] = property.GetValue(current, null);
                }
                tb.Rows.Add(row);
            }
            return tb;
        }
        [Obsolete("Just for beeing reverse compatibile. Use SerializationUtilities.ResolveSerializer instead.")]
        public static XmlSerializer ResolveSerializer(Type objGetType)
        {
            return SerializationUtilities.ResolveSerializer(objGetType);
        }
    }


}
