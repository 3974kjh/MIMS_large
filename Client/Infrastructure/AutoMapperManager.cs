using AutoMapper;
using Client.Model;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Infrastructure
{
    public class AutoMapperManager
    {
        private static readonly object _locker = new object();

        private static volatile AutoMapperManager _instance;

        public AutoMapperManager()
        {

        }

        public static AutoMapperManager Instance()
        {
            if (null == _instance)
            {
                lock (_locker)
                {
                    _instance = new AutoMapperManager();
                }
            }
            return _instance;
        }

        public IMapper Mapper { get; set; }

        public void CreateMapInfo()
        {
            Mapper = null;

            var _config = new MapperConfiguration(config =>
            {
                config.CreateMap<PatientInfoModel, PatientInfo>().ReverseMap();
                config.CreateMap<ImageInfoModel, ImageInfo>().ReverseMap();
            });

            Mapper = _config.CreateMapper();
        }        
    }
}
