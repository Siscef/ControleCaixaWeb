using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class TrocarSenhaMap : ClassMap<TrocarSenha>
    {
        public TrocarSenhaMap( )
        {
            Table("ControleCaixaWeb_Trocar_Senha");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.SenhaAtual, "SenhaAtual").Not.Nullable( ).Length(25).ToString().ToUpper();
            Map(x => x.NovaSenha, "NovaSenha").Not.Nullable( ).Length(25).ToString( ).ToUpper( );
            Map(x => x.ConfirmaSenha, "ConfirmaSenha").Not.Nullable( ).Length(25).ToString( ).ToUpper( );
        }
    }
}