using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class UsuarioMap : ClassMap<Usuario>
    {
        public UsuarioMap( )
        {
            Table("ControleCaixaWeb_Usuario");

            Id( x => x.Codigo,"Codigo");
            Map(x => x.Nome, "Nome").Not.Nullable( ).Unique( ).Length(25).ToString().ToUpper();
            Map(x => x.Senha, "Senha").Not.Nullable( ).Length(25).ToString().ToUpper();
            Map(x => x.Lembrar, "Lembrar").Nullable( );
        }
    }
}