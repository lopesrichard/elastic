using System;
using Elastic.Models;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;
using Nest.JsonNetSerializer;

namespace Elastic.Data
{
    public class ClientProvider
    {
        private IConfiguration _config;

        public ClientProvider(IConfiguration config)
        {
            _config = config;
        }

        public ElasticClient Get()
        {

            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var settings = new ConnectionSettings(pool, sourceSerializer: JsonNetSerializer.Default);

            // settings.DefaultIndex("motoristas");
            settings.BasicAuthentication(_config["Elastic:Username"], _config["Elastic:Password"]);
            settings.EnableDebugMode();
            settings.DefaultMappingFor<Motorista>(mapping => mapping.IdProperty(motorista => motorista.Nome));

            return new ElasticClient(settings);
        }
    }
}