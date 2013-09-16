using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
	public class DespejoOPeracaoCaixaMap : ClassMap<DespejoOPeracaoCaixa>
	{
		public DespejoOPeracaoCaixaMap( )
		{
			Table("ControleCaixaWeb_Despejo_OPeracao_Caixa");
			Id(x => x.Codigo, "Codigo");
			Map(x => x.DataLancamento, "DataLancamento").Not.Nullable( );
			Map(x => x.Valor, "Valor").Not.Nullable( );
			Map(x => x.Descricao, "Descricao").Not.Nullable( ).Length(45).ToString( ).ToUpper( );
			Map(x => x.Observacao, "Observacao").Nullable( ).Length(100).ToString( ).ToUpper( );
            Map(x => x.UsuarioDataHoraInsercao,"UsuarioDataHoraInsercao");
            Map(x => x.DataHoraInsercao,"DataHoraInsercao");
     

			References(x => x.FormaPagamentoUtilizada, "FormaPagfamentoUtilizadaNoDespejo")
			   .Cascade.SaveUpdate( )
				.ForeignKey("fk_DespejoOperacaoCaixa_FormaPagamento")
				.Not.Nullable( );

			References(x => x.UsuarioQueLancou, "UsuarioQueLancouDespejo")
			  .Cascade.SaveUpdate( )
				.ForeignKey("fk_DespejoOperacaoCaixa_CadastraUsuario")
				.Not.Nullable( );

			References(x => x.EstabelecimentoOperacao, "EstabelecimentoOperacaoDesejo")
			   .Cascade.SaveUpdate( )
				.ForeignKey("fk_DespejoOperacao_Estabelecimento")
				.Not.Nullable( );


				References(x => x.OperacaoCaixaOrigem,"OperacaoCaixaOrigem")
				.Cascade.SaveUpdate()
				.ForeignKey("fk_DespejoOPeracaoCaixa_OpercaoCaixa")
				.Not.Nullable();

		}
	}
}