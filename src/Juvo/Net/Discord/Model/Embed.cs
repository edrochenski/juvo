// <copyright file="Embed.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an embed object.
    /// </summary>
    public class Embed
    {
        /// <summary>
        /// Gets or sets the color code.
        /// </summary>
        public int? Color { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Represents an author in an embed object.
        /// </summary>
        public class EmbedAuthor
        {
            /// <summary>
            /// Gets or sets the icon URL. (only supports http(s) and attachments)
            /// </summary>
            [JsonProperty(PropertyName = "icon_url")]
            public string IconUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the proxied icon URL.
            /// </summary>
            [JsonProperty(PropertyName = "proxy_icon_url")]
            public string ProxyIconUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents an field in an embed object.
        /// </summary>
        public class EmbedField
        {
            /// <summary>
            /// Gets or sets a value indicating whether the field should appear inline.
            /// </summary>
            [JsonProperty(PropertyName = "inline")]
            public bool IsInline { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public string Value { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents an footer in an embed object.
        /// </summary>
        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the icon URL. (only supports http(s) and attachments)
            /// </summary>
            [JsonProperty(PropertyName = "icon_url")]
            public string IconUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the proxied icon URL.
            /// </summary>
            [JsonProperty(PropertyName = "proxy_icon_url")]
            public string ProxyIconUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            public string Text { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents an image in an embed object.
        /// </summary>
        public class EmbedImage
        {
            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            public int? Height { get; set; }

            /// <summary>
            /// Gets or sets the proxied URL.
            /// </summary>
            [JsonProperty(PropertyName = "proxy_url")]
            public string ProxyUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the source. (only supports http(s) and attachments)
            /// </summary>
            public string Url { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            public int? Width { get; set; }
        }

        /// <summary>
        /// Represents a provider in an embed object.
        /// </summary>
        public class EmbedProvider
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents a thumbnail in an embed object.
        /// </summary>
        public class EmbedThumbnail
        {
            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            public int? Height { get; set; }

            /// <summary>
            /// Gets or sets the proxied URL.
            /// </summary>
            [JsonProperty(PropertyName = "proxy_url")]
            public string ProxyUrl { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the source. (only supports http(s) and attachments)
            /// </summary>
            public string Url { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            public int? Width { get; set; }
        }

        /// <summary>
        /// Represents a video in an embed object.
        /// </summary>
        public class EmbedVideo
        {
            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            public int? Height { get; set; }

            /// <summary>
            /// Gets or sets the source. (only supports http(s) and attachments)
            /// </summary>
            public string Url { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            public int? Width { get; set; }
        }
    }
}
