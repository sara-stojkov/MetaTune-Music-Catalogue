namespace Core.Services.EmailService
{
    public class Attachment(byte[]? content = null, string name = "", MimeType mimeType = MimeType.TextPlain)
    {
        public byte[]? Content { get; set; } = content;
        public string Name { get; set; } = name;
        public MimeType MimeType { get; set; } = mimeType;

    }
}
