using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ControleCaixaWeb.Models.Mappings
{
    public class EnderecoMap : ClassMap<Endereco>
    {
        public EnderecoMap( )
        {
            Table("ControleCaixaWeb_Endereco");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.NomeRua, "NomeRua").Not.Nullable( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Complemento, "Complemento").Nullable( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.CEP, "CEP").Not.Nullable( ).Length(8);
            Map(x => x.Bairro, "Bairro").Not.Nullable( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Cidade, "Cidade").Not.Nullable( ).Length(50).ToString( ).ToUpper( );
            Map(x => x.Estado, "Estado").Not.Nullable( ).Length(50).ToString( ).ToUpper( );
        }
    }
}