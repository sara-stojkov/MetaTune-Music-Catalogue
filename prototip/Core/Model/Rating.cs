using System;

namespace Core.Model
{
    public class Rating(string ratingId, decimal value, DateTime ratingDate, string userId, string? workId = null, string? authorId = null)
    {
        private readonly string ratingId = ratingId;
        private decimal value = value;
        private DateTime ratingDate = ratingDate;
        private string userId = userId;
        private string? workId = workId;
        private string? authorId = authorId;

        public string RatingId => ratingId;
        public decimal Value { get => value; set => this.value = value; }
        public DateTime RatingDate { get => ratingDate; set => ratingDate = value; }
        public string UserId { get => userId; set => userId = value; }
        public string? WorkId { get => workId; set => workId = value; }
        public string? AuthorId { get => authorId; set => authorId = value; }
    }
}
