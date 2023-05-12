using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SysProgZadatak6
{
    public class Log
    {
        private  string _newLogs;
        private int refreshMin;
        private static readonly object _writeLock = new object();
        private static Timer timerWrite;

        private static Log instance;
        private static readonly object lockObject = new object();

        private Log(int refreshMin = 1)
        {
            _newLogs = "";
            this.refreshMin = refreshMin;
            timerWrite = new Timer(_ => WriteFile(), null, 5000, (int)TimeSpan.FromMinutes(refreshMin).TotalMilliseconds);
        }
        public static Log Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new Log();
                        }
                    }
                }
                return instance;
            }
        }

        private void WriteFile()
        {
            using (StreamWriter writer = new StreamWriter("Logs.txt", true))
            {
                lock (_writeLock)
                {
                    writer.Write(_newLogs);
                    _newLogs = "";
                }
            }
            Console.WriteLine("Logs archived");
        }

        public void RequestLog(HttpListenerRequest request)
        {
            string message = $"{DateTime.Now.ToString()}  REQUEST: {request.HttpMethod} {request.Url?.AbsoluteUri}";
            Console.WriteLine(message);
            lock (_writeLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void ResponseLog(HttpListenerResponse response)
        {
            string message = $"{DateTime.Now.ToString()}  RESPONSE: {response.StatusCode} {response.StatusDescription} ";
            Console.WriteLine(message);
            lock (_writeLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void ExceptionLog(Exception e)
        {
            string message = DateTime.Now.ToString() + " EXCEPTION: " + e.Message;
            Console.WriteLine(message);
            lock (_writeLock)
            {
                _newLogs += message + "\n";
            }
        }
        public void MessageLog(String mess)
        {
            string message = DateTime.Now.ToString() +"  MESSAGE: " + mess;
            Console.WriteLine(message);
            lock (_writeLock)
            {
                _newLogs += message + "\n";
            }

        }
    }
}
