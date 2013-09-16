using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;
using Telerik.Web.Mvc;
using System.Web.UI.WebControls;


namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    [Authorize(Roles = "Escritorio")]
    [HandleError(View = "Error")]
    public class PagamentosController : Controller
    {
        private IContextoDados _contextoPagamento = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PagamentosNaData(DateTime? id)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();


            var listadosPagamentos = _contextoPagamento.GetAll<Pagamento>().Where(x => x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento && x.DataPagamento == id).ToList();

            return View(listadosPagamentos);
        }

        public ActionResult ListaPagamento()
        {
            return View();
        }

        public ActionResult PagamentosNoMes(int id)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            IList<Pagamento> listaDosPagamentos = null;
            listaDosPagamentos = _contextoPagamento.GetAll<Pagamento>()
            .Where(x => x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento && x.DataPagamento.Month == id).OrderByDescending(x => x.DataPagamento).ToList();

            return View(listaDosPagamentos);
        }

        public ActionResult PagamentoFavorecido(int id)
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                         .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            IList<Pagamento> pagamentosDesteFavorecido = null;
            pagamentosDesteFavorecido = _contextoPagamento.GetAll<Pagamento>().Where(x => x.FavorecidoPagamento.Codigo == id && x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento).ToList();

            return View(pagamentosDesteFavorecido);
        }


        public ActionResult Detalhes(int id)
        {
            Pagamento detalhesPagamento = _contextoPagamento.Get<Pagamento>(id);

            return View(detalhesPagamento);
        }



        public ActionResult FazerPagamento()
        {
            string NomeUsuarioLogado = User.Identity.Name;

            long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                          .Where(x => x.Nome == NomeUsuarioLogado)
                                          select c.EstabelecimentoTrabalho.Codigo).First();
            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>().Where(x => x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == codigoEstabelecimento), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido");
            return View();
        }


        [HttpPost]
        public ActionResult FazerPagamento(Pagamento pagamento)
        {
            ModelState["FormaPagamento.NomeTipoFormaPagamento"].Errors.Clear();
            ModelState["FavorecidoPagamento.NomeFavorecido"].Errors.Clear();
            ModelState["EstabelecimentoQuePagou"].Errors.Clear();
            if (ModelState.IsValid)
            {
                if (pagamento.DataPagamento != null)
                {
                    TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(pagamento.DataPagamento.ToString("yyyy/MM/dd HH:mm:ss"));
                    if (diferenca.TotalDays < 0)
                    {
                        ViewBag.DataErradaAmanha = "Você não pode fazer um lançamento para uma data futura!";
                        ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
                        ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido", pagamento.FavorecidoPagamento);
                        return View();

                    }
                    if (diferenca.TotalDays > 31)
                    {
                        ViewBag.DataErradaOntem = "Você não pode fazer um lançamento para uma data em um passado tão distante!";
                        ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
                        ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido", pagamento.FavorecidoPagamento);
                        return View();

                    }


                }



                lock (_contextoPagamento)
                {
                    string NomeUsuarioLogado = User.Identity.Name;
                    IList<CadastrarUsuario> ListaVerificaUsuarioPrivilegiado = _contextoPagamento.GetAll<CadastrarUsuario>()
                                                                                .Where(x => x.Nome == NomeUsuarioLogado && x.Privilegiado == true)
                                                                                .ToList();
                    if (ListaVerificaUsuarioPrivilegiado.Count() == 0)
                    {
                        long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                               .Where(x => x.Nome == NomeUsuarioLogado)
                                                      select c.EstabelecimentoTrabalho.Codigo).First();
                        decimal SaldoAtual = (from c in _contextoPagamento.GetAll<OperacaoCaixa>()
                                             .Where(x => x.EstabelecimentoOperacao.Codigo == codigoEstabelecimento && x.FormaPagamentoUtilizada == pagamento.FormaPagamento)
                                              select c.Valor).Sum();
                        if (pagamento.Valor > SaldoAtual)
                        {
                            decimal SaldoLoja = (from c in _contextoPagamento.GetAll<OperacaoCaixa>()
                                                .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                                                 select c.Valor).Sum();
                            ViewBag.MensagemSaldoInsuficiente = "O saldo da forma de pagamento é menor que o valor: R$  " + SaldoAtual + " .";
                            ViewBag.MensagemSaldoInsuficienteLoja = " O saldo do estabelecimento é:  R$ " + SaldoLoja + " .";
                            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
                            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido", pagamento.FavorecidoPagamento);

                            return View();
                        }

                        IList<Pagamento> ListaVerificaPagamentoDuplicado = _contextoPagamento.GetAll<Pagamento>()
                                                                               .Where(x => x.Valor == pagamento.Valor && x.FavorecidoPagamento.Codigo == pagamento.FavorecidoPagamento.Codigo && x.UsuarioQueLancou.Nome == NomeUsuarioLogado)
                                                                               .ToList();

                        if (ListaVerificaPagamentoDuplicado.Count() >= 1)
                        {
                            foreach (var itemListaVerificaPagamentoDuplicado in ListaVerificaPagamentoDuplicado)
                            {
                                if (itemListaVerificaPagamentoDuplicado.DataHoraInsercao == null)
                                {

                                    OperacaoCaixa OperacaoParaPrimeiroPagamento = new OperacaoCaixa();

                                    OperacaoParaPrimeiroPagamento.DataLancamento = pagamento.DataPagamento.Date;
                                    OperacaoParaPrimeiroPagamento.Descricao = pagamento.Observacao;
                                    OperacaoParaPrimeiroPagamento.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                                    OperacaoParaPrimeiroPagamento.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                                    OperacaoParaPrimeiroPagamento.Observacao = "Pagamento, retirada do caixa. Detalhes: " + pagamento.Observacao;
                                    OperacaoParaPrimeiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.NomeFuncao.NomePapel == "Escritorio").First();
                                    OperacaoParaPrimeiroPagamento.Valor = pagamento.Valor * -1;
                                    OperacaoParaPrimeiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    OperacaoParaPrimeiroPagamento.DataHoraInsercao = DateTime.Now;
                                    OperacaoParaPrimeiroPagamento.TipoOperacao = EnumTipoOperacao.Pagamento;
                                    _contextoPagamento.Add<OperacaoCaixa>(OperacaoParaPrimeiroPagamento);
                                    _contextoPagamento.SaveChanges();

                                    Pagamento PrimeiroPagamento = new Pagamento();

                                    PrimeiroPagamento.DataPagamento = pagamento.DataPagamento.Date;
                                    PrimeiroPagamento.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                                    PrimeiroPagamento.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
                                    PrimeiroPagamento.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                                    if (pagamento.Observacao != null)
                                    {
                                        pagamento.Observacao = pagamento.Observacao.ToUpper();
                                    }
                                    PrimeiroPagamento.Observacao = pagamento.Observacao;
                                    PrimeiroPagamento.Valor = pagamento.Valor;
                                    PrimeiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.NomeFuncao.NomePapel == "Escritorio").First();
                                    PrimeiroPagamento.UsuarioQueAlterouEDataEComputador = "Lançado por:" + User.Identity.Name + " Em " + DateTime.Now.Date + " No " + Environment.MachineName;
                                    PrimeiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    PrimeiroPagamento.DataHoraInsercao = DateTime.Now;
                                    PrimeiroPagamento.CodigoOperacaoCaixa = OperacaoParaPrimeiroPagamento.Codigo;
                                    _contextoPagamento.Add<Pagamento>(PrimeiroPagamento);
                                    _contextoPagamento.SaveChanges();


                                    return RedirectToAction("Sucesso", "Home");
                                }
                                else
                                {
                                    TimeSpan diferenca = Convert.ToDateTime(DateTime.Now) - Convert.ToDateTime(itemListaVerificaPagamentoDuplicado.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                    if (diferenca.TotalMinutes <= 4)
                                    {
                                        double TempoDecorrido = 4 - Math.Round(diferenca.TotalMinutes, 0);
                                        ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes, 0) + " minuto(s)" +
                                                           " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)";
                                        ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
                                        ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido");
                                        return View();
                                    }
                                    else
                                    {
                                        OperacaoCaixa OperacaoParaSegundoPagamento = new OperacaoCaixa();

                                        OperacaoParaSegundoPagamento.DataLancamento = pagamento.DataPagamento.Date;
                                        OperacaoParaSegundoPagamento.Descricao = pagamento.Observacao;
                                        OperacaoParaSegundoPagamento.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                                        OperacaoParaSegundoPagamento.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                                        OperacaoParaSegundoPagamento.Observacao = "Pagamento, retirada do caixa. Detalhes: " + pagamento.Observacao;
                                        OperacaoParaSegundoPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.NomeFuncao.NomePapel == "Escritorio").First();
                                        OperacaoParaSegundoPagamento.Valor = pagamento.Valor * -1;
                                        OperacaoParaSegundoPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        OperacaoParaSegundoPagamento.DataHoraInsercao = DateTime.Now;
                                        OperacaoParaSegundoPagamento.TipoOperacao = EnumTipoOperacao.Pagamento;
                                        _contextoPagamento.Add<OperacaoCaixa>(OperacaoParaSegundoPagamento);
                                        _contextoPagamento.SaveChanges();

                                        Pagamento SegundoPagamento = new Pagamento();
                                        SegundoPagamento.DataPagamento = pagamento.DataPagamento.Date;
                                        SegundoPagamento.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                                        SegundoPagamento.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
                                        SegundoPagamento.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                                        if (pagamento.Observacao != null)
                                        {
                                            pagamento.Observacao = pagamento.Observacao.ToUpper();
                                        }
                                        SegundoPagamento.Observacao = pagamento.Observacao;
                                        SegundoPagamento.Valor = pagamento.Valor;
                                        SegundoPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.NomeFuncao.NomePapel == "Escritorio").First();
                                        SegundoPagamento.UsuarioQueAlterouEDataEComputador = "Lançado por:" + User.Identity.Name + " Em " + DateTime.Now.Date + " No " + Environment.MachineName;
                                        SegundoPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                        SegundoPagamento.DataHoraInsercao = DateTime.Now;
                                        SegundoPagamento.CodigoOperacaoCaixa = OperacaoParaSegundoPagamento.Codigo;
                                        _contextoPagamento.Add<Pagamento>(SegundoPagamento);
                                        _contextoPagamento.SaveChanges();
                                        return RedirectToAction("Sucesso", "Home");

                                    }
                                }

                            }


                        }


                        else
                        {


                            OperacaoCaixa OperacaoParaTerceiroPagamento = new OperacaoCaixa();

                            OperacaoParaTerceiroPagamento.DataLancamento = pagamento.DataPagamento;
                            OperacaoParaTerceiroPagamento.Descricao = pagamento.Observacao;
                            OperacaoParaTerceiroPagamento.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                            OperacaoParaTerceiroPagamento.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                            OperacaoParaTerceiroPagamento.Observacao = "Pagamento, retirada do caixa. Detalhes: " + pagamento.Observacao;
                            OperacaoParaTerceiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.NomeFuncao.NomePapel == "Escritorio").First();
                            OperacaoParaTerceiroPagamento.Valor = pagamento.Valor * -1;
                            OperacaoParaTerceiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            OperacaoParaTerceiroPagamento.DataHoraInsercao = DateTime.Now;
                            OperacaoParaTerceiroPagamento.TipoOperacao = EnumTipoOperacao.Pagamento;
                            _contextoPagamento.Add<OperacaoCaixa>(OperacaoParaTerceiroPagamento);
                            _contextoPagamento.SaveChanges();

                            Pagamento TerceiroPagamento = new Pagamento();

                            TerceiroPagamento.DataPagamento = pagamento.DataPagamento.Date;
                            TerceiroPagamento.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                            TerceiroPagamento.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
                            TerceiroPagamento.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                            if (pagamento.Observacao != null)
                            {
                                pagamento.Observacao = pagamento.Observacao.ToUpper();
                            }
                            TerceiroPagamento.Observacao = pagamento.Observacao;
                            TerceiroPagamento.Valor = pagamento.Valor;
                            TerceiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.NomeFuncao.NomePapel == "Escritorio").First();
                            TerceiroPagamento.UsuarioQueAlterouEDataEComputador = "Lançado por:" + User.Identity.Name + " Em " + DateTime.Now.Date + " No " + Environment.MachineName;
                            TerceiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                            TerceiroPagamento.DataHoraInsercao = DateTime.Now;
                            TerceiroPagamento.CodigoOperacaoCaixa = OperacaoParaTerceiroPagamento.Codigo;
                            _contextoPagamento.Add<Pagamento>(TerceiroPagamento);
                            _contextoPagamento.SaveChanges();

                            return RedirectToAction("Sucesso", "Home");
                        }

                    }
                    else
                    {
                        long codigoEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                              .Where(x => x.Nome == NomeUsuarioLogado)
                                                      select c.EstabelecimentoTrabalho.Codigo).First();

                        OperacaoCaixa OperacaoParaTerceiroPagamento = new OperacaoCaixa();

                        OperacaoParaTerceiroPagamento.DataLancamento = pagamento.DataPagamento;
                        OperacaoParaTerceiroPagamento.Descricao = pagamento.Observacao;
                        OperacaoParaTerceiroPagamento.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                        OperacaoParaTerceiroPagamento.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                        OperacaoParaTerceiroPagamento.Observacao = "Pagamento, retirada do caixa. Detalhes: " + pagamento.Observacao;
                        OperacaoParaTerceiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento && x.NomeFuncao.NomePapel == "Escritorio").First();
                        OperacaoParaTerceiroPagamento.Valor = pagamento.Valor * -1;
                        OperacaoParaTerceiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        OperacaoParaTerceiroPagamento.DataHoraInsercao = DateTime.Now;
                        OperacaoParaTerceiroPagamento.TipoOperacao = EnumTipoOperacao.Pagamento;
                        _contextoPagamento.Add<OperacaoCaixa>(OperacaoParaTerceiroPagamento);
                        _contextoPagamento.SaveChanges();

                        Pagamento TerceiroPagamento = new Pagamento();

                        TerceiroPagamento.DataPagamento = pagamento.DataPagamento.Date;
                        TerceiroPagamento.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                        TerceiroPagamento.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
                        TerceiroPagamento.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                        if (pagamento.Observacao != null)
                        {
                            pagamento.Observacao = pagamento.Observacao.ToUpper();
                        }
                        TerceiroPagamento.Observacao = pagamento.Observacao;
                        TerceiroPagamento.Valor = pagamento.Valor;
                        TerceiroPagamento.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuarioLogado && x.NomeFuncao.NomePapel == "Escritorio").First();
                        TerceiroPagamento.UsuarioQueAlterouEDataEComputador = "Lançado por:" + User.Identity.Name + " Em " + DateTime.Now.Date + " No " + Environment.MachineName;
                        TerceiroPagamento.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiroPagamento.DataHoraInsercao = DateTime.Now;
                        TerceiroPagamento.CodigoOperacaoCaixa = OperacaoParaTerceiroPagamento.Codigo;
                        _contextoPagamento.Add<Pagamento>(TerceiroPagamento);
                        _contextoPagamento.SaveChanges();

                        return RedirectToAction("Sucesso", "Home");

                    }



                }
            }
            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", pagamento.FormaPagamento);
            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido", pagamento.FavorecidoPagamento);
            return View();
        }

        public ActionResult AlterarPagamento(int id)
        {
            Pagamento pagamentoParaAlterar = _contextoPagamento.Get<Pagamento>(id);

            ViewBag.Forma = new SelectList(_contextoPagamento.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.Favorecido = new SelectList(_contextoPagamento.GetAll<Favorecido>(), "Codigo", "NomeFavorecido");
            return View(pagamentoParaAlterar);
        }



        [HttpPost]
        public ActionResult AlterarPagamento(Pagamento pagamento)
        {
            lock (_contextoPagamento)
            {
                string NomeUsuarioLogado = User.Identity.Name;
                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

                Pagamento pagamentoAlterado = _contextoPagamento.Get<Pagamento>(pagamento.Codigo);

                pagamentoAlterado.DataPagamento = pagamento.DataPagamento.Date;
                pagamentoAlterado.FormaPagamento = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                pagamentoAlterado.EstabelecimentoQuePagou = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                pagamentoAlterado.FavorecidoPagamento = _contextoPagamento.Get<Favorecido>(pagamento.FavorecidoPagamento.Codigo);
                pagamentoAlterado.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
                pagamentoAlterado.Valor = pagamento.Valor;
                pagamentoAlterado.UsuarioQueAlterouEDataEComputador = "Alterado por:" + User.Identity.Name + "Em " + DateTime.Now.Date + "No " + Environment.MachineName;
                if (pagamento.Observacao != null)
                {
                    pagamento.Observacao = pagamento.Observacao.ToUpper();
                }
                pagamentoAlterado.Observacao = pagamento.Observacao;
                pagamentoAlterado.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                pagamentoAlterado.DataHoraInsercao = DateTime.Now;
                _contextoPagamento.SaveChanges();

                OperacaoCaixa operacaoParaAlterar = _contextoPagamento.GetAll<OperacaoCaixa>().Where(x => x.Codigo == pagamentoAlterado.CodigoOperacaoCaixa).First();

                operacaoParaAlterar.DataLancamento = pagamento.DataPagamento.Date;
                operacaoParaAlterar.Descricao = pagamento.Codigo.ToString();
                operacaoParaAlterar.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                operacaoParaAlterar.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(pagamento.FormaPagamento.Codigo);
                operacaoParaAlterar.Observacao = pagamento.Observacao;
                operacaoParaAlterar.Valor = pagamento.Valor * -1;
                operacaoParaAlterar.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
                operacaoParaAlterar.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                operacaoParaAlterar.DataHoraInsercao = DateTime.Now;
                _contextoPagamento.SaveChanges();

            }
            return RedirectToAction("Sucesso", "Home");
        }


        public ActionResult ExcluirPagamento(int id)
        {
            Pagamento pagamentoParaExcluir = _contextoPagamento.Get<Pagamento>(id);
            return View(pagamentoParaExcluir);
        }



        [HttpPost]
        public ActionResult ExcluirPagamento(Pagamento pagamento)
        {
            lock (_contextoPagamento)
            {


                string NomeUsuarioLogado = User.Identity.Name;

                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuarioLogado);

                long codigoFormaPagamento = (from c in _contextoPagamento.GetAll<Pagamento>()
                                             .Where(x => x.Codigo == pagamento.Codigo)
                                             select c.FormaPagamento.Codigo).First();



                Pagamento pagamentoExcluido = _contextoPagamento.Get<Pagamento>(pagamento.Codigo);


                OperacaoCaixa operacaoParaEstornar = new OperacaoCaixa();

                operacaoParaEstornar.DataLancamento = pagamentoExcluido.DataPagamento;
                operacaoParaEstornar.Descricao = "Lançamento Estorno Pagamento Código: " + pagamento.Codigo.ToString();
                operacaoParaEstornar.EstabelecimentoOperacao = _contextoPagamento.Get<Estabelecimento>(codigoEstabelecimento);
                operacaoParaEstornar.FormaPagamentoUtilizada = _contextoPagamento.Get<FormaPagamentoEstabelecimento>(codigoFormaPagamento);
                operacaoParaEstornar.Observacao = "Estorno de Pagamento:  " + pagamento.Observacao;
                operacaoParaEstornar.Valor = pagamentoExcluido.Valor;
                operacaoParaEstornar.UsuarioQueLancou = _contextoPagamento.GetAll<CadastrarUsuario>().Where(x => x.EstabelecimentoTrabalho.Codigo == codigoEstabelecimento).First();
                operacaoParaEstornar.Conferido = false;
                operacaoParaEstornar.DataHoraInsercao = DateTime.Now;
                operacaoParaEstornar.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                operacaoParaEstornar.TipoOperacao = EnumTipoOperacao.DevolucaoPagamento;
                _contextoPagamento.Add<OperacaoCaixa>(operacaoParaEstornar);
                _contextoPagamento.SaveChanges();
                _contextoPagamento.Delete<Pagamento>(pagamentoExcluido);
                _contextoPagamento.SaveChanges();
            }
            return RedirectToAction("Sucesso", "Home");
        }

        private bool VerificarUsuarioTemPermissao(string NomeUsuario)
        {
            bool TemPermissao = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                 .Where(x => x.Nome == NomeUsuario)
                                 select c.Privilegiado).First();
            if (TemPermissao == true)
            {
                return true;

            }
            return false;
        }


        public ActionResult PagamentosData()
        {
            if (VerificarUsuarioTemPermissao(User.Identity.Name) == true)
            {
                ViewBag.Estabelecimento = new SelectList(_contextoPagamento.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
                return View();
            }
            else
            {
                return View();

            }

        }

        [HttpPost]
        public ActionResult PagamentosData(ValidarData Datas, int? Estabelecimento)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (Estabelecimento == null)
                    {
                        string nomeUsuario = User.Identity.Name;
                        long codigoEstabelecimento = BuscaEstabelecimento(nomeUsuario);
                        ViewBag.DataInicio = Datas.DataInicial;
                        ViewBag.DataFim = Datas.DataFinal;
                        ViewBag.Loja = (from c in _contextoPagamento.GetAll<Estabelecimento>()
                                        .Where(x => x.Codigo == codigoEstabelecimento)
                                        select c.RazaoSocial).First();

                        IList<Pagamento> listaDosPagamentos = null;
                        listaDosPagamentos = _contextoPagamento.GetAll<Pagamento>()
                                            .Where(x => x.EstabelecimentoQuePagou.Codigo == codigoEstabelecimento && x.DataPagamento.Date >= Datas.DataInicial && x.DataPagamento.Date <= Datas.DataFinal).OrderByDescending(x => x.DataPagamento).ToList();
                        return View("ListaPagamento", listaDosPagamentos);
                        
                    }
                    else
                    {
                        
                        ViewBag.DataInicio = Datas.DataInicial;
                        ViewBag.DataFim = Datas.DataFinal;
                        ViewBag.Loja = (from c in _contextoPagamento.GetAll<Estabelecimento>()
                                        .Where(x => x.Codigo == Estabelecimento)
                                        select c.RazaoSocial).First();

                        IList<Pagamento> listaDosPagamentos = null;
                        listaDosPagamentos = _contextoPagamento.GetAll<Pagamento>()
                                               .Where(x => x.EstabelecimentoQuePagou.Codigo == Estabelecimento && x.DataPagamento.Date >= Datas.DataInicial && x.DataPagamento.Date <= Datas.DataFinal).OrderByDescending(x => x.DataPagamento).ToList();
                        return View("ListaPagamento", listaDosPagamentos);
                    }
                    
                }
            }

            return View();
        }


        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoPagamento.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
        }

        public ActionResult ErroOperacao()
        {
            ViewBag.Saldo = (from c in _contextoPagamento.GetAll<OperacaoCaixa>()
                             .Where(x => x.EstabelecimentoOperacao.Codigo == BuscaEstabelecimento(User.Identity.Name))
                             select c.Valor).Sum();
            return View();
        }

    }
}
