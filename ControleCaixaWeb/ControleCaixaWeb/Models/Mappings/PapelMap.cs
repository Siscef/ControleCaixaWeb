using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class PapelMap : ClassMap<Papel>
    {
        public PapelMap( )
        {
            Table("ControleCaixaWeb_Papel");
            Id(x => x.Codigo,"Codigo" );
            Map(x => x.NomePapel, "NomePapel").Not.Nullable( ).Unique( ).Length(15).ToString().ToUpper();

            HasMany(x => x.FuncaoDoUsuario)
                .KeyColumn("FuncaoDoUsuario")
                .Cascade.SaveUpdate();

            HasMany(x => x.FuncoesDoUsuario)
                .KeyColumn("FuncoesCadastroUsuario")
                .Cascade.SaveUpdate();


        }
    }
}