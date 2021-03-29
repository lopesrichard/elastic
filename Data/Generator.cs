using System;
using System.Collections.Generic;
using Elastic.Models;

namespace Elastic.Data
{
    public static class Generator
    {
        private static List<string> Meses = new List<string>() {
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

        public static Motorista GenerateRandomMotorista(int id)
        {
            var random = new Random();

            var motorista = new Motorista()
            {
                Nome = $"MOTORISTA {id}",
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
                    Mes = Meses[random.Next(0, 12)],
                    Ano = random.Next(2019, 2021 + 1),
                });
            }

            return motorista;
        }
    }
}