using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace CS451Lab01
{
    class Program
    {
        private static async Task Listen()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            Console.WriteLine("Ожидание подключений...");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Stream output = response.OutputStream;
                if (request.HttpMethod.Equals("GET"))
                {
                    var filename = request.QueryString.Get("file");
                    if (!File.Exists(filename))
                    {
                        response.StatusCode = 404;
                        string responseString = "<HTML><BODY><B>404</B> NOT FOUND</BODY></HTML>";
                        byte[] data = System.Text.Encoding.UTF8.GetBytes(responseString);

                        response.ContentLength64 = data.Length;
                        output.Write(data, 0, data.Length);
                    }
                    else
                    {
                        Console.WriteLine($"Got request query {filename}");
                        byte[] data = new byte[0];
                        var ext = Path.GetExtension(filename);
                        if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                        {
                            response.ContentType = $"image/{ext.Substring(1)}";
                            data = File.ReadAllBytes(filename);
                        }
                        if (ext == ".html" || ext == ".txt")
                        {
                            if (ext == ".html")
                            {
                                response.ContentType = "text/html";
                            }
                            else
                            {
                                response.ContentType = "text/plain";
                            }
                            string responseString = File.ReadAllText(filename);
                            data = System.Text.Encoding.UTF8.GetBytes(responseString);
                        }
                        response.ContentLength64 = data.Length;
                        output.Write(data, 0, data.Length);
                        response.StatusCode = 200;
                    }
                }

                output.Close();
                await File.AppendAllTextAsync(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logger.txt"),
                    $"{DateTime.Now.ToString("dd.MM.yyyy")};{request.RemoteEndPoint};{request.RawUrl};{response.StatusCode}\n"
                );
            }
        }

        static void Main(string[] args)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            Listen().Wait();
        }
    }
}
