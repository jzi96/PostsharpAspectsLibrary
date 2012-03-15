using System;
using System.Diagnostics;

namespace Zieschang.Net.Projects.PostsharpAspects
{
    public static class LogHelper
    {
        public static void Send(this log4net.ILog lg, TraceEventType eventType, string message)
        {
            Action<string,Exception> method = lg.GetLogMethod(eventType);
            method(message, null);
        }

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