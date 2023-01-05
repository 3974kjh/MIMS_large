using Common.Foundation;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Server.Database;
using Server.Defines;
using Server.gRPCService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Infrastructure
{
    public class ServerEngine
    {
        private Grpc.Core.Server _gRpcServer;
        private IDatabaseProvider _dbProvider;

        public ServerEngine()
        {

        }

        public bool IsInit { get; private set; }

        public bool Init()
        {
            try
            {
                _dbProvider = new MsSqlDbProvider();
                _dbProvider.OpenDB(ServerDefines.DATABASE_CONNECTION);

                this.IsInit = true;
            }
            catch (Exception e)
            {
                this.IsInit = false;

                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return this.IsInit;
        }

        public bool StartServer()
        {
            bool isStarted = false;

            if (null != _gRpcServer)
                _gRpcServer = null;

            try
            {
                var healthService = new HealthServiceImpl();

                healthService.SetStatus("AccountService", Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.NotServing);

                _gRpcServer = new Grpc.Core.Server
                {
                    Services =
                {
                    Common.AccountGrpcService.BindService(new AccountService(_dbProvider)),
                    Common.FileTransferGrpcService.BindService(new FileTransferService(_dbProvider)),
                 //   Common.OsstemSimpleGrpcService.BindService(new OsstemSimpleService(_dbProvider)),
                    Health.BindService(healthService),
                },
                    Ports =
                {
                    new ServerPort(ServerDefines.SERVER_IP, Int32.Parse(ServerDefines.SERVER_PORT), ServerCredentials.Insecure)
                }
                };

                _gRpcServer.Start();

                isStarted = true;

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[서버 시작]==================");


            }
            catch (Exception e)
            {
                isStarted = false;

                SimpleLogger.Instance()._OutputErrorMsg("Server 시작 실패");
            }

            return isStarted;
        }

        public void EndServer()
        {
            try
            {
                if (null == _gRpcServer)
                    return;

                _gRpcServer.KillAsync().Wait();
                _gRpcServer = null;

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[서버 종료]==================");
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("Server 종료 실패");
            }
        }
    }
}
