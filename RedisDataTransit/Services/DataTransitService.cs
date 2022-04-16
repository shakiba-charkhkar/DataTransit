using RedisDataTransit.Models;
using RedisDataTransit.Context;
using ExcelDataReader;
using System.Data;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Caching.Distributed;
using RedisDataTransit.Models;
using System.Text;
using System.Text.Json;
using AutoMapper;

namespace RedisDataTransit.Services
{
    public interface IDataTransitService
    {
        Task<bool> TransferData(DataFileModel dataFileModel);
        Task<List<DataTransitViewModel>> GetAllData(string authorId, bool enableCache);


    }
    public class DataTransitService:IDataTransitService
    {
        private readonly MyDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        public DataTransitService(MyDbContext context, IConfiguration configuration, IDistributedCache cache,IMapper mapper)
        {
            _dbContext = context;
            _configuration = configuration;
            _cache = cache;
            _mapper = mapper;
        }
        public async Task<bool> TransferData(DataFileModel dataFileModel)
        {
            try
            {
                IExcelDataReader reader = null;


                //Load file into a stream
                //FileStream stream = System.IO.File.OpenRead(FilePath);

                Stream openReadStream = dataFileModel.File.OpenReadStream();
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                string FilePath = dataFileModel.File.FileName;

                //Must check file extension to adjust the reader to the excel file type
                if (Path.GetExtension(FilePath).Equals(".xls"))
                    reader = ExcelReaderFactory.CreateBinaryReader(openReadStream);
                else if (Path.GetExtension(FilePath).Equals(".xlsx"))
                    reader = ExcelReaderFactory.CreateOpenXmlReader(openReadStream);

                List<DataTransit> parentRow = new List<DataTransit>();
                if (reader != null)
                {
                    //Fill DataSet
                    DataSet content = reader.AsDataSet();
                    DataTable contentTable = content.Tables[0];

                    foreach (DataRow row in contentTable.Rows)
                    {
                        foreach (DataColumn col in contentTable.Columns)
                        {
                            parentRow.Add(new DataTransit(row[col].ToString(), dataFileModel.authorId));
                        }
                    }
                    //parentRow = (from DataRow dr in contentTable.Rows
                    //              select new DataTransit()
                    //              {
                    //                  AuthorId = authorId,
                    //                  Data = dr[0].ToString()
                    //              }).ToList();

                    //Read....
                }


                await _dbContext.BulkDeleteAsync(_dbContext.DataTransits.Where(x => x.AuthorId == dataFileModel.authorId).ToList());

                //To improve the speed of using BulkInsertAsync instead of AddRange
                //await _dbContext.AddRangeAsync(parentRow);

                var bulkConfig = new BulkConfig { SetOutputIdentity = true, BatchSize = 10000 };
                await _dbContext.BulkInsertAsync(parentRow, bulkConfig);

                await _cache.RemoveAsync(dataFileModel.authorId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<List<DataTransitViewModel>> GetAllData(string authorId, bool enableCache)
        {
            List<DataTransit> dataTransitsMatrix = new();
            List<DataTransitViewModel> dataTransits = new List<DataTransitViewModel>();
            if (!enableCache)
            {
                dataTransitsMatrix = _dbContext.DataTransits.Where(x => x.AuthorId == authorId).ToList();
            }
            else
            {
                string cacheKey = authorId;
                // Trying to get data from the Redis cache
                byte[] cachedData = await _cache.GetAsync(cacheKey);


                if (cachedData != null)
                {
                    // If the data is found in the cache, encode and deserialize cached data.
                    var cachedDataString = Encoding.UTF8.GetString(cachedData);
                    dataTransitsMatrix = JsonSerializer.Deserialize<List<DataTransit>>(cachedDataString);


                }
                else
                {
                    // If the data is not found in the cache, then fetch data from database
                    dataTransitsMatrix = _dbContext.DataTransits.Where(x => x.AuthorId == authorId).ToList();

                    // Serializing the data
                    string cachedDataString = JsonSerializer.Serialize(dataTransitsMatrix);
                    var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

                    // Setting up the cache options
                    DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                    // Add the data into the cache
                    await _cache.SetAsync(cacheKey, dataToCache, options);
                }
            }

            dataTransits = _mapper.Map<List<DataTransitViewModel>>(dataTransitsMatrix);
            return dataTransits;
        }
    }
}
