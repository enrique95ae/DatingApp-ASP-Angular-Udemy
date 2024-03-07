using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")] //This makes entity framework to create the DB with this name instead of the class name
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId {  get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }

    }

}