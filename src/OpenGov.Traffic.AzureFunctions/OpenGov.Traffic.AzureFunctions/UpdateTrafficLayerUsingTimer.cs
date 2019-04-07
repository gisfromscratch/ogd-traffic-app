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

using System;
using System.Threading.Tasks;
using Anywhere.ArcGIS;
using Anywhere.ArcGIS.Common;
using Anywhere.ArcGIS.GeoJson;
using Anywhere.ArcGIS.Operation;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OpenGov.Traffic.Services;

namespace OpenGov.Traffic.AzureFunctions
{
    /// <summary>
    /// Aktualisiert regelm‰ﬂig die Straﬂenverkehrslage eines Feature Service Layer.
    /// </summary>
    public static class UpdateTrafficLayerUsingTimer
    {
        private static readonly TrafficService TrafficServiceInstance;

        static UpdateTrafficLayerUsingTimer()
        {
            TrafficServiceInstance = new TrafficService();
        }

        /// <summary>
        /// Aktualisiert die Straﬂenverkehrslage eines Feature Service Layer.
        /// Die Zeitzone kann per WEBSITE_TIME_ZONE in den Anwendungseinstellungen z.B. auf "W. Europe Standard Time" gesetzt werden.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("UpdateTrafficLayerUsingTimer")]
        public static async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo timer, ILogger log)
        {

#if DEBUG
            log.LogInformation("Update traffic layer . . .");
#endif
            var trafficUrl = Environment.GetEnvironmentVariable(@"traffic.url");
#if DEBUG
            log.LogInformation($"Connecting to {trafficUrl}");
#endif

            var portalUrl = Environment.GetEnvironmentVariable(@"portal.url");
            var appId = Environment.GetEnvironmentVariable(@"portal.appid");
            var clientId = Environment.GetEnvironmentVariable(@"portal.clientid");
            var featureService = Environment.GetEnvironmentVariable(@"portal.featureservice");

            try
            {
                var roadFeatureCollection = await TrafficServiceInstance.Query(trafficUrl);
                var wgs84 = new Crs { Type = @"EPSG", Properties = new CrsProperties { Wkid = 4326 } };
                roadFeatureCollection.CoordinateReferenceSystem = wgs84;
                var roadFeatures = roadFeatureCollection.ToFeatures();

                using (var gateway = new PortalGateway(portalUrl, tokenProvider: new ArcGISOnlineAppLoginOAuthProvider(appId, clientId)))
                {
#if DEBUG
                    var info = await gateway.Info();
                    log.LogInformation($"Connecting to {info.FullVersion}");
#endif
                    var featureServiceEndpoint = featureService.AsEndpoint();
                    var queryAllIds = new QueryForIds(featureServiceEndpoint);
                    queryAllIds.Where = @"1=1";
                    var queryAllIdsResult = await gateway.QueryForIds(queryAllIds);
                    var deleteAll = new ApplyEdits<IGeometry>(featureServiceEndpoint);
                    deleteAll.Deletes.AddRange(queryAllIdsResult.ObjectIds);
                    var deleteAllResult = await gateway.ApplyEdits(deleteAll);

                    var addRoads = new ApplyEdits<IGeometry>(featureServiceEndpoint);
                    foreach (var roadFeature in roadFeatures)
                    {
                        roadFeature.Geometry.SpatialReference = SpatialReference.WGS84;
                        var serviceDateTime = (DateTime)roadFeature.Attributes[@"auswertezeit"];
                        roadFeature.Attributes[@"auswertezeit"] = DateTimeUtils.ConvertServiceTimeToUniversalTime(serviceDateTime);
                        addRoads.Adds.Add(roadFeature);
                    }
                    var addRoadsResult = await gateway.ApplyEdits(addRoads);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }
    }
}
