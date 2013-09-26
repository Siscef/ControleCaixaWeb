using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class OperacaoFinanceiraContaCorrente
    {
        [Display(Name = "Código:")]
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage = "O campo Data da operação não pode ser vazia")]
        public virtual DateTime Data { get; set; }
        [Required(ErrorMessage = "O campo Valor Não Pode Ser Vazio")]
        public virtual decimal Valor { get; set; }
        [Display(Name = "Valor líquido:")]
        public virtual decimal ValorLiquido { get; set; }
        [Display(Name = "Taxa:")]
        public virtual decimal Taxa { get; set; }
        [Display(Name = "Valor bruto - valor líquido:")]
        public virtual decimal Desconto { get; set; }
        [Display(Name = "Descrição:")]
        public virtual string Descricao { get; set; }
        [Required(ErrorMessage = "A conta corrente não pode ser vazia")]
        public virtual ContaCorrente ContaLancamento { get; set; }
        [Required(ErrorMessage = "A forma de pagamento não pode ser vazia")]
        public virtual FormaPagamentoEstabelecimento FormaPagamento { get; set; }
        [Display(Name = "Usuário/Data/Hora:")]
        public virtual string UsuarioDataHoraInsercao { get; set; }
        [Display(Name = "Data/Hora:")]
        public virtual DateTime DataHoraInsercao { get; set; }
        [Display(Name = "Dia recebimento:")]
        public virtual DateTime DataRecebimento { get; set; }
        [Display(Name = "Operação Origem:")]
        public virtual OperacaoCaixa OperacaoCaixaOrigem { get; set; }




    }
}