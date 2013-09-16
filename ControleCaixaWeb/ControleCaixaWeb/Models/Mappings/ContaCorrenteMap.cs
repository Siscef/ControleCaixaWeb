using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class ContaCorrenteMap : ClassMap<ContaCorrente>
    {
        public ContaCorrenteMap( )
        {
            Table("ControleCaixaWeb_Conta_Corrente");
            Id( x => x.Codigo,"Codigo");
            Map(x => x.Banco, "Banco").Not.Nullable( ).Length(20).ToString( ).ToUpper( );
            Map(x => x.Agencia, "Agencia").Not.Nullable( ).Length(6).ToString( ).ToUpper( );
            Map(x => x.Numero, "Numero").Not.Nullable( ).Unique( ).Length(6);

            References(x => x.EstabelecimentoDaConta, "ContaCorrenteEstabelecimento")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_contaCorrente_estabelecimento")
                .Not.Nullable( );

            HasMany(x => x.OperacaoFinanceira)
                .KeyColumn("ContaCorrente_OperacaoFinanceira")
                .Cascade.SaveUpdate( );

            HasMany(x => x.FormaPagamentoEstabelecimento)
                .KeyColumn("ContaCorrente_FormaPagamento")
                 .Cascade.SaveUpdate( );


        }
    }
}