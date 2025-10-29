
namespace Core.Model
{
    public class Genre
    {
        private string id;
        private string name;
        private string description;
        private List<Genre> subGenres;


        public Genre()
        {
            id = Guid.NewGuid().ToString();
            name = string.Empty;
            description = string.Empty;
            subGenres = new List<Genre>();
        }
        public Genre(string id, string name, string description = "", List<Genre> subGenres = null)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            if (subGenres != null)
            {
                this.subGenres = subGenres;
            }
            else
            {
                this.subGenres = new List<Genre>(); 
            }
        }
        public Genre(string name)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.description = string.Empty;
            this.subGenres = new List<Genre>();
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
        public List<Genre> SubGenres
        {
            get => subGenres;
            set => subGenres = value;
        }
    }
}
