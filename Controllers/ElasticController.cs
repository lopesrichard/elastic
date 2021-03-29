using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Elastic.Data;
using Elastic.Models;
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
        private ClientProvider _provider;

        public ElasticController(ClientProvider provider)
        {
            _provider = provider;
        }

        [HttpPost("setup")]
        public IActionResult Setup()
        {
            var client = _provider.Get();

            // =====================================================================================
            // ================================== REMOVE INDICES ===================================
            // =====================================================================================

            client.Indices.Delete("motoristas");

            // =====================================================================================
            // ================================== CREATE INDICES ===================================
            // =====================================================================================

            client.Indices.Create("motoristas", create => create
                .Map<Motorista>(map => map
                    .Properties(properties => properties
                        .Keyword(keyword => keyword
                            .Name("nome")
                        )
                        .Object<Empresa>(obj => obj
                            .Name("empresa")
                            .Properties(properties => properties
                                .Keyword(keyword => keyword
                                    .Name("nome")
                                )
                            )
                        )
                        .Nested<Repouso>(nested => nested
                            .Name("repousos")
                            .Properties(properties => properties
                                .Number(number => number
                                    .Name("dia")
                                )
                                .Keyword(keyword => keyword
                                    .Name("mes")
                                )
                                .Number(number => number
                                    .Name("ano")
                                )
                            )
                        )
                    )
                )
            );

            // =====================================================================================
            // ================================== INSERT DATA ======================================
            // =====================================================================================

            for (var i = 0; i < 100; i++)
            {
                var motorista = Generator.GenerateRandomMotorista(i);
                client.Create<Motorista>(motorista, create => create.Index("motoristas"));
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Search()
        {
            var client = _provider.Get();
            var response = client.Search<Motorista>(search => search.Index("motoristas"));
            return Ok(response.Documents);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] string id)
        {
            var client = _provider.Get();
            var response = client.Get<Motorista>(id, get => get.Index("motoristas"));
            return Ok(response.Source);
        }

        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] string id, [FromBody] Motorista motorista)
        {
            var client = _provider.Get();
            var response = client.Update<Motorista>(id, update => update.Index("motoristas").Doc(motorista));
            return Ok(response.Result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] string id, [FromBody] Motorista motorista)
        {
            var client = _provider.Get();
            var response = client.Delete<Motorista>(id, delete => delete.Index("motoristas"));
            return Ok(response.Result);
        }

        [HttpGet("empresas")]
        public IActionResult GetTotalEmpresas()
        {
            var client = _provider.Get();

            var search = client.Search<Motorista>(search => search
                .Index("motoristas")
                .Aggregations(agg => agg
                    .Terms("empresa", terms => terms
                        .Field(motorista => motorista.Empresa.Nome)
                    )
                )
            );

            var empresas = search.Aggregations.Terms("empresa");

            var response = new Dictionary<string, long>();

            foreach (var item in empresas.Buckets)
            {
                response.Add(item.Key, item.DocCount ?? 0);
            }

            return Ok(response);
        }

        [HttpGet("repousos")]
        public IActionResult GetRepousosPorMesEAno()
        {
            var client = _provider.Get();

            var search = client.Search<Motorista>(search => search
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

            var repousos = search.Aggregations.Nested("repousos");

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
    }

    public static class ResponseExtension
    {
        public static string Debug(this IResponse response)
        {
            return System.Text.Encoding.UTF8.GetString(response.ApiCall.RequestBodyInBytes);
        }
    }
}
