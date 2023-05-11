using System.Net;
using System.Runtime.Caching;
using System.Text;

namespace SysProgZadatak6;

public class HttpServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly Thread _listenerThread;
    private bool _disposed;
    private static readonly MemoryCache Cache = MemoryCache.Default;

    public HttpServer(string address = "localhost", int port = 5080)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{address}:{port}/");
        _listenerThread = new Thread(Listen);
        _disposed = false;
    }

    public void Start()
    {
        _listener.Start();
        _listenerThread.Start();
    }
    private void Listen()
    {
        while (_listener.IsListening)
        {
            var context = _listener.GetContext();
            if(_disposed) return;
            ThreadPool.QueueUserWorkItem(_ => {
                try
                {
                    HandleRequest(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }

    private static void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.HttpMethod != "GET") return;
        
        Console.WriteLine($"REQUEST: {request.HttpMethod} {request.Url?.AbsoluteUri}");
        
        var filename = request.Url?.AbsolutePath.TrimStart('/');
        
        if (string.IsNullOrEmpty(filename)) MakeFileListResponse(context);
        else if (Cache.Contains(filename)) MakeCachedResponse(context, filename);
        else if (File.Exists(filename)) MakeSingleFileResponse(context, filename);
        else MakeNotFoundResponse(context);
    }

    private static void MakeNotFoundResponse(HttpListenerContext context)
    {
        MakeTextResponse(context, "File not found", true);
    }

    private static void MakeCachedResponse(HttpListenerContext context, string filename)
    {
        MakeBytesResponse(context, (Cache.Get(filename) as byte[])!);
    }

    private static void MakeFileListResponse(HttpListenerContext context)
    {
        var files = Directory.GetFiles(Directory.GetCurrentDirectory());
        var htmlString = new StringBuilder();
        htmlString.Append("<html><body>");
        foreach (var filename in files)
        {
            var relativePath = filename.Replace(Directory.GetCurrentDirectory(), "").TrimStart('/');
            htmlString.Append("<a href=\"" + relativePath + "\">" + relativePath + "</a><br>");
        }
        htmlString.Append("</body></html>");
        MakeTextResponse(context, htmlString.ToString());
    }
    private static void MakeSingleFileResponse(HttpListenerContext context, string filename)
    {
        var buffer = File.ReadAllBytes(filename);
        Cache.Add(filename, buffer, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) });
        MakeBytesResponse(context, buffer);
    }

    private static void MakeTextResponse(HttpListenerContext context, string responseContent, bool badRequest = false)
    {
        var response = context.Response;
        var buffer = Encoding.UTF8.GetBytes(responseContent);
        response.ContentLength64 = buffer.Length;
        var outputString = response.OutputStream;
        outputString.Write(buffer, 0, buffer.Length);
        response.ContentType = "text/html";
        if (badRequest)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.StatusDescription = "Bad Request";
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.StatusDescription = "OK";
        }
        response.ContentEncoding = Encoding.UTF8;
        outputString.Close();
        response.Close();
    }

    private static void MakeBytesResponse(HttpListenerContext context, byte[] responseContent)
    {
        var response = context.Response;
        response.ContentLength64 = responseContent.Length;
        var outputString = response.OutputStream;
        outputString.Write(responseContent, 0, responseContent.Length);
        response.ContentType = "application/octet-stream";
        response.StatusCode = (int)HttpStatusCode.OK;
        response.StatusDescription = "OK";
        response.ContentEncoding = Encoding.UTF8;
        outputString.Close();
        response.Close();
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _listener.Stop();
            _listenerThread.Join();
            _listener.Close();
        }
        _disposed = true;
    }
}