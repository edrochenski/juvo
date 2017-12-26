// <copyright file="WeatherModule.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using System;
    using System.Diagnostics;
    using AngleSharp.Parser.Html;
    using JuvoProcess.Bots;
    using JuvoProcess.Net;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    /// A module for handling weather requests.
    /// </summary>
    public class WeatherModule : IBotModule
    {
/*/ Constants /*/
        private const string DarkSkyForecastUrl = "https://api.darksky.net/forecast/a4b34f1591b0da62f90e1eb28d1eb627/";
        private const string GpsUrl = "https://www.bing.com/search?q={0}+longitude+latitude&qs=n&form=QBLH";
        private const string ModuleName = "weather";

/*/ Fields /*/
        private readonly IJuvoClient juvoClient;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherModule"/> class.
        /// </summary>
        /// <param name="juvoClient">Juvo client.</param>
        public WeatherModule(IJuvoClient juvoClient)
        {
            this.juvoClient = juvoClient ?? throw new System.ArgumentNullException(nameof(juvoClient));
        }

        /*/ Methods /*/

        /// <summary>
        /// Attempts to find coordinates associated with a location.
        /// </summary>
        /// <param name="location">Location text.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="log">Log.</param>
        /// <returns>If successful, a <see cref="GeoCoordinate"/> with valid lat/long values.</returns>
        public static GeoCoordinate? FindCoordinates(string location, IHttpClient httpClient, ILog log)
        {
            var parser = new HtmlParser();
            var gpsQuery = string.Format(GpsUrl, location);

            log.Debug($"[{ModuleName}] Looking up '{location}'");
            using (var gpsResponse = httpClient.GetAsync(gpsQuery).Result)
            {
                var doc = gpsResponse.Content.ReadAsStringAsync().Result;
                using (var gpsDoc = parser.Parse(doc))
                {
                    var input = gpsDoc.QuerySelector("div.tpft div.b_focusTextMedium");
                    if (input != null && !string.IsNullOrEmpty(input.TextContent))
                    {
                        log.Debug($"[{ModuleName}] Using Geo Coords '{input.TextContent}'");
                        return new GeoCoordinate(input.TextContent);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Execute(IBotCommand cmd)
        {
            try
            {
                var cmdParts = cmd.RequestText.Split(' ');
                if (cmdParts.Length > 1)
                {
                    var location = string.Join('+', cmdParts, 1, cmdParts.Length - 1);
                    var geo = FindCoordinates(location, this.juvoClient.HttpClient, this.juvoClient.Log);

                    if (!geo.HasValue)
                    {
                        cmd.ResponseText = $"Could not determine coordinates for '{location}'";
                    }

                    var url = $"{DarkSkyForecastUrl}{geo}";

                    using (var response = this.juvoClient.HttpClient.GetAsync(url).Result)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var dsResponse = JsonConvert.DeserializeObject<DarkSkyResponse>(json);
                        if (dsResponse != null)
                        {
                            var updated = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(dsResponse.Currently.Time);
                            var bearDisplay = this.GetBearingDisplay(dsResponse.Currently.WindBearing);
                            cmd.ResponseText =
                                $"Currently: {dsResponse.Currently.Summary} and " +
                                $"{dsResponse.Currently.Temperature:.0}°F " +
                                $"(Feels like {dsResponse.Currently.ApparentTemperature:.0}°F) " +
                                $"| Dew Point: {dsResponse.Currently.DewPoint:.00}°F, " +
                                $"Humidity: {dsResponse.Currently.Humidity * 100:.0}%, " +
                                $"Pressure: {dsResponse.Currently.Pressure:.00}mb, " +
                                $"UV Index: {dsResponse.Currently.UvIndex:0} " +
                                $"| Wind from the {bearDisplay} " +
                                $"({dsResponse.Currently.WindBearing}°) " +
                                $"at {dsResponse.Currently.WindSpeed}mph " +
                                $"(Gusting at {dsResponse.Currently.WindGust}mph) " +
                                $"| Updated: {updated}";
                        }
                        else
                        {
                            cmd.ResponseText = "No result returned!";
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                cmd.ResponseText = $"Error: {exc.Message}";
                Debug.WriteLine(exc);
            }
        }

        private string GetBearingDisplay(decimal bearing)
        {
            var nol = 0.000M;
            var nne = 22.50M;
            var nea = 45.00M;
            var ene = 67.50M;
            var eas = 90.00M;
            var ese = 112.5M;
            var sea = 135.0M;
            var sse = 157.5M;
            var sou = 180.0M;
            var ssw = 202.5M;
            var swe = 225.0M;
            var wsw = 247.5M;
            var wes = 270.0M;
            var wnw = 292.5M;
            var nwe = 315.0M;
            var nnw = 327.5M;
            var noh = 360.0M;

            var stp = 11.25M;

            if (bearing > noh - stp || bearing < nol - stp)
            {
                return "N";
            }
            else if (bearing > nnw - stp)
            {
                return "NNW";
            }
            else if (bearing > nwe - stp)
            {
                return "NW";
            }
            else if (bearing > wnw - stp)
            {
                return "WNW";
            }
            else if (bearing > wes - stp)
            {
                return "W";
            }
            else if (bearing > wsw - stp)
            {
                return "WSW";
            }
            else if (bearing > swe - stp)
            {
                return "SW";
            }
            else if (bearing > ssw - stp)
            {
                return "SSW";
            }
            else if (bearing > sou - stp)
            {
                return "S";
            }
            else if (bearing > sse - stp)
            {
                return "SSE";
            }
            else if (bearing > sea - stp)
            {
                return "SE";
            }
            else if (bearing > ese - stp)
            {
                return "ESE";
            }
            else if (bearing > eas - stp)
            {
                return "E";
            }
            else if (bearing > ene - stp)
            {
                return "ENE";
            }
            else if (bearing > nea - stp)
            {
                return "NE";
            }
            else if (bearing > nne - stp)
            {
                return "NNE";
            }

            return null;
        }
    }
}
