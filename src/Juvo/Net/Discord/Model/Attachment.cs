// <copyright file="Attachment.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Discord attachment.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Gets or sets the name of the attached file.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the height, if the attachment is an image.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the proxied URL of the file.
        /// </summary>
        [JsonProperty(PropertyName = "proxy_url")]
        public string ProxyUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the source URL of the file.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the width, if the attachment is an image.
        /// </summary>
        public int? Width { get; set; }
    }
}
