using Core.Utils;
using System;

namespace Core.Model
{
    public class Work(
        string workId,
        string workName,
        DateTime publishDate,
        string workType,
        string genreId,
        List<Author> authors,
        string? workDescription = null,
        string? src = null,
        string? albumId = null)
    {
        private readonly string workId = workId;
        private string workName = workName;
        private DateTime publishDate = publishDate;
        private string workType = workType;
        private string? workDescription = workDescription;
        private string? src = src;
        private string? albumId = albumId;
        private string genreId = genreId;
        private List<Author> authors = authors;

        public string WorkId => workId;

        public string WorkName
        {
            get => workName;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("Work name cannot be empty or whitespace.");
                workName = value.Trim();
            }
        }

        public DateTime PublishDate
        {
            get => publishDate;
            set => publishDate = value;
        }

        public string WorkType
        {
            get => workType;
            set => workType = value.Trim();
        }

        public string? WorkDescription
        {
            get => workDescription;
            set => workDescription = value?.Trim();
        }

        public string? Src
        {
            get => src;
            set => src = value?.Trim();
        }

        public string? AlbumId
        {
            get => albumId;
            set => albumId = value;
        }

        public string GenreId
        {
            get => genreId;
            set => genreId = value;
        }

        public List<Author> Authors
            {
            get => authors;
            set => authors = value;
        }
    }
}
