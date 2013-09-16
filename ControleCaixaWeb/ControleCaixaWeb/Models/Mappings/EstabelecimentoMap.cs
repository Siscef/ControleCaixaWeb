using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class EstabelecimentoMap : ClassMap<Estabelecimento>
    {
        public EstabelecimentoMap( )
        {
            Table("ControleCaixaWeb_Estabelecimento");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.RazaoSocial, "RazaoSocial").Not.Nullable( ).Unique( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.CNPJ, "CNPJ").Not.Nullable( ).Unique( ).Length(14);
            Map(x => x.InscricaoEstadual, "InscricaoEstadual").Not.Nullable( ).Unique( ).Length(9);
            Map(x => x.Telefone, "Telefone").Not.Nullable( ).Length(8);

            References(x => x.EnderecoEstabelecimento, "EnderecoEstabelecimento")
                .Cascade.All( )
                .ForeignKey("fk_estabelecimento_endereco")
                .Nullable( );

            References(x => x.UsuarioResponsavel, "UsuarioResponsavel")
              .Cascade.SaveUpdate()           
                .ForeignKey("fk_Estabelecimento_CadastrarUsuario")
                .Nullable( );


            HasMany(x => x.UsuariosFuncionarios)
                .KeyColumn("EstabelecimentoUsuario")
                .Cascade.SaveUpdate( );   

            HasMany(x => x.ContaCorrenteEstabelecimento)
                .KeyColumn("EstabelecimentoConta")
                 .Cascade.SaveUpdate();   

            HasMany(x => x.OperacoesEstabelecimento)
                .KeyColumn("EstabelecimentoOperacao")
                 .Cascade.SaveUpdate();

            HasMany(x => x.PagamentosDoEstabelecimento)
                .KeyColumn("PagamentoEstabelecimento")
               .Cascade.SaveUpdate() ;  


            HasMany(x => x.FaturamentoEstabelecimento)
                .KeyColumn("FaturamentoEstabelecimento")
                .Cascade.SaveUpdate();

        }
    }
}