
namespace Core.Model
{
    public class Genre
    {
        private string id;
        private string name;
        private string description;
        private string parentGenreId;


        public Genre()
        {
            id = Guid.NewGuid().ToString();
            name = string.Empty;
            description = string.Empty;
            parentGenreId = string.Empty;
        }
        public Genre(string id, string name, string parentGenreId, string description = "")
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.parentGenreId = parentGenreId;
        }
        public Genre(string name)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.description = string.Empty;
            this.parentGenreId = string.Empty;
        }
        public string Id { get => id; }
        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or empty");
                name = value;
            }
        }
        public string Description
        {
            get => description;
            set => description = value;
        }
        public string ParentGenreId
        {
            get => parentGenreId;
            set => parentGenreId = value;
        }   
    }
}
