using System;
using System.IO;
using System.Threading.Tasks;
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenGov.Traffic.Services;
using Anywhere.ArcGIS;
using Anywhere.ArcGIS.Operation;
using Anywhere.ArcGIS.Common;
using Anywhere.ArcGIS.GeoJson;
using System.Collections.Generic;

namespace OpenGov.Traffic.AzureFunctions
{
    /// <summary>
    /// Aktualisiert die Straﬂenverkehrslage eines Feature Service Layer.
    /// </summary>
    public static class UpdateTrafficLayerUsingHttp
    {
        private static readonly TrafficService TrafficServiceInstance;

        static UpdateTrafficLayerUsingHttp()
        {
            TrafficServiceInstance = new TrafficService();
        }

        [FunctionName("UpdateTrafficLayerUsingHttp")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest request,
            ILogger log)
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
                        var localDateTime = (DateTime) roadFeature.Attributes[@"auswertezeit"];
                        roadFeature.Attributes[@"auswertezeit"] = localDateTime.ToUniversalTime();
                        addRoads.Adds.Add(roadFeature);
                    }
                    var addRoadsResult = await gateway.ApplyEdits(addRoads);
                }
                return new OkObjectResult(@"Succeeded");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            /*
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult) new OkObjectResult(@"Succeeded")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
                */
        }
    }
}
