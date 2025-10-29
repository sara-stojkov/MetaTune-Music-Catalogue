using Core.Utils;
using System;

namespace Core.Model
{
    public class Review(
        string reviewId,
        string content,
        DateTime reviewDate,
        bool isEditable,
        string? editor,
        string userId,
        string? workId = null,
        string? authorId = null)
    {
        private readonly string reviewId = reviewId;
        private string content = content;
        private DateTime reviewDate = reviewDate;
        private bool isEditable = isEditable;
        private string? editor = editor;
        private string userId = userId;
        private string? workId = workId;
        private string? authorId = authorId;

        public string ReviewId => reviewId;

        public string Content
        {
            get => content;
            set
            {
                if (!Validator.IsValidString(value))
                    throw new ArgumentException("Review content cannot be empty or whitespace.");
                content = value.Trim();
            }
        }

        public DateTime ReviewDate { get => reviewDate; set => reviewDate = value; }
        public bool IsEditable { get => isEditable; set => isEditable = value; }
        public string? UserId { get => editor; set => editor = value; }
        public string UserId2 { get => userId; set => userId = value; }
        public string? WorkId { get => workId; set => workId = value; }
        public string? AuthorId { get => authorId; set => authorId = value; }
    }
}
