using System;


namespace Converter.library
{
    public struct VideoCodec
    {
        public static string WebM { get => "libvpx"; }
        public static string Mp4 { get => "libx264"; }
    }
}