using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class CadastrarUsuarioMap : ClassMap<CadastrarUsuario>
    {
        public CadastrarUsuarioMap( )
        {
            Table("ControleCaixaWeb_Cadastrar_Usuario");
            Id(x => x.Codigo,"Codigo");
            Map(x => x.Nome, "Nome").Not.Nullable( ).Unique( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Email, "Email").Not.Nullable( ).Unique( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Senha, "Senha").Not.Nullable( ).Length(25).ToString( ).ToUpper( );
            Map(x => x.ConfirmeSenha, "ConfirmeSenha").Not.Nullable( ).Length(25).ToString( ).ToUpper( );
            Map(x => x.Telefone, "Telefone").Nullable( ).Length(8);
            Map(x => x.Privilegiado,"Privilegiado");

            References(x => x.EnderecoUsuario, "EnderecoUsuario")
                .Cascade.All( )
                .ForeignKey("fk_Usuario_Endereco")
                .Nullable( );

            References(x => x.EstabelecimentoTrabalho, "EstabelecimentoTrabalho")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_Usuario_Estabelecimento")
                .Nullable( );

            References(x => x.NomeFuncao, "FuncaoFuncionario")
                .Cascade.SaveUpdate()
                .ForeignKey("fk_Funcionario_Funcao")
                .Nullable( );

            HasMany(x => x.OpereacoesDoUsuario)
                .KeyColumn("OperacoesDoUsuario")
                .Cascade.SaveUpdate();

            HasMany(x => x.FechamentoDeCaixa)
                .KeyColumn("FechamentoDoUsuario")
                .Cascade.SaveUpdate();


        }
    }
}