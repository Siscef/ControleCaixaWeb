using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class DespejoOPeracaoCaixa
    {
        public virtual long Codigo { get; set; }

        [Required(ErrorMessage = "O campo data de lancamento é obrigatório")]
        public virtual DateTime DataLancamento { get; set; }

        [Required(ErrorMessage = "O campo valor é obrigatório")]
        [Range(0, 99999, ErrorMessage = "O valor da operacao esta fora do intervalo")]
        public virtual decimal Valor { get; set; }

        [Required(ErrorMessage = "O campo descricao é obrigatório")]
        public virtual string Descricao { get; set; }

        [Required(ErrorMessage = "A forma de pagamento é obrigatória")]
        public virtual FormaPagamentoEstabelecimento FormaPagamentoUtilizada { get; set; }

        [Required(ErrorMessage = "O usuario é obrigatório")]
        public virtual CadastrarUsuario UsuarioQueLancou { get; set; }

        [Required(ErrorMessage = "O estabelecimento é obrigatório")]
        public virtual Estabelecimento EstabelecimentoOperacao { get; set; }

        public virtual OperacaoCaixa OperacaoCaixaOrigem { get; set; }

        public virtual string Observacao { get; set; }

        public virtual string UsuarioDataHoraInsercao { get; set; }

        public virtual DateTime DataHoraInsercao { get; set; }



        public virtual string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }


}