using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace bh3tool
{
    class FiddlerTool
    {
        public static List<Server_modinfo> modinfos = new List<Server_modinfo>();

        public static bool bh3only = false;

        private const string sSecureEndpointHostname = "localhost";
        private const int iSecureEndpointPort = 7777;
        private readonly ICollection<Session> oAllSessions = new List<Session>();

        public void Start(int port, ToolManager manager)
        {
            int requestcount = 0;
            int responsecount = 0;

            FiddlerApplication.BeforeRequest += (Session oS) =>
            {
                requestcount++;
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [request] request:" + requestcount + " response:" + responsecount);
                bool isbh3url = false;
                foreach (var info in modinfos)
                {
                    if (info.Enabletouch && oS.fullUrl.Contains(info.gameserver_url))
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [request] response server info:" + info.server.ToString());
                        oS.utilCreateResponseAndBypassServer();
                        oS.oResponse.headers.SetStatus(200, "OK");
                        oS.utilSetResponseBody(info.mod_gameserver_rsp);
                        isbh3url = true;
                        break;
                    }
                }



                if (oS.fullUrl.Contains("_compressed/DataVersion.unity3d"))
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [request] response dataversion");
                    oS.utilCreateResponseAndBypassServer();
                    oS.oResponse.headers.SetStatus(200, "OK");
                    if (oS.fullUrl.Contains("android_compressed"))
                        oS.ResponseBody = manager.a_dataversion;
                    if (oS.fullUrl.Contains("iphone_compressed"))
                        oS.ResponseBody = manager.i_dataversion;
                }

                if (oS.fullUrl.Contains("_compressed/data/excel_output_"))
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [request] response excel_output.unity3d");
                    oS.utilCreateResponseAndBypassServer();
                    oS.oResponse.headers.SetStatus(200, "OK");
                    if (oS.fullUrl.Contains("android_compressed"))
                        oS.ResponseBody = manager.a_excel_output;
                    if (oS.fullUrl.Contains("iphone_compressed"))
                        oS.ResponseBody = manager.i_excel_output;
                }

                if (oS.fullUrl.Contains("_compressed/data/setting_"))
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [request] response setting.unity3d");
                    oS.utilCreateResponseAndBypassServer();
                    oS.oResponse.headers.SetStatus(200, "OK");
                    if (oS.fullUrl.Contains("android_compressed"))
                        oS.ResponseBody = manager.a_setting;
                    if (oS.fullUrl.Contains("iphone_compressed"))
                        oS.ResponseBody = manager.i_setting;
                }
                
                if (bh3only && !isbh3url)
                    if (!oS.uriContains("bh3.com") && !oS.uriContains("mihoyo"))
                        oS.Abort();

            };

            FiddlerApplication.BeforeResponse += (Session oS) =>
            {
                responsecount++;
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [response] request:" + requestcount + " response:" + responsecount);
            };

            CONFIG.IgnoreServerCertErrors = true;

            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            FiddlerCoreStartupSettings startupSettings =
                new FiddlerCoreStartupSettingsBuilder()
                    .ListenOnPort((ushort)port)
                    .OptimizeThreadPool()
                    .AllowRemoteClients()
                    .Build();

            FiddlerApplication.Startup(startupSettings);

        }
    }
}