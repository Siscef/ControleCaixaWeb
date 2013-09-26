using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.OperadorCaixa.Controllers
{
    [Authorize(Roles = "OperadorCaixa")]
    [HandleError(View = "Error")]
    public class OperacaoCaixaController : Controller
    {
        private IContextoDados _contextoOperacaocaixa = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }



        public ActionResult SangriaOperacaoCaixa()
        {
            string NomeDoCaixa = User.Identity.Name;
            long codigoEstabelecimento = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeDoCaixa)
                                          select c.EstabelecimentoTrabalho.Codigo).First();

            ViewBag.NomeForma = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");
            return View();
        }

        [HttpPost]
        public ActionResult SangriaOperacaoCaixa(OperacaoCaixa operacaoSangria)
        {
            lock (_contextoOperacaocaixa)
            {
                string NomeDoCaixa = User.Identity.Name;

                OperacaoCaixa OperacaoCaixaSangria = new OperacaoCaixa();

                OperacaoCaixaSangria.DataLancamento = DateTime.Now;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);

                long ContadorSangria = (from c in _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                                        .Where(x => x.DataLancamento.Date == OperacaoCaixaSangria.DataLancamento.Date && x.TipoOperacao == EnumTipoOperacao.Sangria && x.UsuarioQueLancou.Nome == NomeDoCaixa)
                                        select c).Count();
                ContadorSangria = ContadorSangria + 1;
                OperacaoCaixaSangria.Descricao = "SANGRIA:  " + ContadorSangria;


                if (operacaoSangria.Valor < 0)
                {

                    OperacaoCaixaSangria.Valor = operacaoSangria.Valor * -1;
                }
                else
                {
                    OperacaoCaixaSangria.Valor = operacaoSangria.Valor;
                }

                OperacaoCaixaSangria.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                         .Where(x => x.Nome == NomeDoCaixa)
                                                         select c).First();

                OperacaoCaixaSangria.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(operacaoSangria.FormaPagamentoUtilizada.Codigo);


                OperacaoCaixaSangria.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                OperacaoCaixaSangria.Observacao = operacaoSangria.Observacao;
                OperacaoCaixaSangria.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaSangria.DataHoraInsercao = DateTime.Now;
                OperacaoCaixaSangria.TipoOperacao = EnumTipoOperacao.Sangria;
                _contextoOperacaocaixa.Add<OperacaoCaixa>(OperacaoCaixaSangria);
                _contextoOperacaocaixa.SaveChanges();
            }
            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult SangriaRapidaOperacaoCaixa()
        {
            string NomeDoCaixa = User.Identity.Name;
            long codigoEstabelecimento = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeDoCaixa)
                                          select c.EstabelecimentoTrabalho.Codigo).First();

            IList<FormaPagamentoEstabelecimento> ListaVerificaFormaPadrao = _contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>()
                                                 .Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false && x.Padrao == true)
                                                 .ToList();
            if (ListaVerificaFormaPadrao.Count() == 0)
            {
                ViewBag.SemFormaPagamento = User.Identity.Name + ", Não existe uma forma de pagamento padrão \n" +
                                                                 "Por favor tente no modo normal.";
                return View();
            }
            else
            {
                return View();
            }


        }

        [HttpPost]
        public ActionResult SangriaRapidaOperacaoCaixa(OperacaoCaixa OperacaoCaixaSangria)
        {
            lock (_contextoOperacaocaixa)
            {
                string NomeDoCaixa = User.Identity.Name;

                OperacaoCaixa SangriaRapida = new OperacaoCaixa();

                SangriaRapida.DataLancamento = DateTime.Now;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);

                long ContadorSangria = (from c in _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                                        .Where(x => x.DataLancamento.Date == SangriaRapida.DataLancamento.Date && x.TipoOperacao == EnumTipoOperacao.Sangria && x.UsuarioQueLancou.Nome == NomeDoCaixa)
                                        select c).Count();
                ContadorSangria = ContadorSangria + 1;
                SangriaRapida.Descricao = "SANGRIA:  " + ContadorSangria;


                if (OperacaoCaixaSangria.Valor < 0)
                {

                    SangriaRapida.Valor = OperacaoCaixaSangria.Valor * -1;
                }
                else
                {
                    SangriaRapida.Valor = OperacaoCaixaSangria.Valor;
                }

                SangriaRapida.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                         .Where(x => x.Nome == NomeDoCaixa)
                                                  select c).First();

                SangriaRapida.FormaPagamentoUtilizada = (from c in _contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>()
                                                         .Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false && x.Padrao == true)
                                                         select c).FirstOrDefault();



                SangriaRapida.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                SangriaRapida.Observacao = "Sangria rápida";
                SangriaRapida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                SangriaRapida.DataHoraInsercao = DateTime.Now;
                SangriaRapida.TipoOperacao = EnumTipoOperacao.Sangria;
                _contextoOperacaocaixa.Add<OperacaoCaixa>(SangriaRapida);
                _contextoOperacaocaixa.SaveChanges();
            }
            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult LancamentoOperacaoCaixa()
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");

            return View();
        }



        [HttpPost]
        public ActionResult LancamentoOperacaoCaixa(OperacaoCaixa OperacaoCaixaLancamento)
        {

            lock (_contextoOperacaocaixa)
            {
                ModelState["FormaPagamentoUtilizada.NomeTipoFormaPagamento"].Errors.Clear();
                ModelState["UsuarioQueLancou"].Errors.Clear();
                ModelState["EstabelecimentoOperacao"].Errors.Clear();
                if (OperacaoCaixaLancamento.DataLancamento != null)
                {
                    TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(OperacaoCaixaLancamento.DataLancamento.ToString("yyyy/MM/dd HH:mm:ss"));
                    if (diferenca.TotalDays < 0)
                    {
                        ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                        ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", OperacaoCaixaLancamento.FormaPagamentoUtilizada);
                        return View();

                    }
                    if (diferenca.TotalDays > 31)
                    {
                        ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                        ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", OperacaoCaixaLancamento.FormaPagamentoUtilizada);
                        return View();

                    }


                }
                if (ModelState.IsValid)
                {

                    IList<OperacaoCaixa> ListaVerificarLancamentoDuplicado = _contextoOperacaocaixa.GetAll<OperacaoCaixa>().AsParallel()
                                                                           .Where(x => x.Valor == OperacaoCaixaLancamento.Valor && x.Observacao == OperacaoCaixaLancamento.Observacao && x.UsuarioQueLancou.Nome == User.Identity.Name)
                                                                           .ToList();
                    bool VerificaSeSistemaFazDepositoAutomatico = (from c in _contextoOperacaocaixa.GetAll<Configuracao>()
                                                                   select c.FazerLancamentoContaCorrente).FirstOrDefault();


                    if (ListaVerificarLancamentoDuplicado.Count() >= 1)
                    {
                        foreach (var itemListaVerificaOperacaoDuplicada in ListaVerificarLancamentoDuplicado)
                        {
                            if (itemListaVerificaOperacaoDuplicada.DataHoraInsercao == null)
                            {
                                string NomeDoCaixa = User.Identity.Name;

                                //Entrada no caixa

                                OperacaoCaixa PrimeiroLancamentoOperacaoCaixa = new OperacaoCaixa();

                                if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                                {

                                    PrimeiroLancamentoOperacaoCaixa.DataLancamento = DateTime.Now;
                                }
                                else
                                {
                                    PrimeiroLancamentoOperacaoCaixa.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                                }
                                PrimeiroLancamentoOperacaoCaixa.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                                OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));

                                if (OperacaoCaixaLancamento.Valor < 0)
                                {

                                    PrimeiroLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor * -1;
                                }
                                else
                                {
                                    PrimeiroLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor;
                                }
                                long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);

                                PrimeiroLancamentoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                                                   .Where(x => x.Nome == NomeDoCaixa)
                                                                                    select c).First();

                                PrimeiroLancamentoOperacaoCaixa.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                PrimeiroLancamentoOperacaoCaixa.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                PrimeiroLancamentoOperacaoCaixa.Observacao = OperacaoCaixaLancamento.Observacao;
                                PrimeiroLancamentoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                PrimeiroLancamentoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                                PrimeiroLancamentoOperacaoCaixa.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                                _contextoOperacaocaixa.Add<OperacaoCaixa>(PrimeiroLancamentoOperacaoCaixa);
                                _contextoOperacaocaixa.SaveChanges();

                                //Operacao Caixa Saída

                                if (VerificarSeFormaPagamentoBaixaAutomatico(PrimeiroLancamentoOperacaoCaixa.FormaPagamentoUtilizada.Codigo) == true)
                                {

                                    OperacaoCaixa PrimeiroLancamentoOperacaoCaixaSaida = new OperacaoCaixa();

                                    if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                                    {

                                        PrimeiroLancamentoOperacaoCaixaSaida.DataLancamento = DateTime.Now;
                                    }
                                    else
                                    {
                                        PrimeiroLancamentoOperacaoCaixaSaida.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                                    }
                                    PrimeiroLancamentoOperacaoCaixaSaida.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                                    OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));



                                    PrimeiroLancamentoOperacaoCaixaSaida.Valor = OperacaoCaixaLancamento.Valor * -1;

                                    PrimeiroLancamentoOperacaoCaixaSaida.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                                                       .Where(x => x.Nome == NomeDoCaixa)
                                                                                             select c).First();

                                    PrimeiroLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                    PrimeiroLancamentoOperacaoCaixaSaida.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                    PrimeiroLancamentoOperacaoCaixaSaida.Observacao = PrimeiroLancamentoOperacaoCaixa.Codigo.ToString();
                                    PrimeiroLancamentoOperacaoCaixaSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    PrimeiroLancamentoOperacaoCaixaSaida.DataHoraInsercao = DateTime.Now;
                                    PrimeiroLancamentoOperacaoCaixaSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                                    _contextoOperacaocaixa.Add<OperacaoCaixa>(PrimeiroLancamentoOperacaoCaixaSaida);
                                    _contextoOperacaocaixa.SaveChanges();

                                    //Fim saída de caixa

                                    //Deposito na conta corrente da forma de pagamento

                                    OperacaoFinanceiraContaCorrente OperacaoDeposito = new OperacaoFinanceiraContaCorrente();
                                    OperacaoDeposito.ContaLancamento = _contextoOperacaocaixa.Get<ContaCorrente>(PrimeiroLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada.ContaCorrenteFormaPagamento.Codigo);
                                    OperacaoDeposito.Data = PrimeiroLancamentoOperacaoCaixaSaida.DataLancamento;
                                    OperacaoDeposito.DataHoraInsercao = DateTime.Now;
                                    OperacaoDeposito.Desconto = OperacaoDeposito.Valor - OperacaoDeposito.ValorLiquido;
                                    OperacaoDeposito.Descricao = "Simulação de depósito";
                                    OperacaoDeposito.FormaPagamento = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(PrimeiroLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada.Codigo);
                                    OperacaoDeposito.Taxa = PrimeiroLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada.TaxaFormaPagamento;
                                    OperacaoDeposito.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    OperacaoDeposito.Valor = PrimeiroLancamentoOperacaoCaixaSaida.Valor * -1;
                                    OperacaoDeposito.ValorLiquido = OperacaoDeposito.Valor - ((OperacaoDeposito.Valor * OperacaoDeposito.Taxa) / 100);
                                    _contextoOperacaocaixa.Add<OperacaoFinanceiraContaCorrente>(OperacaoDeposito);
                                    _contextoOperacaocaixa.SaveChanges();


                                    //Despejo Automatico

                                    DespejoOPeracaoCaixa PrimeiroDespejoAutomatico = new DespejoOPeracaoCaixa();

                                    string identificadorOperacao = PrimeiroDespejoAutomatico.GenerateId();
                                    PrimeiroDespejoAutomatico.DataLancamento = DateTime.Now;
                                    PrimeiroDespejoAutomatico.Descricao = "DESPEJO AUTOMATICO" + identificadorOperacao;
                                    PrimeiroDespejoAutomatico.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);
                                    PrimeiroDespejoAutomatico.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                    PrimeiroDespejoAutomatico.UsuarioQueLancou = _contextoOperacaocaixa.Get<CadastrarUsuario>(PrimeiroLancamentoOperacaoCaixaSaida.UsuarioQueLancou.Codigo);
                                    PrimeiroDespejoAutomatico.Valor = OperacaoCaixaLancamento.Valor;
                                    PrimeiroDespejoAutomatico.Observacao = "Despejo Feito em: " + DateTime.Now + "Por: " + NomeDoCaixa + " " + "PC: " + Environment.MachineName;
                                    PrimeiroDespejoAutomatico.OperacaoCaixaOrigem = _contextoOperacaocaixa.Get<OperacaoCaixa>(PrimeiroLancamentoOperacaoCaixa.Codigo);
                                    PrimeiroDespejoAutomatico.UsuarioDataHoraInsercao = "Lançado por: " + NomeDoCaixa + " Data: " + DateTime.Now;
                                    PrimeiroDespejoAutomatico.DataHoraInsercao = DateTime.Now;
                                    _contextoOperacaocaixa.Add<DespejoOPeracaoCaixa>(PrimeiroDespejoAutomatico);
                                    _contextoOperacaocaixa.SaveChanges();
                                    return RedirectToAction("Sucesso", "Home");
                                }
                                else
                                {
                                    return RedirectToAction("Sucesso", "Home");
                                }
                                //Fim do despejo                               


                            }
                            else
                            {
                                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemListaVerificaOperacaoDuplicada.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                if (diferenca.TotalMinutes <= 4)
                                {
                                    double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                    ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                       " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", OperacaoCaixaLancamento.FormaPagamentoUtilizada);
                                    return View();
                                }
                                else
                                {

                                    string NomeDoCaixa = User.Identity.Name;

                                    OperacaoCaixa SegundoLancamentoOperacaoCaixa = new OperacaoCaixa();
                                    if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                                    {

                                        SegundoLancamentoOperacaoCaixa.DataLancamento = DateTime.Now;
                                    }
                                    else
                                    {
                                        SegundoLancamentoOperacaoCaixa.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                                    }
                                    SegundoLancamentoOperacaoCaixa.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                                    OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));

                                    if (OperacaoCaixaLancamento.Valor < 0)
                                    {

                                        SegundoLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor * -1;
                                    }
                                    else
                                    {
                                        SegundoLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor;
                                    }
                                    long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);

                                    SegundoLancamentoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                                                       .Where(x => x.Nome == NomeDoCaixa)
                                                                                       select c).First();

                                    SegundoLancamentoOperacaoCaixa.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                    SegundoLancamentoOperacaoCaixa.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                    SegundoLancamentoOperacaoCaixa.Observacao = OperacaoCaixaLancamento.Observacao;
                                    SegundoLancamentoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SegundoLancamentoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                                    SegundoLancamentoOperacaoCaixa.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                                    _contextoOperacaocaixa.Add<OperacaoCaixa>(SegundoLancamentoOperacaoCaixa);
                                    _contextoOperacaocaixa.SaveChanges();

                                    //Operacao Saida

                                    if (VerificarSeFormaPagamentoBaixaAutomatico(SegundoLancamentoOperacaoCaixa.FormaPagamentoUtilizada.Codigo) == true)
                                    {


                                        OperacaoCaixa SegundoLancamentoOperacaoCaixaSaida = new OperacaoCaixa();

                                        if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                                        {

                                            SegundoLancamentoOperacaoCaixaSaida.DataLancamento = DateTime.Now;
                                        }
                                        else
                                        {
                                            SegundoLancamentoOperacaoCaixaSaida.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                                        }
                                        SegundoLancamentoOperacaoCaixaSaida.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                                        OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));



                                        SegundoLancamentoOperacaoCaixaSaida.Valor = OperacaoCaixaLancamento.Valor * -1;

                                        SegundoLancamentoOperacaoCaixaSaida.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                                                           .Where(x => x.Nome == NomeDoCaixa)
                                                                                                select c).First();

                                        SegundoLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                        SegundoLancamentoOperacaoCaixaSaida.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                        SegundoLancamentoOperacaoCaixaSaida.Observacao = SegundoLancamentoOperacaoCaixa.Codigo.ToString();
                                        SegundoLancamentoOperacaoCaixaSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        SegundoLancamentoOperacaoCaixaSaida.DataHoraInsercao = DateTime.Now;
                                        SegundoLancamentoOperacaoCaixaSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                                        _contextoOperacaocaixa.Add<OperacaoCaixa>(SegundoLancamentoOperacaoCaixaSaida);
                                        _contextoOperacaocaixa.SaveChanges();
                                        //Fim saída de caixa



                                        //Despejo Automatico

                                        DespejoOPeracaoCaixa SegundoDespejoAutomatico = new DespejoOPeracaoCaixa();

                                        string identificadorOperacao = SegundoDespejoAutomatico.GenerateId();
                                        SegundoDespejoAutomatico.DataLancamento = DateTime.Now;
                                        SegundoDespejoAutomatico.Descricao = "DESPEJO AUTOMATICO " + identificadorOperacao;
                                        SegundoDespejoAutomatico.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);
                                        SegundoDespejoAutomatico.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                                        SegundoDespejoAutomatico.UsuarioQueLancou = _contextoOperacaocaixa.Get<CadastrarUsuario>(SegundoLancamentoOperacaoCaixa.UsuarioQueLancou.Codigo);
                                        SegundoDespejoAutomatico.Valor = OperacaoCaixaLancamento.Valor;
                                        SegundoDespejoAutomatico.Observacao = "Despejo Feito em: " + DateTime.Now + "Por: " + NomeDoCaixa + " " + "PC: " + Environment.MachineName;
                                        SegundoDespejoAutomatico.OperacaoCaixaOrigem = _contextoOperacaocaixa.Get<OperacaoCaixa>(SegundoLancamentoOperacaoCaixa.Codigo);
                                        SegundoDespejoAutomatico.UsuarioDataHoraInsercao = "Lançado por: " + NomeDoCaixa + " Data: " + DateTime.Now;
                                        SegundoDespejoAutomatico.DataHoraInsercao = DateTime.Now;
                                        _contextoOperacaocaixa.Add<DespejoOPeracaoCaixa>(SegundoDespejoAutomatico);
                                        _contextoOperacaocaixa.SaveChanges();

                                        //Fim do despejo
                                        return RedirectToAction("Sucesso", "Home");

                                    }
                                    else
                                    {
                                        return RedirectToAction("Sucesso", "Home");
                                    }
                                }


                            }

                        }

                    }
                    else
                    {
                        string NomeDoCaixa = User.Identity.Name;

                        OperacaoCaixa TerceiroLancamentoOperacaoCaixa = new OperacaoCaixa();
                        if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                        {

                            TerceiroLancamentoOperacaoCaixa.DataLancamento = DateTime.Now;
                        }
                        else
                        {
                            TerceiroLancamentoOperacaoCaixa.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                        }
                        TerceiroLancamentoOperacaoCaixa.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                        OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));

                        if (OperacaoCaixaLancamento.Valor < 0)
                        {

                            TerceiroLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor * -1;
                        }
                        else
                        {
                            TerceiroLancamentoOperacaoCaixa.Valor = OperacaoCaixaLancamento.Valor;
                        }
                        long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);

                        TerceiroLancamentoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                    .Where(x => x.Nome == NomeDoCaixa)
                                                                            select c).First();

                        TerceiroLancamentoOperacaoCaixa.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                        TerceiroLancamentoOperacaoCaixa.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                        TerceiroLancamentoOperacaoCaixa.Observacao = OperacaoCaixaLancamento.Observacao;
                        TerceiroLancamentoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiroLancamentoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                        TerceiroLancamentoOperacaoCaixa.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;
                        _contextoOperacaocaixa.Add<OperacaoCaixa>(TerceiroLancamentoOperacaoCaixa);
                        _contextoOperacaocaixa.SaveChanges();

                        //Saida terceiro Operacao Caixa

                        if (VerificarSeFormaPagamentoBaixaAutomatico(TerceiroLancamentoOperacaoCaixa.FormaPagamentoUtilizada.Codigo) == true)
                        {


                            OperacaoCaixa TerceiroLancamentoOperacaoCaixaSaida = new OperacaoCaixa();

                            if (OperacaoCaixaLancamento.DataLancamento.Date > DateTime.Now.Date)
                            {

                                TerceiroLancamentoOperacaoCaixaSaida.DataLancamento = DateTime.Now;
                            }
                            else
                            {
                                TerceiroLancamentoOperacaoCaixaSaida.DataLancamento = OperacaoCaixaLancamento.DataLancamento;
                            }
                            TerceiroLancamentoOperacaoCaixaSaida.Descricao = "Lancamento:" + DateTime.Now.Date.ToString("dd-MM-yyyy");


                            OperacaoCaixaLancamento.Valor = Convert.ToDecimal(OperacaoCaixaLancamento.Valor.ToString().Replace(".", ","));



                            TerceiroLancamentoOperacaoCaixaSaida.Valor = OperacaoCaixaLancamento.Valor * -1;

                            TerceiroLancamentoOperacaoCaixaSaida.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                                               .Where(x => x.Nome == NomeDoCaixa)
                                                                                     select c).First();

                            TerceiroLancamentoOperacaoCaixaSaida.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                            TerceiroLancamentoOperacaoCaixaSaida.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);

                            TerceiroLancamentoOperacaoCaixaSaida.Observacao = TerceiroLancamentoOperacaoCaixa.Codigo.ToString();
                            TerceiroLancamentoOperacaoCaixaSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            TerceiroLancamentoOperacaoCaixaSaida.DataHoraInsercao = DateTime.Now;
                            TerceiroLancamentoOperacaoCaixaSaida.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;
                            _contextoOperacaocaixa.Add<OperacaoCaixa>(TerceiroLancamentoOperacaoCaixaSaida);
                            _contextoOperacaocaixa.SaveChanges();

                            //Fim da saída operacao caixa

                            //Despejo Automatico

                            DespejoOPeracaoCaixa TerceiroDespejoAutomatico = new DespejoOPeracaoCaixa();

                            string identificadorOperacao = TerceiroDespejoAutomatico.GenerateId();
                            TerceiroDespejoAutomatico.DataLancamento = DateTime.Now;
                            TerceiroDespejoAutomatico.Descricao = " DESPEJO AUTOMATICO " + identificadorOperacao;
                            TerceiroDespejoAutomatico.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(codigoEstabelecimento);
                            TerceiroDespejoAutomatico.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaLancamento.FormaPagamentoUtilizada.Codigo);
                            TerceiroDespejoAutomatico.UsuarioQueLancou = _contextoOperacaocaixa.Get<CadastrarUsuario>(TerceiroLancamentoOperacaoCaixaSaida.UsuarioQueLancou.Codigo);
                            TerceiroDespejoAutomatico.Valor = OperacaoCaixaLancamento.Valor;
                            TerceiroDespejoAutomatico.Observacao = "Despejo Feito em: " + DateTime.Now + "Por: " + NomeDoCaixa + " " + "PC: " + Environment.MachineName;
                            TerceiroDespejoAutomatico.OperacaoCaixaOrigem = _contextoOperacaocaixa.Get<OperacaoCaixa>(TerceiroLancamentoOperacaoCaixa.Codigo);
                            TerceiroDespejoAutomatico.UsuarioDataHoraInsercao = "Lançado por: " + NomeDoCaixa + " Data: " + DateTime.Now;
                            TerceiroDespejoAutomatico.DataHoraInsercao = DateTime.Now;
                            _contextoOperacaocaixa.Add<DespejoOPeracaoCaixa>(TerceiroDespejoAutomatico);
                            _contextoOperacaocaixa.SaveChanges();

                            //Fim do despejo


                            return RedirectToAction("Sucesso", "Home");
                        }
                        else
                        {
                            return RedirectToAction("Sucesso", "Home");
                        }
                    }

                }

            }

            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == BuscaEstabelecimento(User.Identity.Name)), "Codigo", "NomeTipoFormaPagamento", OperacaoCaixaLancamento.FormaPagamentoUtilizada);
            return View();


        }



        public ActionResult AlterarLancamentoOperacaoCaixa(int id)
        {
            string NomeDoCaixa = User.Identity.Name;
            OperacaoCaixa OperacaoAlterar = _contextoOperacaocaixa.Get<OperacaoCaixa>(id);
            long codigoEstabelecimento = BuscaEstabelecimento(NomeDoCaixa);
            ViewBag.NomeForma = new SelectList(_contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");

            return View(OperacaoAlterar);

        }



        [HttpPost]
        public ActionResult AlterarLancamentoOperacaoCaixa(OperacaoCaixa OperacaoCaixaParaAlterar)
        {
            lock (_contextoOperacaocaixa)
            {

                string NomeDoCaixa = User.Identity.Name;
                if (VerificarSeFormaPagamentoBaixaAutomatico(OperacaoCaixaParaAlterar.FormaPagamentoUtilizada.Codigo) == false)
                {
                    OperacaoCaixa OperacaoAlterada = _contextoOperacaocaixa.Get<OperacaoCaixa>(OperacaoCaixaParaAlterar.Codigo);

                    OperacaoAlterada.DataLancamento = OperacaoCaixaParaAlterar.DataLancamento;
                    OperacaoAlterada.Descricao = OperacaoCaixaParaAlterar.Descricao;
                    OperacaoAlterada.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(BuscaEstabelecimento(NomeDoCaixa));
                    OperacaoAlterada.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                         .Where(x => x.Nome == NomeDoCaixa)
                                                         select c).First();
                    OperacaoAlterada.Valor = OperacaoCaixaParaAlterar.Valor;
                    OperacaoAlterada.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaParaAlterar.FormaPagamentoUtilizada.Codigo);
                    if (OperacaoCaixaParaAlterar.Observacao != null)
                    {
                        OperacaoCaixaParaAlterar.Observacao = OperacaoCaixaParaAlterar.Observacao.ToUpper();
                    }
                    OperacaoAlterada.Observacao = OperacaoCaixaParaAlterar.Observacao;
                    OperacaoAlterada.Conferido = false;
                    OperacaoAlterada.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoAlterada.DataHoraInsercao = DateTime.Now;
                    _contextoOperacaocaixa.SaveChanges();

                    return RedirectToAction("Sucesso", "Home");
                }
                else
                {
                    OperacaoCaixa OperacaoCaixaPositiva = _contextoOperacaocaixa.Get<OperacaoCaixa>(OperacaoCaixaParaAlterar.Codigo);
                    OperacaoCaixaPositiva.DataLancamento = OperacaoCaixaParaAlterar.DataLancamento;
                    OperacaoCaixaPositiva.Descricao = OperacaoCaixaParaAlterar.Descricao;
                    OperacaoCaixaPositiva.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(BuscaEstabelecimento(NomeDoCaixa));
                    OperacaoCaixaPositiva.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                             .Where(x => x.Nome == NomeDoCaixa)
                                                              select c).First();
                    OperacaoCaixaPositiva.Valor = OperacaoCaixaParaAlterar.Valor;
                    OperacaoCaixaPositiva.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaParaAlterar.FormaPagamentoUtilizada.Codigo);
                    OperacaoCaixaParaAlterar.Observacao = OperacaoCaixaParaAlterar.Codigo.ToString();
                    OperacaoCaixaPositiva.Observacao = OperacaoCaixaParaAlterar.Observacao;
                    OperacaoCaixaPositiva.Conferido = false;
                    OperacaoCaixaPositiva.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoCaixaPositiva.DataHoraInsercao = DateTime.Now;

                    _contextoOperacaocaixa.SaveChanges();

                    OperacaoCaixa OperacaoNegativa = (from c in _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                                                      .Where(x => x.Observacao.ToString() == OperacaoCaixaParaAlterar.Codigo.ToString())
                                                      select c).First();

                    OperacaoNegativa.DataLancamento = OperacaoCaixaParaAlterar.DataLancamento;
                    OperacaoNegativa.Descricao = OperacaoCaixaParaAlterar.Descricao;
                    OperacaoNegativa.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(BuscaEstabelecimento(NomeDoCaixa));
                    OperacaoNegativa.UsuarioQueLancou = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                                         .Where(x => x.Nome == NomeDoCaixa)
                                                         select c).First();
                    OperacaoNegativa.Valor = OperacaoCaixaParaAlterar.Valor * -1;
                    OperacaoNegativa.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoCaixaParaAlterar.FormaPagamentoUtilizada.Codigo);

                    OperacaoNegativa.Observacao = OperacaoCaixaPositiva.Codigo.ToString();
                    OperacaoNegativa.Conferido = false;
                    OperacaoNegativa.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoNegativa.DataHoraInsercao = DateTime.Now;

                    _contextoOperacaocaixa.SaveChanges();

                    DespejoOPeracaoCaixa Despejo = (from t in _contextoOperacaocaixa.GetAll<DespejoOPeracaoCaixa>()
                                                    .Where(x => x.OperacaoCaixaOrigem.Codigo == OperacaoCaixaParaAlterar.Codigo)
                                                    select t).First();


                    string identificadorOperacao = Despejo.GenerateId();
                    Despejo.DataLancamento = DateTime.Now;
                    Despejo.Descricao = " DESPEJO AUTOMATICO " + identificadorOperacao;
                    Despejo.EstabelecimentoOperacao = _contextoOperacaocaixa.Get<Estabelecimento>(OperacaoNegativa.EstabelecimentoOperacao.Codigo);
                    Despejo.FormaPagamentoUtilizada = _contextoOperacaocaixa.Get<FormaPagamentoEstabelecimento>(OperacaoNegativa.FormaPagamentoUtilizada.Codigo);
                    Despejo.UsuarioQueLancou = _contextoOperacaocaixa.Get<CadastrarUsuario>(OperacaoNegativa.UsuarioQueLancou.Codigo);
                    Despejo.Valor = OperacaoCaixaParaAlterar.Valor;
                    Despejo.Observacao = "Despejo Feito em: " + DateTime.Now + "Por: " + NomeDoCaixa + " " + "PC: " + Environment.MachineName;
                    Despejo.OperacaoCaixaOrigem = _contextoOperacaocaixa.Get<OperacaoCaixa>(OperacaoCaixaParaAlterar.Codigo);
                    Despejo.UsuarioDataHoraInsercao = "Lançado por: " + NomeDoCaixa + " Data: " + DateTime.Now;
                    Despejo.DataHoraInsercao = DateTime.Now;

                    _contextoOperacaocaixa.SaveChanges();


                }


            }
            return RedirectToAction("Sucesso", "Home");
        }



        public ActionResult ExcluirLancamentoCaixa(int id)
        {
            OperacaoCaixa OperacaoParaExcluir = _contextoOperacaocaixa.Get<OperacaoCaixa>(id);

            return View(OperacaoParaExcluir);
        }



        [HttpPost]
        public ActionResult ExcluirLancamentoCaixa(OperacaoCaixa operacaoCaixaExcluir)
        {
            lock (_contextoOperacaocaixa)
            {
                if (VerificarSeFormaPagamentoBaixaAutomatico(operacaoCaixaExcluir.FormaPagamentoUtilizada.Codigo) == false)
                {
                    OperacaoCaixa OperacaoExcluida = _contextoOperacaocaixa.Get<OperacaoCaixa>(operacaoCaixaExcluir.Codigo);
                    _contextoOperacaocaixa.Delete<OperacaoCaixa>(OperacaoExcluida);
                    _contextoOperacaocaixa.SaveChanges();

                }
                else
                {
                    OperacaoCaixa OperacaoPositiva = _contextoOperacaocaixa.Get<OperacaoCaixa>(operacaoCaixaExcluir.Codigo);
                    OperacaoCaixa OperacaoNegativa = (from c in _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                                                      .Where(x => x.Observacao.ToString() == operacaoCaixaExcluir.Codigo.ToString())
                                                      select c).First();
                    DespejoOPeracaoCaixa DespejoExcluido = (from c in _contextoOperacaocaixa.GetAll<DespejoOPeracaoCaixa>()
                                                            .Where(x => x.OperacaoCaixaOrigem.Codigo == operacaoCaixaExcluir.Codigo)
                                                            select c).First();
                    _contextoOperacaocaixa.Delete<DespejoOPeracaoCaixa>(DespejoExcluido);
                    _contextoOperacaocaixa.SaveChanges();
                    _contextoOperacaocaixa.Delete<OperacaoCaixa>(OperacaoNegativa);
                    _contextoOperacaocaixa.SaveChanges();
                    _contextoOperacaocaixa.Delete<OperacaoCaixa>(OperacaoPositiva);
                    _contextoOperacaocaixa.SaveChanges();
                }

            }
            return RedirectToAction("Sucesso", "Home");
        }




        public ActionResult LancamentoIndividual(DateTime dataLancamento)
        {
            string nomeUsuario = User.Identity.Name;
            var OperacaoIndividual = (from c in _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                                     .Where(x => x.UsuarioQueLancou.Nome == nomeUsuario && x.DataLancamento == dataLancamento)
                                      select c);
            return View();
        }

        [HandleError(View = "Error")]
        public ActionResult TodosLancamentos()
        {
            return View();
        }

        public ActionResult LancamentosData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LancamentosData(ValidarData Datas)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = User.Identity.Name + ",a data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {

                    string nomeCaixa = User.Identity.Name;
                    ViewBag.DataInicio = Datas.DataInicial;
                    ViewBag.DataFim = Datas.DataFinal;

                    IList<OperacaoCaixa> todosLancamentos = null;
                    todosLancamentos = _contextoOperacaocaixa.GetAll<OperacaoCaixa>().Where(x => x.UsuarioQueLancou.Nome == nomeCaixa && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal && x.Valor > 0).OrderByDescending(x => x.DataLancamento).ToList();

                    return View("TodosLancamentos", todosLancamentos);
                }
            }

            return View();
        }


        public ActionResult LancamentosNaData(DateTime? id)
        {
            string nomeUsuario = User.Identity.Name;

            IList<OperacaoCaixa> listaPorData = null;
            listaPorData = _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                .Where(x => x.DataLancamento.Date == id && x.UsuarioQueLancou.Nome == nomeUsuario && x.Valor > 0).ToList();

            return View(listaPorData);
        }

        public String DataAgora()
        {
            return DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString();

        }

        public ActionResult LancamentosHoje()
        {
            string NomeUsuario = User.Identity.Name;
            long CodigoEstabelecimento = BuscaEstabelecimento(NomeUsuario);
            IList<OperacaoCaixa> ListaOperacaoHoje = null;
            ListaOperacaoHoje = _contextoOperacaocaixa.GetAll<OperacaoCaixa>()
                               .Where(x => x.EstabelecimentoOperacao.Codigo == CodigoEstabelecimento && x.UsuarioQueLancou.Nome == NomeUsuario && x.DataLancamento.Date == DateTime.Now.Date && x.Valor > 0).OrderByDescending(x => x.Descricao)
                               .ToList();
            return View(ListaOperacaoHoje);
        }

        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoOperacaocaixa.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
        }

        private bool VerificarSeFormaPagamentoBaixaAutomatico(long p)
        {
            bool EbaixaAutomatica = (from c in _contextoOperacaocaixa.GetAll<FormaPagamentoEstabelecimento>()
                                     .Where(x => x.Codigo == p)
                                     select c.DespejoAutomatico).First();
            if (EbaixaAutomatica)
            {
                return true;
            }
            return false;
        }

    }
}
