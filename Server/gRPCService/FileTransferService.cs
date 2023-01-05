using Common;
using Common.Foundation;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Server.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.gRPCService
{
    public class FileTransferService : FileTransferGrpcService.FileTransferGrpcServiceBase
    {
        private readonly IDatabaseProvider _dbProvider;

        private string _imageFilePath;
        public FileTransferService(IDatabaseProvider dbProvider)
        {
            _dbProvider = dbProvider;
            _imageFilePath = GetFileTablePath("ImageFiles");
        }

        public override async Task DownloadFile(FileInfo fileInfo, IServerStreamWriter<BytesContent> responseStream, ServerCallContext context)
        {
            if (null == fileInfo)
                return;

            var filePath = System.IO.Path.Combine(_imageFilePath, fileInfo.FileName);

            if (false == System.IO.File.Exists(filePath))
                return;

            try
            {
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var reader = new System.IO.BinaryReader(stream))
                    {
                        byte[] byteArray = reader.ReadBytes((int)stream.Length);

                        int btSize = byteArray.Length;
                        int buffSize = 1024 * 1024; //1MB

                        int lastBiteSize = btSize % buffSize;
                        int currentTimes = 0;
                        int loopTimes = btSize / buffSize;


                        BytesContent contentStruct;

                        while (currentTimes <= loopTimes)
                        {
                            if (true == context.CancellationToken.IsCancellationRequested)
                                return;

                            ByteString sbytes;
                            if (currentTimes == loopTimes)
                            {
                                sbytes = ByteString.CopyFrom(byteArray, currentTimes * buffSize, lastBiteSize);
                            }
                            else
                            {
                                sbytes = ByteString.CopyFrom(byteArray, currentTimes * buffSize, buffSize);
                            }

                            contentStruct = new BytesContent
                            {
                                FileName = fileInfo.FileName,
                                Block = currentTimes,
                                Content = sbytes,
                                CurrentSize = sbytes.Length,
                                TotalSize = btSize,
                            };

                            await responseStream.WriteAsync(contentStruct);

                            currentTimes++;
                        }

                        byteArray = null;
                    }
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }
        }

        public override async Task<Empty> UploadFile(IAsyncStreamReader<BytesContent> requestStream, ServerCallContext context)
        {
            string fileName = string.Empty;
            var meta = context.RequestHeaders.Where(p => p.Key == "filename").FirstOrDefault();

            if (null != meta)
                fileName = meta.Value;

            try
            {
                var newFilePath = System.IO.Path.Combine(_imageFilePath, fileName);

                using (var fs = new System.IO.FileStream(newFilePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    while (false == context.CancellationToken.IsCancellationRequested && await requestStream.MoveNext())
                    {
                        requestStream.Current.Content.WriteTo(fs);
                    }
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return await Task.FromResult<Empty>(new Empty());
        }

        public string GetFileTablePath(string tableName)
        {
            string path = string.Empty;

            try
            {
                using (var reader = _dbProvider.GetDataReader(String.Format(@"SELECT FileTableRootPath('{0}') AS FileTableRootPathLong", tableName)))
                {
                    if (null == reader || false == reader.Read())
                        throw new ApplicationException();

                    path = reader["FileTableRootPathLong"].ToString();
                }

            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return path;
        }
    }
}
