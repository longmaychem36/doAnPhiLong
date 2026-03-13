namespace MakerSpot.Models
{
    public class ProductTopic
    {
        public int ProductId { get; set; }
        public int TopicId { get; set; }
        
        public Product Product { get; set; } = null!;
        public Topic Topic { get; set; } = null!;
    }
}
