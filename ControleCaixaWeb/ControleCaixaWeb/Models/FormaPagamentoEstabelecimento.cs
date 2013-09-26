using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class FormaPagamentoEstabelecimento
    {
        public virtual long Codigo { get; set; }

        [Required(ErrorMessage = "O campo nome da forma pagamento é obrigatório ")]
        public virtual string NomeTipoFormaPagamento { get; set; }
        [Required(ErrorMessage = "O campo Taxa é obrigatório")]
        public virtual decimal TaxaFormaPagamento { get; set; }
        public virtual ContaCorrente ContaCorrenteFormaPagamento { get; set; }
        public virtual bool DespejoAutomatico { get; set; }
        [Display(Name="Dias Para Recebimento:")]
        public virtual int DiasRecebimento { get; set; }
        [Display(Name="Padrão:")]
        public virtual bool Padrao { get; set; }

    }
}