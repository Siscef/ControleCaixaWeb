using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles="Administrador")]
	[HandleError(View = "Error")]
    public class OperacaoContaCorrenteController : Controller
    {
        private IContextoDados _contextoOperacaoContaCorrente = new ContextoDadosNH( );
       
        public ActionResult Index()
        {
            return View();
        }

        #region Deposito
        public ActionResult DepositoOperacaoCaixa( )
        {
            ViewBag.ContaCorrente = new SelectList(_contextoOperacaoContaCorrente.GetAll<ContaCorrente>( ), "Codigo", "Numero");
            ViewBag.Forma = new SelectList(_contextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>( ), "Codigo", "NomeTipoFormaPagamento");

            return View( );
        }

        [HttpPost]
        public ActionResult DepositoOperacaoCaixa(OperacaoFinanceiraContaCorrente operacaoDeposito)
        {
            string nomeUsuario = User.Identity.Name;

            OperacaoCaixa operacaoSaida = new OperacaoCaixa( );

            operacaoSaida.DataLancamento = DateTime.Now.Date;
            operacaoSaida.Descricao = "SAIDA PARA O BANCO, VALOR: " + operacaoDeposito.Valor;
            operacaoSaida.EstabelecimentoOperacao = (from c in _contextoOperacaoContaCorrente.GetAll<ContaCorrente>( )
                                                    .Where(x => x.Codigo == operacaoDeposito.ContaLancamento.Codigo)
                                                     select c.EstabelecimentoDaConta).First( );
            operacaoSaida.FormaPagamentoUtilizada = _contextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(operacaoDeposito.FormaPagamento.Codigo);
            operacaoSaida.Observacao = "LANCAMENTO EM CONTA CORRENTE:  "  + operacaoDeposito.ContaLancamento.Numero;

            operacaoSaida.UsuarioQueLancou = (from c in _contextoOperacaoContaCorrente.GetAll<CadastrarUsuario>( )
                                              .Where(x => x.Nome == nomeUsuario)
                                              select c).First( );
            operacaoSaida.Valor = operacaoDeposito.Valor * -1;
            operacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            operacaoSaida.DataHoraInsercao = DateTime.Now;
            operacaoSaida.TipoOperacao = EnumTipoOperacao.Deposito;

            ModelState["ContaLancamento.Banco"].Errors.Clear( );
            ModelState["ContaLancamento.Agencia"].Errors.Clear( );
            ModelState["ContaLancamento.Numero"].Errors.Clear( );
            ModelState["FormaPagamento.NomeTipoFormaPagamento"].Errors.Clear( );


            if (ModelState.IsValid)
            {
                operacaoDeposito.ContaLancamento = _contextoOperacaoContaCorrente.Get<ContaCorrente>(operacaoDeposito.ContaLancamento.Codigo);
                operacaoDeposito.FormaPagamento = _contextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(operacaoDeposito.FormaPagamento.Codigo);
                operacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                operacaoDeposito.DataHoraInsercao = DateTime.Now;
                operacaoDeposito.Desconto = 0;
                operacaoDeposito.Taxa = 0;
                operacaoDeposito.ValorLiquido = operacaoDeposito.Valor;                

                _contextoOperacaoContaCorrente.Add<OperacaoFinanceiraContaCorrente>(operacaoDeposito);
                _contextoOperacaoContaCorrente.SaveChanges( );

                _contextoOperacaoContaCorrente.Add<OperacaoCaixa>(operacaoSaida);
                _contextoOperacaoContaCorrente.SaveChanges( );
                return RedirectToAction("Sucesso", "Home");
            }
            ViewBag.ContaCorrente = new SelectList(_contextoOperacaoContaCorrente.GetAll<ContaCorrente>( ), "Codigo", "Numero", operacaoDeposito.ContaLancamento);
            ViewBag.Forma = new SelectList(_contextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>( ), "Codigo", "NomeTipoFormaPagamento", operacaoDeposito.FormaPagamento);
            return View( );
        }


        public ActionResult AlterarDeposito(int id)
        {
            OperacaoFinanceiraContaCorrente operacaoAlterar = _contextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            ViewBag.ContaCorrente = new SelectList(_contextoOperacaoContaCorrente.GetAll<ContaCorrente>( ), "Codigo", "Numero");
            ViewBag.Forma = new SelectList(_contextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>( ), "Codigo", "NomeTipoFormaPagamento");

            return View(operacaoAlterar);
        }

        [HttpPost]
        public ActionResult AlterarDeposito(OperacaoFinanceiraContaCorrente operacaoFinanceira)
        {
            OperacaoFinanceiraContaCorrente operacaoAlterada = _contextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(operacaoFinanceira.Codigo);

            operacaoAlterada.Data = operacaoFinanceira.Data.Date;
            operacaoAlterada.Descricao = operacaoFinanceira.Descricao.ToUpper();
            operacaoAlterada.Valor = operacaoFinanceira.Valor;
            operacaoAlterada.FormaPagamento = _contextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(operacaoFinanceira.FormaPagamento.Codigo);
            operacaoAlterada.ContaLancamento = _contextoOperacaoContaCorrente.Get<ContaCorrente>(operacaoFinanceira.ContaLancamento.Codigo);
            operacaoAlterada.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            operacaoAlterada.DataHoraInsercao = DateTime.Now;
            operacaoAlterada.Desconto = 0;
            operacaoAlterada.Taxa = 0;
            operacaoAlterada.ValorLiquido = operacaoAlterada.Valor;      
  
            _contextoOperacaoContaCorrente.SaveChanges( );
            return RedirectToAction("Sucesso","Home" );
        }

        public ActionResult ListaOperacaoContaCorrente( )
        {
            IList<OperacaoFinanceiraContaCorrente> listaOperacoes = null;
            listaOperacoes = _contextoOperacaoContaCorrente.GetAll<OperacaoFinanceiraContaCorrente>( ).ToList( );
            return View(listaOperacoes);
        }

        public ActionResult ExcluirDeposito(int id)
        {
            OperacaoFinanceiraContaCorrente operacaoParaExcluir = _contextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            return View(operacaoParaExcluir);
        }

        [HttpPost]
        public ActionResult ExcluirDeposito(OperacaoFinanceiraContaCorrente operacaoFinanceira)
        {
            OperacaoFinanceiraContaCorrente operacaoExcluida = _contextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(operacaoFinanceira.Codigo);
            _contextoOperacaoContaCorrente.Delete<OperacaoFinanceiraContaCorrente>(operacaoExcluida);
            _contextoOperacaoContaCorrente.SaveChanges( );
            return RedirectToAction("Sucesso","Home" );
        }

        public ActionResult Detalhes(int id)
        {
            OperacaoFinanceiraContaCorrente detalhesOperacao = _contextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            return View(detalhesOperacao);
        }

        #endregion


        #region TransFerencia



        #endregion

    }
}
