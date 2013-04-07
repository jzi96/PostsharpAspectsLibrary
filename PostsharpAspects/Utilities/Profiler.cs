using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using log4net;

namespace Zieschang.Net.Projects.PostsharpAspects.Utilities
{
    /// <summary>
    /// A class for writing special entries for performance logging
    /// to the _logger. All entries are equal formatted, so there is a more
    /// easier way of evaluating the timeperiods.
    /// It's important to call the <see cref="Profiler.Dispose()"/> method.
    /// </summary>
    /// <example>
    /// A typical usage of the tracer is to use <see langword="using"/>-statements
    /// <code><![CDATA[using (new Profiler("mein tester", "category"))
    /// {
    /// }]]></code>
    /// </example>
#if(RELEASE)
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
#endif
    public sealed class Profiler : IDisposable
    {
        private const string HirarchicalTemplate = Template + " <{3}::{4}>";
        private const string Template = "<{0}> <{1}> <Elapsed {2}ms>";

        [ThreadStatic]
        private static List<Profiler> _hirarchieStorage;
        //use a static instance to reduce overhead!'
        private static readonly Stopwatch Watch = Stopwatch.StartNew();

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Profiler));
        private readonly long _startTime;

        //public string Template { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="name">A name for the measure point.</param>
        public Profiler(string name)
            : this(name, null, TraceLevel.Info, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="name">A name for the measure point.</param>
        /// <param name="logLevel">The log level.</param>
        public Profiler(string name, TraceLevel logLevel)
            : this(name, null, logLevel, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="name">A name for the measure point.</param>
        /// <param name="category">An grouping identifier for the log.</param>
        public Profiler(string name, string category)
            : this(name, category, TraceLevel.Info, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="name">A name for the measure point.</param>
        /// <param name="category">An grouping identifier for the log.</param>
        /// <param name="logLevel">The log level.</param>
        public Profiler(string name, string category, TraceLevel logLevel)
            : this(name, category, logLevel, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class. This will set
        /// all properties, there is no setter in properties all initialization must be
        /// done here.
        /// </summary>
        /// <param name="name">A name for the measure point.</param>
        /// <param name="category">An grouping identifier for the log.</param>
        /// <param name="hirachical">if set to <see langword="true"/>the additional
        /// hirarchi info is done.</param>
        public Profiler(string name, string category, bool hirachical)
            : this(name, category, TraceLevel.Info, hirachical) { }

        /// <summary>
        /// Creates an instance and starts the timer.
        /// </summary>
        /// <param name="name">A name to identify the current position</param>
        /// <param name="category">An identifier to mark similar regions</param>
        /// <param name="logLevel">The level to write the information to. This means, a level 
        /// of info would result in writing log entries with the level entry. The final logging
        /// depends on the filter configuration of the _logger.</param>
        /// <param name="hirachical">if set to <c>true</c> the _logger will write all upper
        /// level, when time has finished and also add it self recognize to the level list. 
        /// The default setting is <c>false</c>.</param>
        public Profiler(string name, string category, TraceLevel logLevel, bool hirachical)
        {
            this.Hirachical = hirachical;
            this.LogLevel = logLevel;
            this.Name = name;
            this.Category = category;
            if (hirachical)
            {
                if (_hirarchieStorage == null)
                {
                    lock (Watch)
                    {
                        if (_hirarchieStorage == null) _hirarchieStorage = new List<Profiler>();
                    }
                }
                lock (_hirarchieStorage)
                    _hirarchieStorage.Add(this);
            }
            _startTime = Watch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public string Category { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Profiler"/> is hirachical.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if hirachical; otherwise, <see langword="false"/>.
        /// </value>
        public bool Hirachical { get; private set; }
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>The log level.</value>
        public TraceLevel LogLevel { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Profiler"/> is disposed.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if disposed; otherwise, <see langword="false"/>.
        /// </value>
        public bool Disposed { get; private set; }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>If the method is called and <see cref="Disposed"/>
        /// is <see langword="false"/>, the timespan will be logged.</remarks>
        public void Dispose()
        {
            try
            {
                this.Dispose(true);
            }
            catch
            {
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Profiler"/> is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>If the destructor is called and <see cref="Disposed"/>
        /// is <see langword="false"/>, the timespan will be logged.</remarks>
        ~Profiler()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <remarks>If raised the first time,
        /// the timer is stopped and the operation result is
        /// written to the _logger. After logging, resources
        /// are destroyed.</remarks>
        /// <param name="isDisposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        private void Dispose(bool isDisposing)
        {
            if (!this.Disposed && _startTime > 0)
            {
                long elapsedTime = Watch.ElapsedMilliseconds - _startTime;
                HandleHirachy();
                if ((this.LogLevel == TraceLevel.Error && _logger.IsFatalEnabled)
                   || (this.LogLevel == TraceLevel.Warning && _logger.IsWarnEnabled)
                   || (this.LogLevel == TraceLevel.Info && _logger.IsInfoEnabled)
                   || (this.LogLevel == TraceLevel.Verbose && _logger.IsDebugEnabled))
                {
                    string msg;
                    StringBuilder levelBuilder = PrepareMessage(out msg);

                    SendToLogger(elapsedTime, msg, levelBuilder);
                }
            }
            this.Disposed = true;
        }

        private void HandleHirachy()
        {
            if (this.Hirachical && _hirarchieStorage != null && _hirarchieStorage.Count > 0)
            {
                //remove from stack
                lock (_hirarchieStorage)
                {
                    if (_hirarchieStorage[_hirarchieStorage.Count - 1] == this)
                        _hirarchieStorage.RemoveAt(_hirarchieStorage.Count - 1);
                }
            }
        }

        private StringBuilder PrepareMessage(out string msg)
        {

            StringBuilder levelBuilder = null;
            if (this.Hirachical && _hirarchieStorage != null && _hirarchieStorage.Count > 0)
            {
                lock (_hirarchieStorage)
                {
                    levelBuilder = new StringBuilder(25 * _hirarchieStorage.Count);
                    msg = HirarchicalTemplate;
                    //ggf. name aus hierarchie bilden
                    for (int i = 0; i < _hirarchieStorage.Count; i++)
                    {
                        Profiler p = _hirarchieStorage[i];
                        if (p != null)
                        {
                            levelBuilder.Append(p.Category).Append(":").Append(p.Name);
                            if ((i + 1) < _hirarchieStorage.Count)
                                levelBuilder.Append(".");
                        }
                    }
                }
            }
            else
            {
                msg = Template;
                //alternativ das setzen über den Logger-Context
            }
            return levelBuilder;
        }

        private void SendToLogger(long elapsedTime, string msg, StringBuilder levelBuilder)
        {
            int hirarchieStorageCount = _hirarchieStorage == null ? 0 : _hirarchieStorage.Count;
            switch (this.LogLevel)
            {
                case TraceLevel.Error:
                    _logger.ErrorFormat(CultureInfo.InvariantCulture,
                                            msg,
                                            this.Category,
                                            this.Name,
                                            elapsedTime,
                                            hirarchieStorageCount,
                                            levelBuilder);
                    break;
                case TraceLevel.Warning:
                    _logger.WarnFormat(CultureInfo.InvariantCulture,
                                           msg,
                                           this.Category,
                                           this.Name,
                                           elapsedTime,
                                           hirarchieStorageCount,
                                           levelBuilder);
                    break;
                case TraceLevel.Info:
                    _logger.InfoFormat(CultureInfo.InvariantCulture,
                                           msg,
                                           this.Category,
                                           this.Name,
                                           elapsedTime,
                                           hirarchieStorageCount,
                                           levelBuilder);
                    break;
                case TraceLevel.Verbose:
                    _logger.DebugFormat(CultureInfo.InvariantCulture,
                                            msg,
                                            this.Category,
                                            this.Name,
                                            elapsedTime,
                                            hirarchieStorageCount,
                                            levelBuilder);
                    break;
            }
        }
    }
}