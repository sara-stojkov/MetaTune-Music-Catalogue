using System;

namespace Core.Model
{
    public class Task(string taskId, DateTime assignmentDate, bool done, string userId, string? workId = null, string? authorId = null)
    {
        private readonly string taskId = taskId;
        private DateTime assignmentDate = assignmentDate;
        private bool done = done;
        private string userId = userId;
        private string? workId = workId;
        private string? authorId = authorId;

        public string TaskId => taskId;
        public DateTime AssignmentDate { get => assignmentDate; set => assignmentDate = value; }
        public bool Done { get => done; set => done = value; }
        public string UserId { get => userId; set => userId = value; }
        public string? WorkId { get => workId; set => workId = value; }
        public string? AuthorId { get => authorId; set => authorId = value; }
    }
}
