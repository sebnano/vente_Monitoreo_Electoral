using System.Runtime.CompilerServices;

namespace ElectoralMonitoring
{
    public class LoggerAnalytics : IFirebaseAnalytics
    {
        public void LogEvent(string eventName, IDictionary<string, object>? parameters)
        {
            
        }
    }
    public interface IFirebaseAnalytics
    {
        void LogEvent(string eventName, IDictionary<string, object>? parameters);
    }

    public class AnalyticsService
    {
        readonly IFirebaseAnalytics _analyticsProvider;

        public AnalyticsService(IFirebaseAnalytics firebaseAnalytics)
        {
            _analyticsProvider = firebaseAnalytics;
        }
        public void Track(string eventId, IDictionary<string, object>? parameters = null,
                                [CallerMemberName] string callerMemberName = "",
                                [CallerLineNumber] int lineNumber = 0,
                                [CallerFilePath] string filePath = "")
        {
            PrintEvent(eventId, parameters, callerMemberName, lineNumber, filePath);

            _analyticsProvider.LogEvent(eventId, parameters);
        }

        public void Track(string eventId, string paramName, string value,
                                [CallerMemberName] string callerMemberName = "",
                                [CallerLineNumber] int lineNumber = 0,
                                [CallerFilePath] string filePath = "") =>
            Track(eventId, new Dictionary<string, object> { { paramName, value } }, callerMemberName, lineNumber, filePath);

        public TimedEvent TrackTime(string eventId, IDictionary<string, object>? parameters = null) =>
            new TimedEvent(this, eventId, parameters);

        public TimedEvent TrackTime(string eventId, string paramName, string value) =>
            TrackTime(eventId, new Dictionary<string, object> { { paramName, value } });

        public void Report(Exception exception,
                                  string key,
                                  string value,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {
            Report(exception, new Dictionary<string, object> { { key, value } }, callerMemberName, lineNumber, filePath);
        }

        public void Report(Exception ex,
                                  IDictionary<string, object>? properties = null,
                                  [CallerMemberName] string callerMemberName = "",
                                  [CallerLineNumber] int lineNumber = 0,
                                  [CallerFilePath] string filePath = "")
        {

            PrintException(ex, callerMemberName, lineNumber, filePath, properties);

            var fileName = System.IO.Path.GetFileName(filePath);

            if (properties is null)
                properties = new Dictionary<string,object>();

            properties.Add("Error: ", ex?.Message ?? string.Empty);
            properties.Add("lineNumber: ", lineNumber);
            properties.Add("callerMemberName: ", callerMemberName);
            properties.Add("fileName: ", fileName);

            //CrossFirebaseCrashlytics.Current.SetCustomKeys(properties);
            //CrossFirebaseCrashlytics.Current.RecordException(ex);
        }

        [Conditional("DEBUG")]
        void PrintException(Exception exception, string callerMemberName, int lineNumber, string filePath, IDictionary<string, object>? properties = null)
        {
            var fileName = System.IO.Path.GetFileName(filePath);

            Debug.WriteLine(exception.GetType());
            Debug.WriteLine($"Error: {exception.Message}");
            Debug.WriteLine($"Line Number: {lineNumber}");
            Debug.WriteLine($"Caller Name: {callerMemberName}");
            Debug.WriteLine($"File Name: {fileName}");

            if (properties != null)
            {
                foreach (var property in properties)
                    Debug.WriteLine($"{property.Key}: {property.Value}");
            }

            Debug.WriteLine(exception);
        }

        [Conditional("DEBUG")]
        void PrintEvent(string eventId,
                                IDictionary<string, object>? parameters = null,
                                string callerMemberName = "",
                                int lineNumber = 0,
                                string filePath = "")
        {
            var fileName = System.IO.Path.GetFileName(filePath);

            Debug.WriteLine($"EventId: {eventId}");
            Debug.WriteLine($"Line Number: {lineNumber}");
            Debug.WriteLine($"Caller Name: {callerMemberName}");
            Debug.WriteLine($"File Name: {fileName}");

            if (parameters != null)
            {
                Debug.WriteLine($"Parameters {parameters.Count}");
                foreach (var property in parameters)
                    Debug.WriteLine($"{property.Key}: {property.Value}");
            }
        }
    }
}

