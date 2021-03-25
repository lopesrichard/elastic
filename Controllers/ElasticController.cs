using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using Nest.JsonNetSerializer;

namespace Elastic.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticController : ControllerBase
    {
        private IConfiguration _config;

        public ElasticController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // =====================================================================================
            // ================================== CONFIGURATION ====================================
            // =====================================================================================

            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var settings = new ConnectionSettings(pool, sourceSerializer: JsonNetSerializer.Default);

            // settings.DefaultIndex("example");
            settings.BasicAuthentication(_config["Elastic:Username"], _config["Elastic:Password"]);
            settings.EnableDebugMode();

            settings.DefaultMappingFor<Example>(mapping =>
                mapping.IdProperty(veiculo => veiculo.Id)
            );

            var client = new ElasticClient(settings);

            // =====================================================================================
            // ================================== REMOVE INDICES ===================================
            // =====================================================================================

            client.Indices.Delete("example");

            // =====================================================================================
            // ================================== CREATE INDICES ===================================
            // =====================================================================================

            client.Indices.Create("example");

            // =====================================================================================
            // ================================== INSERT DATA ======================================
            // =====================================================================================

            client.Create<Example>(new Example()
            {
                Id = Guid.NewGuid(),
                Code = "0001"
            }, create => create.Index("example"));

            client.Create<Example>(new Example()
            {
                Id = Guid.NewGuid(),
                Code = "0002"
            }, create => create.Index("example"));

            client.Create<Example>(new Example()
            {
                Id = Guid.NewGuid(),
                Code = "0003"
            }, create => create.Index("example"));

            // =====================================================================================
            // ================================== FETCH ALL DATA ===================================
            // =====================================================================================

            var fetchAllResponse = client.Search<Example>(search => search.Index("example"));
            var first = fetchAllResponse.Documents.First();

            // =====================================================================================
            // ================================== FETCH DATA BY ID =================================
            // =====================================================================================

            var fetchByIdResponse = client.Get<Example>(first.Id, get => get.Index("example"));

            // =====================================================================================
            // ==================================== UPDATE DATA ====================================
            // =====================================================================================

            first.Code = "CHANGE";
            var updateResponse = client.Update<Example>(first.Id, update => update.Index("example").Doc(first));

            // =====================================================================================
            // ================================== REMOVE DATA ======================================
            // =====================================================================================

            var deleteFirstResponse = client.Delete<Example>(first.Id, delete => delete.Index("example"));

            // =====================================================================================
            // ================================ FETCH AFTER DELETE =================================
            // =====================================================================================

            var fetchAfterDeleteResponse = client.Search<Example>(search => search.Index("example"));

            // var query = System.Text.Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);

            return Ok();
        }

        class Document
        {
            public Example Example { get; set; }
        }

        class Example
        {
            public Guid Id { get; set; }
            public string Code { get; set; }
        }
    }
}
