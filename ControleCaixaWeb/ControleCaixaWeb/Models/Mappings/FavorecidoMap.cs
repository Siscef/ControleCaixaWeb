using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class FavorecidoMap : ClassMap<Favorecido>
    {
        public FavorecidoMap( )
        {
            Table("ControleCaixaWeb_Favorecido");
            Id(x => x.Codigo,"Codigo");
            Map(x => x.NomeFavorecido, "NomeFavorecido").Not.Nullable( ).Unique( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Observacao, "Observacao").Nullable( ).Length(100).ToString( ).ToUpper( );

            HasMany(x => x.PagamentosRecebidos)
                .KeyColumn("PagamentosRecebidos")
                .Cascade.SaveUpdate( );
        }
    }
}