using Core.Utils;
using System;

namespace Core.Model
{
    public class Author(string authorId, string? authorName = null, string? biography = null, string? personId = null)
    {
        private readonly string authorId = authorId;
        private string? authorName = authorName;
        private string? biography = biography;
        private string? personId = personId;

        public string AuthorId => authorId;

        public string? AuthorName
        {
            get => authorName;
            set
            {
                if (value != null && !Validator.IsValidString(value))
                    throw new ArgumentException("Author name cannot be empty or whitespace.");
                authorName = value?.Trim();
            }
        }

        public string? Biography
        {
            get => biography;
            set => biography = value?.Trim();
        }

        public string? PersonId
        {
            get => personId;
            set => personId = value;
        }
    }
}
