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

            // settings.DefaultIndex("motoristas");
            settings.BasicAuthentication(_config["Elastic:Username"], _config["Elastic:Password"]);
            settings.EnableDebugMode();
            settings.DefaultMappingFor<Motorista>(mapping => mapping.IdProperty(motorista => motorista.Nome));

            var client = new ElasticClient(settings);

            // =====================================================================================
            // ================================== REMOVE INDICES ===================================
            // =====================================================================================

            // client.Indices.Delete("motoristas");

            // =====================================================================================
            // ================================== CREATE INDICES ===================================
            // =====================================================================================

            // client.Indices.Create("motoristas", create => create
            //     .Map<Motorista>(map => map
            //         .Properties(properties => properties
            //             .Keyword(keyword => keyword
            //                 .Name("nome")
            //             )
            //             .Object<Empresa>(obj => obj
            //                 .Name("empresa")
            //                 .Properties(properties => properties
            //                     .Keyword(keyword => keyword
            //                         .Name("nome")
            //                     )
            //                 )
            //             )
            //             .Nested<Repouso>(nested => nested
            //                 .Name("repousos")
            //                 .Properties(properties => properties
            //                     .Number(number => number
            //                         .Name("dia")
            //                     )
            //                     .Keyword(keyword => keyword
            //                         .Name("mes")
            //                     )
            //                     .Number(number => number
            //                         .Name("ano")
            //                     )
            //                 )
            //             )
            //         )
            //     )
            // );

            // =====================================================================================
            // ================================== INSERT DATA ======================================
            // =====================================================================================

            // CreateMotoristas(client);

            // =====================================================================================
            // ================================== FETCH ALL DATA ===================================
            // =====================================================================================

            // var fetchAllResponse = client.Search<Motorista>(search => search.Index("motoristas"));
            // var first = fetchAllResponse.Documents.First();

            // =====================================================================================
            // ================================== FETCH DATA BY ID =================================
            // =====================================================================================

            // var fetchByIdResponse = client.Get<Motorista>(first.Id, get => get.Index("motoristas"));

            // =====================================================================================
            // ==================================== UPDATE DATA ====================================
            // =====================================================================================

            // first.Codigo = "CODIGO ALTERADO";
            // var updateResponse = client.Update<Motorista>(first.Id, update => update.Index("motoristas").Doc(first));

            // =====================================================================================
            // ================================== REMOVE DATA ======================================
            // =====================================================================================

            // var deleteFirstResponse = client.Delete<Motorista>(first.Id, delete => delete.Index("motoristas"));

            // =====================================================================================
            // ================================ FETCH AFTER DELETE =================================
            // =====================================================================================

            // var fetchAfterDeleteResponse = client.Search<Motorista>(search => search.Index("motoristas"));

            // =====================================================================================
            // ================================ AGGREGATION ========================================
            // =====================================================================================

            // -------------------------------- EMPRESAS -------------------------------------------

            // var aggregationResponse = client.Search<Motorista>(search => search
            //     .Index("motoristas")
            //     .Aggregations(agg => agg
            //         .Terms("empresa", terms => terms
            //             .Field(motorista => motorista.Empresa.Nome)
            //         )
            //     )
            // );

            // var empresas = aggregationResponse.Aggregations.Terms("empresa");

            // var response = new Dictionary<string, long>();

            // foreach (var item in empresas.Buckets)
            // {
            //     response.Add(item.Key, item.DocCount ?? 0);
            // }

            // -------------------------- REPOUSOS POR MES E ANO ----------------------------------

            var aggregationResponse = client.Search<Motorista>(search => search
                .Index("motoristas")
                .Aggregations(agg => agg
                    .Nested("repousos", nested => nested
                        .Path(motorista => motorista.Repousos)
                        .Aggregations(agg => agg
                            .Terms("anos", terms => terms
                                .Field(motorista => motorista.Repousos.First().Ano)
                                .Aggregations(agg => agg
                                    .Terms("meses", terms => terms
                                        .Field(motorista => motorista.Repousos.First().Mes)
                                    )
                                )
                            )
                        )
                    )
                )
            );

            var repousos = aggregationResponse.Aggregations.Nested("repousos");

            var response = new Dictionary<string, Dictionary<string, long>>();

            foreach (var ano in repousos.Terms("anos").Buckets)
            {
                if (!response.ContainsKey(ano.Key))
                {
                    response[ano.Key] = new Dictionary<string, long>()
                    {
                        { "Janeiro", 0 },
                        { "Fevereiro", 0 },
                        { "Março", 0 },
                        { "Abril", 0 },
                        { "Maio", 0 },
                        { "Junho", 0 },
                        { "Julho", 0 },
                        { "Agosto", 0 },
                        { "Setembro", 0 },
                        { "Outubro", 0 },
                        { "Novembro", 0 },
                        { "Dezembro", 0 }
                    };

                    foreach (var mes in ano.Terms("meses").Buckets)
                    {
                        response[ano.Key][mes.Key] += mes.DocCount ?? 0;
                    }
                }
            }

            return Ok(response);
        }

        class Document
        {
            public Motorista Motorista { get; set; }
        }

        class Motorista
        {
            public string Nome { get; set; }
            public Empresa Empresa { get; set; }
            public List<Repouso> Repousos { get; set; }
        }

        class Empresa
        {
            public string Nome { get; set; }
        }

        class Repouso
        {
            public int Dia { get; set; }
            public string Mes { get; set; }
            public int Ano { get; set; }
        }

        public void CreateMotoristas(ElasticClient client)
        {
            var meses = new List<string>() {
                "Janeiro",
                "Fevereiro",
                "Março",
                "Abril",
                "Maio",
                "Junho",
                "Julho",
                "Agosto",
                "Setembro",
                "Outubro",
                "Novembro",
                "Dezembro"
            };

            for (var i = 0; i < 100; i++)
            {
                var random = new Random();

                var motorista = new Motorista()
                {
                    Nome = $"MOTORISTA {i}",
                    Empresa = new Empresa()
                    {
                        Nome = $"EMPRESA {random.Next(1, 10 + 1)}",
                    },
                    Repousos = new List<Repouso>()
                };

                var count = random.Next(1, 10 + 1);

                for (var j = 0; j < count; j++)
                {
                    motorista.Repousos.Add(new Repouso()
                    {
                        Dia = random.Next(1, 31 + 1),
                        Mes = meses[random.Next(0, 12)],
                        Ano = random.Next(2019, 2021 + 1),
                    });
                }

                var create = client.Create<Motorista>(motorista, create => create.Index("motoristas"));
            }
        }
    }

    public static class ResponseExtension
    {
        public static string Debug(this IResponse response)
        {
            return System.Text.Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
        }
    }
}
