using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleCaixaWeb.Models
{
    public class ResultadoCaixa
    {
        public virtual long Codigo { get; set; }

        public virtual DateTime DataLancamento { get; set; }

        public virtual CadastrarUsuario UsuarioOPeradorCaixa { get; set; }

        public virtual Estabelecimento EstabelecimentoOperacao { get; set; }

        public virtual decimal ValorResultadoCaixa { get; set; }

        public virtual int ContadorVisualizacao { get; set; }
    }
}