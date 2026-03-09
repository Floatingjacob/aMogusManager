/*
 
 This file is the UI backend.
 This file was created by FloatingJacob for use with aMogusManager.
 
*/

namespace aMogusManager
{
    class UIBackend
    {
        public static async Task startBackend()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:6741/"); // Listens on a port that no sane programmer will use (i know the insinuations of that statement.)
            listener.Start();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Info] Backend ready."); // Wheeeeeeee, logging.
            Console.ForegroundColor = ConsoleColor.White;

            while (true)
            {
                var context = await listener.GetContextAsync(); // No deaf ears here...
                if (context != null) _ = HandleHttp(context);
            }
        }
        static async Task HandleHttp(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string path = request.Url.AbsolutePath;


            if (request.HttpMethod == "OPTIONS") // totally not stolen from stackoverflow
            {
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                response.AddHeader("Access-Control-Max-Age", "1728000");
            }
            response.AppendHeader("Access-Control-Allow-Origin", "*"); // I hate CORS.

            // Here begins the "ignore me if you want to stay sane" part of the file
            
            if (path == "/runMod")
            {
                Console.WriteLine($"[GET] {path}");
                int modID;
                string modIDStr = request.QueryString["modID"];

                if (string.IsNullOrEmpty(modIDStr) || !int.TryParse(modIDStr, out modID))
                {
                    SendError(response, 400, "Invalid parameters");
                }
                else
                {
                    Program.runMod(modID);
                    string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                    SendJson(response, msg);
                    response.Close();
                }
            }

            if (path == "/removeMod")
            {
                Console.WriteLine($"[GET] {path}");
                int modID;
                string modIDStr = request.QueryString["modID"];

                if (string.IsNullOrEmpty(modIDStr) || !int.TryParse(modIDStr, out modID))
                {
                    SendError(response, 400, "Invalid parameters");
                }
                else
                {

                    await instanceStuffs.removeMod(modID);
                    string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                    SendJson(response, msg);
                    response.Close();
                }
            }

            if (path == "/installModded" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                string name = request.QueryString["name"];
                string version = request.QueryString["version"];
                string zipMod = request.QueryString["zipMod"];
                await instanceStuffs.installMod(zipMod, name, version);
                string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                SendJson(response, msg);
                response.Close();

            }

            if (path == "/installPlugin" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                string modID = request.QueryString["modID"];

                string zipPlugin = request.QueryString["zipPlugin"];
                await instanceStuffs.installPlugin(modID, zipPlugin);
                string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                SendJson(response, msg);
                response.Close();
            }

            if (path == "/installVanilla" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                string name = request.QueryString["name"];
                string version = request.QueryString["version"];
                await instanceStuffs.installVanilla(name, version);
                string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                SendJson(response, msg);
                response.Close();
            }

            if (path == "/changePath" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                string newPath = request.QueryString["newPath"];
                await Program.changePath(newPath);
                string msg = JsonConvert.SerializeObject("gud", Newtonsoft.Json.Formatting.Indented);
                SendJson(response, msg);
                response.Close();
            }

            if (path == "/mods" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                SendJson(response, File.ReadAllText("mods.json"));
                response.Close();
            }

            if (path == "/versions" && request.HttpMethod == "GET")
            {
                Console.WriteLine($"[GET] {path}");
                SendJson(response, File.ReadAllText("versions.json"));
                response.Close();
            }
        }

        static void SendJson(HttpListenerResponse response, string json) // I probably don't need an entire function for this, but whatever.
        {
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        static void SendError(HttpListenerResponse response, int code, string msg) // Same thing
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            response.StatusCode = code;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
