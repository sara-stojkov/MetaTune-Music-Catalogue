namespace Core.Services.EmailService
{
    public static class MimeTypeConverter
    {
        public static string GetString(this MimeType mime)
        {
            return mime switch
            {
                // Text
                MimeType.TextPlain => "text/plain",
                MimeType.TextHtml => "text/html",
                MimeType.TextCss => "text/css",
                MimeType.TextCsv => "text/csv",
                MimeType.TextXml => "text/xml",
                MimeType.TextJavaScript => "text/javascript",

                // Application
                MimeType.ApplicationOctetStream => "application/octet-stream",
                MimeType.ApplicationJson => "application/json",
                MimeType.ApplicationPdf => "application/pdf",
                MimeType.ApplicationXml => "application/xml",
                MimeType.ApplicationZip => "application/zip",
                MimeType.ApplicationGzip => "application/gzip",
                MimeType.ApplicationFormUrlEncoded => "application/x-www-form-urlencoded",
                MimeType.ApplicationJavascript => "application/javascript",
                MimeType.ApplicationMsWord => "application/msword",
                MimeType.ApplicationMsExcel => "application/vnd.ms-excel",
                MimeType.ApplicationDocx => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                MimeType.ApplicationXlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                MimeType.ApplicationTar => "application/x-tar",
                MimeType.Application7Zip => "application/x-7z-compressed",

                // Image
                MimeType.ImageJpeg => "image/jpeg",
                MimeType.ImagePng => "image/png",
                MimeType.ImageGif => "image/gif",
                MimeType.ImageWebp => "image/webp",
                MimeType.ImageSvgXml => "image/svg+xml",
                MimeType.ImageBmp => "image/bmp",
                MimeType.ImageTiff => "image/tiff",
                MimeType.ImageAvif => "image/avif",
                MimeType.ImageHeif => "image/heif",

                // Audio
                MimeType.AudioMpeg => "audio/mpeg",
                MimeType.AudioOgg => "audio/ogg",
                MimeType.AudioWav => "audio/wav",
                MimeType.AudioWebm => "audio/webm",
                MimeType.AudioFlac => "audio/flac",
                MimeType.AudioAac => "audio/aac",
                MimeType.AudioMidi => "audio/midi",

                // Video
                MimeType.VideoMp4 => "video/mp4",
                MimeType.VideoOgg => "video/ogg",
                MimeType.VideoWebm => "video/webm",
                MimeType.VideoQuicktime => "video/quicktime",
                MimeType.VideoAvi => "video/x-msvideo",
                MimeType.VideoMatroska => "video/x-matroska",

                // Multipart
                MimeType.MultipartMixed => "multipart/mixed",
                MimeType.MultipartAlternative => "multipart/alternative",
                MimeType.MultipartRelated => "multipart/related",
                MimeType.MultipartFormData => "multipart/form-data",
                MimeType.MultipartSigned => "multipart/signed",

                // Message
                MimeType.MessageRfc822 => "message/rfc822",

                _ => throw new ArgumentOutOfRangeException(nameof(mime), mime, null)
            };
        }
    }
}
