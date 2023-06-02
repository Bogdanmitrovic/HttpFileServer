using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace SysProgZadatak6
{
    public class Log : IDisposable
    {
        private StringBuilder _logBuilder;
        private static readonly object WriteLock = new object();

        private static Log? _instance;
        private static readonly object LockObject = new object();

        //private Timer _timerWrite;
        private StreamWriter _writer;

        private Log(int refreshMin = 1)
        {
            _logBuilder = new StringBuilder();
            //_timerWrite = new Timer(_ => WriteFile(), null, 5000, (int)TimeSpan.FromMinutes(refreshMin).TotalMilliseconds);
            _writer = new StreamWriter("Logs.txt", true);
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
        /*
        private void WriteFile()
        {
            using (r = new StreamWriter("Logs.txt", true))
            {
                lock (WriteLock)
                {
                    writer.Write(_logBuilder.ToString());
                    _logBuilder.Clear();
                }
            }
            Console.WriteLine("Logs archived");
        }
        */
        public void RequestLog(HttpListenerRequest request)
        {
            lock (WriteLock)
            {
                string message = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}  REQUEST: {request.HttpMethod} {request.Url?.AbsoluteUri}";
                Console.WriteLine(message);
                _writer.WriteLine(message);
            }
        }
        public void ResponseLog(HttpListenerResponse response)
        {
            string message = $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}  RESPONSE: {response.StatusCode} {response.StatusDescription} ";
            Console.WriteLine(message);
            lock (WriteLock)
            {
                _logBuilder.AppendLine(message);
            }
        }
        public void ExceptionLog(Exception e)
        {
            lock (WriteLock)
            {
                string message = DateTime.Now + " EXCEPTION: " + e.Message;
                Console.WriteLine(message);
                _writer.WriteLine(message);
            }
        }
        public void MessageLog(String mess)
        {
            lock (WriteLock)
            {
                string message = DateTime.Now +"  MESSAGE: " + mess;
                Console.WriteLine(message);
                _writer.WriteLine(message);
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_writer != null)
                {
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                }
            }
        }
    }
}
