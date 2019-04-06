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

namespace OpenGov.Traffic.AzureFunctions
{
    /// <summary>
    /// Aktualisiert die Straﬂenverkehrslage eines Feature Service Layer.
    /// </summary>
    public static class UpdateTrafficLayerUsingHttp
    {
        [FunctionName("UpdateTrafficLayerUsingHttp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Update traffic layer . . .");

            var trafficUrl = Environment.GetEnvironmentVariable(@"traffic.url");
            log.LogInformation($"Connecting to {trafficUrl}");

            return new OkObjectResult(@"Succeeded");
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
