namespace Core.Services.EmailService
{
    public class Email(string name = "", string address = "", string subject = "", string content = "")
    {
        public string Name { get; set; } = name;
        public string Address { get; set; } = address;
        public string Subject { get; set; } = subject;
        public string Content { get; set; } = content;
        public List<Attachment> Attachments { get; set; } = [];
    }
}
