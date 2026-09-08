using System;
using System.Collections.Generic;
using System.Text;

namespace Compartilhei.Infrastructure.Images.Configuration
{
    public sealed class ImageProcessingOptions
    {
        public const string SectionName = "ImageProcessing";

        public int DisplayMaxWidth { get; init; } = 1920;

        public int DisplayMaxHeight { get; init; } = 1920;

        public int ThumbnailMaxWidth { get; init; } = 400;

        public int ThumbnailMaxHeight { get; init; } = 400;

        public int DisplayQuality { get; init; } = 85;

        public int ThumbnailQuality { get; init; } = 80;
    }
}
