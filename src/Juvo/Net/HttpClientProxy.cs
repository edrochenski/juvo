// <copyright file="HttpClientProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System.Net.Http;

    /// <summary>
    /// Proxy HttpClient for implementing IHttpClient.
    /// </summary>
    public class HttpClientProxy : HttpClient, IHttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy"/> class.
        /// </summary>
        /// <param name="handler">Http message handler.</param>
        public HttpClientProxy(HttpMessageHandler handler)
            : base(handler)
        {
        }
    }
}
