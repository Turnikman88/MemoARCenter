using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MemoARCenter.Services.Helpers
{
    public static class Helper
    {
        public static string GetValidExtension(string extension, List<string> extsToCheck)
        {
            return extsToCheck.Contains(extension) ? extension : null;
        }


        public static IConversion ChangeBitRateAndFormat(this IConversion conversion, long bitRate)
        {
            if (bitRate > 2000000)
            {
                conversion.SetPreset(ConversionPreset.Medium)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .SetOutputFormat(Format.h264)
                    .SetVideoBitrate(2000000);
            }

            return conversion;
        }

        public static IConversion AddAudioStream(this IConversion conversion, IMediaInfo mediaInfo)
        {
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (audioStream != null)
            {
                conversion.AddStream(audioStream);
            }
            return conversion;
        }

        public static string EncodeToSafeBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convert to UTF-8 bytes
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // Convert to Base64 and replace invalid characters
            string base64 = Convert.ToBase64String(bytes);
            return base64.Replace("/", "_").Replace("+", "-").Replace("=", "");
        }
    }
}
