using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class FechamentoCaixaMap :ClassMap<FechamentoCaixa>
    {
        public FechamentoCaixaMap( )
        {
            Table("ControleCaixaWeb_Fechamento_Caixa");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.DataLancamento, "DataLancamento").Not.Nullable( );
            Map(x => x.CaixaAbertura, "CaixaAbertura").Not.Nullable( );
            Map(x => x.CaixaFechamento, "CaixaFechamento").Not.Nullable( ).Check("CaixaFechamento >=0");
            Map(x => x.Faturamento, "Faturamento").Not.Nullable( ).Check("Faturamento >=0");
            Map(x => x.DataHoraInsercao, "DataHoraInsercao");
            Map(x => x.UsuarioDataHoraInsercao,"UsuarioDataHoraInsercao");

            References(x => x.FaturamentoEstabelecimento, "FaturamentoEstabelecimento")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_FechamentoCaixa_Estabelecimento")
                .Not.Nullable( );


            References(x => x.FaturamentoUsuario, "FaturamentoUsuario")
                .Cascade.SaveUpdate( )
                .ForeignKey("fk_FaturamentoCaixa_CadastroUsuario")
                .Not.Nullable( );
           

        }

    }
}