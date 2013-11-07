using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleCaixaWeb.Models.Mappings
{
    public class ResultadoCaixaMap : ClassMap<ResultadoCaixa>
    {
        public ResultadoCaixaMap()
        {
            Table("ControleCaixaWeb_Resultado_Caixa");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.DataLancamento, "DataLancamento");
            Map(x => x.ValorResultadoCaixa, "ValorResultadoCaixa").Default("0");
            Map(x => x.ContadorVisualizacao, "ContadorVisualizacao").Default("0");

            References(x => x.EstabelecimentoOperacao, "EstabelecimentoOperacao")
                .ForeignKey("FK_RESULTADO_CAIXA_PERTEENCE_UM_ESTABELECIMENTO")
                .Cascade.SaveUpdate()
                .Not.Nullable();

            References(x => x.UsuarioOPeradorCaixa, "UsuarioOPeradorCaixa")
                .ForeignKey("FK_RESULTADO_CAIXA_PERTENCE_UM_OPERADOR_CAIXA")
                .Cascade.SaveUpdate()
                .Not.Nullable();


        }
    }
}