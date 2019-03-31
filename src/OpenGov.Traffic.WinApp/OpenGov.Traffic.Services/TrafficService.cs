/*
 * Copyright 2019 Jan Tschada
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 
using Anywhere.ArcGIS;
using Anywhere.ArcGIS.GeoJson;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OpenGov.Traffic.Services
{
    /// <summary>
    /// Stellt einen Dienst für das Erfragen der aktuellen Verkehrslage dar.
    /// </summary>
    public class TrafficService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposedValue;

        // Registriert weitere Codepages z.B. für ISO-8859-15.
        static TrafficService()
        {
            var instance = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(instance);
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        public TrafficService()
        {
            _httpClient = HttpClientFactory.Get();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));
            _httpClient.DefaultRequestHeaders.Add(@"User-Agent", @"OpenGov Client");
        }

        /// <summary>
        /// Fragt den entsprechenden Endpunkt ab.
        /// </summary>
        /// <param name="url">der Endpunkt</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns><see cref="FeatureCollection{TGeometry}"/></returns>
        public async Task<FeatureCollection<GeoJsonPolygon>> Query(string url)
        {
            var geoJson = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<FeatureCollection<GeoJsonPolygon>>(geoJson);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (null != _httpClient)
                    {
                        _httpClient.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}