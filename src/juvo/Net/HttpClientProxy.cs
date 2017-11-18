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
    }
}
