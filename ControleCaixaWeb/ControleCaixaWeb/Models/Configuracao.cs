using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class Configuracao
    {
        [Display(Name = "Código:")]
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage = "Casas decimais é obrigatório...")]
        [Display(Name = "Casas decimais:")]
        public virtual int CasasDecimais { get; set; }
        [Display(Name = "Estabelecimento:")]
        public virtual Estabelecimento EstabelecimentoPadrao { get; set; }
        [Display(Name = "Lançamento em conta corrente:")]
        public virtual bool FazerLancamentoContaCorrente { get; set; }
        [Display(Name = "Enviar email para caixa alterado:")]
        public virtual bool EnviarEmailCaixaAlterado { get; set; }
        [Display(Name="Email padrão:")]
        public virtual string Email { get; set; }
        [Display(Name="Assunto padrão:")]
        public virtual string Assunto { get; set; }

      

    }
}