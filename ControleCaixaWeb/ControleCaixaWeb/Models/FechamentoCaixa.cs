using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class FechamentoCaixa
    {
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage = "O campo data não pode ser vazio")]
        public virtual DateTime DataLancamento { get; set; }

        [Required(ErrorMessage = "O campo caixa de abertura não pode ser vazio, caso não tenha preencha com: 0,00")]
        [Range(0, 999, ErrorMessage = "O valor do caixa de abertura está muito alto você não prefere fazer uma sangria ?")]
        public virtual decimal CaixaAbertura { get; set; }

        [Range(0, 999, ErrorMessage = "O valor do caixa de fechamento está muito alto você não prefere fazer uma sangria ?")]
        [Required(ErrorMessage = "O campo caixa de fechamento não pode ser vazio, caso não tenha preencha com: 0,00")]
        public virtual decimal CaixaFechamento { get; set; }
        [Range(0, 20000, ErrorMessage = "O valor do faturamento está muito alto, caso você esteja certo(a) disso preencha com no máximo R$ 20.000,00 e a diferença em outro lançamento")]
        [Required(ErrorMessage = "O campo faturamento não pode ser vazio, caso não tenha preencha com: 0,00")]
        public virtual decimal Faturamento { get; set; }
        public virtual Estabelecimento FaturamentoEstabelecimento { get; set; }
        public virtual CadastrarUsuario FaturamentoUsuario { get; set; }

        public virtual string UsuarioDataHoraInsercao { get; set; }

        public virtual DateTime DataHoraInsercao { get; set; }



    }
}