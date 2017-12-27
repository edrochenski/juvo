// <copyright file="WeatherModule.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
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
        private const string GpsCachFile = "gps.dat";
        private const string GpsUrl = "https://www.bing.com/search?q={0}+longitude+latitude&qs=n&form=QBLH";
        private const string ModuleName = "weather";
        private const string UsnoSunMoonUrl = "http://api.usno.navy.mil/rstt/oneday";

/*/ Fields /*/
        private static readonly Dictionary<string, GeoCoordinate> GpsCache;
        private static bool gpsCachLoaded;
        private readonly IJuvoClient juvoClient;

/*/ Constructors /*/

        static WeatherModule()
        {
            GpsCache = new Dictionary<string, GeoCoordinate>();
            gpsCachLoaded = false;
        }

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
        public static GeoCoordinate? FindCoordinates(
            string location,
            IHttpClient httpClient,
            ILog log)
        {
            var cache = TryGpsCache(location);
            if (cache.HasValue)
            {
                return cache.Value;
            }

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
                        var geo = new GeoCoordinate(input.TextContent);
                        AddToCache(location, geo);
                        return geo;
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
                        return;
                    }

                    if (cmdParts[0] == "gps")
                    {
                        cmd.ResponseText = $"Coordinates: {geo.Value}";
                        return;
                    }

                    if (cmdParts[0] == "sky")
                    {
                        this.ExecuteSky(cmd, geo.Value);
                        return;
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
                                $"{dsResponse.Currently.Temperature:0.0}°F " +
                                $"(Feels like {dsResponse.Currently.ApparentTemperature:0.0}°F) " +
                                $"| Dew Point: {dsResponse.Currently.DewPoint:0.00}°F, " +
                                $"Humidity: {dsResponse.Currently.Humidity * 100:0.0}%, " +
                                $"Pressure: {dsResponse.Currently.Pressure:0.00}mb, " +
                                $"UV Index: {dsResponse.Currently.UvIndex:0} " +
                                $"| Wind from the {bearDisplay} " +
                                $"({dsResponse.Currently.WindBearing}°) " +
                                $"at {dsResponse.Currently.WindSpeed:0.0}mph " +
                                $"(Gusting at {dsResponse.Currently.WindGust:0.0}mph) " +
                                $"| Updated: {updated} UTC";
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
                this.juvoClient.Log.Error("[{ModuleName}] Error executing", exc);
                cmd.ResponseText = $"Error: {exc.Message}";
            }
        }

        private static void AddToCache(string location, GeoCoordinate coord)
        {
            Debug.Assert(!string.IsNullOrEmpty(location), $"{nameof(location)} == null|empty");

            Program.Juvo.Log.Debug($"[{ModuleName}] Adding '{location}' @ {coord} to cache");

            if (!gpsCachLoaded && !LoadGpsCache())
            {
                Program.Juvo.Log.Warn($"[{ModuleName}] Could not add GPS to cache");
                return;
            }

            GpsCache.Add(location.ToLowerInvariant(), coord);
            SaveGpsCache();
        }

        private static bool CreateGpsCache()
        {
            try
            {
                var path = Path.Combine(
                    Program.Juvo.SystemInfo.AppDataPath.FullName, GpsCachFile);

                Program.Juvo.Log.Debug($"[{ModuleName}] Creating GPS cache '{path}'");

                using (var file = new BinaryWriter(File.Create(path)))
                {
                    file.Write(0);
                    file.Flush();
                }

                return true;
            }
            catch (Exception exc)
            {
                Program.Juvo.Log.Error($"[{ModuleName}] Failed creating GPS cache file", exc);
                return false;
            }
        }

        private static bool LoadGpsCache()
        {
            try
            {
                Program.Juvo.Log.Debug($"[{ModuleName}] Loading GPS cache");

                var path = Path.Combine(
                    Program.Juvo.SystemInfo.AppDataPath.FullName, GpsCachFile);

                GpsCache.Clear();

                if (!File.Exists(path))
                {
                    if (!CreateGpsCache())
                    {
                        Program.Juvo.Log.Warn($"[{ModuleName}] Skipping GPS cache load");
                        return false;
                    }
                }

                using (var file = new BinaryReader(File.OpenRead(path)))
                {
                    var count = file.ReadInt32();

                    for (var x = 0; x < count; ++x)
                    {
                        GpsCache.Add(
                            file.ReadString(),
                            new GeoCoordinate(file.ReadDouble(), file.ReadDouble()));
                    }

                    Program.Juvo.Log.Info($"[{ModuleName}] Loaded {count} GPS cache entries");
                    gpsCachLoaded = true;
                }

                return true;
            }
            catch (Exception exc)
            {
                Program.Juvo.Log.Error($"[{ModuleName}] Failed loading GPS cache", exc);
                return false;
            }
        }

        private static void SaveGpsCache()
        {
            try
            {
                Program.Juvo.Log.Debug($"[{ModuleName}] Saving GPS cache");

                var path = Path.Combine(
                    Program.Juvo.SystemInfo.AppDataPath.FullName, GpsCachFile);
                using (var file = new BinaryWriter(File.Create(path)))
                {
                    file.Write(GpsCache.Count);
                    foreach (var item in GpsCache)
                    {
                        file.Write(item.Key);
                        file.Write(item.Value.Latitude);
                        file.Write(item.Value.Longitude);
                    }

                    file.Flush();
                }
            }
            catch (Exception exc)
            {
                Program.Juvo.Log.Error($"[{ModuleName}] Failed to save GPS file", exc);
            }
        }

        private static GeoCoordinate? TryGpsCache(string location)
        {
            Debug.Assert(!string.IsNullOrEmpty(location), $"{nameof(location)} == null|empty");

            if (!gpsCachLoaded && !LoadGpsCache())
            {
                return null;
            }

            Program.Juvo.Log.Debug($"[{ModuleName}] Searching cache for '{location}'");
            if (GpsCache.TryGetValue(location.ToLowerInvariant(), out GeoCoordinate geo))
            {
                Program.Juvo.Log.Info($"[{ModuleName}] GPS cache hit on '{location}'");
                return geo;
            }

            return null;
        }

        private void ExecuteSky(IBotCommand cmd, GeoCoordinate coords)
        {
            Debug.Assert(cmd != null, $"{nameof(cmd)} is null");
            this.juvoClient.Log.Debug($"[{ModuleName}] Getting sky results for {coords}");

            try
            {
                // NOTE(er): Dates *need* to be MM/dd/yyyy, was having trouble
                // trying to do this in string.Format, date always came out
                // with hyphens instead of forward slashes
                var now = DateTime.Now;
                var dt = $"{now.Month:00}/{now.Day:00}/{now.Year}";
                var url = $"{UsnoSunMoonUrl}" +
                    $"?date={dt}" +
                    $"&coords={coords.Latitude}N,{coords.Longitude}E";

                this.juvoClient.Log.Debug($"[{ModuleName}] Retrieving '{url}'");
                using (var response = this.juvoClient.HttpClient.GetAsync(url).Result)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var resp = JsonConvert.DeserializeObject<UsnoSunMoonData>(json);
                    if (resp != null && !resp.Error)
                    {
                        var skyMessage = new StringBuilder();

                        // TODO(er): extract into a method
                        if (resp.SunData != null && resp.SunData.Length > 0)
                        {
                            skyMessage.Append("(Sun) ");
                            for (var x = 0; x < resp.SunData.Length; ++x)
                            {
                                var phen = resp.SunData[x];
                                if (x > 0)
                                {
                                    skyMessage.Append(", ");
                                }

                                skyMessage.Append(
                                    $"{this.GetPhenomenaDisplay(phen.Phen)}@{phen.Time}UTC ");
                            }
                        }

                        if (resp.NextSunData != null && resp.NextSunData.Length > 0)
                        {
                            skyMessage.Append("(Next Sun) ");
                            for (var x = 0; x < resp.NextSunData.Length; ++x)
                            {
                                var phen = resp.NextSunData[x];
                                if (x > 0)
                                {
                                    skyMessage.Append(", ");
                                }

                                skyMessage.Append(
                                    $"{this.GetPhenomenaDisplay(phen.Phen)}@{phen.Time}UTC ");
                            }
                        }

                        skyMessage.Append($"(Moon) {resp.CurPhase}/{resp.Fracillum} ");
                        if (resp.MoonData != null && resp.MoonData.Length > 0)
                        {
                            for (var x = 0; x < resp.MoonData.Length; ++x)
                            {
                                var phen = resp.MoonData[x];
                                if (x > 0)
                                {
                                    skyMessage.Append(", ");
                                }

                                skyMessage.Append(
                                    $"{this.GetPhenomenaDisplay(phen.Phen)}@{phen.Time}UTC ");
                            }
                        }

                        if (resp.NextMoonData != null && resp.NextMoonData.Length > 0)
                        {
                            skyMessage.Append("(Next Moon) ");
                            for (var x = 0; x < resp.NextMoonData.Length; ++x)
                            {
                                var phen = resp.NextMoonData[x];
                                if (x > 0)
                                {
                                    skyMessage.Append(", ");
                                }

                                skyMessage.Append(
                                    $"{this.GetPhenomenaDisplay(phen.Phen)}@{phen.Time}UTC ");
                            }
                        }

                        cmd.ResponseText = skyMessage.ToString();
                    }
                    else if (resp.Error)
                    {
                        cmd.ResponseText = resp.Type;
                    }
                    else
                    {
                        cmd.ResponseText = "No results found!";
                    }
                }
            }
            catch (Exception exc)
            {
                this.juvoClient.Log.Error("[{ModuleName}] Error retrieivng sky results", exc);
                cmd.ResponseText = $"Error: {exc.Message}";
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

        private string GetPhenomenaDisplay(string phen)
        {
            switch (phen.ToLowerInvariant())
            {
                case "bc": return "Begin CT";
                case "ec": return "End CT";
                case "l": return "Lower Transit";
                case "r": return "Rise";
                case "s": return "Set";
                case "u": return "Upper Transit";
                case "**": return "OCA horizon";
                case "--": return "OCB horizon";
                case "^^": return "OCA twilight limit";
                case "~~": return "OCB twilight limit";
                default: return string.Empty;
            }
        }
    }
}
