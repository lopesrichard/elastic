using System.Collections.Generic;

namespace Elastic.Models
{
    public class Motorista
    {
        public string Nome { get; set; }
        public Empresa Empresa { get; set; }
        public List<Repouso> Repousos { get; set; }
    }
}