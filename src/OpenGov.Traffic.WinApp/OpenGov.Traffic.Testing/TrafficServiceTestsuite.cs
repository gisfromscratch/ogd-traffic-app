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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenGov.Traffic.Services;

namespace OpenGov.Traffic.Testing
{
    /// <summary>
    /// Repräsentiert die Testfälle für die Abfrage der aktuellen Verkehrslage.
    /// </summary>
    [TestClass]
    public class TrafficServiceTestsuite
    {
        [TestMethod]
        public void TestQueryCityOfBonn()
        {
            using (var service = new TrafficService())
            {
                var featureCollection = service.Query(@"http://stadtplan.bonn.de/geojson?Thema=19584").Result;
                Assert.IsNotNull(featureCollection, @"Die FeatureCollection muss instanziert sein!");
                foreach (var feature in featureCollection.Features)
                {
                    Assert.IsNotNull(feature, @"Ein jedes Feature muss instanziert sein!");
                    var geometry = feature.Geometry;
                    Assert.IsNotNull(geometry, @"Die Geometrie muss instanziert sein!");
                    var properties = feature.Properties;
                    Assert.IsNotNull(properties, @"Die Eigenschaften müssen instanziert sein!");
                }
            }
        }
    }
}