using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class FormaPagamentoEstabelcimentoMap : ClassMap<FormaPagamentoEstabelecimento>
    {
        public FormaPagamentoEstabelcimentoMap()
        {
            Table("ControleCaixaWeb_Forma_Pagamento_Estabelecimento");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.NomeTipoFormaPagamento, "NomeFormaPagamento").Not.Nullable().Unique().Length(45).ToString().ToUpper();
            Map(x => x.TaxaFormaPagamento, "TaxaFormaPagamento").Not.Nullable();
            Map(x => x.DespejoAutomatico, "DespejoAutomatico");
            Map(x => x.DiasRecebimento, "DiasRecebimento").Nullable();
            Map(x => x.Padrao, "Padrao").Nullable();

            References(x => x.ContaCorrenteFormaPagamento, "ContaCorrenteFormaPagamento")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_FormaPagamento_ContaCorrente")
                .Nullable();



        }
    }
}