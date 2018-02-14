// <copyright file="HackerNewsItem.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Modules.HackerNews
{
    using System.Collections.Generic;

    /// <summary>
    /// Hacker news item. (story, comment, etc..)
    /// </summary>
    public class HackerNewsItem
    {
        /// <summary>
        /// Gets or sets the author's username.
        /// </summary>
        public string By { get; set; }

        /// <summary>
        /// Gets or sets the number of descendants.
        /// </summary>
        public int Descendants { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is dead.
        /// </summary>
        public bool Dead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item has been deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the list of kids.
        /// </summary>
        public List<long> Kids { get; set; }

        /// <summary>
        /// Gets or sets the parent ID.
        /// </summary>
        public long Parent { get; set; }

        /// <summary>
        /// Gets or sets the list of parts.
        /// </summary>
        public List<long> Parts { get; set; }

        /// <summary>
        /// Gets or sets the poll ID.
        /// </summary>
        public long Poll { get; set; }

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the unix-time of creation.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the text value. This could a comment, story or poll text in HTML.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }
    }
}
