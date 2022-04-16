namespace RedisDataTransit.Models
{
    public class DataTransit
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
        public string? AuthorId { get; set; }
        public DataTransit(string data,string? authorId)
        {
            Data = data;
            Id = Guid.NewGuid();
            AuthorId = authorId;
        }
        public DataTransit()
        {
            Id = Guid.NewGuid();
        }

    }
}
