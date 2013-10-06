using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleCaixaWeb.Models.Mappings
{
    public class ConfiguracaoMap : ClassMap<Configuracao>
    {
        public ConfiguracaoMap()
        {
            Table("ControleCaixaWeb_Configuracao");
            Id(x => x.Codigo, "Codigo");
            Map(x => x.CasasDecimais, "CasasDecimais").Not.Nullable();
            Map(x => x.FazerLancamentoContaCorrente, "FazerLancamentoContaCorrente").Nullable();
            Map(x => x.EnviarEmailCaixaAlterado, "EnviarEmailCaixaAlterado").Nullable();
            Map(x => x.Email, "Email").Nullable();
            Map(x => x.Assunto, "Assunto").Nullable();

            References(x => x.EstabelecimentoPadrao, "EstabelecimentoPadrao")
                .Cascade.SaveUpdate()
                .ForeignKey("Fk_Configuracao_Tem_EstabelecimentoPadrao")
                .Not.Nullable();

        }
    }
}