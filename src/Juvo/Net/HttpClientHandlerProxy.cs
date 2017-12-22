// <copyright file="HttpClientHandlerProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System.Net.Http;

    /// <summary>
    /// Proxy HttpClientHandler for implementing IHttpClientHandler.
    /// </summary>
    public class HttpClientHandlerProxy : HttpClientHandler, IHttpMessageHandler
    {
    }
}
