using Common;
using Common.Foundation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Server.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class AccountService : AccountGrpcService.AccountGrpcServiceBase
    {
        private readonly IDatabaseProvider _dbProvider;

        public AccountService(IDatabaseProvider dbProvider) => _dbProvider = dbProvider;

        public override Task<CurNumber> GetNewPatientNumber(Empty request, ServerCallContext context)
        {
            CurNumber num = null;

            try
            {
                using (var reader = _dbProvider.GetDataReader(string.Format("Select MAX(PatientNumber) as MaxNumber FROM PATIENT_INFO")))
                {
                    if (null == reader || false == reader.Read())
                        throw new ApplicationException();
                    num = new CurNumber();
                    num.CurNum = (int)reader["MaxNumber"];
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 번호가 없습니다.]" + e.Message);
            }
            return Task.FromResult<CurNumber>(num);
        }

        public override Task<PatientResponse> GetPatientInfoListByPatientName(SearchKeyword searchKeyword, ServerCallContext context)
        {
            var response = new PatientResponse();

            try
            {
                var searchedPatientList = new List<PatientInfo>();

                using (var reader = _dbProvider.GetDataReader(String.Format("SELECT * FROM PATIENT_INFO WHERE PatientName like '%{0}%'", searchKeyword.Keyword)))
                {
                    if (null == reader || false == reader.Read())
                        throw new ApplicationException();
                    do
                    {
                        var patient = new PatientInfo();

                        patient.PatientNumber = (int)reader["PatientNumber"];
                        patient.PatientName = reader["PatientName"].ToString();
                        patient.PatientResnum = reader["PatientResnum"].ToString();
                        patient.PatientPhonenum = reader["PatientPhonenum"].ToString();
                        patient.PatientImage = reader["PatientImage"].ToString();

                        searchedPatientList.Add(patient);
                    } while (reader.Read());
                }
                response.Patients.AddRange(searchedPatientList);
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[관련 환자 정보가 없습니다.]" + e.Message);
            }

            return Task.FromResult<PatientResponse>(response);
        }

        public override Task<Empty> InsertPortrait(FileInfo request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(string.Format("UPDATE PATIENT_INFO SET PatientImage = '{0}' where PatientNumber = '{1}'", request.FileName, request.PatientNumber));         
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 프로필 정보 저장에 실패했습니다.]" + e.Message);
            }

            return Task.FromResult<Empty>(new Empty());
        }

        public override Task<Empty> InsertPatientInfo(PatientInfo request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(String.Format("INSERT INTO PATIENT_INFO (PatientNumber, PatientName, PatientResnum, PatientPhonenum, PatientImage) VALUES ({0},'{1}','{2}','{3}','{4}')", request.PatientNumber, request.PatientName, request.PatientResnum, request.PatientPhonenum, request.PatientImage));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 정보 저장에 실패했습니다.]" + e.Message);
            }
            return Task.FromResult<Empty>(new Empty());
        }

        public override Task<Empty> EditPatientInfo(PatientInfo request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(string.Format("UPDATE PATIENT_INFO SET PatientName = '{0}', PatientResnum = '{1}', PatientPhonenum = '{2}' where PatientNumber = '{3}'", request.PatientName, request.PatientResnum, request.PatientPhonenum, request.PatientNumber));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 정보 수정에 실패했습니다.]" + e.Message);
            }
            return Task.FromResult<Empty>(new Empty());
        }

        public override Task<Empty> DeletePatientInfoList(Patient request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(string.Format("DELETE FROM PATIENT_INFO WHERE PatientNumber = '{0}'", request.PatientNumber));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 정보 삭제에 실패했습니다.]" + e.Message);
            }
            return Task.FromResult<Empty>(new Empty());
        }

        //-------------------------------------------------------------------------------------------------------------------------영상 관련 부분

        public override Task<Empty> InsertImageInfo(ImageInfo request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(String.Format("INSERT INTO PATIENT_IMAGE_INFO (PatientNumber, ImageCategory, ImageID, TImageID, ImageDate) VALUES ({0},'{1}','{2}','{3}','{4}')", request.PatientNumber, request.ImageCategory, request.ImageID, request.TImageID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 영상정보 저장에 실패했습니다.]" + e.Message);
            }
            return Task.FromResult<Empty>(new Empty());
        }

        public override Task<ImageResponse> GetTimageInfoList(Patient request, ServerCallContext context)
        {
            var response = new ImageResponse();

            try
            {
                var searchedImageList = new List<ImageInfo>();

                using (var reader = _dbProvider.GetDataReader(String.Format("SELECT * FROM PATIENT_IMAGE_INFO WHERE PatientNumber = {0}", request.PatientNumber)))
                {                   
                    if (null == reader || false == reader.Read())
                        throw new ApplicationException();
                    do
                    {
                        var image = new ImageInfo();

                        image.PatientNumber = (int)reader["PatientNumber"];
                        image.ImageCategory = (int)reader["ImageCategory"];
                        image.ImageID = reader["ImageID"].ToString();
                        image.TImageID = reader["TImageID"].ToString();
                        var date = (DateTime)reader["ImageDate"];
                        image.ImageDate = date.ToString("yyyy-MM-dd HH:mm:ss");

                        searchedImageList.Add(image);
                    } while (reader.Read());
                }
                response.Images.AddRange(searchedImageList);
            }
            catch (Exception e)
            {
                //SimpleLogger.Instance()._OutputErrorMsg("[관련 환자 이미지 정보가 없습니다.]" + e.Message);
            }

            return Task.FromResult<ImageResponse>(response);
        }

        public override Task<Empty> DeleteOneImageInfoLIst(CheckId request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(string.Format("DELETE FROM PATIENT_IMAGE_INFO WHERE TImageID = '{0}'", request.ImageID));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 이미지 삭제에 실패했습니다.]" + e.Message);
            }

            return Task.FromResult<Empty>(new Empty());
        }

        public override Task<Empty> DeleteAllImageInfoLIst(Patient request, ServerCallContext context)
        {
            try
            {
                _dbProvider.Execute(string.Format("DELETE FROM PATIENT_IMAGE_INFO WHERE PatientNumber = '{0}'", request.PatientNumber));
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[환자 정보 삭제에 실패했습니다.]" + e.Message);
            }

            return Task.FromResult<Empty>(new Empty());
        }
    }
}
