using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class DespejoOperacaoCaixaController : Controller
    {
        private IContextoDados _contextoDespejoOperacaoCaixa = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Detalhes(int id)
        {
            DespejoOPeracaoCaixa DetalhesDespejo = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);
            return View(DetalhesDespejo);
        }

        public ActionResult AdicionarDespejoOperacaoCaixa()
        {

            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();

            ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimento), "Codigo", "RazaoSocial");
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento), "Codigo", "Nome");


            return View();
        }

        [HttpPost]
        public ActionResult AdicionarDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoOperacaoCaixa)
        {
            ModelState["UsuarioQueLancou.Nome"].Errors.Clear();
            ModelState["UsuarioQueLancou.Email"].Errors.Clear();
            ModelState["UsuarioQueLancou.Senha"].Errors.Clear();
            ModelState["UsuarioQueLancou.ConfirmeSenha"].Errors.Clear();
            ModelState["FormaPagamentoUtilizada.NomeTipoFormaPagamento"].Errors.Clear();
            ModelState["UsuarioQueLancou.EnderecoUsuario"].Errors.Clear();
            ModelState["EstabelecimentoOPeracao.RazaoSocial"].Errors.Clear();
            ModelState["EstabelecimentoOPeracao.CNPJ"].Errors.Clear();
            ModelState["EstabelecimentoOPeracao.InscricaoEstadual"].Errors.Clear();

            if (ModelState.IsValid)
            {


                lock (_contextoDespejoOperacaoCaixa)
                {

                    if (despejoOperacaoCaixa.DataLancamento != null)
                    {
                        TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(despejoOperacaoCaixa.DataLancamento.ToString("yyyy/MM/dd HH:mm:ss"));
                        if (diferenca.TotalDays < 0)
                        {
                            ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                            ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                            return View();

                        }
                        if (diferenca.TotalDays > 31)
                        {
                            ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                            ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                            return View();

                        }


                    }
                    IList<DespejoOPeracaoCaixa> ListaVerificaDespejoDuplicado = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>()
                                                                                .Where(x => x.Valor == despejoOperacaoCaixa.Valor && x.DataLancamento == despejoOperacaoCaixa.DataLancamento && x.Descricao == despejoOperacaoCaixa.Descricao)
                                                                                .ToList();
                    if (ListaVerificaDespejoDuplicado.Count() >= 1)
                    {
                        foreach (var itemVerificaDespejoDuplicado in ListaVerificaDespejoDuplicado)
                        {
                            if (itemVerificaDespejoDuplicado.DataHoraInsercao == null)
                            {
                                //primeiro despejo
                                string nomeUsuarioLogado = User.Identity.Name;

                                OperacaoCaixa PrimeiraOperacaoCaixaDespejo = new OperacaoCaixa();

                                PrimeiraOperacaoCaixaDespejo.DataLancamento = DateTime.Now;
                                PrimeiraOperacaoCaixaDespejo.Descricao = "DESPEJO DO CAIXA " + despejoOperacaoCaixa.GenerateId();

                                if (despejoOperacaoCaixa.Valor < 0)
                                {

                                    PrimeiraOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor;
                                }
                                else
                                {
                                    PrimeiraOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor * -1;
                                }

                                long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuarioLogado);
                                decimal SaldoAtual = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                     .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                                      select c.Valor).Sum();

                                if (despejoOperacaoCaixa.Valor > SaldoAtual)
                                {
                                    decimal FormaPagamentoSaldo = _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                               .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name) && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                                               .Select(x => x.Valor).Sum();
                                    decimal SaldoLoja = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                        .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                                                         select c.Valor).Sum();
                                    ViewBag.MensagemSaldoInsuficiente = "O saldo da forma de pagamento é menor que o valor: R$  " + FormaPagamentoSaldo + " .";
                                    ViewBag.MensagemSaldoInsuficienteLoja = " O saldo do estabelecimento é:  R$ " + SaldoLoja + " .";
                                    ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                                    ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                                    return View();
                                }

                                PrimeiraOperacaoCaixaDespejo.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                         .Where(x => x.Nome == nomeUsuarioLogado)
                                                                                 select c).First();

                                PrimeiraOperacaoCaixaDespejo.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);

                                PrimeiraOperacaoCaixaDespejo.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                if (despejoOperacaoCaixa.Observacao != null)
                                {
                                    PrimeiraOperacaoCaixaDespejo.Observacao = despejoOperacaoCaixa.Observacao + "Lançado por:" + nomeUsuarioLogado + "Data: " + DateTime.Now + "PC " + Request.UserHostName.ToString();
                                }
                                else
                                {
                                    PrimeiraOperacaoCaixaDespejo.Observacao = "Lançado por:" + nomeUsuarioLogado + " Data: " + DateTime.Now + " PC " + Environment.MachineName;

                                }

                                PrimeiraOperacaoCaixaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                PrimeiraOperacaoCaixaDespejo.DataHoraInsercao = DateTime.Now;
                                PrimeiraOperacaoCaixaDespejo.TipoOperacao = EnumTipoOperacao.DespejoManual;
                                _contextoDespejoOperacaoCaixa.Add<OperacaoCaixa>(PrimeiraOperacaoCaixaDespejo);
                                _contextoDespejoOperacaoCaixa.SaveChanges();

                                despejoOperacaoCaixa.OperacaoCaixaOrigem = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(PrimeiraOperacaoCaixaDespejo.Codigo);
                                despejoOperacaoCaixa.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
                                despejoOperacaoCaixa.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                despejoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                        .Where(x => x.Nome == nomeUsuarioLogado)
                                                                         select c).First();
                                despejoOperacaoCaixa.DataLancamento = despejoOperacaoCaixa.DataLancamento.Date;
                                despejoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                despejoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                                _contextoDespejoOperacaoCaixa.Add<DespejoOPeracaoCaixa>(despejoOperacaoCaixa);
                                _contextoDespejoOperacaoCaixa.SaveChanges();
                                return RedirectToAction("Sucesso", "Home");

                            }
                            else
                            {
                                TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemVerificaDespejoDuplicado.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                if (diferenca.TotalMinutes <= 4)
                                {
                                    double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                    ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                       " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                    ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                                    ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                                    ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                                    return View();
                                }
                                else
                                {
                                    //segunda tentativa despejo após passar os 4 minutos
                                    string nomeUsuarioLogado = User.Identity.Name;

                                    OperacaoCaixa SegundoOperacaoCaixaDespejo = new OperacaoCaixa();

                                    SegundoOperacaoCaixaDespejo.DataLancamento = DateTime.Now;
                                    SegundoOperacaoCaixaDespejo.Descricao = "DESPEJO DO CAIXA " + despejoOperacaoCaixa.GenerateId();

                                    if (despejoOperacaoCaixa.Valor < 0)
                                    {

                                        SegundoOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor;
                                    }
                                    else
                                    {
                                        SegundoOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor * -1;
                                    }

                                    long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuarioLogado);
                                    decimal SaldoAtual = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                         .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                                          select c.Valor).Sum();

                                    if (despejoOperacaoCaixa.Valor > SaldoAtual)
                                    {
                                        decimal FormaPagamentoSaldo = _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                                   .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name) && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                                                   .Select(x => x.Valor).Sum();
                                        decimal SaldoLoja = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                            .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                                                             select c.Valor).Sum();
                                        ViewBag.MensagemSaldoInsuficiente = "O saldo da forma de pagamento é menor que o valor: R$  " + FormaPagamentoSaldo + " .";
                                        ViewBag.MensagemSaldoInsuficienteLoja = " O saldo do estabelecimento é:  R$ " + SaldoLoja + " .";
                                        ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                                        ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                                        ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                                        return View();
                                    }
                                    SegundoOperacaoCaixaDespejo.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                             .Where(x => x.Nome == nomeUsuarioLogado)
                                                                                    select c).First();

                                    SegundoOperacaoCaixaDespejo.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);

                                    SegundoOperacaoCaixaDespejo.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);

                                    if (despejoOperacaoCaixa.Observacao != null)
                                    {
                                        SegundoOperacaoCaixaDespejo.Observacao = despejoOperacaoCaixa.Observacao + "Lançado por:" + nomeUsuarioLogado + "Data: " + DateTime.Now + "PC " + Request.UserHostName.ToString();
                                    }
                                    else
                                    {
                                        SegundoOperacaoCaixaDespejo.Observacao = "Lançado por:" + nomeUsuarioLogado + " Data: " + DateTime.Now + " PC " + Environment.MachineName;

                                    }

                                    SegundoOperacaoCaixaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SegundoOperacaoCaixaDespejo.DataHoraInsercao = DateTime.Now;
                                    SegundoOperacaoCaixaDespejo.TipoOperacao = EnumTipoOperacao.DespejoManual;
                                    _contextoDespejoOperacaoCaixa.Add<OperacaoCaixa>(SegundoOperacaoCaixaDespejo);
                                    _contextoDespejoOperacaoCaixa.SaveChanges();

                                    despejoOperacaoCaixa.OperacaoCaixaOrigem = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(SegundoOperacaoCaixaDespejo.Codigo);
                                    despejoOperacaoCaixa.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
                                    despejoOperacaoCaixa.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
                                    despejoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                            .Where(x => x.Nome == nomeUsuarioLogado)
                                                                             select c).First();
                                    despejoOperacaoCaixa.DataLancamento = despejoOperacaoCaixa.DataLancamento.Date;
                                    despejoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    despejoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                                    _contextoDespejoOperacaoCaixa.Add<DespejoOPeracaoCaixa>(despejoOperacaoCaixa);
                                    _contextoDespejoOperacaoCaixa.SaveChanges();
                                    return RedirectToAction("Sucesso", "Home");

                                }
                            }

                        }

                    }
                    else
                    {
                        //terceira tentativa
                        string nomeUsuarioLogado = User.Identity.Name;

                        OperacaoCaixa TerceiroOperacaoCaixaDespejo = new OperacaoCaixa();

                        TerceiroOperacaoCaixaDespejo.DataLancamento = DateTime.Now;
                        TerceiroOperacaoCaixaDespejo.Descricao = "DESPEJO DO CAIXA " + despejoOperacaoCaixa.GenerateId();

                        if (despejoOperacaoCaixa.Valor < 0)
                        {

                            TerceiroOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor;
                        }
                        else
                        {
                            TerceiroOperacaoCaixaDespejo.Valor = despejoOperacaoCaixa.Valor * -1;
                        }

                        long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuarioLogado);
                        decimal SaldoAtual = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                              select c.Valor).Sum();

                        if (despejoOperacaoCaixa.Valor > SaldoAtual)
                        {
                            decimal FormaPagamentoSaldo = _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                       .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name) && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo)
                                                       .Select(x => x.Valor).Sum();
                            decimal SaldoLoja = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                                .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                                                 select c.Valor).Sum();
                            ViewBag.MensagemSaldoInsuficiente = "O saldo da forma de pagamento é menor que o valor: R$  " + FormaPagamentoSaldo + " .";
                            ViewBag.MensagemSaldoInsuficienteLoja = " O saldo do estabelecimento é:  R$ " + SaldoLoja + " .";
                            ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
                            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
                            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
                            return View();
                        }

                        TerceiroOperacaoCaixaDespejo.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                 .Where(x => x.Nome == nomeUsuarioLogado)
                                                                         select c).First();

                        TerceiroOperacaoCaixaDespejo.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);

                        TerceiroOperacaoCaixaDespejo.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);

                        if (despejoOperacaoCaixa.Observacao != null)
                        {
                            TerceiroOperacaoCaixaDespejo.Observacao = despejoOperacaoCaixa.Observacao + "Lançado por:" + nomeUsuarioLogado + "Data: " + DateTime.Now + "PC " + Request.UserHostName.ToString();
                        }
                        else
                        {
                            TerceiroOperacaoCaixaDespejo.Observacao = "Lançado por:" + nomeUsuarioLogado + " Data: " + DateTime.Now + " PC " + Environment.MachineName;

                        }

                        TerceiroOperacaoCaixaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiroOperacaoCaixaDespejo.DataHoraInsercao = DateTime.Now;
                        TerceiroOperacaoCaixaDespejo.TipoOperacao = EnumTipoOperacao.DespejoManual;
                        _contextoDespejoOperacaoCaixa.Add<OperacaoCaixa>(TerceiroOperacaoCaixaDespejo);
                        _contextoDespejoOperacaoCaixa.SaveChanges();

                        despejoOperacaoCaixa.OperacaoCaixaOrigem = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(TerceiroOperacaoCaixaDespejo.Codigo);
                        despejoOperacaoCaixa.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
                        despejoOperacaoCaixa.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
                        despejoOperacaoCaixa.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                .Where(x => x.Nome == nomeUsuarioLogado)
                                                                 select c).First();
                        despejoOperacaoCaixa.DataLancamento = despejoOperacaoCaixa.DataLancamento.Date;
                        despejoOperacaoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        despejoOperacaoCaixa.DataHoraInsercao = DateTime.Now;
                        _contextoDespejoOperacaoCaixa.Add<DespejoOPeracaoCaixa>(despejoOperacaoCaixa);
                        _contextoDespejoOperacaoCaixa.SaveChanges();
                        return RedirectToAction("Sucesso", "Home");

                    }

                }
            }

            ViewBag.Estabelecimento = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", despejoOperacaoCaixa.EstabelecimentoOperacao);
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", despejoOperacaoCaixa.FormaPagamentoUtilizada);
            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", despejoOperacaoCaixa.UsuarioQueLancou);
            return View();

        }


        public ActionResult FazerDespejoAutomatico(string NomeUsuarioLogado)
        {
            lock (_contextoDespejoOperacaoCaixa)
            {


                long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                              .Where(x => x.Nome == NomeUsuarioLogado)
                                              select c.EstabelecimentoTrabalho.Codigo).First();
                var ListaFormaPagamento = (from c in _contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>()
                                           .Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == true)
                                           join op in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                           on c.Codigo equals op.FormaPagamentoUtilizada.Codigo
                                           select c).ToList();

                long UsuarioLogado = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                            .Where(x => x.Nome == NomeUsuarioLogado)
                                      select c.Codigo).First();

                foreach (var itemFormaPagamento in ListaFormaPagamento)
                {
                    if (ListaFormaPagamento.Count() > 0)
                    {
                        decimal Valor = (from c in _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                                         .Where(x => x.FormaPagamentoUtilizada.Codigo == itemFormaPagamento.Codigo)
                                         select c.Valor).Sum();
                        if (Valor > 0)
                        {
                            DespejoOPeracaoCaixa DespejoAutomatico = new DespejoOPeracaoCaixa();
                            string identificadorOperacao = DespejoAutomatico.GenerateId();

                            OperacaoCaixa OperacaoAutomaticaParaDespejo = new OperacaoCaixa();

                            OperacaoAutomaticaParaDespejo.DataLancamento = DateTime.Now;
                            OperacaoAutomaticaParaDespejo.Descricao = "DESPEJO DO CAIXA" + identificadorOperacao;
                            OperacaoAutomaticaParaDespejo.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
                            OperacaoAutomaticaParaDespejo.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(itemFormaPagamento.Codigo);
                            OperacaoAutomaticaParaDespejo.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(UsuarioLogado);
                            OperacaoAutomaticaParaDespejo.Valor = Valor * -1;
                            OperacaoAutomaticaParaDespejo.Observacao = "Operacao Feita em: " + DateTime.Now + "Por: " + NomeUsuarioLogado + " " + "PC: " + Environment.MachineName;
                            OperacaoAutomaticaParaDespejo.DataHoraInsercao = DateTime.Now;
                            OperacaoAutomaticaParaDespejo.UsuarioDataHoraInsercao = "Lançado por: " + NomeUsuarioLogado + " Data: " + DateTime.Now;
                            OperacaoAutomaticaParaDespejo.TipoOperacao = EnumTipoOperacao.DespejoAutomatico;

                            _contextoDespejoOperacaoCaixa.Add<OperacaoCaixa>(OperacaoAutomaticaParaDespejo);
                            _contextoDespejoOperacaoCaixa.SaveChanges();


                            DespejoAutomatico.DataLancamento = DateTime.Now;
                            DespejoAutomatico.Descricao = "DESPEJO AUTOMATICO" + identificadorOperacao;
                            DespejoAutomatico.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(codigoEstabelecimento);
                            DespejoAutomatico.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(itemFormaPagamento.Codigo);
                            DespejoAutomatico.UsuarioQueLancou = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                                                .Where(x => x.Nome == NomeUsuarioLogado)
                                                                  select c).First();
                            DespejoAutomatico.Valor = Valor;
                            DespejoAutomatico.Observacao = "Despejo Feito em: " + DateTime.Now + "Por: " + NomeUsuarioLogado + " " + "PC: " + Environment.MachineName;
                            DespejoAutomatico.OperacaoCaixaOrigem = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(OperacaoAutomaticaParaDespejo.Codigo);
                            DespejoAutomatico.UsuarioDataHoraInsercao = "Lançado por: " + NomeUsuarioLogado + " Data: " + DateTime.Now;
                            DespejoAutomatico.DataHoraInsercao = DateTime.Now;
                            _contextoDespejoOperacaoCaixa.Add<DespejoOPeracaoCaixa>(DespejoAutomatico);
                            _contextoDespejoOperacaoCaixa.SaveChanges();
                        }

                    }
                }
            }
            return View("Index", "Home");
        }


        //
        // GET: /Administracao/DespejoOperacaoCaixa/Edit/5

        public ActionResult AlterarDespejoOperacaoCaixa(int id)
        {
            DespejoOPeracaoCaixa DespejoOperacaoCaixa = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento), "Codigo", "Nome");
            return View(DespejoOperacaoCaixa);
        }

        //
        // POST: /Administracao/DespejoOperacaoCaixa/Edit/5

        [HttpPost]
        public ActionResult AlterarDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoOperacaoCaixa)
        {
            lock (_contextoDespejoOperacaoCaixa)
            {


                DespejoOPeracaoCaixa DespejoAlterado = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(despejoOperacaoCaixa.Codigo);

                DespejoAlterado.DataLancamento = despejoOperacaoCaixa.DataLancamento;
                DespejoAlterado.Descricao = despejoOperacaoCaixa.Descricao;
                DespejoAlterado.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoOperacaoCaixa.EstabelecimentoOperacao.Codigo);
                DespejoAlterado.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
                DespejoAlterado.Observacao = despejoOperacaoCaixa.Observacao;
                DespejoAlterado.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoOperacaoCaixa.UsuarioQueLancou.Codigo);
                DespejoAlterado.Valor = despejoOperacaoCaixa.Valor;
                DespejoAlterado.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                DespejoAlterado.DataHoraInsercao = DateTime.Now;

                _contextoDespejoOperacaoCaixa.SaveChanges();

                OperacaoCaixa OperacaoCaixaAlterar = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(despejoOperacaoCaixa.OperacaoCaixaOrigem.Codigo);
                OperacaoCaixaAlterar.DataLancamento = despejoOperacaoCaixa.DataLancamento;
                OperacaoCaixaAlterar.Descricao = despejoOperacaoCaixa.Descricao;
                OperacaoCaixaAlterar.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoOperacaoCaixa.EstabelecimentoOperacao.Codigo);
                OperacaoCaixaAlterar.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoOperacaoCaixa.UsuarioQueLancou.Codigo);
                OperacaoCaixaAlterar.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoOperacaoCaixa.FormaPagamentoUtilizada.Codigo);
                OperacaoCaixaAlterar.Observacao = despejoOperacaoCaixa.Codigo.ToString();
                OperacaoCaixaAlterar.Valor = despejoOperacaoCaixa.Valor * -1;
                OperacaoCaixaAlterar.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaAlterar.TipoOperacao = EnumTipoOperacao.DespejoManual;
                OperacaoCaixaAlterar.DataHoraInsercao = DateTime.Now;
                _contextoDespejoOperacaoCaixa.SaveChanges();

            }
            return RedirectToAction("Sucesso", "Home");
        }

        //
        // GET: /Administracao/DespejoOperacaoCaixa/Delete/5

        public ActionResult ExcluirDespejoOperacaoCaixa(int id)
        {

            DespejoOPeracaoCaixa despejoOperacaoParaExcluir = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(id);
            FormaPagamentoEstabelecimento f = _contextoDespejoOperacaoCaixa.GetAll<FormaPagamentoEstabelecimento>()
                                              .Where(x => x.Codigo == despejoOperacaoParaExcluir.FormaPagamentoUtilizada.Codigo)
                                              .First();
            Estabelecimento e = _contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>()
                                .Where(x => x.Codigo == despejoOperacaoParaExcluir.EstabelecimentoOperacao.Codigo).First();
            CadastrarUsuario c = _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                .Where(x => x.Codigo == despejoOperacaoParaExcluir.UsuarioQueLancou.Codigo).First();
            OperacaoCaixa op = _contextoDespejoOperacaoCaixa.GetAll<OperacaoCaixa>()
                               .Where(x => x.Valor * -1 == despejoOperacaoParaExcluir.Valor && x.UsuarioQueLancou.Codigo == despejoOperacaoParaExcluir.UsuarioQueLancou.Codigo && x.FormaPagamentoUtilizada.Codigo == despejoOperacaoParaExcluir.FormaPagamentoUtilizada.Codigo)
                               .First();

            return View(despejoOperacaoParaExcluir);
        }

        //
        // POST: /Administracao/DespejoOperacaoCaixa/Delete/5

        [HttpPost]
        public ActionResult ExcluirDespejoOperacaoCaixa(DespejoOPeracaoCaixa despejoExcluido)
        {
            lock (_contextoDespejoOperacaoCaixa)
            {
                OperacaoCaixa OperacaoCaixaAlterar = _contextoDespejoOperacaoCaixa.Get<OperacaoCaixa>(despejoExcluido.OperacaoCaixaOrigem.Codigo);
                OperacaoCaixaAlterar.DataLancamento = DateTime.Now;
                OperacaoCaixaAlterar.Descricao = "Exclusão Retorno";
                OperacaoCaixaAlterar.Valor = despejoExcluido.Valor;
                OperacaoCaixaAlterar.EstabelecimentoOperacao = _contextoDespejoOperacaoCaixa.Get<Estabelecimento>(despejoExcluido.EstabelecimentoOperacao.Codigo);
                OperacaoCaixaAlterar.UsuarioQueLancou = _contextoDespejoOperacaoCaixa.Get<CadastrarUsuario>(despejoExcluido.UsuarioQueLancou.Codigo);
                OperacaoCaixaAlterar.FormaPagamentoUtilizada = _contextoDespejoOperacaoCaixa.Get<FormaPagamentoEstabelecimento>(despejoExcluido.FormaPagamentoUtilizada.Codigo);
                OperacaoCaixaAlterar.Observacao = despejoExcluido.Codigo.ToString();

                _contextoDespejoOperacaoCaixa.SaveChanges();

                DespejoOPeracaoCaixa DespejoParaExcluir = _contextoDespejoOperacaoCaixa.Get<DespejoOPeracaoCaixa>(despejoExcluido.Codigo);
                _contextoDespejoOperacaoCaixa.Delete<DespejoOPeracaoCaixa>(DespejoParaExcluir);
                _contextoDespejoOperacaoCaixa.SaveChanges();
            }
            return RedirectToAction("Sucesso", "Home");
        }

        public ActionResult ListaDespejo()
        {
            return View();
        }


        public ActionResult DespejoPorForma(int id, DateTime? data)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            IList<DespejoOPeracaoCaixa> despejoDaForma = null;
            despejoDaForma = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>().Where(x => x.FormaPagamentoUtilizada.Codigo == id && x.DataLancamento.Date == data && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).OrderByDescending(x => x.DataLancamento)
                            .ToList();
            return View(despejoDaForma);
        }
        public ActionResult DespejoPorUsuário(int id, DateTime? data)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            IList<DespejoOPeracaoCaixa> despejoUsuario = null;
            despejoUsuario = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>().Where(x => x.UsuarioQueLancou.Codigo == id && x.DataLancamento.Date == data && x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento).OrderByDescending(x => x.DataLancamento)
                            .ToList();
            return View(despejoUsuario);
        }

        public ActionResult DespejoEstabelecimento(int id, DateTime? data)
        {
            IList<DespejoOPeracaoCaixa> despejoEstabelecimento = null;
            despejoEstabelecimento = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>().Where(x => x.EstabelecimentoOperacao.Codigo == id && x.DataLancamento.Date == data).OrderByDescending(x => x.DataLancamento)
                            .ToList();
            return View(despejoEstabelecimento);
        }

        public ActionResult DespejosData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DespejosData(ValidarData Datas)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    ViewBag.DataInicio = Datas.DataInicial;
                    ViewBag.DataFim = Datas.DataFinal;
                    string nomeUsuario = User.Identity.Name;
                    long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);
                    ViewBag.Loja = (from c in _contextoDespejoOperacaoCaixa.GetAll<Estabelecimento>()
                                    .Where(x => x.Codigo == codigoEstabelecimento)
                                    select c.RazaoSocial).First();

                    IList<DespejoOPeracaoCaixa> listaDespejo = null;
                    listaDespejo = _contextoDespejoOperacaoCaixa.GetAll<DespejoOPeracaoCaixa>()
                    .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(x => x.DataLancamento).ToList();

                    return View("ListaDespejo", listaDespejo);
                }
            }

            return View();
        }
        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoDespejoOperacaoCaixa.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
        }


    }
}

