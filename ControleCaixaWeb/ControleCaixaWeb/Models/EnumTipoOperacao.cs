using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleCaixaWeb.Models
{
    public enum EnumTipoOperacao
    {

        Transferencia,       
        Sangria,
        Recolhimento,
        Pagamento,
        DevolucaoLancamento,
        DevolucaoPagamento,
        DevolucaoRecolhimento,
        DevolucaoRecebimento,
        DevolucaoDespejoManual,
        DevolucaoDespejoAutomatico,
        Recebimento,
        DespejoAutomatico,
        DespejoManual,
        LancamentoCaixa,
        SaidaLancamentoCaixa,
        Deposito,
        DevolucaoDeposito



    }
}