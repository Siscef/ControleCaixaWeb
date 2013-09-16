using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class OperacaoFinanceiraMap : ClassMap<OperacaoFinanceiraContaCorrente>
    {
        public OperacaoFinanceiraMap( )
        {
            Table("ControleCaixaWeb_Operacao_Financeira");
            Id(x => x.Codigo,"Codigo");
            Map(x => x.Data,"DataLancamento").Not.Nullable();
            Map(x => x.Valor, "Valor").Not.Nullable( ).Check("Valor > 0");
            Map(x => x.Descricao, "Descricao").Length(255).ToString( ).ToUpper( );
            Map(x => x.UsuarioDataHoraInsercao, "UsuarioDataHoraInsercao");
            Map(x => x.DataHoraInsercao, "DataHoraInsercao");
            Map(x => x.Taxa, "Taxa").Check("Taxa >=0").Nullable();
            Map(x => x.ValorLiquido, "ValorLiquido").Nullable().Check("ValorLiquido >= 0");
            Map(x => x.Desconto, "Desconto").Formula("Valor - ValorLiquido").Nullable();

            References(x => x.ContaLancamento, "ContaCorrenteLancamento")
               .Cascade.SaveUpdate()
                .ForeignKey("fk_OperacaoFinanceira_ContaCorrente")
                .Not.Nullable( );

            References(x => x.FormaPagamento, "FormaPagamento")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_formaPagamento_Operacao")
                .Not.Nullable( );

        }
    }
}