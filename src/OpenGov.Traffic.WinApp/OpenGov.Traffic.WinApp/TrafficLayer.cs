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

using Esri.ArcGISRuntime.UI;
using OpenGov.Traffic.Services;

namespace OpenGov.Traffic.WinApp
{
    /// <summary>
    /// Stellt einen Layer für die Visualisierung der Straßenverkehrslage dar.
    /// </summary>
    public class TrafficLayer
    {
        private readonly TrafficService _service;
        private readonly string _url;

        /// <summary>
        /// Erzeugt eine neue Instanz und verwendet den angegebenen Endpunkt.
        /// </summary>
        /// <param name="url">der Endpunkt eines Dienstes</param>
        public TrafficLayer(string url)
        {
            _service = new TrafficService();
            _url = url;
            Overlay = new GraphicsOverlay();
        }

        /// <summary>
        /// Overlay, in dem die Straßen visualisiert werden.
        /// </summary>
        public GraphicsOverlay Overlay { get; private set; }

        /// <summary>
        /// Aktualisiert diesen Layer.
        /// </summary>
        internal async void UpdateAsync()
        {
            var featureCollection = await _service.Query(_url);
            foreach (var feature in featureCollection.Features)
            {
                var roadGeometry = feature.Geometry;
                switch (roadGeometry.Type)
                {
                    case @"MultiLineString":
                        break;
                }
            }
        }
    }
}
