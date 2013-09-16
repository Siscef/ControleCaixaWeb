using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class ContaCorrente
    {
        public virtual long Codigo { get; set; }

        [Required(ErrorMessage="O campo banco é obrigatório")]
        [MaxLength(25,ErrorMessage="O campo banco não pode ter mais que 25 caracteres"),MinLength(3,ErrorMessage="O campo banco deve ter mais que 3 caracteres")]
        public virtual string Banco { get ; set; }
        [Required(ErrorMessage="O campo agencia é obrigatório")]
        [StringLength(25, ErrorMessage = "A Agencia Deve Ter no Mínimo 6 Dígitos", MinimumLength = 4)]
        public virtual string Agencia { get; set; }
        [Required(ErrorMessage="O campo número é obrigatório")]
        public virtual string Numero { get; set; }
        public virtual IList<OperacaoFinanceiraContaCorrente> OperacaoFinanceira { get; set; }
        public virtual Estabelecimento EstabelecimentoDaConta { get; set; }
        public virtual IList<FormaPagamentoEstabelecimento> FormaPagamentoEstabelecimento { get; set; }
    }
}