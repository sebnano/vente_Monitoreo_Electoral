using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ElectoralMonitoring
{
    public class TimedEvent : IDisposable
    {
        readonly Stopwatch _stopwatch = new();
        readonly string _eventId;
        readonly AnalyticsService _analyticsService;

        public TimedEvent(in AnalyticsService analyticsService, string eventId, IDictionary<string, object>? parameters)
        {
            Data = parameters ?? new Dictionary<string, object>();
            _eventId = eventId;
            _analyticsService = analyticsService;

            _stopwatch.Start();
        }

        public IDictionary<string, object> Data { get; }

        public void Dispose()
        {
            _stopwatch.Stop();

            Data.Add("Timed_Event", $"{_stopwatch.Elapsed:ss\\.fff}s");

            _analyticsService.Track($"{_eventId}_te", new Dictionary<string, object>(Data));
        }
    }
}

