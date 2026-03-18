/*
 
 This file contains the code that allows the frontend to communicate with the backend.
 This file was created by FloatingJacob for use with aMogusManager.
 
*/



namespace aMogusManager
{
    class FrontendBackend : WebSocketBehavior
    {

        public static void startFrontendBackend()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Info] Backend ready."); // Wheeeeeeee, logging.
            Console.ForegroundColor = ConsoleColor.White;

            WebSocketServer wssv = new("ws://localhost:6741");
            wssv.AddWebSocketService<FrontendBackend>("/backend"); // It took me forever to figure out how to do this lol
            wssv.Start();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            string name;
            string version;
            int modID;
            string modIDStr;
            dynamic m = JObject.Parse(e.Data);
            Console.WriteLine($"[Frontend] {m.action} requested");

            switch ((string)m.action)
            {
                case "runMod":
                    modIDStr = m.modID;
                    if (string.IsNullOrEmpty(modIDStr) || !int.TryParse(modIDStr, out modID)) Send("Invalid Paramaters");
                    else Program.runMod(modID); Send("gud");
                    break;
                case "removeMod":
                    modIDStr = m.modID;
                    if (string.IsNullOrEmpty(modIDStr) || !int.TryParse(modIDStr, out modID)) Send("Invalid Paramaters");
                    else instanceStuffs.removeMod(modID).Wait(); Send("gud");
                    break;
                case "installModded":
                    name = m.name;
                    version = m.version;
                    string zipMod = m.zipMod;
                    instanceStuffs.installMod(zipMod, name, version).Wait();
                    Send("gud");
                    break;
                case "installPlugin":
                    modID = m.modID;
                    string zipPlugin = m.zipPlugin;
                    instanceStuffs.installPlugin(modID, zipPlugin).Wait();
                    Send("gud");
                    break;
                case "installVanilla":
                    name = m.name;
                    version = m.version;
                    instanceStuffs.installVanilla(name, version).Wait();
                    Send("gud");
                    break;
                case "changePath":
                    string newPath = m.newPath;
                    Program.changePath(newPath).Wait();
                    Send("gud");
                    break;
                case "mods":
                    Send(File.ReadAllText("mods.json"));
                    break;
                case "versions":
                    Send(File.ReadAllText("versions.json"));
                    break;
            }
        }

        protected override void OnOpen()
        {
            Send("HELLO"); // Send a HELLO message to the frontend so it knows its connected.
            Console.WriteLine("[INFO] Frontend connected!");
        }
    }
}