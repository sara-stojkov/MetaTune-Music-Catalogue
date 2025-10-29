
namespace Core.Model
{
    public class Genre
    {
        private string id;
        private string name;
        private string description;
        private string parentGenreId;
        private List<Genre> subGenres;


        public Genre()
        {
            id = Guid.NewGuid().ToString();
            name = string.Empty;
            description = string.Empty;
            parentGenreId = string.Empty;
            subGenres = new List<Genre>();
        }
        public Genre(string id, string name, string parentGenreId, string description = "", List<Genre> subs = null)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.parentGenreId = parentGenreId;
            if (subs == null) this.subGenres = new List<Genre>();
            else this.subGenres = subs;
        }
        public Genre(string name)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.description = string.Empty;
            this.parentGenreId = string.Empty;
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
        public string ParentGenreId
        {
            get => parentGenreId;
            set => parentGenreId = value;
        } 
        public List<Genre> SubGenres 
        {
            get => subGenres;
            set => subGenres = value;
        }

        public List<Genre> Flat
        {
            get
            {
                var flatList = new List<Genre> { this }; // include the current genre
                if (subGenres != null && subGenres.Count > 0) 
                {
                    foreach (var sub in subGenres)
                    {
                        flatList.AddRange(sub.Flat); // recursively add sub-genres
                    }
                }
                return flatList;
            }
        }
    }
}
