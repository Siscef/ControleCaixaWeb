using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;


namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class OperacaoContaCorrenteController : Controller
    {
        private IContextoDados _ContextoOperacaoContaCorrente = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Escritorio/OperacaoContaCorrente/Details/5

        public ActionResult DetalhesDeposito(int id)
        {
            OperacaoFinanceiraContaCorrente OperacaoContaCorrenteDetalhes = _ContextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            return View(OperacaoContaCorrenteDetalhes);
        }

        //
        // GET: /Escritorio/OperacaoContaCorrente/Create

        public ActionResult FazerDeposito()
        {
            if (VerificarUsuarioTemPermissao() == false)
            {
                return RedirectToAction("UsuarioSemPermissao");  
            }
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

            ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero");
            return View();
        }

        //
        // POST: /Escritorio/OperacaoContaCorrente/Create

        [HttpPost]
        public ActionResult FazerDeposito(OperacaoFinanceiraContaCorrente OperacaoFinanceiraDeposito)
        {
            string NomeUsuarioLogadoVericarPermissao = User.Identity.Name;
            IList<CadastrarUsuario> ListaCadastroUsuarioVerificarPermissao = _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                           .Where(x => x.Nome == NomeUsuarioLogadoVericarPermissao && x.Privilegiado == true)
                                                                           .ToList();

          
            ModelState["ContaLancamento.Banco"].Errors.Clear();
            ModelState["ContaLancamento.Agencia"].Errors.Clear();
            ModelState["ContaLancamento.Numero"].Errors.Clear();
            ModelState["FormaPagamento.NomeTipoFormaPagamento"].Errors.Clear();

            if (OperacaoFinanceiraDeposito.Data != null)
            {
                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(OperacaoFinanceiraDeposito.Data.ToString("yyyy/MM/dd HH:mm:ss"));
                if (diferenca.TotalDays < 0)
                {
                    ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                    ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", OperacaoFinanceiraDeposito.FormaPagamento);
                    ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero", OperacaoFinanceiraDeposito.ContaLancamento);
                    return View();

                }
                if (diferenca.TotalDays > 31)
                {
                    ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                    ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", OperacaoFinanceiraDeposito.FormaPagamento);
                    ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero", OperacaoFinanceiraDeposito.ContaLancamento);
                    return View();

                }


            }

            if (ModelState.IsValid)
            {
                lock (_ContextoOperacaoContaCorrente)
                {
                    string NomeUsuarioLogado = User.Identity.Name;

                    long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
                    decimal SaldoAtual = (from c in _ContextoOperacaoContaCorrente.GetAll<OperacaoCaixa>()
                                          .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                          select c.Valor).Sum();
                    if (OperacaoFinanceiraDeposito.Valor > SaldoAtual)
                    {
                        return RedirectToAction("ErroOperacao");
                    }

                    IList<OperacaoFinanceiraContaCorrente> ListaVerificaDepositoDuplicado = _ContextoOperacaoContaCorrente.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                                            .Where(x => x.FormaPagamento.Codigo == OperacaoFinanceiraDeposito.FormaPagamento.Codigo && x.Valor == OperacaoFinanceiraDeposito.Valor && x.ContaLancamento.Codigo == OperacaoFinanceiraDeposito.ContaLancamento.Codigo)
                                                                                            .ToList();
                    if (ListaVerificaDepositoDuplicado.Count() >= 1)
                    {
                        foreach (var itemVerificaDepositoDuplicado in ListaVerificaDepositoDuplicado)
                        {
                            if (itemVerificaDepositoDuplicado.DataHoraInsercao == null)
                            {
                                //fazer o primeiro deposito
                                OperacaoFinanceiraContaCorrente PrimeiroOPeracaoDeposito = new OperacaoFinanceiraContaCorrente();
                                PrimeiroOPeracaoDeposito.Data = OperacaoFinanceiraDeposito.Data;
                                if (OperacaoFinanceiraDeposito.Descricao != null)
                                {
                                    PrimeiroOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao.ToUpper();

                                }
                                PrimeiroOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao;
                                if (OperacaoFinanceiraDeposito.Valor < 0)
                                {
                                    PrimeiroOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                                }
                                PrimeiroOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                                PrimeiroOPeracaoDeposito.FormaPagamento = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                                PrimeiroOPeracaoDeposito.ContaLancamento = _ContextoOperacaoContaCorrente.Get<ContaCorrente>(OperacaoFinanceiraDeposito.ContaLancamento.Codigo);
                                PrimeiroOPeracaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                PrimeiroOPeracaoDeposito.DataHoraInsercao = DateTime.Now;
                                _ContextoOperacaoContaCorrente.Add<OperacaoFinanceiraContaCorrente>(PrimeiroOPeracaoDeposito);
                                _ContextoOperacaoContaCorrente.SaveChanges();

                                OperacaoCaixa SaidaOperacaoCaixaPrimeiroDeposito = new OperacaoCaixa();
                                SaidaOperacaoCaixaPrimeiroDeposito.DataLancamento = OperacaoFinanceiraDeposito.Data;
                                SaidaOperacaoCaixaPrimeiroDeposito.Descricao = "DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraDeposito.Valor;
                                SaidaOperacaoCaixaPrimeiroDeposito.DataHoraInsercao = DateTime.Now;
                                SaidaOperacaoCaixaPrimeiroDeposito.EstabelecimentoOperacao = _ContextoOperacaoContaCorrente.Get<Estabelecimento>(OperacaoFinanceiraDeposito.ContaLancamento.EstabelecimentoDaConta.Codigo);
                                SaidaOperacaoCaixaPrimeiroDeposito.FormaPagamentoUtilizada = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                                SaidaOperacaoCaixaPrimeiroDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                SaidaOperacaoCaixaPrimeiroDeposito.UsuarioQueLancou = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                                       .Where(x => x.Nome == NomeUsuarioLogado)
                                                                                       select c).First();
                                if (OperacaoFinanceiraDeposito.Valor < 0)
                                {
                                    SaidaOperacaoCaixaPrimeiroDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                                }
                                SaidaOperacaoCaixaPrimeiroDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                                SaidaOperacaoCaixaPrimeiroDeposito.Observacao = OperacaoFinanceiraDeposito.Descricao;
                                SaidaOperacaoCaixaPrimeiroDeposito.TipoOperacao = EnumTipoOperacao.Deposito;
                                _ContextoOperacaoContaCorrente.Add<OperacaoCaixa>(SaidaOperacaoCaixaPrimeiroDeposito);
                                _ContextoOperacaoContaCorrente.SaveChanges();

                                return RedirectToAction("Sucesso", "Home");

                            }
                            else
                            {
                                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemVerificaDepositoDuplicado.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                if (diferenca.TotalMinutes <= 4)
                                {
                                    double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                    ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                       " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                    ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", OperacaoFinanceiraDeposito.FormaPagamento);
                                    ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero", OperacaoFinanceiraDeposito.ContaLancamento);
                                    return View();
                                }
                                else
                                {
                                    //aqui é o segundo deposito

                                    OperacaoFinanceiraContaCorrente SegundaOPeracaoDeposito = new OperacaoFinanceiraContaCorrente();
                                    SegundaOPeracaoDeposito.Data = OperacaoFinanceiraDeposito.Data;
                                    if (OperacaoFinanceiraDeposito.Descricao != null)
                                    {
                                        SegundaOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao.ToUpper();

                                    }
                                    SegundaOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao;
                                    if (OperacaoFinanceiraDeposito.Valor < 0)
                                    {
                                        SegundaOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                                    }
                                    SegundaOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                                    SegundaOPeracaoDeposito.FormaPagamento = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                                    SegundaOPeracaoDeposito.ContaLancamento = _ContextoOperacaoContaCorrente.Get<ContaCorrente>(OperacaoFinanceiraDeposito.ContaLancamento.Codigo);
                                    SegundaOPeracaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SegundaOPeracaoDeposito.DataHoraInsercao = DateTime.Now;
                                    _ContextoOperacaoContaCorrente.Add<OperacaoFinanceiraContaCorrente>(SegundaOPeracaoDeposito);
                                    _ContextoOperacaoContaCorrente.SaveChanges();

                                    OperacaoCaixa SaidaOperacaoCaixaSegundoDeposito = new OperacaoCaixa();
                                    SaidaOperacaoCaixaSegundoDeposito.DataLancamento = OperacaoFinanceiraDeposito.Data;
                                    SaidaOperacaoCaixaSegundoDeposito.Descricao = "DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraDeposito.Valor;
                                    SaidaOperacaoCaixaSegundoDeposito.DataHoraInsercao = DateTime.Now;
                                    SaidaOperacaoCaixaSegundoDeposito.EstabelecimentoOperacao = _ContextoOperacaoContaCorrente.Get<Estabelecimento>(codigoEstabelecimento);
                                    SaidaOperacaoCaixaSegundoDeposito.FormaPagamentoUtilizada = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                                    SaidaOperacaoCaixaSegundoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SaidaOperacaoCaixaSegundoDeposito.UsuarioQueLancou = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                                           .Where(x => x.Nome == NomeUsuarioLogado)
                                                                                          select c).First();
                                    if (OperacaoFinanceiraDeposito.Valor < 0)
                                    {
                                        SaidaOperacaoCaixaSegundoDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                                    }
                                    SaidaOperacaoCaixaSegundoDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                                    SaidaOperacaoCaixaSegundoDeposito.Observacao = OperacaoFinanceiraDeposito.Descricao;
                                    SaidaOperacaoCaixaSegundoDeposito.TipoOperacao = EnumTipoOperacao.Deposito;
                                    _ContextoOperacaoContaCorrente.Add<OperacaoCaixa>(SaidaOperacaoCaixaSegundoDeposito);
                                    _ContextoOperacaoContaCorrente.SaveChanges();

                                    return RedirectToAction("Sucesso", "Home");


                                }
                            }

                        }

                    }
                    else
                    {
                        //fazer o terceiro
                        OperacaoFinanceiraContaCorrente TerceiraOPeracaoDeposito = new OperacaoFinanceiraContaCorrente();
                        TerceiraOPeracaoDeposito.Data = OperacaoFinanceiraDeposito.Data;
                        if (OperacaoFinanceiraDeposito.Descricao != null)
                        {
                            TerceiraOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao.ToUpper();

                        }
                        TerceiraOPeracaoDeposito.Descricao = OperacaoFinanceiraDeposito.Descricao;
                        if (OperacaoFinanceiraDeposito.Valor < 0)
                        {
                            TerceiraOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                        }
                        TerceiraOPeracaoDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                        TerceiraOPeracaoDeposito.FormaPagamento = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                        TerceiraOPeracaoDeposito.ContaLancamento = _ContextoOperacaoContaCorrente.Get<ContaCorrente>(OperacaoFinanceiraDeposito.ContaLancamento.Codigo);
                        TerceiraOPeracaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiraOPeracaoDeposito.DataHoraInsercao = DateTime.Now;
                        _ContextoOperacaoContaCorrente.Add<OperacaoFinanceiraContaCorrente>(TerceiraOPeracaoDeposito);
                        _ContextoOperacaoContaCorrente.SaveChanges();

                        OperacaoCaixa SaidaOperacaoCaixaTerceiroDeposito = new OperacaoCaixa();
                        SaidaOperacaoCaixaTerceiroDeposito.DataLancamento = OperacaoFinanceiraDeposito.Data;
                        SaidaOperacaoCaixaTerceiroDeposito.Descricao = "DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraDeposito.Valor;
                        SaidaOperacaoCaixaTerceiroDeposito.DataHoraInsercao = DateTime.Now;
                        SaidaOperacaoCaixaTerceiroDeposito.EstabelecimentoOperacao = _ContextoOperacaoContaCorrente.Get<Estabelecimento>(codigoEstabelecimento);
                        SaidaOperacaoCaixaTerceiroDeposito.FormaPagamentoUtilizada = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraDeposito.FormaPagamento.Codigo);
                        SaidaOperacaoCaixaTerceiroDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        SaidaOperacaoCaixaTerceiroDeposito.UsuarioQueLancou = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                               .Where(x => x.Nome == NomeUsuarioLogado)
                                                                               select c).First();
                        if (OperacaoFinanceiraDeposito.Valor < 0)
                        {
                            SaidaOperacaoCaixaTerceiroDeposito.Valor = OperacaoFinanceiraDeposito.Valor;
                        }
                        SaidaOperacaoCaixaTerceiroDeposito.Valor = OperacaoFinanceiraDeposito.Valor * -1;
                        SaidaOperacaoCaixaTerceiroDeposito.Observacao = OperacaoFinanceiraDeposito.Descricao;
                        SaidaOperacaoCaixaTerceiroDeposito.TipoOperacao = EnumTipoOperacao.Deposito;
                        _ContextoOperacaoContaCorrente.Add<OperacaoCaixa>(SaidaOperacaoCaixaTerceiroDeposito);
                        _ContextoOperacaoContaCorrente.SaveChanges();

                        return RedirectToAction("Sucesso", "Home");
                    }


                    //aqui termina o lock
                }
                //aqui termina ModelisValid
            }
            ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", OperacaoFinanceiraDeposito.FormaPagamento);
            ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero", OperacaoFinanceiraDeposito.ContaLancamento);
            return View();
        }

        //
        // GET: /Escritorio/OperacaoContaCorrente/Edit/5

        public ActionResult AlterarDeposito(int id)
        {
            if (VerificarUsuarioTemPermissao() == false)
            {
                return RedirectToAction("UsuarioSemPermissao");
            }
            OperacaoFinanceiraContaCorrente OperacaoFinanceiraParaSerAlterada = _ContextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

            ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(), "Codigo", "Numero");
            return View(OperacaoFinanceiraParaSerAlterada);
        }

        //
        // POST: /Escritorio/OperacaoContaCorrente/Edit/5

        [HttpPost]
        public ActionResult AlterarDeposito(OperacaoFinanceiraContaCorrente OperacaoFinanceiraContaCorrenteParaAlterar)
        {
            lock (_ContextoOperacaoContaCorrente)
            {
                string NomeUsuarioLogado = User.Identity.Name;

                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
                CadastrarUsuario UsuarioLogado = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                               .Where(x => x.Nome == NomeUsuarioLogado)
                                                  select c).First();

                OperacaoFinanceiraContaCorrente OperacaoFinanceiraAlterada = _ContextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(OperacaoFinanceiraContaCorrenteParaAlterar.Codigo);


                OperacaoCaixa OperacaoDepositoParaAlterar = (from c in _ContextoOperacaoContaCorrente.GetAll<OperacaoCaixa>()
                                                             .Where(x => x.Valor == OperacaoFinanceiraAlterada.Valor * -1 && x.UsuarioQueLancou.Codigo == UsuarioLogado.Codigo)
                                                             select c).First();
                // TimeSpan diferenca = Convert.ToDateTime(OperacaoDepositoParaAlterar.DataHoraInsercao) - Convert.ToDateTime(OperacaoFinanceiraAlterada.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                OperacaoDepositoParaAlterar.DataLancamento = OperacaoFinanceiraContaCorrenteParaAlterar.Data;
                if (OperacaoFinanceiraContaCorrenteParaAlterar.Valor < 0)
                {
                    OperacaoDepositoParaAlterar.Descricao = " ALTERAÇÃO: " + "DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraContaCorrenteParaAlterar.Valor;
                }
                OperacaoDepositoParaAlterar.Descricao = " ALTERAÇÃO: " + "DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraContaCorrenteParaAlterar.Valor;

                OperacaoDepositoParaAlterar.DataHoraInsercao = DateTime.Now;
                OperacaoDepositoParaAlterar.EstabelecimentoOperacao = _ContextoOperacaoContaCorrente.Get<Estabelecimento>(codigoEstabelecimento);
                OperacaoDepositoParaAlterar.FormaPagamentoUtilizada = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraContaCorrenteParaAlterar.FormaPagamento.Codigo);
                OperacaoDepositoParaAlterar.Observacao = OperacaoFinanceiraContaCorrenteParaAlterar.Descricao;
                OperacaoDepositoParaAlterar.UsuarioDataHoraInsercao = OperacaoDepositoParaAlterar.UsuarioDataHoraInsercao + "Alterado por: " + NomeUsuarioLogado + " Data: " + DateTime.Now;
                OperacaoDepositoParaAlterar.UsuarioQueLancou = _ContextoOperacaoContaCorrente.Get<CadastrarUsuario>(UsuarioLogado.Codigo);
                if (OperacaoFinanceiraContaCorrenteParaAlterar.Valor < 0)
                {
                    OperacaoDepositoParaAlterar.Valor = OperacaoFinanceiraContaCorrenteParaAlterar.Valor;
                }

                OperacaoDepositoParaAlterar.Valor = OperacaoFinanceiraContaCorrenteParaAlterar.Valor;

                OperacaoFinanceiraAlterada.Data = OperacaoFinanceiraContaCorrenteParaAlterar.Data;
                if (OperacaoFinanceiraContaCorrenteParaAlterar.Descricao != null)
                {
                    OperacaoFinanceiraAlterada.Descricao = OperacaoFinanceiraContaCorrenteParaAlterar.Descricao.ToUpper();
                }
                OperacaoFinanceiraAlterada.Descricao = OperacaoFinanceiraContaCorrenteParaAlterar.Descricao;
                OperacaoFinanceiraAlterada.FormaPagamento = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraContaCorrenteParaAlterar.FormaPagamento.Codigo);
                OperacaoFinanceiraAlterada.ContaLancamento = _ContextoOperacaoContaCorrente.Get<ContaCorrente>(OperacaoFinanceiraContaCorrenteParaAlterar.ContaLancamento.Codigo);
                OperacaoFinanceiraAlterada.UsuarioDataHoraInsercao = OperacaoFinanceiraContaCorrenteParaAlterar.DataHoraInsercao + "Alterado por: " + NomeUsuarioLogado + " Data: " + DateTime.Now;
                OperacaoFinanceiraAlterada.DataHoraInsercao = DateTime.Now;
                if (OperacaoFinanceiraContaCorrenteParaAlterar.Valor < 0)
                {
                    OperacaoFinanceiraAlterada.Valor = OperacaoFinanceiraContaCorrenteParaAlterar.Valor * -1;
                }
                else
                {
                    OperacaoFinanceiraAlterada.Valor = OperacaoFinanceiraContaCorrenteParaAlterar.Valor;
                }
                
                _ContextoOperacaoContaCorrente.SaveChanges();
              
            }
            return RedirectToAction("Sucesso", "Home");
        }

        //
        // GET: /Escritorio/OperacaoContaCorrente/Delete/5

        public ActionResult ExcluirDeposito(int id)
        {
            if (VerificarUsuarioTemPermissao() == false)
            {
                return RedirectToAction("UsuarioSemPermissao");
            }

            OperacaoFinanceiraContaCorrente OperacaoFinanceiraParaExclusao = _ContextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(id);
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

            ViewBag.FormaPagamentoUtilizada = new SelectList(_ContextoOperacaoContaCorrente.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.ContaCorrente = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>().Where(x => x.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "Numero");
            return View(OperacaoFinanceiraParaExclusao);
        }

        //
        // POST: /Escritorio/OperacaoContaCorrente/Delete/5

        [HttpPost]
        public ActionResult ExcluirDeposito(OperacaoFinanceiraContaCorrente OperacaoContaCorrenteExcluida)
        {
            lock (_ContextoOperacaoContaCorrente)
            {
                string NomeUsuarioLogado = User.Identity.Name;

                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

                OperacaoFinanceiraContaCorrente OperacaoFinanceiraExcluida = _ContextoOperacaoContaCorrente.Get<OperacaoFinanceiraContaCorrente>(OperacaoContaCorrenteExcluida.Codigo);

                OperacaoCaixa OperacaoCaixaExclusaoDeposito = new OperacaoCaixa();

                OperacaoCaixaExclusaoDeposito.DataLancamento = OperacaoFinanceiraExcluida.Data;
                OperacaoCaixaExclusaoDeposito.Descricao = "EXCLUSÃO DO DEPOSITO EM CONTA CORRENTE, VALOR: " + OperacaoFinanceiraExcluida.Valor * -1;
                OperacaoCaixaExclusaoDeposito.DataHoraInsercao = DateTime.Now;
                OperacaoCaixaExclusaoDeposito.EstabelecimentoOperacao = _ContextoOperacaoContaCorrente.Get<Estabelecimento>(codigoEstabelecimento);
                OperacaoCaixaExclusaoDeposito.FormaPagamentoUtilizada = _ContextoOperacaoContaCorrente.Get<FormaPagamentoEstabelecimento>(OperacaoFinanceiraExcluida.FormaPagamento.Codigo);
                OperacaoCaixaExclusaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaExclusaoDeposito.UsuarioQueLancou = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                       .Where(x => x.Nome == NomeUsuarioLogado)
                                                                  select c).First();
                if (OperacaoFinanceiraExcluida.Valor < 0)
                {
                    OperacaoCaixaExclusaoDeposito.Valor = OperacaoFinanceiraExcluida.Valor * -1;
                }
                OperacaoCaixaExclusaoDeposito.Valor = OperacaoFinanceiraExcluida.Valor;
                OperacaoCaixaExclusaoDeposito.Observacao = OperacaoFinanceiraExcluida.Descricao;
                OperacaoCaixaExclusaoDeposito.TipoOperacao = EnumTipoOperacao.DevolucaoDeposito;
                _ContextoOperacaoContaCorrente.Add<OperacaoCaixa>(OperacaoCaixaExclusaoDeposito);
                _ContextoOperacaoContaCorrente.SaveChanges();
                _ContextoOperacaoContaCorrente.Delete<OperacaoFinanceiraContaCorrente>(OperacaoFinanceiraExcluida);
                _ContextoOperacaoContaCorrente.SaveChanges();
            }
            return RedirectToAction("Sucesso", "Home");
        }

        public ActionResult DepositosData()
        {
            ViewBag.Conta = new SelectList(_ContextoOperacaoContaCorrente.GetAll<ContaCorrente>(),"Codigo","Numero");
            return View();
        }

        [HttpPost]
        public ActionResult DepositosData(ValidarData Datas, int? Conta)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {

                    string nomeUsuario = User.Identity.Name;
                    ViewBag.DataInicio = Datas.DataInicial;
                    ViewBag.DataFim = Datas.DataFinal;

                    long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);

                    IList<OperacaoFinanceiraContaCorrente> listaPorData = null;
                    if (Conta == null)
                    {
                        listaPorData = _ContextoOperacaoContaCorrente.GetAll<OperacaoFinanceiraContaCorrente>()
                                    .Where(x => x.Data.Date >= Datas.DataInicial && x.Data.Date <= Datas.DataFinal).OrderByDescending(x => x.Data).ToList();  
                    }
                    else
                    {
                        listaPorData = _ContextoOperacaoContaCorrente.GetAll<OperacaoFinanceiraContaCorrente>()
                                    .Where(x => x.Data.Date >= Datas.DataInicial && x.Data.Date <= Datas.DataFinal && x.ContaLancamento.Codigo == Conta).OrderByDescending(x => x.Data).ToList();  
                    }
                   

                    return View("ListaDepositoPorData", listaPorData);
                }
            }

            return View();
        }

        public ActionResult ListaDepositoPorData()
        {
            return View();
        }

        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
        }

        public ActionResult ErroOperacao()
        {
            ViewBag.Saldo = (from c in _ContextoOperacaoContaCorrente.GetAll<OperacaoCaixa>()
                                .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                             select c.Valor).Sum();
            return View();
        }

        private bool VerificarUsuarioTemPermissao()
        {

            string NomeUsuarioLogadoVericarPermissao = User.Identity.Name;
            IList<CadastrarUsuario> ListaCadastroUsuarioVerificarPermissao = _ContextoOperacaoContaCorrente.GetAll<CadastrarUsuario>()
                                                                             .Where(x => x.Nome == NomeUsuarioLogadoVericarPermissao && x.Privilegiado == true)
                                                                             .ToList();
            if (ListaCadastroUsuarioVerificarPermissao.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public ActionResult UsuarioSemPermissao()
        {
            return View();
        }
    }
}
