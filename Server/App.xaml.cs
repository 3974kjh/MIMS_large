using System;
using Common.Foundation;
using Server.Infrastructure;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServerEngine Engine { get; private set; }
        public App()
        {
            this.Engine = new ServerEngine();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var appDataFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sample");

                if (false == System.IO.Directory.Exists(appDataFolderPath))
                    System.IO.Directory.CreateDirectory(appDataFolderPath);

                SimpleLogger.Instance().Init(System.IO.Path.Combine(appDataFolderPath, $"Sample/Server.log"));
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"Server를 시작합니다.");

                if (false == this.Engine.Init())
                {
                    SimpleLogger.Instance()._OutputErrorMsg("Server 초기화에 실패하였습니다.");
                    this.Shutdown();
                    return;
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("Server 초기화에 실패하였습니다.");
                this.Shutdown();
            }

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            //로그 종료
            SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"Server를 종료합니다.");
            SimpleLogger.Instance().Terminate();

            base.OnExit(e);
        }
    }
}
