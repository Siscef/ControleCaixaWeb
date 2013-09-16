using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class PagamentoMap : ClassMap<Pagamento>
    {
        public PagamentoMap( )
        {
            Table("ControleCaixaWeb_Pagamento");
            Id(x => x.Codigo,"Codigo");
            Map(x => x.DataPagamento, "DataPagamento").Not.Nullable( );
            Map(x => x.Valor, "Valor").Not.Nullable( ).Check("Valor > 0");
            Map(x => x.Observacao, "Observacao").Nullable( ).Length(100).ToString().ToUpper();
            Map(x => x.UsuarioQueAlterouEDataEComputador, "UsuarioQueAlterouNomeMaquina").Nullable( );
            Map(x => x.UsuarioDataHoraInsercao, "UsuarioDataHoraInsercao");
            Map(x => x.DataHoraInsercao,"DataHoraInsercao");
            Map(x => x.CodigoOperacaoCaixa,"CodigoOperacaoCaixa");

            References(x => x.EstabelecimentoQuePagou, "EstabelecimentoQuePagou")
                .Cascade.SaveUpdate()               
                .ForeignKey("fk_Pagamento_Estabelecimento")
                .Not.Nullable( );


            References(x => x.FavorecidoPagamento, "Favorecido")
               .Cascade.SaveUpdate()
                .ForeignKey("fk_Pagamento_Favorecido")
                .Not.Nullable( );

            References(x => x.UsuarioQueLancou, "UsuarioQueLancou")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_Usuario_Pagamento")
                .Not.Nullable( );

            References(x => x.FormaPagamento, "FormaPagamento")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_Pagamento_FormaPagamento")
                .Not.Nullable( );

        }
    }
}