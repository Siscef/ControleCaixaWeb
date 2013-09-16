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

            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento), "Codigo", "Nome");

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
                                                                       .Where(x => x.Valor == operacaoCaixa.Valor && x.Descricao == operacaoCaixa.Descricao)
                                                                       .ToList();
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

                                //TODO:verificar se 
                                if (VerificaSeFormaPagamentoDebitaAutomatico(PrimeiraOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
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

                                    if (VerificaSeFormaPagamentoDebitaAutomatico(SegundaOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
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

                        if (VerificaSeFormaPagamentoDebitaAutomatico(TerceiraOperacaoCaixaEntrada.FormaPagamentoUtilizada.Codigo) == true)
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
            if (VerificarUsuarioTemPermissao() == false)
            {
                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");

                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.RazaoSocial == "CAIXA GERAL"), "Codigo", "RazaoSocial");

                return View();

            }
            else
            {
                ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo != codigoEstabelecimento && x.DespejoAutomatico == false), "Codigo", "NomeTipoFormaPagamento");

                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.RazaoSocial == "CAIXA GERAL"), "Codigo", "RazaoSocial");

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

                OperacaoCaixa operacaoSaida = new OperacaoCaixa();

                operacaoSaida.DataLancamento = operacaoCaixa.DataLancamento;
                operacaoSaida.Descricao = "RECOLHIMENTO " + operacaoCaixa.Descricao;
                operacaoSaida.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                operacaoSaida.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                  .Where(x => x.Nome == NomeUsuarioLogado)
                                                  select c).First();
                operacaoSaida.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(codigoEstabelecimento);
                if (operacaoCaixa.Observacao != null)
                {
                    operacaoCaixa.Observacao = operacaoCaixa.Observacao.ToUpper();
                }
                operacaoSaida.Observacao = operacaoCaixa.Observacao;
                operacaoSaida.Valor = operacaoCaixa.Valor * -1;

                operacaoSaida.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                operacaoSaida.DataHoraInsercao = DateTime.Now;
                operacaoSaida.TipoOperacao = EnumTipoOperacao.Recolhimento;
                _contextoOperacao.Add<OperacaoCaixa>(operacaoSaida);
                _contextoOperacao.SaveChanges();


                OperacaoCaixa OperacaoEntrada = new OperacaoCaixa();

                OperacaoEntrada.DataLancamento = operacaoCaixaEntrada.DataLancamento;
                OperacaoEntrada.Descricao = " Entrada Transferencia " + operacaoCaixa.Descricao;
                OperacaoEntrada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(Convert.ToInt64(Request["FormaPagamento"]));
                OperacaoEntrada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                OperacaoEntrada.Observacao = operacaoCaixaEntrada.Observacao;
                OperacaoEntrada.UsuarioQueLancou = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                   .Where(x => x.Nome == NomeUsuarioLogado)
                                                    select c).First();
                OperacaoEntrada.Valor = operacaoCaixaEntrada.Valor;
                OperacaoEntrada.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoEntrada.DataHoraInsercao = DateTime.Now;
                OperacaoEntrada.TipoOperacao = EnumTipoOperacao.Recebimento;
                _contextoOperacao.Add<OperacaoCaixa>(OperacaoEntrada);
                _contextoOperacao.SaveChanges();





            }


            return RedirectToAction("Sucesso", "Home");

        }

        #region Alteracao

        public ActionResult AlterarOperacaoCaixa(int id)
        {
            string NomeUsuarioLogado = User.Identity.Name;
            long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);
            OperacaoCaixa OperacaoParaAlterar = _contextoOperacao.Get<OperacaoCaixa>(id);
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento), "Codigo", "Nome");


            return View(OperacaoParaAlterar);
        }

        [HttpPost]
        public ActionResult AlterarOperacaoCaixa(OperacaoCaixa operacaoCaixa)
        {
            lock (_contextoOperacao)
            {
                string NomeUsuarioLogado = User.Identity.Name;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);


                if (VerificaSeFormaPagamentoDebitaAutomatico(operacaoCaixa.FormaPagamentoUtilizada.Codigo) == false)
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
                    TryUpdateModel<OperacaoCaixa>(operacaoAlterada);
                    _contextoOperacao.SaveChanges();
                    return RedirectToAction("Sucesso", "Home");
                }
                else
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

                }


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
                if (VerificaSeFormaPagamentoDebitaAutomatico(operacaoExcluida.FormaPagamentoUtilizada.Codigo) == false)
                {
                    OperacaoCaixa operacao = _contextoOperacao.Get<OperacaoCaixa>(operacaoExcluida.Codigo);
                    _contextoOperacao.Delete<OperacaoCaixa>(operacao);
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
                    _contextoOperacao.Delete<DespejoOPeracaoCaixa>(Despejo);
                    _contextoOperacao.SaveChanges();

                    _contextoOperacao.Delete<OperacaoCaixa>(OperacaoNegativa);
                    _contextoOperacao.SaveChanges();

                    _contextoOperacao.Delete<OperacaoCaixa>(OperacaoPositiva);
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
                return View();

            }
            else
            {
                ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
                ViewBag.UsuarioPrevilegiado = UsuarioEPrivilegiado;
                return View();
            }


        }

        [HttpPost]
        public ActionResult LancamentosData(ValidarData Datas, int? Loja)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
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

                        listaPorData = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                         .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento
                                        && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento)

                                        select c).ToList();

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

                    ViewBag.Loja = (from c in _contextoOperacao.GetAll<CadastrarUsuario>()
                                                .Where(x => x.Nome == nomeUsuario)
                                    select c.EstabelecimentoTrabalho.RazaoSocial).First();

                    IList<OperacaoCaixa> listaPorData = null;

                    listaPorData = _contextoOperacao.GetAll<OperacaoCaixa>()
                        .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0 != x.Descricao.ToUpper().StartsWith("DESPEJO DO CAIXA") != x.Observacao.StartsWith("Pagamento, retirada do caixa") && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento).ToList();

                    ViewBag.Saldo = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                     .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento)
                                     select c.Valor).Sum();
                    ViewBag.SomaSaida = (from c in _contextoOperacao.GetAll<OperacaoCaixa>()
                                         .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.Valor <= 0 != x.Descricao.ToUpper().StartsWith("DESPEJO DO CAIXA") != x.Observacao.StartsWith("Pagamento, retirada do caixa") && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(X => X.DataLancamento).ToList()
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

        private bool VerificaSeFormaPagamentoDebitaAutomatico(long IdFormaPagamento)
        {
            bool FazDespejo = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                               .Where(x => x.Codigo == IdFormaPagamento)
                               select c.DespejoAutomatico).First();
            if (FazDespejo)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
