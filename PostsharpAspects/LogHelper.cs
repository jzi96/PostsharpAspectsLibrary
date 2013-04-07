using System;
using System.Diagnostics;

namespace Zieschang.Net.Projects.PostsharpAspects
{
    /// <summary>
    /// Set of Helper and extension methods for <see cref="log4net.ILog"/>
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Write the message to the logger.
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="eventType">Specifies the level of the error</param>
        /// <param name="message">message</param>
        public static void Send(this log4net.ILog lg, TraceEventType eventType, string message)
        {
            Action<string,Exception> method = lg.GetLogMethod(eventType);
            method(message, null);
        }
        /// <summary>
        /// Return the log method for the defined level
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static Action<string,Exception> GetLogMethod(this log4net.ILog lg, TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Warning:
                    return lg.Warn;
                case TraceEventType.Error:
                    return lg.Error;
                case TraceEventType.Critical:
                    return lg.Fatal;
                case TraceEventType.Verbose:
                    return lg.Debug;
            }
            return lg.Info;
        }
    }
}