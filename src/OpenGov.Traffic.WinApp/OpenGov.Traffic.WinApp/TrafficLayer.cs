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

using Anywhere.ArcGIS.Common;
using Anywhere.ArcGIS.GeoJson;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using OpenGov.Traffic.Services;
using System.Drawing;

namespace OpenGov.Traffic.WinApp
{
    /// <summary>
    /// Stellt einen Layer für die Visualisierung der Straßenverkehrslage dar.
    /// </summary>
    public class TrafficLayer
    {
        private readonly TrafficService _service;
        private readonly string _url;
        private readonly Esri.ArcGISRuntime.Geometry.SpatialReference _mapSpatialReference;

        /// <summary>
        /// Erzeugt eine neue Instanz und verwendet den angegebenen Endpunkt.
        /// </summary>
        /// <param name="url">der Endpunkt eines Dienstes</param>
        /// <param name="mapSpatialReference">der Raumbezug der Karte</param>
        public TrafficLayer(string url, Esri.ArcGISRuntime.Geometry.SpatialReference mapSpatialReference)
        {
            // TODO: Dispose des Dienstes implementieren
            _service = new TrafficService();
            _url = url;
            _mapSpatialReference = mapSpatialReference;
            Overlay = CreateOverlay();
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
            Overlay.Graphics.Clear();

            var featureCollection = await _service.Query(_url);
            foreach (var feature in featureCollection.Features)
            {
                var roadGeometry = feature.Geometry;
                switch (roadGeometry.Type)
                {
                    case @"MultiLineString":
                        var roadGraphic = CreateRoadGraphic(feature, SpatialReferences.Wgs84, _mapSpatialReference);
                        Overlay.Graphics.Add(roadGraphic);
                        break;
                }
            }
        }

        private static Graphic CreateRoadGraphic(GeoJsonFeature<GeoJsonPolygon> roadFeature, Esri.ArcGISRuntime.Geometry.SpatialReference originalSpatialReference, Esri.ArcGISRuntime.Geometry.SpatialReference targetSpatialReference)
        {
            var roadPolyline = CreatePolyline(roadFeature.Geometry.Coordinates, originalSpatialReference);
            if (!originalSpatialReference.IsEqual(targetSpatialReference))
            {
                // In den Raumbezug der Karte projezieren
                roadPolyline = (Esri.ArcGISRuntime.Geometry.Polyline) GeometryEngine.Project(roadPolyline, targetSpatialReference);
            }

            var roadGraphic = new Graphic(roadPolyline, roadFeature.Properties);
            return roadGraphic;
        }

        private static Esri.ArcGISRuntime.Geometry.Polyline CreatePolyline(PointCollectionList parts, Esri.ArcGISRuntime.Geometry.SpatialReference spatialReference)
        {
            var builder = new PolylineBuilder(spatialReference);
            foreach (var part in parts)
            {
                var roadPart = new Part(spatialReference);
                foreach (var vertex in part)
                {
                    var point = CreatePoint(vertex, spatialReference);
                    if (!point.IsEmpty)
                    {
                        roadPart.AddPoint(point);
                    }
                }
                builder.AddPart(roadPart);
            }
            return builder.ToGeometry();
        }

        private static MapPoint CreatePoint(double[] coordinates, Esri.ArcGISRuntime.Geometry.SpatialReference spatialReference)
        {
            if (coordinates.Length < 1)
            {
                return new MapPoint(double.NaN, double.NaN);
            }

            return new MapPoint(coordinates[0], coordinates[1], spatialReference);
        }

        private static GraphicsOverlay CreateOverlay()
        {
            var overlay = new GraphicsOverlay();
            overlay.Renderer = CreateRenderer();
            overlay.Opacity = 0.55;
            return overlay;
        }

        private static Renderer CreateRenderer()
        {
            var renderer = new ClassBreaksRenderer();
            var redSymbol = CreateLineSymbol(Color.Red);
            renderer.DefaultSymbol = redSymbol;
            renderer.FieldName = @"geschwindigkeit";
            var lowestBreak = new ClassBreak(@"Unterste Geschwindigkeit", @"weniger als 20 km/h", 0, 20 - double.Epsilon, redSymbol);
            renderer.ClassBreaks.Add(lowestBreak);
            var yellowSymbol = CreateLineSymbol(Color.Yellow);
            var mediumBreak = new ClassBreak(@"Mittlere Geschwindigkeit", @"weniger als 50 km/h", lowestBreak.MaxValue + double.Epsilon, 50 - double.Epsilon, yellowSymbol);
            renderer.ClassBreaks.Add(mediumBreak);
            var greenSymbol = CreateLineSymbol(Color.Green);
            var highestBreak = new ClassBreak(@"Hohe Geschwindigkeit", @"mindestens 50 km/h", lowestBreak.MaxValue + double.Epsilon, double.MaxValue, greenSymbol);
            renderer.ClassBreaks.Add(highestBreak);
            return renderer;
        }

        private static Symbol CreateLineSymbol(Color lineColor)
        {
            return new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, lineColor, 3);
        }
    }
}
