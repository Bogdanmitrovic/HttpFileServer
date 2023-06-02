using System.Globalization;
using System.Net;

namespace SysProgZadatak6
{
    public class Log
    {
        private string _newLogs;
        private static readonly object WriteLock = new object();

        private static Log? _instance;
        private static readonly object LockObject = new object();

        private Timer _timerWrite;

        private Log(int refreshMin = 1)
        {
            _newLogs = "";
            _timerWrite = new Timer(_ => WriteFile(), null, 5000, (int)TimeSpan.FromMinutes(refreshMin).TotalMilliseconds);
        }
        public static Log Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new Log();
                        }
                    }
                }
                return _instance;
            }
        }

        private void WriteFile()
        {
            using (StreamWriter writer = new StreamWriter("Logs.txt", true))
            {
                lock (WriteLock)
                {
                    writer.WriteAsync(_newLogs);        // neblokirajuca funkcija
                    _newLogs = "";
                }
            }
            Console.WriteLine("Logs archived");
        }

        public void RequestLog(HttpListenerRequest request)
        {
            string message = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}  REQUEST: {request.HttpMethod} {request.Url?.AbsoluteUri}";
            Console.WriteLine(message);
            lock (WriteLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void ResponseLog(HttpListenerResponse response)
        {
            string message = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}  RESPONSE: {response.StatusCode} {response.StatusDescription} ";
            Console.WriteLine(message);
            lock (WriteLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void ExceptionLog(Exception e)
        {
            string message = DateTime.Now + " EXCEPTION: " + e.Message;
            Console.WriteLine(message);
            lock (WriteLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void MessageLog(String mess)
        {
            string message = DateTime.Now +"  MESSAGE: " + mess;
            Console.WriteLine(message);
            lock (WriteLock)
            {
                _newLogs += message + "\n";
            }

        }
    }
}
