using System.ComponentModel.DataAnnotations;

namespace RedisDataTransit.Models
{
    public class DataFileModel
    {
        [Display(Name = "فایل")]
        public IFormFile File { get; set; }
        public string authorId { get; set; }
    }
}
