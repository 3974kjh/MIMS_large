using Client.Defines;
using Client.Model;
using Common;
using Common.Foundation;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static Common.AccountGrpcService;
using static Common.FileTransferGrpcService;

namespace Client.Infrastructure
{
    public class DataManager
    {
        CancellationTokenSource _tokenSource;
        private DispatcherTimer _timer = null;
        private static object lockObj = new object();
        public DataManager()

        {
            _tokenSource = new CancellationTokenSource();
        }

        public bool Init()
        {
            StartTimer();
            return true;
        }

        public bool IsConnectedserver()
        {
            bool success = true;

            var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

            if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
            {
                MessageBox.Show("서버와 연결이 끊겼습니다.");
                success = false;
                return success;
            }

            return success;
        }

        public bool IsImageConnectedserver()
        {
            bool success = true;

            var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

            if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
            {
                MessageBox.Show("이미지를 표시할 수 없습니다.");
                success = false;
                return success;
            }

            return success;
        }

        public void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 30, 0); // 30분마다 타이머
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

            if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
            {
                MessageBox.Show("서버와 연결이 끊겼습니다.");
                return;
            }
        }

        public bool IsServerReadyCheck(Channel channel)
        {
            channel.ConnectAsync();
            return channel.State == ChannelState.Ready;
        }

        public void CreateFolder() // CreateFolder
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory + @"\PatientThumbnailImage";

            if (false == Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public int GetNewPatientNumber()
        {
            int result = 0;

            try
            {
                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return result;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                var response = client.GetNewPatientNumber(new Google.Protobuf.WellKnownTypes.Empty(), null, null, _tokenSource.Token);

                if (null == response)
                    throw new ApplicationException();

                result = response.CurNum;

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자번호] 최신화");
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return result;
        }

        public bool InsertPortrait(PatientInfoModel imageInfo)                                                   // 얼굴 사진 등록 하는 함수
        {
            bool success = false;

            try
            {
                if (null == imageInfo)
                    throw new ApplicationException("imageInfo is null");

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                client.InsertPortrait(new Common.FileInfo { FileName = imageInfo.PatientImage, PatientNumber = imageInfo.PatientNumber }, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return success;
        }

        public List<PatientInfoModel> GetPatientInfoListByPatientName(string keyword)
        {
            if (true == String.IsNullOrEmpty(keyword))
                return null;

            List<PatientInfoModel> patientList = null;

            try
            {
                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return patientList;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                var response = client.GetPatientInfoListByPatientName(new Common.SearchKeyword { Keyword = keyword }, null, null, _tokenSource.Token);

                if (null == response)
                    throw new ApplicationException();

                patientList = AutoMapperManager.Instance().Mapper.Map<List<PatientInfo>, List<PatientInfoModel>>(response.Patients.ToList());

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자리스트] 호출");
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return patientList;
        }

        public bool InsertPatientInfo(PatientInfoModel patientInfo)                                                     // 입력한 정보 등록하는 함수
        {
            bool success = false;

            try
            {
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자등록] 호출");

                if (null == patientInfo)
                    throw new ApplicationException("patientInfo is null");

                var sendPatient = new Common.PatientInfo();

                sendPatient.PatientNumber = patientInfo.PatientNumber;
                sendPatient.PatientName = patientInfo.PatientName;
                sendPatient.PatientResnum = patientInfo.PatientResnum;
                sendPatient.PatientPhonenum = patientInfo.PatientPhonenum;
                sendPatient.PatientImage = patientInfo.PatientImage;

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                client.InsertPatientInfo(sendPatient, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return success;
        }

        public bool EditPatientInfo(PatientInfoModel patientInfo)                   // 선택한 정보 수정하는 함수
        {
            bool success = false;

            try
            {
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자수정] 호출");

                if (null == patientInfo)
                    throw new ApplicationException("patientInfo is null");

                var sendPatient = new Common.PatientInfo();

                sendPatient.PatientNumber = patientInfo.PatientNumber;
                sendPatient.PatientName = patientInfo.PatientName;
                sendPatient.PatientResnum = patientInfo.PatientResnum;
                sendPatient.PatientPhonenum = patientInfo.PatientPhonenum;
                sendPatient.PatientImage = patientInfo.PatientImage;

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                client.EditPatientInfo(sendPatient, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return success;
        }

        public bool DeletePatientInfoList(int patientNumber)                                                 // 선택한 정보 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (0 == patientNumber)
                    throw new ApplicationException("database is null");

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);
                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자삭제] 호출");

                client.DeletePatientInfoList(new Common.Patient { PatientNumber = patientNumber }, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return success;
        }

        /// <summary>
        /// 이미지 저장하는 부분
        /// </summary>

        public async Task UploadFile(string filePath, string fileName)
        {
            if (null == filePath)
                return;

            /*var watch = new Stopwatch();*/
            var uploadFileSize = 0;

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[파일 업로드 서비스] 호출");

                //watch.Start();

                var meta = new Metadata();
                meta.Add("filename", fileName);

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    throw new ApplicationException("database is null");
                }

                var client = new Common.FileTransferGrpcService.FileTransferGrpcServiceClient(channel);

                using (var response = client.UploadFile(meta, null, tokenSource.Token))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(fileStream))
                        {
                            byte[] byteArray = reader.ReadBytes((int)fileStream.Length);

                            int btSize = byteArray.Length;
                            int buffSize = 1024 * 1024;
                            int lastBiteSize = btSize % buffSize;
                            int currentTimes = 0;
                            int loopTimes = btSize / buffSize;

                            BytesContent contentStruct;

                            while (false == tokenSource.Token.IsCancellationRequested && currentTimes <= loopTimes)
                            {
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
                                    FileName = filePath,
                                    Block = currentTimes,
                                    Content = sbytes,
                                    CurrentSize = sbytes.Length,
                                    TotalSize = btSize,
                                };

                                Thread.Sleep(500);

                                await response.RequestStream.WriteAsync(contentStruct);

                                currentTimes++;

                                uploadFileSize += sbytes.Length;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }
            finally
            {               
                tokenSource.Dispose();
                tokenSource = null;
            }
        }

        public string TUploadFile(string filePath, string fileName)
        {
            Image image = Image.FromFile(filePath);
            Image thumb = image.GetThumbnailImage(100, 115, () => false, IntPtr.Zero);
            string thumbnailFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage", Path.GetFileName(filePath) + Path.GetExtension(filePath));

            thumb.Save(thumbnailFilePath);
            image.Dispose();
            thumb.Dispose();
            return thumbnailFilePath;
        }

        public async Task InsertImageInfo(string filePath, int imageCategory, PatientInfoModel _patientInfo)          // 이미지 정보 등록하는 함수      
        {
            try
            {
                if (null == _patientInfo)
                    throw new ApplicationException("imageInfo is null");

                string extension = Path.GetExtension(filePath);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + extension;

                await UploadFileAsync(filePath, fileName);

                var sendImage = new Common.ImageInfo();

                sendImage.PatientNumber = _patientInfo.PatientNumber;
                sendImage.ImageCategory = imageCategory;
                sendImage.ImageID = fileName;
                sendImage.TImageID = "T" + fileName;

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자영상등록] 호출");

                client.InsertImageInfo(sendImage, null, null, _tokenSource.Token);

            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[InsertImageInfo] " + e.Message);
            }

        }

        private async Task UploadFileAsync(string filePath, string fileName)
        {
            await UploadFile(filePath, fileName);
            await UploadFile(TUploadFile(filePath, "T" + fileName), "T" + fileName);
        }

        public async Task<List<ImageInfoModel>> GetTimageInfoList(int patientNumber)                             // DB에서 썸네일 영상 다운받고 썸네일이미지 리스트 모두 불러오는 함수
        {
            //List<ImageInfoModel> patientImageList = null;
            List<ImageInfoModel> outPatientImageLIst = new List<ImageInfoModel>();

            try
            {
                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return outPatientImageLIst;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                var response = client.GetTimageInfoList(new Common.Patient { PatientNumber = patientNumber }, null, null, _tokenSource.Token);

                if (null == response)
                    throw new ApplicationException();

                //patientImageList = AutoMapperManager.Instance().Mapper.Map<List<ImageInfo>, List<ImageInfoModel>>(response.Images.ToList());

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자영상리스트] 호출");

                foreach (var document in response.Images)
                {
                    await DownloadAsync(document.TImageID);

                    ImageInfoModel patientImage = new ImageInfoModel();

                    patientImage.TImagePath = Path.Combine(System.Environment.CurrentDirectory, "ImageFiles" , document.TImageID);
                    patientImage.ImageDate = DateTime.Parse(document.ImageDate);           
                    patientImage.ImageCategory = document.ImageCategory;
                    patientImage.PatientNumber = document.PatientNumber;
                    patientImage.ImageID = document.ImageID;
                    patientImage.TImageID = document.TImageID;

                    outPatientImageLIst.Add(patientImage);                                    
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }

            return outPatientImageLIst;
        }

        //private async void Test()
        //{
        //    await Upload();
        //    await DownLoad();
        //}
        //private async Task DownLoad()
        //{
        //    await Task.Run(() => { });
        //}
        //private  async Task Upload()
        //{
        //    await Task.Run(() => { });
        //}

        public async Task DownloadImage(string fileName)                                     // 이미지 파일 다운로드하는 함수	
        {
            var downloadFileSize = 0;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            try
            {
                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[파일 다운로드 서비스] 시작");

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    throw new ApplicationException("database is null");
                }

                var client = new Common.FileTransferGrpcService.FileTransferGrpcServiceClient(channel);

                using (var response = client.DownloadFile(new Common.FileInfo() { FileName = fileName }, null, null, tokenSource.Token))
                {
                    var directoryPath = Path.Combine(System.Environment.CurrentDirectory, "ImageFiles");

                    if (false == Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                    {
                        while (false == tokenSource.Token.IsCancellationRequested && await response.ResponseStream.MoveNext())
                        {
                            var contentStruct = response.ResponseStream.Current;

                            contentStruct.Content.WriteTo(fs);

                            downloadFileSize += contentStruct.CurrentSize;

                            var percentValue = (int)(100.0 * downloadFileSize / (contentStruct.TotalSize * 1.0));
                        }
                    }
                }

                if (true == tokenSource.Token.IsCancellationRequested)
                {
                    SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[파일 다운로드 서비스] 취소");
                }
                else
                {
                    SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[파일 다운로드 서비스] 완료");
                }
                tokenSource.Dispose();
                tokenSource = null;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg(e.Message);
            }
        }

        public async Task<string> GetImagePath(string imageID)                             // DB에서 원본 영상 다운받고 원본이미지 주소 출력하는 함수
        {
            string path = string.Empty;

            try
            {
                await DownloadAsync(imageID);
                path = Path.Combine(System.Environment.CurrentDirectory, "ImageFiles", imageID);
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[GetImagePath] " + e.Message);
            }
            return path;
        }

        private async Task DownloadAsync(string Id)
        {
            await DownloadImage(Id);
        }

        public bool DeleteOneImageInfoLIst(string tImageID)                                        // 선택한 이미지 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (true == string.IsNullOrEmpty(tImageID))
                    throw new ApplicationException("TImageID is null");

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자삭제] 호출");

                client.DeleteOneImageInfoLIst(new Common.CheckId { ImageID = tImageID }, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteOneImageInfoLIst] " + e.Message);
            }

            return success;
        }

        public void Term()
        {
            _timer.Stop();
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        public bool DeleteAllImageInfoLIst(int patientNumber)                                             // 선택한 이미지정보 모두 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (0 >= patientNumber)
                    throw new ApplicationException("patientNumber is incorrect");

                var channel = new Channel(ClientDefines.SERVER_IP + ":" + ClientDefines.SERVER_PORT, ChannelCredentials.Insecure);

                if (false == IsServerReadyCheck(channel))/* SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.ERROR, "서버연결 X"*/
                {
                    MessageBox.Show("서버와 연결이 끊겼습니다.");
                    return success;
                }

                var client = new Common.AccountGrpcService.AccountGrpcServiceClient(channel);

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, "[환자삭제버튼] 클릭");

                client.DeleteAllImageInfoLIst(new Common.Patient { PatientNumber = patientNumber }, null, null, _tokenSource.Token);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteAllImageInfoLIst] " + e.Message);
            }

            return success;
        }
    }
}