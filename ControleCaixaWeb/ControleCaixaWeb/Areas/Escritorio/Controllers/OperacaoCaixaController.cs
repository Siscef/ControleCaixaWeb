using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;
using System.Web.Script.Serialization;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class OperacaoCaixaController : Controller
    {
        private IContextoDados _contextoOperacao = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detalhes(int id)
        {
            OperacaoCaixa OperacaoCaixa = _contextoOperacao.Get<OperacaoCaixa>(id);

            return View(OperacaoCaixa);
        }

        public ActionResult ListaDasOperacaoes()
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
            ViewBag.Loja = (from c in _contextoOperacao.GetAll<Estabelecimento>()
                            .Where(x => x.Codigo == codigoEstabelecimento)
                            select c.RazaoSocial).First();
            IList<OperacaoCaixa> listaOperacao = null;
            listaOperacao = _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).OrderByDescending(x => x.DataLancamento).ToList();
            return View(listaOperacao);
        }


        public ActionResult FazerDespejo(int forma, int estabelecimento, decimal valor)
        {
            lock (_contextoOperacao)
            {
                decimal Valor = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                 .Where(x => x.FormaPagamentoUtilizada.Codigo == forma)
                                 select c.Valor).Sum();
                if (Valor > 0)
                {

                    string nomeUsuarioLogado = User.Identity.Name;

                    DespejoOPeracaoCaixa despejoOperacaoCaixa = new DespejoOPeracaoCaixa();

                    string HashOperacao = despejoOperacaoCaixa.GenerateId();
                    OperacaoCaixa operacaoCaixaDespejo = new OperacaoCaixa();
                    operacaoCaixaDespejo.DataLancamento = DateTime.Now;
                    operacaoCaixaDespejo.Descricao = "DESPEJO DO CAIXA " + HashOperacao;
                    operacaoCaixaDespejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(estabelecimento);
                    operacaoCaixaDespejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(forma);
                    operacaoCaixaDespejo.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                             .Where(x => x.Nome == nomeUsuarioLogado)
                                                             select c).First();
                    operacaoCaixaDespejo.Valor = valor * -1;
                    operacaoCaixaDespejo.Observacao = "Despejo Automático ";
                    operacaoCaixaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    operacaoCaixaDespejo.DataHoraInsercao = DateTime.Now;
                    operacaoCaixaDespejo.TipoOperacao = EnumTipoOperacao.DespejoManual;
                    _contextoOperacao.Add<OperacaoCaixa>(operacaoCaixaDespejo);
                    _contextoOperacao.SaveChanges();

                    despejoOperacaoCaixa.DataLancamento = DateTime.Now;
                    despejoOperacaoCaixa.Descricao = "Entrada da Operação " + HashOperacao;
                    despejoOperacaoCaixa.Observacao = "Despejo Automático";
                    despejoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                             .Where(x => x.Nome == nomeUsuarioLogado)
                                                             select c).First();
                    despejoOperacaoCaixa.Valor = valor;
                    despejoOperacaoCaixa.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixaDespejo.Codigo);
                    despejoOperacaoCaixa.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(forma);
                    despejoOperacaoCaixa.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(estabelecimento);
                    despejoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    despejoOperacaoCaixa.DataHoraInsercao = DateTime.Now;

                    _contextoOperacao.Add<DespejoOPeracaoCaixa>(despejoOperacaoCaixa);
                    _contextoOperacao.SaveChanges();
                    return RedirectToAction("Sucesso", "Home");
                }
                else
                {

                    return RedirectToAction("ErroOPeracaoDespejo", forma);
                }

            }

        }


        public ActionResult CadastrarOperacaoCaixa()
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento).OrderBy(x => x.NomeTipoFormaPagamento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).OrderBy(x => x.Nome), "Codigo", "Nome");

            return View();
        }



        [HttpPost]
        public ActionResult CadastrarOperacaoCaixa(OperacaoCaixa operacaoCaixa)
        {
            lock (_contextoOperacao)
            {
                ModelState["FormaPagamentoUtilizada.NomeTipoFormaPagamento"].Errors.Clear();
                ModelState["UsuarioQueLancou.Nome"].Errors.Clear();
                ModelState["UsuarioQueLancou.Email"].Errors.Clear();
                ModelState["UsuarioQueLancou.Senha"].Errors.Clear();
                ModelState["UsuarioQueLancou.ConfirmeSenha"].Errors.Clear();
                ModelState["UsuarioQueLancou.EnderecoUsuario"].Errors.Clear();
                ModelState["Observacao"].Errors.Clear();
                ModelState["EstabelecimentoOperacao"].Errors.Clear();
                if (operacaoCaixa.DataLancamento != null)
                {
                    TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(operacaoCaixa.DataLancamento.ToString("yyyy/MM/dd HH:mm:ss"));
                    if (diferenca.TotalDays < 0)
                    {
                        ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                        ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
                        ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                        return View();

                    }
                    if (diferenca.TotalDays > 31)
                    {
                        ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                        ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
                        ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                        return View();

                    }

                }

                if (ModelState.IsValid)
                {

                    IList<OperacaoCaixa> ListaVerificarLancamentoDuplicado = _contextoOperacao.GetAll<OperacaoCaixa>()
                                                                           .AsParallel()
                                                                           .Where(x => x.Valor == operacaoCaixa.Valor && x.Descricao == operacaoCaixa.Descricao)
                                                                           .ToList();
                    bool seDeposita = VerificaSeSistemaDepositaAutomatico();
                    decimal taxaFormaPagamento = ObtemTaxaFormaPagamento(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    double diasRecebimento = ObtemDiasRecebimentoFormaPagamento(operacaoCaixa.FormaPagamentoUtilizada.Codigo);

                    if (ListaVerificarLancamentoDuplicado.Count() >= 1)
                    {
                        foreach (var itemVerificaLancamentoDuplicado in ListaVerificarLancamentoDuplicado)
                        {
                            if (itemVerificaLancamentoDuplicado.DataHoraInsercao == null)
                            {
                                string nomeUsuario = User.Identity.Name;
                                long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);


                                OperacaoCaixa PrimeiraOperacaoCaixaEntrada = new OperacaoCaixa();

                                PrimeiraOperacaoCaixaEntrada.DataLancamento = operacaoCaixa.DataLancamento;
                                if (operacaoCaixa.Descricao != null)
                                {
                                    PrimeiraOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao.ToUpper();
                                }
                                else
                                {
                                    PrimeiraOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao;
                                }

                                if (operacaoCaixa.Observacao != null)
                                {
                                    operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                                }
                                PrimeiraOperacaoCaixaEntrada.Observacao = operacaoCaixa.Observacao;
                                if (operacaoCaixa.Valor < 0)
                                {
                                    PrimeiraOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor * -1;
                                }
                                else
                                {
                                    PrimeiraOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor;
                                }

                                PrimeiraOperacaoCaixaEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                PrimeiraOperacaoCaixaEntrada.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                PrimeiraOperacaoCaixaEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                PrimeiraOperacaoCaixaEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                PrimeiraOperacaoCaixaEntrada.DataHoraInsercao = DateTime.Now;
                                PrimeiraOperacaoCaixaEntrada.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                                _contextoOperacao.Add<OperacaoCaixa>(PrimeiraOperacaoCaixaEntrada);
                                _contextoOperacao.SaveChanges();

                                if (seDeposita == true)
                                {
                                    OperacaoFinanceiraContaCorrente OperacaoDeposito = new OperacaoFinanceiraContaCorrente();
                                    OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                                    OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                                    OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                                    OperacaoDeposito.Descricao = "Simulação de depósito";
                                    OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                    OperacaoDeposito.Taxa = taxaFormaPagamento;
                                    OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    OperacaoDeposito.Valor = operacaoCaixa.Valor;
                                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                                    OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(PrimeiraOperacaoCaixaEntrada.Codigo);
                                    _contextoOperacao.Add<OperacaoFinanceiraContaCorrente>(OperacaoDeposito);
                                    _contextoOperacao.SaveChanges();

                                }


                                //TODO:verificar se 
                                if (VerificarSeFormaPagamentoBaixaAutomatico(PrimeiraOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
                                {
                                    OperacaoCaixa PrimeiraOperacaoSaida = new OperacaoCaixa();

                                    PrimeiraOperacaoSaida.DataHoraInsercao = DateTime.Now;
                                    PrimeiraOperacaoSaida.Conferido = false;
                                    PrimeiraOperacaoSaida.DataLancamento = DateTime.Now;
                                    PrimeiraOperacaoSaida.Descricao = "Despejo Automático";
                                    PrimeiraOperacaoSaida.Valor = operacaoCaixa.Valor * -1;
                                    PrimeiraOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                    PrimeiraOperacaoSaida.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                    PrimeiraOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                    PrimeiraOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    PrimeiraOperacaoSaida.Observacao = PrimeiraOperacaoCaixaEntrada.Codigo.ToString();
                                    PrimeiraOperacaoSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                                    _contextoOperacao.Add<OperacaoCaixa>(PrimeiraOperacaoSaida);
                                    _contextoOperacao.SaveChanges();


                                    DespejoOPeracaoCaixa Despejo = new DespejoOPeracaoCaixa();

                                    Despejo.DataHoraInsercao = DateTime.Now;
                                    Despejo.DataLancamento = DateTime.Now;
                                    Despejo.Descricao = " Despejo referente a operação caixa: " + PrimeiraOperacaoSaida.Codigo;
                                    Despejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                    Despejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                    Despejo.Observacao = " Entrada referente a operação caixa valor: " + PrimeiraOperacaoSaida.Valor * -1;
                                    Despejo.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(PrimeiraOperacaoCaixaEntrada.Codigo);
                                    Despejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    Despejo.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                    Despejo.Valor = PrimeiraOperacaoSaida.Valor * -1;

                                    _contextoOperacao.Add<DespejoOPeracaoCaixa>(Despejo);
                                    _contextoOperacao.SaveChanges();

                                    return RedirectToAction("Sucesso", "Home");

                                }

                                return RedirectToAction("Sucesso", "Home");

                            }
                            else
                            {
                                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemVerificaLancamentoDuplicado.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                if (diferenca.TotalMinutes <= 4)
                                {
                                    double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                    ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                       " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
                                    ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                                    return View();
                                }
                                else
                                {

                                    string nomeUsuario = User.Identity.Name;
                                    long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);

                                    OperacaoCaixa SegundaOperacaoCaixaEntrada = new OperacaoCaixa();

                                    SegundaOperacaoCaixaEntrada.DataLancamento = operacaoCaixa.DataLancamento;
                                    if (operacaoCaixa.Descricao != null)
                                    {
                                        SegundaOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao.ToUpper();
                                    }
                                    else
                                    {
                                        SegundaOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao;
                                    }
                                    if (operacaoCaixa.Observacao != null)
                                    {
                                        operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                                    }
                                    SegundaOperacaoCaixaEntrada.Observacao = operacaoCaixa.Observacao;
                                    if (operacaoCaixa.Valor < 0)
                                    {
                                        SegundaOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor * -1;
                                    }
                                    else
                                    {
                                        SegundaOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor;
                                    }

                                    SegundaOperacaoCaixaEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                    SegundaOperacaoCaixaEntrada.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                    SegundaOperacaoCaixaEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                    SegundaOperacaoCaixaEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SegundaOperacaoCaixaEntrada.DataHoraInsercao = DateTime.Now;
                                    SegundaOperacaoCaixaEntrada.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                                    _contextoOperacao.Add<OperacaoCaixa>(SegundaOperacaoCaixaEntrada);
                                    _contextoOperacao.SaveChanges();


                                    if (seDeposita == true)
                                    {
                                        OperacaoFinanceiraContaCorrente OperacaoDeposito = new OperacaoFinanceiraContaCorrente();
                                        OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                                        OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                                        OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                                        OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                                        OperacaoDeposito.Descricao = "Simulação de depósito";
                                        OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                        OperacaoDeposito.Taxa = taxaFormaPagamento;
                                        OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        OperacaoDeposito.Valor = operacaoCaixa.Valor;
                                        OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                                        OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                                        OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(SegundaOperacaoCaixaEntrada.Codigo);
                                        _contextoOperacao.Add<OperacaoFinanceiraContaCorrente>(OperacaoDeposito);
                                        _contextoOperacao.SaveChanges();

                                    }


                                    if (VerificarSeFormaPagamentoBaixaAutomatico(SegundaOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
                                    {
                                        OperacaoCaixa SegundaOperacaoSaida = new OperacaoCaixa();

                                        SegundaOperacaoSaida.DataHoraInsercao = DateTime.Now;
                                        SegundaOperacaoSaida.Conferido = false;
                                        SegundaOperacaoSaida.DataLancamento = DateTime.Now;
                                        SegundaOperacaoSaida.Descricao = "Despejo Automático";
                                        SegundaOperacaoSaida.Valor = operacaoCaixa.Valor * -1;
                                        SegundaOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                        SegundaOperacaoSaida.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                        SegundaOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                        SegundaOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        SegundaOperacaoSaida.Observacao = SegundaOperacaoCaixaEntrada.Codigo.ToString();
                                        SegundaOperacaoSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                                        _contextoOperacao.Add<OperacaoCaixa>(SegundaOperacaoSaida);
                                        _contextoOperacao.SaveChanges();


                                        DespejoOPeracaoCaixa Despejo = new DespejoOPeracaoCaixa();

                                        Despejo.DataHoraInsercao = DateTime.Now;
                                        Despejo.DataLancamento = DateTime.Now;
                                        Despejo.Descricao = " Despejo referente a operação caixa: " + SegundaOperacaoSaida.Codigo;
                                        Despejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                        Despejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                        Despejo.Observacao = " Entrada referente a operação caixa valor: " + SegundaOperacaoSaida.Valor * -1;
                                        Despejo.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(SegundaOperacaoCaixaEntrada.Codigo);
                                        Despejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        Despejo.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                                        Despejo.Valor = SegundaOperacaoSaida.Valor * -1;

                                        _contextoOperacao.Add<DespejoOPeracaoCaixa>(Despejo);
                                        _contextoOperacao.SaveChanges();

                                        return RedirectToAction("Sucesso", "Home");
                                    }

                                    return RedirectToAction("Sucesso", "Home");

                                }
                            }
                        }

                    }
                    else
                    {
                        string nomeUsuario = User.Identity.Name;
                        long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);

                        OperacaoCaixa TerceiraOperacaoCaixaEntrada = new OperacaoCaixa();

                        TerceiraOperacaoCaixaEntrada.DataLancamento = operacaoCaixa.DataLancamento;
                        if (operacaoCaixa.Descricao != null)
                        {
                            TerceiraOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao.ToUpper();
                        }
                        else
                        {
                            TerceiraOperacaoCaixaEntrada.Descricao = operacaoCaixa.Descricao;
                        }
                        if (operacaoCaixa.Observacao != null)
                        {
                            operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                        }
                        TerceiraOperacaoCaixaEntrada.Observacao = operacaoCaixa.Observacao;
                        if (operacaoCaixa.Valor < 0)
                        {
                            TerceiraOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor * -1;
                        }
                        else
                        {
                            TerceiraOperacaoCaixaEntrada.Valor = operacaoCaixa.Valor;
                        }

                        TerceiraOperacaoCaixaEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                        TerceiraOperacaoCaixaEntrada.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                        TerceiraOperacaoCaixaEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                        TerceiraOperacaoCaixaEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiraOperacaoCaixaEntrada.DataHoraInsercao = DateTime.Now;
                        TerceiraOperacaoCaixaEntrada.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                        _contextoOperacao.Add<OperacaoCaixa>(TerceiraOperacaoCaixaEntrada);
                        _contextoOperacao.SaveChanges();


                        if (seDeposita == true)
                        {
                            OperacaoFinanceiraContaCorrente OperacaoDeposito = new OperacaoFinanceiraContaCorrente();
                            OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                            OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                            OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                            OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                            OperacaoDeposito.Descricao = "Simulação de depósito";
                            OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                            OperacaoDeposito.Taxa = taxaFormaPagamento;
                            OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            OperacaoDeposito.Valor = operacaoCaixa.Valor;
                            OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                            OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                            OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(TerceiraOperacaoCaixaEntrada.Codigo);
                            _contextoOperacao.Add<OperacaoFinanceiraContaCorrente>(OperacaoDeposito);
                            _contextoOperacao.SaveChanges();

                        }


                        if (VerificarSeFormaPagamentoBaixaAutomatico(TerceiraOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
                        {
                            OperacaoCaixa TerceiraOperacaoSaida = new OperacaoCaixa();

                            TerceiraOperacaoSaida.DataHoraInsercao = DateTime.Now;
                            TerceiraOperacaoSaida.Conferido = false;
                            TerceiraOperacaoSaida.DataLancamento = DateTime.Now;
                            TerceiraOperacaoSaida.Descricao = "Despejo Automático";
                            TerceiraOperacaoSaida.Valor = operacaoCaixa.Valor * -1;
                            TerceiraOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                            TerceiraOperacaoSaida.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                            TerceiraOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                            TerceiraOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            TerceiraOperacaoSaida.Observacao = TerceiraOperacaoCaixaEntrada.Codigo.ToString();
                            TerceiraOperacaoSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                            _contextoOperacao.Add<OperacaoCaixa>(TerceiraOperacaoSaida);
                            _contextoOperacao.SaveChanges();


                            DespejoOPeracaoCaixa Despejo = new DespejoOPeracaoCaixa();

                            Despejo.DataHoraInsercao = DateTime.Now;
                            Despejo.DataLancamento = DateTime.Now;
                            Despejo.Descricao = " Despejo referente a operação caixa: " + TerceiraOperacaoSaida.Codigo;
                            Despejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                            Despejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                            Despejo.Observacao = " Entrada referente a operação caixa valor: " + TerceiraOperacaoSaida.Valor * -1;
                            Despejo.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(TerceiraOperacaoCaixaEntrada.Codigo);
                            Despejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            Despejo.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                            Despejo.Valor = TerceiraOperacaoSaida.Valor * -1;

                            _contextoOperacao.Add<DespejoOPeracaoCaixa>(Despejo);
                            _contextoOperacao.SaveChanges();

                            return RedirectToAction("Sucesso", "Home");

                        }
                        return RedirectToAction("Sucesso", "Home");
                    }


                }

            }
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);

            return View();
        }


        public ActionResult SaidaOperacaoCaixa()
        {
            string NomeUsuarioLogado = User.Identity.Name;
            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
            long codigoEstabelecimentoPadrao = (from c in _contextoOperacao.GetAll<Configuracao>()
                                                select c.EstabelecimentoPadrao.Codigo).FirstOrDefault();

            if (VerificarUsuarioTemPermissao() == false)
            {
                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");

                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimentoPadrao), "Codigo", "RazaoSocial");

                return View();

            }
            else
            {
                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo != codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");

                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimentoPadrao), "Codigo", "RazaoSocial");

                return View();

            }

        }

        [HttpPost]
        public ActionResult SaidaOperacaoCaixa(OperacaoCaixa operacaoCaixa, OperacaoCaixa operacaoCaixaEntrada)
        {
            if (operacaoCaixa.DataLancamento != null)
            {
                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(operacaoCaixa.DataLancamento.ToString("yyyy/MM/dd HH:mm:ss"));
                if (diferenca.TotalDays < 0)
                {
                    ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);

                    ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.UsuarioQueVai = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", operacaoCaixa.EstabelecimentoOperacao.Codigo);
                    return View();

                }
                if (diferenca.TotalDays > 31)
                {
                    ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
                    ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.UsuarioQueVai = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", operacaoCaixa.EstabelecimentoOperacao.Codigo);
                    return View();

                }


            }
            lock (_contextoOperacao)
            {
                string NomeUsuarioLogado = User.Identity.Name;

                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
                decimal SaldoAtual = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                      .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada == _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"])))
                                      select c.Valor).Sum();

                if (operacaoCaixa.Valor > SaldoAtual)
                {
                    decimal FormaPagamentoSaldo = _contextoOperacao.GetAll<OperacaoCaixa>()
                                               .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name) && x.FormaPagamentoUtilizada.Codigo == Convert.ToInt64(Request["FormaPagamento"]))
                                               .Select(x => x.Valor).Sum();
                    decimal SaldoLoja = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                        .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                                         select c.Valor).Sum();
                    ViewBag.MensagemSaldoInsuficiente = "O saldo da forma de pagamento é menor que o valor: R$  " + FormaPagamentoSaldo + " .";
                    ViewBag.MensagemSaldoInsuficienteLoja = " O saldo do estabelecimento é:  R$ " + SaldoLoja + " .";
                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(NomeUsuarioLogado)), "Codigo", "NomeTipoFormaPagamento", operacaoCaixaEntrada.FormaPagamentoUtilizada);
                    ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.UsuarioQueVai = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                    ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", operacaoCaixa.EstabelecimentoOperacao.Codigo);
                    return View();
                }

                IList<OperacaoCaixa> ListaVerificaOperacaoCaixaDuplicada = _contextoOperacao.GetAll<OperacaoCaixa>()
                                                                           .AsParallel()
                                                                           .Where(x => x.TipoOperacao == EnumTipoOperacao.Recolhimento && x.Valor == operacaoCaixa.Valor * -1)
                                                                           .ToList();
                if (ListaVerificaOperacaoCaixaDuplicada.Count() >= 1)
                {
                    foreach (var itemVerificaLancamentoDuplicado in ListaVerificaOperacaoCaixaDuplicada)
                    {
                        if (itemVerificaLancamentoDuplicado.DataHoraInsercao == null)
                        {
                            OperacaoCaixa PrimeiraOperacaoSaida = new OperacaoCaixa();

                            PrimeiraOperacaoSaida.DataLancamento = operacaoCaixa.DataLancamento;
                            PrimeiraOperacaoSaida.Descricao = "RECOLHIMENTO " + operacaoCaixa.Descricao;
                            PrimeiraOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                            PrimeiraOperacaoSaida.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                                      .AsParallel()
                                                                      .Where(x => x.Nome == NomeUsuarioLogado)
                                                                      select c).First();
                            PrimeiraOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                            if (operacaoCaixa.Observacao != null)
                            {
                                operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                            }
                            PrimeiraOperacaoSaida.Observacao = operacaoCaixa.Observacao;
                            PrimeiraOperacaoSaida.Valor = operacaoCaixa.Valor * -1;

                            PrimeiraOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            PrimeiraOperacaoSaida.DataHoraInsercao = DateTime.Now;
                            PrimeiraOperacaoSaida.TipoOperacao = EnumTipoOperacao.Recolhimento;
                            _contextoOperacao.Add<OperacaoCaixa>(PrimeiraOperacaoSaida);
                            _contextoOperacao.SaveChanges();


                            OperacaoCaixa PrimeiraOperacaoEntrada = new OperacaoCaixa();

                            PrimeiraOperacaoEntrada.DataLancamento = operacaoCaixaEntrada.DataLancamento;
                            PrimeiraOperacaoEntrada.Descricao = " Entrada Transferencia " + operacaoCaixa.Descricao;
                            PrimeiraOperacaoEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                            PrimeiraOperacaoEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                            PrimeiraOperacaoEntrada.Observacao = PrimeiraOperacaoSaida.Codigo.ToString();
                            PrimeiraOperacaoEntrada.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                                        .Where(x => x.Nome == NomeUsuarioLogado)
                                                                        select c).First();
                            PrimeiraOperacaoEntrada.Valor = operacaoCaixaEntrada.Valor;
                            PrimeiraOperacaoEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            PrimeiraOperacaoEntrada.DataHoraInsercao = DateTime.Now;
                            PrimeiraOperacaoEntrada.TipoOperacao = EnumTipoOperacao.Recebimento;
                            _contextoOperacao.Add<OperacaoCaixa>(PrimeiraOperacaoEntrada);
                            _contextoOperacao.SaveChanges();
                            return RedirectToAction("Sucesso", "Home");

                        }
                        else
                        {
                            TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemVerificaLancamentoDuplicado.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                            if (diferenca.TotalMinutes <= 4)
                            {
                                double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                   " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(NomeUsuarioLogado) && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento", operacaoCaixaEntrada.FormaPagamentoUtilizada);
                                ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                                ViewBag.UsuarioQueVai = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
                                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", operacaoCaixa.EstabelecimentoOperacao.Codigo);
                                return View();
                            }

                            //aqui faz a segunda parte
                            else
                            {
                                OperacaoCaixa SegundaOperacaoSaida = new OperacaoCaixa();

                                SegundaOperacaoSaida.DataLancamento = operacaoCaixa.DataLancamento;
                                SegundaOperacaoSaida.Descricao = "RECOLHIMENTO " + operacaoCaixa.Descricao;
                                SegundaOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                                SegundaOperacaoSaida.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                                          .AsParallel()
                                                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                                                         select c).First();
                                SegundaOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                                if (operacaoCaixa.Observacao != null)
                                {
                                    operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                                }
                                SegundaOperacaoSaida.Observacao = operacaoCaixa.Observacao;
                                SegundaOperacaoSaida.Valor = operacaoCaixa.Valor * -1;

                                SegundaOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                SegundaOperacaoSaida.DataHoraInsercao = DateTime.Now;
                                SegundaOperacaoSaida.TipoOperacao = EnumTipoOperacao.Recolhimento;
                                _contextoOperacao.Add<OperacaoCaixa>(SegundaOperacaoSaida);
                                _contextoOperacao.SaveChanges();


                                OperacaoCaixa SegundaOperacaoEntrada = new OperacaoCaixa();

                                SegundaOperacaoEntrada.DataLancamento = operacaoCaixaEntrada.DataLancamento;
                                SegundaOperacaoEntrada.Descricao = " Entrada Transferencia " + operacaoCaixa.Descricao;
                                SegundaOperacaoEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                                SegundaOperacaoEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                                SegundaOperacaoEntrada.Observacao = SegundaOperacaoSaida.Codigo.ToString();
                                SegundaOperacaoEntrada.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                                            .Where(x => x.Nome == NomeUsuarioLogado)
                                                                           select c).First();
                                SegundaOperacaoEntrada.Valor = operacaoCaixaEntrada.Valor;
                                SegundaOperacaoEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                SegundaOperacaoEntrada.DataHoraInsercao = DateTime.Now;
                                SegundaOperacaoEntrada.TipoOperacao = EnumTipoOperacao.Recebimento;
                                _contextoOperacao.Add<OperacaoCaixa>(SegundaOperacaoEntrada);
                                _contextoOperacao.SaveChanges();
                                return RedirectToAction("Sucesso", "Home");


                            }



                        }



                    }


                }
                else
                {
                    OperacaoCaixa TerceiraOperacaoSaida = new OperacaoCaixa();

                    TerceiraOperacaoSaida.DataLancamento = operacaoCaixa.DataLancamento;
                    TerceiraOperacaoSaida.Descricao = "RECOLHIMENTO " + operacaoCaixa.Descricao;
                    TerceiraOperacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                    TerceiraOperacaoSaida.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                              .AsParallel()
                                                              .Where(x => x.Nome == NomeUsuarioLogado)
                                                              select c).First();
                    TerceiraOperacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    if (operacaoCaixa.Observacao != null)
                    {
                        operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                    }
                    TerceiraOperacaoSaida.Observacao = operacaoCaixa.Observacao;
                    TerceiraOperacaoSaida.Valor = operacaoCaixa.Valor * -1;

                    TerceiraOperacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    TerceiraOperacaoSaida.DataHoraInsercao = DateTime.Now;
                    TerceiraOperacaoSaida.TipoOperacao = EnumTipoOperacao.Recolhimento;
                    _contextoOperacao.Add<OperacaoCaixa>(TerceiraOperacaoSaida);
                    _contextoOperacao.SaveChanges();


                    OperacaoCaixa TerceiraOperacaoEntrada = new OperacaoCaixa();

                    TerceiraOperacaoEntrada.DataLancamento = operacaoCaixaEntrada.DataLancamento;
                    TerceiraOperacaoEntrada.Descricao = " Entrada Transferencia " + operacaoCaixa.Descricao;
                    TerceiraOperacaoEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                    TerceiraOperacaoEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                    TerceiraOperacaoEntrada.Observacao = TerceiraOperacaoSaida.Codigo.ToString();
                    TerceiraOperacaoEntrada.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                       .Where(x => x.Nome == NomeUsuarioLogado)
                                                                select c).First();
                    TerceiraOperacaoEntrada.Valor = operacaoCaixaEntrada.Valor;
                    TerceiraOperacaoEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    TerceiraOperacaoEntrada.DataHoraInsercao = DateTime.Now;
                    TerceiraOperacaoEntrada.TipoOperacao = EnumTipoOperacao.Recebimento;
                    _contextoOperacao.Add<OperacaoCaixa>(TerceiraOperacaoEntrada);
                    _contextoOperacao.SaveChanges();

                    return RedirectToAction("Sucesso", "Home");
                }

            }

            return RedirectToAction("Sucesso", "Home");

        }

        #region Alteracao

        public ActionResult AlterarOperacaoCaixa(int id)
        {
            string NomeUsuarioLogado = User.Identity.Name;
            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
            OperacaoCaixa OperacaoParaAlterar = _contextoOperacao.Get<OperacaoCaixa>(id);
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento).OrderBy(x => x.NomeTipoFormaPagamento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).OrderBy(x => x.Nome), "Codigo", "Nome");


            return View(OperacaoParaAlterar);
        }

        [HttpPost]
        public ActionResult AlterarOperacaoCaixa(OperacaoCaixa operacaoCaixa)
        {
            lock (_contextoOperacao)
            {
                string NomeUsuarioLogado = User.Identity.Name;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
                decimal taxaFormaPagamento = ObtemTaxaFormaPagamento(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                double diasRecebimento = ObtemDiasRecebimentoFormaPagamento(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                OperacaoCaixa OperacaoCaixaParaverificarFormaPagto = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

                //Escritorio
                if (VerificarSeFormaPagamentoBaixaAutomatico(operacaoCaixa.FormaPagamentoUtilizada.Codigo) == false && VerificarSeFormaPagamentoBaixaAutomatico(OperacaoCaixaParaverificarFormaPagto.FormaPagamentoUtilizada.Codigo) == false)
                {
                    OperacaoCaixa operacaoAlterada = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

                    operacaoAlterada.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    operacaoAlterada.Descricao = operacaoCaixa.Descricao;
                    operacaoAlterada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    operacaoAlterada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    operacaoAlterada.Valor = operacaoCaixa.Valor;
                    operacaoAlterada.Observacao = operacaoCaixa.Observacao;
                    operacaoAlterada.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    operacaoAlterada.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    operacaoAlterada.DataHoraInsercao = DateTime.Now;
                    operacaoCaixa.TipoOperacao = operacaoAlterada.TipoOperacao;
                    _contextoOperacao.SaveChanges();

                    OperacaoFinanceiraContaCorrente OperacaoDeposito = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                        .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoAlterada.Codigo)
                                                                        select c).First();

                    OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                    OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                    OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                    OperacaoDeposito.Descricao = "Simulação de depósito";
                    OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoDeposito.Taxa = taxaFormaPagamento;
                    OperacaoDeposito.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoDeposito.Valor = operacaoCaixa.Valor;
                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                    OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(operacaoAlterada.Codigo);
                    _contextoOperacao.SaveChanges();



                    return RedirectToAction("Sucesso", "Home");
                }

                else if (VerificarSeFormaPagamentoBaixaAutomatico(OperacaoCaixaParaverificarFormaPagto.FormaPagamentoUtilizada.Codigo) == true && VerificarSeFormaPagamentoBaixaAutomatico(operacaoCaixa.FormaPagamentoUtilizada.Codigo) == true)
                {

                    OperacaoCaixa OperacaoPositiva = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

                    OperacaoPositiva.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    OperacaoPositiva.Descricao = operacaoCaixa.Descricao;
                    OperacaoPositiva.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    OperacaoPositiva.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoPositiva.Valor = operacaoCaixa.Valor;
                    OperacaoPositiva.Observacao = operacaoCaixa.Observacao;
                    OperacaoPositiva.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoPositiva.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoPositiva.DataHoraInsercao = DateTime.Now;
                    OperacaoPositiva.TipoOperacao = OperacaoPositiva.TipoOperacao;
                    TryUpdateModel<OperacaoCaixa>(OperacaoPositiva);
                    _contextoOperacao.SaveChanges();
                    //Fim da Operacao Positiva

                    OperacaoCaixa OperacaoNegativa = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                      .Where(x => x.Observacao.ToString() == operacaoCaixa.Codigo.ToString())
                                                      select c).First();
                    OperacaoNegativa.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    OperacaoNegativa.Descricao = operacaoCaixa.Descricao;
                    OperacaoNegativa.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    OperacaoNegativa.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoNegativa.Valor = operacaoCaixa.Valor * -1;
                    OperacaoNegativa.Observacao = OperacaoPositiva.Codigo.ToString();
                    OperacaoNegativa.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoNegativa.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoNegativa.DataHoraInsercao = DateTime.Now;
                    OperacaoNegativa.TipoOperacao = OperacaoNegativa.TipoOperacao;
                    _contextoOperacao.SaveChanges();


                    DespejoOPeracaoCaixa Despejo = (from c in _contextoOperacao.GetAll<DespejoOPeracaoCaixa>()
                                                    .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoCaixa.Codigo)
                                                    select c).First();
                    Despejo.DataHoraInsercao = DateTime.Now;
                    Despejo.DataLancamento = DateTime.Now;
                    Despejo.Descricao = "Alteração de Despejo Automático";
                    Despejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    Despejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    Despejo.Observacao = Despejo.GenerateId();
                    Despejo.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);
                    Despejo.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    Despejo.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    Despejo.Valor = OperacaoNegativa.Valor * -1;
                    _contextoOperacao.SaveChanges();


                    OperacaoFinanceiraContaCorrente OperacaoDeposito = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                        .Where(x => x.OperacaoCaixaOrigem.Codigo == OperacaoPositiva.Codigo)
                                                                        select c).FirstOrDefault();

                    OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                    OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                    OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                    OperacaoDeposito.Descricao = "Simulação de depósito";
                    OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoDeposito.Taxa = taxaFormaPagamento;
                    OperacaoDeposito.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoDeposito.Valor = operacaoCaixa.Valor;
                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                    OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(OperacaoPositiva.Codigo);
                    _contextoOperacao.SaveChanges();
                }
                else if (VerificarSeFormaPagamentoBaixaAutomatico(OperacaoCaixaParaverificarFormaPagto.FormaPagamentoUtilizada.Codigo) == false && VerificarSeFormaPagamentoBaixaAutomatico(operacaoCaixa.FormaPagamentoUtilizada.Codigo) == true)
                {

                    OperacaoCaixa OperacaoPositiva = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

                    OperacaoPositiva.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    OperacaoPositiva.Descricao = operacaoCaixa.Descricao;
                    OperacaoPositiva.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    OperacaoPositiva.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoPositiva.Valor = operacaoCaixa.Valor;
                    OperacaoPositiva.Observacao = operacaoCaixa.Observacao;
                    OperacaoPositiva.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoPositiva.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoPositiva.DataHoraInsercao = DateTime.Now;
                    OperacaoPositiva.TipoOperacao = OperacaoPositiva.TipoOperacao;
                    _contextoOperacao.SaveChanges();


                    OperacaoCaixa OperacaoNegativa = new OperacaoCaixa();

                    OperacaoNegativa.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    OperacaoNegativa.Descricao = operacaoCaixa.Descricao;
                    OperacaoNegativa.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    OperacaoNegativa.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoNegativa.Valor = operacaoCaixa.Valor * -1;
                    OperacaoNegativa.Observacao = OperacaoPositiva.Codigo.ToString();
                    OperacaoNegativa.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoNegativa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoNegativa.DataHoraInsercao = DateTime.Now;
                    OperacaoNegativa.TipoOperacao = OperacaoNegativa.TipoOperacao;
                    _contextoOperacao.Add<OperacaoCaixa>(OperacaoNegativa);
                    _contextoOperacao.SaveChanges();


                    DespejoOPeracaoCaixa Despejo = new DespejoOPeracaoCaixa();
                    Despejo.DataHoraInsercao = DateTime.Now;
                    Despejo.DataLancamento = DateTime.Now;
                    Despejo.Descricao = "Alteração de Despejo Automático";
                    Despejo.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    Despejo.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    Despejo.Observacao = Despejo.GenerateId();
                    Despejo.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);
                    Despejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    Despejo.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    Despejo.Valor = OperacaoNegativa.Valor * -1;
                    _contextoOperacao.Add<DespejoOPeracaoCaixa>(Despejo);
                    _contextoOperacao.SaveChanges();


                    OperacaoFinanceiraContaCorrente OperacaoDeposito = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                        .Where(x => x.OperacaoCaixaOrigem.Codigo == OperacaoPositiva.Codigo)
                                                                        select c).FirstOrDefault();

                    OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                    OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                    OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                    OperacaoDeposito.Descricao = "Simulação de depósito";
                    OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoDeposito.Taxa = taxaFormaPagamento;
                    OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoDeposito.Valor = operacaoCaixa.Valor;
                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                    OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(OperacaoPositiva.Codigo);

                    _contextoOperacao.SaveChanges();




                }
                else if (VerificarSeFormaPagamentoBaixaAutomatico(OperacaoCaixaParaverificarFormaPagto.FormaPagamentoUtilizada.Codigo) == true && VerificarSeFormaPagamentoBaixaAutomatico(operacaoCaixa.FormaPagamentoUtilizada.Codigo) == false)
                {

                    OperacaoCaixa OperacaoPositiva = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

                    OperacaoPositiva.DataLancamento = operacaoCaixa.DataLancamento.Date;
                    OperacaoPositiva.Descricao = operacaoCaixa.Descricao;
                    OperacaoPositiva.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                    OperacaoPositiva.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoPositiva.Valor = operacaoCaixa.Valor;
                    OperacaoPositiva.Observacao = operacaoCaixa.Observacao;
                    OperacaoPositiva.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoPositiva.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoPositiva.DataHoraInsercao = DateTime.Now;
                    OperacaoPositiva.TipoOperacao = OperacaoPositiva.TipoOperacao;
                    OperacaoPositiva.Conferido = false;
                    _contextoOperacao.SaveChanges();

                    OperacaoCaixa OperacaoCaixaNegativaParaExclusao = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                                       .Where(x => x.Observacao == OperacaoPositiva.Codigo.ToString())
                                                                       select c).First();
                    _contextoOperacao.Delete<OperacaoCaixa>(OperacaoCaixaNegativaParaExclusao);
                    _contextoOperacao.SaveChanges();

                    OperacaoFinanceiraContaCorrente OperacaoDeposito = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                        .Where(x => x.OperacaoCaixaOrigem.Codigo == OperacaoPositiva.Codigo)
                                                                        select c).First();

                    OperacaoDeposito.ContaLancamento = _contextoOperacao.Get<ContaCorrente>(ObtemNumeroContaCorrente(operacaoCaixa.FormaPagamentoUtilizada.Codigo));
                    OperacaoDeposito.Data = operacaoCaixa.DataLancamento;
                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                    OperacaoDeposito.DataRecebimento = operacaoCaixa.DataLancamento.AddDays(diasRecebimento);
                    OperacaoDeposito.Descricao = "Simulação de depósito";
                    OperacaoDeposito.FormaPagamento = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
                    OperacaoDeposito.Taxa = taxaFormaPagamento;
                    OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoDeposito.Valor = operacaoCaixa.Valor;
                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * taxaFormaPagamento) / 100);
                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                    OperacaoDeposito.OperacaoCaixaOrigem = _contextoOperacao.Get<OperacaoCaixa>(OperacaoPositiva.Codigo);

                    _contextoOperacao.SaveChanges();

                }


            }

            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult AlterarRecolhimento(int id)
        {
            string NomeUsuarioLogado = User.Identity.Name;
            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
            OperacaoCaixa OperacaoParaAlterar = _contextoOperacao.Get<OperacaoCaixa>(id);
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.Nome == NomeUsuarioLogado), "Codigo", "Nome");

            return View(OperacaoParaAlterar);
        }


        [HttpPost]
        public ActionResult AlterarRecolhimento(OperacaoCaixa operacaoCaixaRecolhimento)
        {
            try
            {
                decimal valorRecolhimento = operacaoCaixaRecolhimento.Valor > 0 ? operacaoCaixaRecolhimento.Valor : operacaoCaixaRecolhimento.Valor * -1;
                OperacaoCaixa OperacaoCaixaRecolhimentoNegativa = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixaRecolhimento.Codigo);
                OperacaoCaixa OperacaoCaixaRecolhimentoPositiva = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                                  .AsParallel()
                                                                  .Where(x => x.TipoOperacao == EnumTipoOperacao.Recebimento && x.Observacao == OperacaoCaixaRecolhimentoNegativa.Codigo.ToString())
                                                                   select c).FirstOrDefault();

                OperacaoCaixaRecolhimentoNegativa.Conferido = operacaoCaixaRecolhimento.Conferido;
                OperacaoCaixaRecolhimentoNegativa.DataHoraInsercao = OperacaoCaixaRecolhimentoNegativa.DataHoraInsercao;
                OperacaoCaixaRecolhimentoNegativa.DataLancamento = operacaoCaixaRecolhimento.DataLancamento;
                OperacaoCaixaRecolhimentoNegativa.Descricao = operacaoCaixaRecolhimento.Descricao;
                OperacaoCaixaRecolhimentoNegativa.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(OperacaoCaixaRecolhimentoNegativa.EstabelecimentoOperacao.Codigo);
                OperacaoCaixaRecolhimentoNegativa.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixaRecolhimento.FormaPagamentoUtilizada.Codigo);
                OperacaoCaixaRecolhimentoNegativa.Observacao = operacaoCaixaRecolhimento.Observacao;
                OperacaoCaixaRecolhimentoNegativa.TipoOperacao = EnumTipoOperacao.Recolhimento;
                OperacaoCaixaRecolhimentoNegativa.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaRecolhimentoNegativa.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixaRecolhimento.UsuarioQueLancou.Codigo);
                if (operacaoCaixaRecolhimento.Valor > 0)
                {
                    OperacaoCaixaRecolhimentoNegativa.Valor = operacaoCaixaRecolhimento.Valor * -1;
                }
                else
                {
                    OperacaoCaixaRecolhimentoNegativa.Valor = operacaoCaixaRecolhimento.Valor;
                }


                OperacaoCaixaRecolhimentoPositiva.Conferido = OperacaoCaixaRecolhimentoPositiva.Conferido;
                OperacaoCaixaRecolhimentoPositiva.DataHoraInsercao = OperacaoCaixaRecolhimentoPositiva.DataHoraInsercao;
                OperacaoCaixaRecolhimentoPositiva.DataLancamento = OperacaoCaixaRecolhimentoPositiva.DataLancamento;
                OperacaoCaixaRecolhimentoPositiva.Descricao = operacaoCaixaRecolhimento.Descricao;
                OperacaoCaixaRecolhimentoPositiva.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(OperacaoCaixaRecolhimentoPositiva.EstabelecimentoOperacao.Codigo);
                OperacaoCaixaRecolhimentoPositiva.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixaRecolhimento.FormaPagamentoUtilizada.Codigo);
                OperacaoCaixaRecolhimentoPositiva.Observacao = OperacaoCaixaRecolhimentoNegativa.Codigo.ToString();
                OperacaoCaixaRecolhimentoPositiva.TipoOperacao = EnumTipoOperacao.Recebimento;
                OperacaoCaixaRecolhimentoPositiva.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaRecolhimentoPositiva.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixaRecolhimento.UsuarioQueLancou.Codigo);
                if (operacaoCaixaRecolhimento.Valor < 0)
                {
                    OperacaoCaixaRecolhimentoPositiva.Valor = operacaoCaixaRecolhimento.Valor * -1;

                }
                else
                {
                    OperacaoCaixaRecolhimentoPositiva.Valor = operacaoCaixaRecolhimento.Valor;
                }

                _contextoOperacao.SaveChanges();




            }
            catch (Exception)
            {
                string NomeUsuarioLogado = User.Identity.Name;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");
                ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.Nome == NomeUsuarioLogado), "Codigo", "Nome");

                ViewBag.Mensagem = "Não foi possível alterar a operação equivalente";

                return View();
            }



            return RedirectToAction("Sucesso", "Home");
        }

        #endregion

        #region Exclusao
        public ActionResult ExcluirOperacaoCaixa(int id)
        {
            OperacaoCaixa operacaoParaExcluir = _contextoOperacao.Get<OperacaoCaixa>(id);
            return View(operacaoParaExcluir);
        }

        [HttpPost]
        public ActionResult ExcluirOperacaoCaixa(OperacaoCaixa operacaoExcluida)
        {
            lock (_contextoOperacao)
            {
                if (VerificarSeFormaPagamentoBaixaAutomatico(operacaoExcluida.FormaPagamentoUtilizada.Codigo) == false)
                {
                    OperacaoCaixa operacao = _contextoOperacao.Get<OperacaoCaixa>(operacaoExcluida.Codigo);
                    _contextoOperacao.Delete<OperacaoCaixa>(operacao);
                    _contextoOperacao.SaveChanges();

                    OperacaoFinanceiraContaCorrente OperacaoFinanceiraEquivalente = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                                     .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoExcluida.Codigo)
                                                                                     select c).FirstOrDefault();
                    _contextoOperacao.Delete<OperacaoFinanceiraContaCorrente>(OperacaoFinanceiraEquivalente);
                    _contextoOperacao.SaveChanges();


                }
                else
                {
                    OperacaoCaixa OperacaoPositiva = _contextoOperacao.Get<OperacaoCaixa>(operacaoExcluida.Codigo);
                    OperacaoCaixa OperacaoNegativa = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                      .Where(x => x.Observacao.ToString() == operacaoExcluida.Codigo.ToString())
                                                      select c).First();
                    DespejoOPeracaoCaixa Despejo = (from c in _contextoOperacao.GetAll<DespejoOPeracaoCaixa>()
                                                    .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoExcluida.Codigo)
                                                    select c).First();

                    OperacaoFinanceiraContaCorrente OperacaoFinanceiraEquivalente = (from c in _contextoOperacao.GetAll<OperacaoFinanceiraContaCorrente>()
                                                                                    .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoExcluida.Codigo)
                                                                                     select c).FirstOrDefault();

                    _contextoOperacao.Delete<DespejoOPeracaoCaixa>(Despejo);
                    _contextoOperacao.SaveChanges();

                    _contextoOperacao.Delete<OperacaoCaixa>(OperacaoNegativa);
                    _contextoOperacao.SaveChanges();

                    _contextoOperacao.Delete<OperacaoCaixa>(OperacaoPositiva);
                    _contextoOperacao.SaveChanges();

                    _contextoOperacao.Delete<OperacaoFinanceiraContaCorrente>(OperacaoFinanceiraEquivalente);
                    _contextoOperacao.SaveChanges();
                }


            }

            return RedirectToAction("Sucesso", "Home");
        }

        #endregion

        #region Consultas
        public ActionResult OperacoesNestaData(DateTime? id)
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);
            IList<OperacaoCaixa> listaPorData = null;
            listaPorData = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.DataLancamento.Date == id && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).ToList();

            return View(listaPorData);
        }

        public ActionResult VerUsuario(int id, DateTime? data)
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);
            IList<OperacaoCaixa> listaPorNome = null;
            listaPorNome = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.UsuarioQueLancou.Codigo == id && x.DataLancamento.Date == data && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).ToList();

            return View(listaPorNome);
        }

        public ActionResult VerPorFormaPagamento(int forma, DateTime? data)
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);
            IList<OperacaoCaixa> listaPorForma = null;
            listaPorForma = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.FormaPagamentoUtilizada.Codigo == forma && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.DataLancamento.Date == data).OrderByDescending(x => x.DataLancamento).ToList();

            return View(listaPorForma);
        }

        public ActionResult CartoesPorData()
        {
            if (VerificarUsuarioTemPermissao() == false)
            {
                string NomeUsuarioLogado = User.Identity.Name;
                long CodigoEstabelecimentoLocalTrabalho = BuscaEstabelecimento(NomeUsuarioLogado);
                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1 && x.Codigo == CodigoEstabelecimentoLocalTrabalho), "Codigo", "RazaoSocial");
                return View();
            }
            ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
            return View();
        }

        [HttpPost]
        public ActionResult CartoesPorData(ValidarData Datas, int? Loja, int? FormaPagto)
        {
            if (Loja == null || FormaPagto == null)
            {
                ViewBag.OpcaoInvalida = "Por favor selecione uma opcao válida";
                return View("Error");
            }
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";

            }
            if (ModelState.IsValid)
            {
                ViewBag.DataInicio = Datas.DataInicial;
                ViewBag.DataFim = Datas.DataFinal;

                IList<OperacaoCaixa> ListaCartoesPorData = _contextoOperacao.GetAll<OperacaoCaixa>()
                                                          .Where(x => x.EstabelecimentoOperacao.Codigo == Loja && x.FormaPagamentoUtilizada.Codigo == FormaPagto
                                                          && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal && x.Valor > 0)
                                                          .OrderBy(t => t.DataLancamento)
                                                           .ToList();
                return View("RelatoriosCartoesPorData", ListaCartoesPorData);

            }

            return View("Error");
        }


        public ActionResult LancamentosData()
        {
            string NomeUsuario = User.Identity.Name;
            bool UsuarioEPrivilegiado = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeUsuario)
                                         select c.Privilegiado).First();
            if (UsuarioEPrivilegiado == false)
            {
                ViewBag.OperadorCaixa = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == BuscaEstabelecimento(NomeUsuario)), "Codigo", "Nome");
                return View();

            }
            else
            {
                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
                ViewBag.OperadorCaixa = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == BuscaEstabelecimento(NomeUsuario)), "Codigo", "Nome");
                ViewBag.UsuarioPrevilegiado = UsuarioEPrivilegiado;
                return View();
            }


        }

        [HttpPost]
        public ActionResult LancamentosData(ValidarData Datas, int? Loja, int? OperadorCaixa)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = User.Identity.Name + ", a data final não pode ser menor que a data inicial!";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (Loja == null)
                    {
                        string nomeUsuario = User.Identity.Name;
                        ViewBag.DataInicio = Datas.DataInicial;
                        ViewBag.DataFim = Datas.DataFinal;

                        long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);


                        IList<OperacaoCaixa> listaPorData = null;
                        IList<Pagamento> ListaPagtos = (from c in _contextoOperacao.GetAll<Pagamento>()
                                                        .Where(x => x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento)
                                                        join o in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                        on c.CodigoOperacaoCaixa equals o.Codigo
                                                        select c).ToList();
                        var ListaPagamentosVazias = new HashSet<string>(ListaPagtos.Select(x => x.Codigo.ToString()));
                        if (OperadorCaixa == null)
                        {
                            listaPorData = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                          .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento
                                           && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento)
                                            select c).ToList();

                        }
                        else
                        {
                            listaPorData = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                          .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.UsuarioQueLancou.Codigo == OperadorCaixa
                                           && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento)
                                            select c).ToList();
                        }



                        IList<OperacaoCaixa> listaPorDataSemPagamento = listaPorData.Where(x => !ListaPagamentosVazias.Contains(x.Descricao)).ToList();

                        ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                        .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                         select c.Valor).Sum();

                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                             select c).Count() == 0)
                        {
                            ViewBag.Saldo = 0;

                        }
                        else
                        {
                            ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                            .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                             select c.Valor).Sum();

                        }


                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor >= 0)
                             select c).Count() == 0)
                        {
                            ViewBag.Entrada = 0;

                        }
                        else
                        {
                            ViewBag.Entrada = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                              .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor >= 0)
                                               select c.Valor).Sum();

                        }

                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0)
                             select c).ToList().Count() == 0)
                        {
                            ViewBag.Saida = 0;
                        }
                        else
                        {
                            ViewBag.Saida = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                            .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0)
                                             select c.Valor).Sum();
                        }
                        return View("TodosLancamentos", listaPorDataSemPagamento);
                    }
                    else
                    {
                        ViewBag.DataInicio = Datas.DataInicial;
                        ViewBag.DataFim = Datas.DataFinal;
                        ViewBag.Loja = (from e in _contextoOperacao.GetAll<Estabelecimento>()
                                        .Where(c => c.Codigo == Loja)
                                        select e.RazaoSocial).First();


                        IList<OperacaoCaixa> listaPorData = null;

                        IList<Pagamento> ListaPagtos = (from c in _contextoOperacao.GetAll<Pagamento>()
                                                       .Where(x => x.EstabelecimentoQuePagou.Codigo == Loja)
                                                        join o in _contextoOperacao.GetAll<OperacaoCaixa>()
                                                        on c.CodigoOperacaoCaixa equals o.Codigo
                                                        select c).ToList();
                        var ListaPagamentosVazias = new HashSet<string>(ListaPagtos.Select(x => x.Codigo.ToString()));



                        listaPorData = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                        .Where(x => x.EstabelecimentoOperacao.Codigo == Loja
                                        && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal && x.Valor < 0 != x.Observacao.StartsWith("Pagamento") && x.FormaPagamentoUtilizada.DespejoAutomatico == false).OrderByDescending(X => X.DataLancamento)
                                        select c).ToList();

                        IList<OperacaoCaixa> listaPorDataSemPagamento = listaPorData.Where(x => !ListaPagamentosVazias.Contains(x.Descricao)).ToList();

                        ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                        .Where(x => x.EstabelecimentoOperacao.Codigo == Loja)
                                         select c.Valor).Sum();

                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == Loja)
                             select c).Count() == 0)
                        {
                            ViewBag.Saldo = 0;

                        }
                        else
                        {
                            ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                            .Where(x => x.EstabelecimentoOperacao.Codigo == Loja)
                                             select c.Valor).Sum();

                        }


                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == Loja && x.Valor >= 0)
                             select c).Count() == 0)
                        {
                            ViewBag.Entrada = 0;

                        }
                        else
                        {
                            ViewBag.Entrada = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                            .Where(x => x.EstabelecimentoOperacao.Codigo == Loja && x.Valor >= 0)
                                               select c.Valor).Sum();

                        }

                        if ((from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == Loja && x.Valor <= 0)
                             select c).ToList().Count() == 0)
                        {
                            ViewBag.Saida = 0;
                        }
                        else
                        {
                            ViewBag.Saida = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                            .Where(x => x.EstabelecimentoOperacao.Codigo == Loja && x.Valor <= 0)
                                             select c.Valor).Sum();
                        }
                        return View("TodosLancamentos", listaPorDataSemPagamento);

                    }



                }
            }

            return View();
        }

        public ActionResult SaidasData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaidasData(ValidarData Datas)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = User.Identity.Name + ", a data final não pode ser menor que a data inicial!";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    ViewBag.DataInicio = Datas.DataInicial;
                    ViewBag.DataFim = Datas.DataFinal;
                    string nomeUsuario = User.Identity.Name;

                    long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);

                    ViewBag.Loja = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                .Where(x => x.Nome == nomeUsuario)
                                    select c.EstabelecimentoTrabalho.RazaoSocial).First();

                    IList<OperacaoCaixa> listaPorData = null;

                    listaPorData = _contextoOperacao.GetAll<OperacaoCaixa>()
                                  .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0 && x.TipoOperacao == EnumTipoOperacao.Recolhimento && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento).ToList();

                    ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                     .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                     select c.Valor).Sum();
                    ViewBag.SomaSaida = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                         .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0 && x.TipoOperacao == EnumTipoOperacao.Recolhimento && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento).ToList()
                                         select c.Valor).Sum();


                    return View("TodasSaidas", listaPorData);
                }
            }

            return View();
        }

        public ActionResult ErroOPeracaoDespejo()
        {
            return View();
        }

        public ActionResult TodosLancamentos()
        {

            return View();
        }

        public ActionResult TodasSaidas()
        {
            return View();
        }


        public ActionResult EstabelecimentoReceptor(int id)
        {
            OperacaoCaixa OperacaoCaixaNegativa = _contextoOperacao.Get<OperacaoCaixa>(id);
            OperacaoCaixa OperacaoCaixaPositiva = _contextoOperacao.GetAll<OperacaoCaixa>()
                                                  .Where(x => x.Valor == OperacaoCaixaNegativa.Valor * -1 && x.DataLancamento == OperacaoCaixaNegativa.DataLancamento && x.FormaPagamentoUtilizada.Codigo == OperacaoCaixaNegativa.FormaPagamentoUtilizada.Codigo && x.UsuarioQueLancou.Codigo == OperacaoCaixaNegativa.UsuarioQueLancou.Codigo)
                                                  .First();
            ViewBag.EstabelecimentoEmissor = (from c in _contextoOperacao.GetAll<Estabelecimento>()
                                              .Where(x => x.Codigo == OperacaoCaixaNegativa.EstabelecimentoOperacao.Codigo)
                                              select c.RazaoSocial).First();
            return View(OperacaoCaixaPositiva);
        }

        [GridAction]
        public ActionResult _Lancamentos()
        {
            string nomeUsuario = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);


            var Lancamentos = (from op in _contextoOperacao.GetAll<OperacaoCaixa>()
                              .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                               join es in _contextoOperacao.GetAll<Estabelecimento>()
                               on op.EstabelecimentoOperacao.Codigo equals es.Codigo
                               join fp in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                               on op.FormaPagamentoUtilizada.Codigo equals fp.Codigo
                               join cd in _contextoOperacao.GetAll<CadastrarUsuario>()
                               on op.UsuarioQueLancou.Codigo equals cd.Codigo
                               select new { DataLancamento = op.DataLancamento, FormaPagamentoUtilizada = op.FormaPagamentoUtilizada.NomeTipoFormaPagamento, Valor = op.Valor, Descricao = op.Descricao, UsuarioQueLancou = op.UsuarioQueLancou.Nome });


            return View(new GridModel(Lancamentos.ToList()));
        }

        public ActionResult Lancamentos()
        {
            return View();
        }

        public ActionResult ErroOperacao()
        {

            return View();
        }

        public ActionResult OPeracaoConferida(int id)
        {
            if (VerificarUsuarioTemPermissao() == false)
            {
                return RedirectToAction("UsuarioSemPermissao", "OperacaoContaCorrente");
            }
            OperacaoCaixa OperacaoCaixaParaConferir = _contextoOperacao.Get<OperacaoCaixa>(id);
            OperacaoCaixaParaConferir.Conferido = true;
            _contextoOperacao.SaveChanges();
            return View("MostrarOperacaoConferida", OperacaoCaixaParaConferir);
        }

        public ActionResult MostrarOperacaoConferida()
        {
            return View();
        }

        public ActionResult RelatoriosCartoesPorData()
        {
            return View();
        }

        #endregion

        #region Metodos

        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();

            return codEstabelecimento;
        }

        private long BuscaFormaPagamento(long CodigoFormaPagamento)
        {
            long CodigoDaFormaPagamentoEncontrado = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                                     .Where(x => x.Codigo == CodigoFormaPagamento)
                                                     select c.Codigo).First();
            return CodigoDaFormaPagamentoEncontrado;
        }

        private bool VerificarUsuarioTemPermissao()
        {

            string NomeUsuarioLogadoVericarPermissao = User.Identity.Name;
            IList<CadastrarUsuario> ListaCadastroUsuarioVerificarPermissao = _contextoOperacao.GetAll<CadastrarUsuario>()
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

        public string FormasPagamentoEstabelecimento(string id)
        {
            var Formas = new List<SelectListItem>();

            IList<FormaPagamentoEstabelecimento> FormasEncontradas = _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                                                    .Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo.ToString() == id)
                                                                    .ToList();
            if (FormasEncontradas.Count() > 0)
            {
                foreach (var item in FormasEncontradas)
                {
                    Formas.Add(new SelectListItem() { Text = item.NomeTipoFormaPagamento.ToString(), Value = item.Codigo.ToString() });
                }
            }

            return new JavaScriptSerializer().Serialize(Formas);
        }



        private bool VerificaSeSistemaDepositaAutomatico()
        {
            bool seDeposita = (from c in _contextoOperacao.GetAll<Configuracao>()
                               select c.FazerLancamentoContaCorrente).FirstOrDefault();
            return seDeposita;
        }


        private long ObtemNumeroContaCorrente(long codigoFormaPagamento)
        {
            long codConta = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                             .Where(x => x.Codigo == codigoFormaPagamento)
                             select c.ContaCorrenteFormaPagamento.Codigo).FirstOrDefault();
            return codConta;
        }


        private decimal ObtemTaxaFormaPagamento(long codFormaPagamento)
        {
            decimal taxaFormaPagamento = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                        .Where(x => x.Codigo == codFormaPagamento)
                                          select c.TaxaFormaPagamento).FirstOrDefault();
            return taxaFormaPagamento;

        }

        private double ObtemDiasRecebimentoFormaPagamento(long codFormaPagamento)
        {
            double numeroDiasRecebimento = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                             .Where(x => x.Codigo == codFormaPagamento)
                                            select c.DiasRecebimento).FirstOrDefault();

            return numeroDiasRecebimento;
        }

        private bool VerificarSeFormaPagamentoBaixaAutomatico(long id)
        {
            bool EdespejoAutomatico = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                       .Where(x => x.Codigo == id)
                                       select c.DespejoAutomatico).First();
            return EdespejoAutomatico;
        }


        #endregion
    }
}
