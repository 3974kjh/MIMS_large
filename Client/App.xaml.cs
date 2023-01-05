using System;
using Client.Defines;
using Client.Infrastructure;
using Common.Foundation;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.IO;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ClientEngine Engine { get; private set; }

        public App()
        {
            this.Engine = new ClientEngine();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var splashWindow = new OneVueSplashWindow();
                splashWindow.Show();

                if (false == this.Engine.Init())
                {
                    SimpleLogger.Instance()._OutputErrorMsg("Client 초기화에 실패하였습니다.");
                    this.Shutdown();
                    return;
                }
                //스플래쉬 윈도우를 표시한다.

                var appDataFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ClientDefines.PROGRAM_NAME, ClientDefines.LOG_FOLDER_NAME);

                if (false == System.IO.Directory.Exists(appDataFolderPath))
                    System.IO.Directory.CreateDirectory(appDataFolderPath);

                SimpleLogger.Instance().Init(System.IO.Path.Combine(appDataFolderPath, ClientDefines.LOG_FILE_NAME));
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"Client를 시작합니다.");

                AutoMapperManager.Instance().CreateMapInfo();                
            }
            catch (Exception ex)
            {
                SimpleLogger.Instance()._OutputErrorMsg("Client 초기화에 실패하였습니다.");
                this.Shutdown();
            }

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            //engine 종료
            this.Engine.Term();

            //로그 종료
            SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"Client를 종료합니다.");
            SimpleLogger.Instance().Terminate();

            DirectoryInfo downloadTImage = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage");
            DirectoryInfo downloadImage = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "ImageFiles");

            foreach (var file in downloadTImage.GetFiles())
            {
                file.Delete();
            }

            foreach (var file in downloadImage.GetFiles())
            {
                file.Delete();
            }

            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Exception.Message);
            e.Handled = true;

            var sb = new System.Text.StringBuilder();
            sb.Append("프로그램을 진행하는 도중 알 수 없는 오류가 발생하였습니다.\n");
            sb.Append("프로그램 관리자나 제조사로 문의하시기 바랍니다.\n 프로그램이 종료됩니다.");
            sb.Append("\n- 오류 : ");
            sb.Append(e.Exception.Message);

            if (null != this.MainWindow && true == this.MainWindow.IsActive)
            {
                MessageBox.Show(this.MainWindow, sb.ToString(), "알 수 없는 프로그램 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(sb.ToString(), "알 수 없는  프로그램 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SimpleLogger.Instance()._OutputErrorMsg(sb.ToString());

            this.Shutdown();
        }
    }
}
