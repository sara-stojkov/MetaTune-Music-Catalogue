namespace Core.Services.EmailService
{
    /// <summary>
    /// Common MIME types used for file attachments, HTTP responses, and email messages.
    /// </summary>
    public enum MimeType
    {
        // 📝 Text
        TextPlain,              // text/plain
        TextHtml,               // text/html
        TextCss,                // text/css
        TextCsv,                // text/csv
        TextXml,                // text/xml
        TextJavaScript,         // text/javascript (legacy)

        // 🧠 Application
        ApplicationOctetStream, // application/octet-stream
        ApplicationJson,        // application/json
        ApplicationPdf,         // application/pdf
        ApplicationXml,         // application/xml
        ApplicationZip,         // application/zip
        ApplicationGzip,        // application/gzip
        ApplicationFormUrlEncoded, // application/x-www-form-urlencoded
        ApplicationJavascript,  // application/javascript
        ApplicationMsWord,      // application/msword
        ApplicationMsExcel,     // application/vnd.ms-excel
        ApplicationDocx,        // application/vnd.openxmlformats-officedocument.wordprocessingml.document
        ApplicationXlsx,        // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        ApplicationTar,         // application/x-tar
        Application7Zip,        // application/x-7z-compressed

        // 🖼 Image
        ImageJpeg,              // image/jpeg
        ImagePng,               // image/png
        ImageGif,               // image/gif
        ImageWebp,              // image/webp
        ImageSvgXml,            // image/svg+xml
        ImageBmp,               // image/bmp
        ImageTiff,              // image/tiff
        ImageAvif,              // image/avif
        ImageHeif,              // image/heif

        // 🔊 Audio
        AudioMpeg,              // audio/mpeg
        AudioOgg,               // audio/ogg
        AudioWav,               // audio/wav
        AudioWebm,              // audio/webm
        AudioFlac,              // audio/flac
        AudioAac,               // audio/aac
        AudioMidi,              // audio/midi

        // 📽 Video
        VideoMp4,               // video/mp4
        VideoOgg,               // video/ogg
        VideoWebm,              // video/webm
        VideoQuicktime,         // video/quicktime
        VideoAvi,               // video/x-msvideo
        VideoMatroska,          // video/x-matroska

        // 📦 Multipart
        MultipartMixed,         // multipart/mixed
        MultipartAlternative,   // multipart/alternative
        MultipartRelated,       // multipart/related
        MultipartFormData,      // multipart/form-data
        MultipartSigned,        // multipart/signed

        // 📨 Message
        MessageRfc822           // message/rfc822
    }


}
