using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class Pagamento
    {
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage = "O campo data pagamento é obrigatório")]
        [Display(Name="Data Pagamento:")]
        public virtual DateTime DataPagamento { get; set; }
        [Required(ErrorMessage = "O campo valor pagamento é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor não pode ser negativo")]
        public virtual decimal Valor { get; set; }
        public virtual FormaPagamentoEstabelecimento FormaPagamento { get; set; }
        [Required(ErrorMessage = "O Estabelecimento não pode ser vazio")]
        public virtual Estabelecimento EstabelecimentoQuePagou { get; set; }
        [Required(ErrorMessage = "O Favorecido não pode ser vazio ")]
        public virtual Favorecido FavorecidoPagamento { get; set; }
        public virtual string Observacao { get; set; }
        public virtual CadastrarUsuario UsuarioQueLancou { get; set; }
        public virtual string UsuarioQueAlterouEDataEComputador { get; set; }
        public virtual string UsuarioDataHoraInsercao { get; set; }
        public virtual DateTime DataHoraInsercao { get; set; }
        public virtual long CodigoOperacaoCaixa { get; set; }

    }
}