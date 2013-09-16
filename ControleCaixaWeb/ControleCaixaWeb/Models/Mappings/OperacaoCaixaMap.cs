using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class OperacaoCaixaMap:ClassMap<OperacaoCaixa>
    {
        public OperacaoCaixaMap( )
        {
            Table("ControleCaixaWeb_Operacao_Caixa");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.DataLancamento, "DataLancamento").Not.Nullable( );
            Map(x => x.Valor, "Valor").Not.Nullable( );
            Map(x => x.Descricao, "Descricao").Length(255).ToString( ).ToUpper( );
            Map(x => x.Observacao, "Observacao").Nullable( ).Length(100).ToString( ).ToUpper( );
            Map(x => x.UsuarioDataHoraInsercao,"UsuarioDataHoraInsercao");
            Map(x => x.DataHoraInsercao,"DataHoraInsercao");
            Map(x => x.Conferido,"Conferido");
            Map(x => x.TipoOperacao, "TipoOperacao").Not.Nullable();

            References(x => x.FormaPagamentoUtilizada, "FormaPagfamentoUtilizada")
               .Cascade.SaveUpdate()
                .ForeignKey("fk_OperacaoCaixa_FormaPagamento")
                .Not.Nullable( );

            References(x => x.UsuarioQueLancou, "UsuarioQueLancou")
              .Cascade.SaveUpdate()
                .ForeignKey("fk_OperacaoCaixa_CadastraUsuario")
                .Not.Nullable( );

            References(x => x.EstabelecimentoOperacao, "EstabelecimentoOperacao")
               .Cascade.SaveUpdate()
                .ForeignKey("fk_Operacao_Estabelecimento")
                .Not.Nullable( );

        }
    }
}