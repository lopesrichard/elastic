DELETE motoristas

PUT motoristas
{
  "mappings":{
    "properties":{
      "nome" : {
        "type" : "keyword"
      },
      "empresa": {
        "properties": {
          "nome": {
            "type": "keyword"
          }
        }
      },
      "repousos":{
        "type" : "nested",
        "properties": {
          "dia": {
            "type": "integer"
          },
          "mes": {
            "type": "keyword"
          },
          "ano": {
            "type": "integer"
          }
        }
      }
    }
  }
}

PUT motoristas/_doc/1
{
  "nome": "Reginalo",
  "empresa": {
    "nome": "1001"
  },
  "repousos": [
    { "dia": 5, "mes": "Janeiro", "ano": 2020 },
    { "dia": 6, "mes": "Janeiro", "ano": 2020 },
    { "dia": 7, "mes": "Fevereiro", "ano": 2020 }
  ]
}

PUT motoristas/_doc/2
{
  "nome": "maria",
  "empresa": {
    "nome": "1001"
  },
  "repousos": [
    { "dia": 5, "mes": "Janeiro", "ano": 2020 },
    { "dia": 6, "mes": "Fevereiro", "ano": 2020 },
    { "dia": 7, "mes": "Março", "ano": 2020 },
    { "dia": 17, "mes": "Abril", "ano": 2021 }
  ]
}

PUT motoristas/_doc/3
{
  "nome": "joão",
  "empresa": {
    "nome": "expresso do sul"
  }, 
  "repousos": [
    { "dia": 25, "mes": "Outubro", "ano": 2019 }
  ]
}

GET motoristas/_search
{
  "aggs": {
    "empresas": {
      "terms": {
        "field": "empresa.nome"
      }
    }
  }
}

GET motoristas/_search
{
  "aggs": {
    "repousos": {
      "nested": {
        "path": "repousos"
      },
      "aggs": {
        "anos": {
          "terms": { "field": "repousos.ano" },
          "aggs": {
            "meses": {
              "terms": { "field": "repousos.mes" }
            }
          }
        }
      }
    }
  }
}