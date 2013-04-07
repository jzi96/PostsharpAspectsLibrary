using System;
using System.Diagnostics;
using System.Globalization;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// set of extensions methods to make
    /// logging with <see cref="log4net.ILog"/> easier.
    /// </summary>
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public static class LogHelper
    {
        /// <summary>
        /// A special context to attach useful logging details
        /// </summary>
        public const string DefaultLoggerContextStack = "NDC";

        /// <summary>
        /// Send a message to the logger
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="eventType"></param>
        /// <param name="message"></param>
        public static void Send(this log4net.ILog lg, TraceEventType eventType, string message)
        {
            Action<string,Exception> method = lg.GetLogMethod(eventType);
            method(message, null);
        }
        /// <summary>
        /// Method to find the correct logging method
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

        /// <summary>
        /// Set context informations for the logging.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <returns>Returns an <see cref="IDisposable"/>-object.</returns>
        /// <remarks><para>The setting of context should be wrapped in a <see langword="using"/>
        /// statement.
        /// </para><para>The context is added to the <c>NDC</c> stack of log4net</para></remarks>
        public static IDisposable AddLoggerContext(this log4net.ILog logger, string context)
        {
            return AddLoggerContext(logger, DefaultLoggerContextStack, context);
        }
        /// <summary>
        /// Set context informations for the logging.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <param name="stack"></param>
        /// <returns>Returns an <see cref="IDisposable"/>-object.</returns>
        /// <remarks><para>The setting of context should be wrapped in a <see langword="using"/>
        /// statement.
        /// </para></remarks>
        public static IDisposable AddLoggerContext(this log4net.ILog logger, string stack, string context)
        {
            return log4net.ThreadContext.Stacks[stack].Push(context);
        }
        /// <summary>
        /// Logs a message and throws the specified exception after the message has been logged
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void ThrowException<T>(this log4net.ILog logger, string message, params object[] parameters) where T : Exception
        {
            ThrowException<T>(logger, message, (Exception)null, parameters);
        }

        /// <summary>
        /// Logs a message and throws the specified exception after the message has been logged
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        public static void ThrowException<T>(this log4net.ILog logger, string message, Exception exception, params object[] @params) where T : Exception
        {
            string msg = null;
            if ((@params == null || @params.Length == 0))
                msg = message;
            else
                msg = string.Format(message, @params);
            logger.Error(msg, exception);
            throw (T)Activator.CreateInstance(typeof(T), msg);
        }
        /// <summary>
        /// Sends a debug statement to the logger
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static void Dbg(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsDebugEnabled))
            {
                logger.Debug(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        /// <summary>
        /// Sends the message as info to the logger
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static void Info(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsInfoEnabled))
            {
                logger.Info(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        /// <summary>
        /// Send the message as error to the logger
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static void Error(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsErrorEnabled))
            {
                logger.Error(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        /// <summary>
        /// Send the message as warning to the logger
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static void Warn(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsWarnEnabled))
            {
                logger.Warn(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }


        /// <summary>
        /// Logs the message to the corresponding log
        /// level.
        /// </summary>
        /// <remarks><para>If no <paramref name="level"/> match
        /// <see cref="TraceEventType.Information"/> is assumed</para></remarks>
        /// <param name="logger">Instance of the logger</param>
        /// <param name="level">Level to log.</param>
        /// <param name="message">the message to log</param>
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void Log(this log4net.ILog logger, TraceEventType level, string message)
        {
            Log(logger, level, message, null);
        }
        /// <summary>
        /// Logs the message to the corresponding log
        /// level.
        /// </summary>
        /// <remarks><para>If no <paramref name="level"/> match
        /// <see cref="TraceEventType.Information"/> is assumed</para></remarks>
        /// <param name="logger">Instance of the logger</param>
        /// <param name="level">Level to log.</param>
        /// <param name="message">the message to log</param>
        /// <param name="exception"></param>
        public static void Log(this log4net.ILog logger, TraceEventType level, string message, Exception exception)
        {
            switch (level)
            {
                case TraceEventType.Information:
                    logger.Info(message, exception);
                    break;
                case TraceEventType.Critical:
                    logger.Fatal(message, exception);
                    break;
                case TraceEventType.Error:
                    logger.Error(message, exception);
                    break;
                case TraceEventType.Warning:
                    logger.Warn(message, exception);
                    break;
                case TraceEventType.Verbose:
                    logger.Debug(message, exception);
                    break;
                default:
                    logger.Info(message, exception);
                    break;
            }
        }
    }
    /*
        /// <summary>
        /// Set context informations for the logging.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <returns>Returns an <see cref="IDisposable"/>-object.</returns>
        /// <remarks><para>The setting of context should be wrapped in a <see langword="using"/>
        /// statement.
        /// </para><para>The context is added to the <c>NDC</c> stack of log4net</para></remarks>
        public static IDisposable AddLoggerContext(this log4net.ILog logger, string context)
        {
            return AddLoggerContext(logger, DefaultLoggerContextStack, context);
        }
        /// <summary>
        /// Set context informations for the logging.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <param name="stack"></param>
        /// <returns>Returns an <see cref="IDisposable"/>-object.</returns>
        /// <remarks><para>The setting of context should be wrapped in a <see langword="using"/>
        /// statement.
        /// </para></remarks>
        public static IDisposable AddLoggerContext(this log4net.ILog logger, string stack, string context)
        {
            return log4net.ThreadContext.Stacks[stack].Push(context);
        }
        public static void ThrowException<T>(this log4net.ILog logger, string message, params object[] parameters) where T : Exception
        {
            ThrowException<T>(logger, message, (Exception)null, parameters);
        }

        public static void ThrowException<T>(this log4net.ILog logger, string message, Exception exception, params object[] @params) where T : Exception
        {
            string msg = null;
            if ((@params == null || @params.Length == 0))
                msg = message;
            else
                msg = string.Format(message, @params);
            logger.Error(msg, exception);
            throw (T)Activator.CreateInstance(typeof(T), msg);
        }
        public static void Dbg(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsDebugEnabled))
            {
                logger.Debug(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        public static void Info(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsInfoEnabled))
            {
                logger.Info(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        public static void Error(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsErrorEnabled))
            {
                logger.Error(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
        public static void Warn(this log4net.ILog logger, string message, Exception ex, params object[] parameters)
        {
            if ((logger.IsWarnEnabled))
            {
                logger.Warn(string.Format(CultureInfo.InvariantCulture, message, parameters), ex);
            }
        }
     */
}