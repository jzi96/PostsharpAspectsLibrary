using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// Extension methods for easier use of serialization
    /// </summary>
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public static class SerializationUtilities
    {
        /// <summary>
        /// Setting for writing formatted xml without the Byte Order Mark (BOM)
        /// </summary>
        public static readonly XmlWriterSettings FormattedSettings = new XmlWriterSettings()
        {
            Indent = true,
            ConformanceLevel = ConformanceLevel.Auto,
            OmitXmlDeclaration = true,
            Encoding = new System.Text.UTF8Encoding(false)
        };
        /// <summary>
        /// Setting for writing unformatted xml without the Byte Order Mark (BOM)
        /// </summary>
        public static readonly XmlWriterSettings UnformattedSettings = new XmlWriterSettings() { Encoding = new System.Text.UTF8Encoding(false) };
        /// <summary>
        /// cache for reusing the <see cref="XmlSerializer"/> for a given <see cref="Type"/>
        /// </summary>
        static readonly Dictionary<Type, XmlSerializer> SerializerCache = new Dictionary<Type, XmlSerializer>();
        /// <summary>
        /// Deserialize the specified <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Result type, must much the serialized content</typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        /// <summary>
        /// Deserialized <paramref name="message"/>
        /// of <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static object Deserialize(Type type, string message)
        {
            XmlSerializer ser = ResolveSerializer(type);
            using (StringReader sr = new StringReader(message))
                return ser.Deserialize(sr);
        }
        /// <summary>
        /// Deserialize the specified <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Result type, must much the serialized content</typeparam>
        public static T Deserialize<T>(byte[] message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        /// <summary>
        /// Deserialized <paramref name="message"/>
        /// of <paramref name="type"/>
        /// </summary>
        public static object Deserialize(Type type, byte[] message)
        {
            using (MemoryStream sr = new MemoryStream(message))
                return Deserialize(type, sr);
        }
        /// <summary>
        /// Deserialize the specified <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Result type, must much the serialized content</typeparam>
        public static T Deserialize<T>(Stream message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        /// <summary>
        /// Deserialized <paramref name="message"/>
        /// of <paramref name="type"/>
        /// </summary>
        public static object Deserialize(Type type, Stream message)
        {
            XmlSerializer ser = ResolveSerializer(type);
            return ser.Deserialize(message);
        }
        /// <summary>
        /// Saves the current object into it's serialized
        /// string representation.
        /// </summary>
        /// <param name="value">object to serialize. The object must support XmlSerialization.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Serialize(this object value)
        {
            return Serialize(value, UnformattedSettings);
        }
        /// <summary>
        /// Saves the current object into it's serialized
        /// string representation.
        /// </summary>
        public static string Serialize(this object value, bool formatted)
        {
            if ((formatted))
            {
                return Serialize(value, FormattedSettings);
            }
            else
            {
                return Serialize(value, UnformattedSettings);
            }
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into an byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatted"></param>
        /// <returns></returns>
        public static byte[] SerializeToArray(this object value, bool formatted)
        {
            if ((value == null)) return null;
            var setting = formatted ? FormattedSettings : UnformattedSettings;
            XmlSerializer ser = null;
            Type objGetType = value.GetType();
            ser = ResolveSerializer(objGetType);
            using (MemoryStream mem = new MemoryStream(2000))
            {
                using (XmlWriter xw = XmlWriter.Create(mem, setting))
                {
                    ser.Serialize(xw, value);
                }
                mem.Flush();
                mem.Position = 0;
                return mem.ToArray();
            }
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <paramref name="targetStream"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatted"></param>
        /// <param name="targetStream"></param>
        public static void Serialize(this object value, bool formatted, Stream targetStream)
        {
            Serialize(value, formatted ? FormattedSettings : UnformattedSettings, targetStream);
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <paramref name="targetStream"/>
        /// </summary>
        /// <param name="value"><see cref="Object"/> to serialize</param>
        /// <param name="format"></param>
        /// <param name="targetStream"></param>
        public static void Serialize(this object value, XmlWriterSettings format, Stream targetStream)
        {
            if ((value == null)) return;
            if (targetStream == null) throw new ArgumentNullException("targetStream");
            if (!targetStream.CanWrite) throw new ArgumentOutOfRangeException("targetStream", "Stream must be writable!");
            XmlSerializer ser = null;
            Type objGetType = value.GetType();
            ser = ResolveSerializer(objGetType);
            //Dim ser As New NetDataContractSerializer
            using (XmlWriter xw = XmlWriter.Create(targetStream, format))
            {
                ser.Serialize(xw, value);
            }
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <see cref="string"/>
        /// </summary>
        /// <param name="value"><see cref="Object"/> to serialize</param>
        /// <param name="format">Formatting to be used</param>
        /// <returns></returns>
        public static string Serialize(this object value, XmlWriterSettings format)
        {
            if ((value == null)) return null;
            using (MemoryStream mem = new MemoryStream())
            {
                Serialize(value, format, mem);
                mem.Flush();
                mem.Position = 0;
                return Encoding.UTF8.GetString(mem.ToArray());
            }
        }
        /// <summary>
        /// Returns an XmlSerializer instance for the specified type.
        /// </summary>
        /// <param name="objGetType"></param>
        /// <returns></returns>
        public static XmlSerializer ResolveSerializer(Type objGetType)
        {
            XmlSerializer ser = null;
            if ((!SerializerCache.TryGetValue(objGetType, out ser)))
            {
                lock (SerializerCache)
                {
                    if ((!SerializerCache.TryGetValue(objGetType, out ser)))
                    {
                        ser = new XmlSerializer(objGetType);
                        SerializerCache.Add(objGetType, ser);
                    }
                }
            }
            return ser;
        }
    }

    /*

        static readonly Dictionary<Type, XmlSerializer> SerializerCache = new Dictionary<Type, XmlSerializer>();
        public static T Deserialize<T>(string message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        public static object Deserialize(Type type, string message)
        {
            XmlSerializer ser = ResolveSerializer(type);
            using (StringReader sr = new StringReader(message))
                return ser.Deserialize(sr);
        }
        public static T Deserialize<T>(byte[] message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        public static object Deserialize(Type type, byte[] message)
        {
            using (MemoryStream sr = new MemoryStream(message))
                return Deserialize(type, sr);
        }
        public static T Deserialize<T>(Stream message)
        {
            return (T)Deserialize(typeof(T), message);
        }
        public static object Deserialize(Type type, Stream message)
        {
            XmlSerializer ser = ResolveSerializer(type);
            return ser.Deserialize(message);
        }
        /// <summary>
        /// Saves the current object into it's serialized
        /// string representation.
        /// </summary>
        /// <param name="value">object to serialize. The object must support XmlSerialization.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Serialize(this object value)
        {
            return Serialize(value, UnformattedSettings);
        }
        public static string Serialize(this object value, bool formatted)
        {
            if ((formatted))
            {
                return Serialize(value, FormattedSettings);
            }
            else
            {
                return Serialize(value, UnformattedSettings);
            }
        }
        public static byte[] SerializeToArray(this object value, bool formatted)
        {
            if ((value == null)) return null;
            var setting = formatted ? FormattedSettings : UnformattedSettings;
            XmlSerializer ser = null;
            Type objGetType = value.GetType();
            ser = ResolveSerializer(objGetType);
            using (MemoryStream mem = new MemoryStream(2000))
            {
                using (XmlWriter xw = XmlWriter.Create(mem, setting))
                {
                    ser.Serialize(xw, value);
                }
                mem.Flush();
                mem.Position = 0;
                return mem.ToArray();
            }
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <paramref name="targetStream"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatted"></param>
        /// <param name="targetStream"></param>
        public static void Serialize(this object value, bool formatted, Stream targetStream)
        {
            Serialize(value, formatted ? FormattedSettings : UnformattedSettings, targetStream);
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <paramref name="targetStream"/>
        /// </summary>
        /// <param name="value"><see cref="Object"/> to serialize</param>
        /// <param name="format"></param>
        /// <param name="targetStream"></param>
        public static void Serialize(this object value, XmlWriterSettings format, Stream targetStream)
        {
            if ((value == null)) return;
            if (targetStream == null) throw new ArgumentNullException("targetStream");
            if (!targetStream.CanWrite) throw new ArgumentOutOfRangeException("targetStream", "Stream must be writable!");
            XmlSerializer ser = null;
            Type objGetType = value.GetType();
            ser = ResolveSerializer(objGetType);
            //Dim ser As New NetDataContractSerializer
            using (XmlWriter xw = XmlWriter.Create(targetStream, format))
            {
                ser.Serialize(xw, value);
            }
        }
        /// <summary>
        /// Serialize the <paramref name="value"/> into 
        /// a <see cref="string"/>
        /// </summary>
        /// <param name="value"><see cref="Object"/> to serialize</param>
        /// <param name="format">Formatting to be used</param>
        /// <returns></returns>
        public static string Serialize(this object value, XmlWriterSettings format)
        {
            if ((value == null)) return null;
            using (MemoryStream mem = new MemoryStream())
            {
                Serialize(value, format, mem);
                mem.Flush();
                mem.Position = 0;
                return Encoding.UTF8.GetString(mem.ToArray());
            }
        }
        /// <summary>
        /// Returns an XmlSerializer instance for the specified type.
        /// </summary>
        /// <param name="objGetType"></param>
        /// <returns></returns>
        public static XmlSerializer ResolveSerializer(Type objGetType)
        {
            XmlSerializer ser = null;
            if ((!SerializerCache.TryGetValue(objGetType, out ser)))
            {
                lock (SerializerCache)
                {
                    if ((!SerializerCache.TryGetValue(objGetType, out ser)))
                    {
                        ser = new XmlSerializer(objGetType);
                        SerializerCache.Add(objGetType, ser);
                    }
                }
            }
            return ser;
        }

     * */
}
