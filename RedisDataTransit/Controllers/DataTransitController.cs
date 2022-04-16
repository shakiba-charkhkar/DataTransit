using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using RedisDataTransit.Models;
using RedisDataTransit.Context;

using RedisDataTransit.Services;
using Microsoft.EntityFrameworkCore;

namespace RedisDataTransit.Controllers
{


    [Route("[controller]")]
    [ApiController]
    public class DataTransitController : ControllerBase
    {

        private readonly IDataTransitService _dataTransitService;
        public DataTransitController(IDataTransitService dataTransitService)
        {

            _dataTransitService = dataTransitService;

        }

        [HttpPost]
        [Route("TransferData")]
        public async Task<bool> TransferData([FromForm] DataFileModel dataFileModel)
        {
            try
            {
                var res=await _dataTransitService.TransferData(dataFileModel);
                return true;
            }
            catch
            {
                return false;
            }
        }

        

        [HttpGet]
        [Route("GetAllData/{authorId}/{enableCache}")]
        public async Task<List<DataTransitViewModel>> GetAllData(string authorId, bool enableCache)
        {
            var data = await _dataTransitService.GetAllData(authorId, enableCache);
            return data;
        }
    }
}





