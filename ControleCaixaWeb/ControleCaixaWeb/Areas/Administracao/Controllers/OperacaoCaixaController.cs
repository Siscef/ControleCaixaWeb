using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleCaixaWeb.Models;
using ControleCaixaWeb.Models.Context;

namespace ControleCaixaWeb.Areas.Administracao.Controllers
{
    [Authorize(Roles = "Administrador")]
    [HandleError(View = "Error")]
    public class OperacaoCaixaController : Controller
    {
        private IContextoDados _contextoOperacao = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }



        public ActionResult ListaEstabelecimento(int id)
        {
            IList<OperacaoCaixa> listaPorEstabelecimento = null;
            listaPorEstabelecimento = _contextoOperacao.GetAll<OperacaoCaixa>()
                                      .Where(x => x.EstabelecimentoOperacao.Codigo == id).OrderByDescending(x => x.DataLancamento).ToList();
            return View(listaPorEstabelecimento);
        }
        public ActionResult ListaEstabelecimentoNumaData(int codigo, DateTime? data)
        {
            IList<OperacaoCaixa> listaPorEstabelecimento = null;
            listaPorEstabelecimento = _contextoOperacao.GetAll<OperacaoCaixa>()
                                      .Where(x => x.EstabelecimentoOperacao.Codigo == codigo && x.DataLancamento.Date == data).ToList();

            return View(listaPorEstabelecimento);
        }


        public ActionResult VisualizarOperacoes()
        {
            ViewBag.Loja = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
            return View();
        }
        [HttpPost]
        public ActionResult VisualizarOperacoes(ValidarData Datas, int? Loja, int? TipoOperacao)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = User.Identity.Name + ", a data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    IList<OperacaoCaixa> listaOperacao = null;
                    string tipoEnum = Enum.GetName(typeof(EnumTipoOperacao), TipoOperacao);

                    listaOperacao = _contextoOperacao.GetAll<OperacaoCaixa>()
                                     .Where(x => x.DataLancamento >= Datas.DataInicial && x.DataLancamento <= Datas.DataFinal && x.EstabelecimentoOperacao.Codigo == Loja
                                     && x.TipoOperacao.ToString() == tipoEnum.ToString())
                                    .OrderByDescending(x => x.DataLancamento).ToList();

                    return View("ListaDasOperacaoes", listaOperacao);
                }
            }
            ViewBag.Loja = new SelectList(_contextoOperacao.GetAll<Estabelecimento>().Where(x => x.Codigo != 1), "Codigo", "RazaoSocial");
            return View();
        }


        public ActionResult ListaDasOperacaoes()
        {
            return View();
        }


        public ActionResult CadastrarOperacaoCaixa()
        {
            ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial");
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome");

            return View();
        }



        [HttpPost]
        public ActionResult CadastrarOperacaoCaixa(OperacaoCaixa operacaoCaixa)
        {

            ModelState["FormaPagamentoUtilizada.NomeTipoFormaPagamento"].Errors.Clear();
            ModelState["UsuarioQueLancou.Nome"].Errors.Clear();
            ModelState["UsuarioQueLancou.Email"].Errors.Clear();
            ModelState["UsuarioQueLancou.Senha"].Errors.Clear();
            ModelState["UsuarioQueLancou.ConfirmeSenha"].Errors.Clear();
            ModelState["UsuarioQueLancou.EnderecoUsuario"].Errors.Clear();
            ModelState["EstabelecimentoOperacao.RazaoSocial"].Errors.Clear();
            ModelState["EstabelecimentoOperacao.CNPJ"].Errors.Clear();
            ModelState["EstabelecimentoOperacao.InscricaoEstadual"].Errors.Clear();


            if (ModelState.IsValid)
            {
                OperacaoCaixa OperacaoCaixaPositiva = new OperacaoCaixa();

                OperacaoCaixaPositiva.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                OperacaoCaixaPositiva.FormaPagamentoUtilizada = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                                         .Where(x => x.Codigo == operacaoCaixa.FormaPagamentoUtilizada.Codigo && x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == operacaoCaixa.EstabelecimentoOperacao.Codigo)
                                                                 select c).First();

                OperacaoCaixaPositiva.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                OperacaoCaixaPositiva.Descricao = operacaoCaixa.Descricao.ToUpper();
                if (operacaoCaixa.Observacao != null)
                {
                    OperacaoCaixaPositiva.Observacao = operacaoCaixa.Observacao.ToUpper();

                }
                OperacaoCaixaPositiva.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                OperacaoCaixaPositiva.DataHoraInsercao = DateTime.Now;
                OperacaoCaixaPositiva.TipoOperacao = EnumTipoOperacao.LancamentoCaixa;

                _contextoOperacao.Add<OperacaoCaixa>(OperacaoCaixaPositiva);
                _contextoOperacao.SaveChanges();
                if (OperacaoCaixaPositiva.FormaPagamentoUtilizada.DespejoAutomatico == true)
                {
                    OperacaoCaixa OperacaoNegativa = new OperacaoCaixa();

                    OperacaoNegativa.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
                    OperacaoNegativa.FormaPagamentoUtilizada = (from c in _contextoOperacao.GetAll<FormaPagamentoEstabelecimento>()
                                                             .Where(x => x.Codigo == operacaoCaixa.FormaPagamentoUtilizada.Codigo && x.ContaCorrenteFormaPagamento.EstabelecimentoDaConta.Codigo == operacaoCaixa.EstabelecimentoOperacao.Codigo)
                                                                select c).First();

                    OperacaoNegativa.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
                    OperacaoNegativa.Descricao = operacaoCaixa.Descricao.ToUpper();
                    if (operacaoCaixa.Observacao != null)
                    {
                        OperacaoNegativa.Observacao = operacaoCaixa.Observacao.ToUpper();

                    }
                    OperacaoNegativa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                    OperacaoNegativa.DataHoraInsercao = DateTime.Now;
                    OperacaoNegativa.TipoOperacao = EnumTipoOperacao.SaidaLancamentoCaixa;

                    _contextoOperacao.Add<OperacaoCaixa>(OperacaoNegativa);
                    _contextoOperacao.SaveChanges();


                }

                return RedirectToAction("Sucesso", "Home");
            }
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento", operacaoCaixa.FormaPagamentoUtilizada);
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome", operacaoCaixa.UsuarioQueLancou);
            ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", operacaoCaixa.EstabelecimentoOperacao);
            return View();
        }

        public ActionResult AlterarOperacaoCaixa(int id)
        {
            OperacaoCaixa OperacaoParaAlterar = _contextoOperacao.Get<OperacaoCaixa>(id);
            ViewBag.Estabelecimento = new SelectList(_contextoOperacao.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial");
            ViewBag.FormaPagamentoUtilizada = new SelectList(_contextoOperacao.GetAll<FormaPagamentoEstabelecimento>(), "Codigo", "NomeTipoFormaPagamento");
            ViewBag.UsuarioQueLancou = new SelectList(_contextoOperacao.GetAll<CadastrarUsuario>(), "Codigo", "Nome");
            return View(OperacaoParaAlterar);
        }



        [HttpPost]
        public ActionResult AlterarOperacaoCaixa(OperacaoCaixa operacaoCaixa)
        {
            OperacaoCaixa operacaoAlterada = _contextoOperacao.Get<OperacaoCaixa>(operacaoCaixa.Codigo);

            operacaoAlterada.DataLancamento = operacaoCaixa.DataLancamento.Date;
            operacaoAlterada.Descricao = operacaoCaixa.Descricao.ToUpper();
            operacaoAlterada.EstabelecimentoOperacao = _contextoOperacao.Get<Estabelecimento>(operacaoCaixa.EstabelecimentoOperacao.Codigo);
            operacaoAlterada.FormaPagamentoUtilizada = _contextoOperacao.Get<FormaPagamentoEstabelecimento>(operacaoCaixa.FormaPagamentoUtilizada.Codigo);
            operacaoAlterada.Valor = operacaoCaixa.Valor;
            if (operacaoAlterada.Observacao != null)
            {
                operacaoAlterada.Observacao = operacaoCaixa.Observacao.ToUpper();

            }

            operacaoAlterada.UsuarioQueLancou = _contextoOperacao.Get<CadastrarUsuario>(operacaoCaixa.UsuarioQueLancou.Codigo);
            operacaoCaixa.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            operacaoCaixa.DataHoraInsercao = DateTime.Now;
            _contextoOperacao.SaveChanges();


            return RedirectToAction("Sucesso", "Home");
        }

        public ActionResult ExcluirOperacaoCaixa(int id)
        {
            OperacaoCaixa operacaoParaExcluir = _contextoOperacao.Get<OperacaoCaixa>(id);
            return View(operacaoParaExcluir);
        }

        [HttpPost]
        public ActionResult ExcluirOperacaoCaixa(OperacaoCaixa operacaoExcluida)
        {
            OperacaoCaixa operacao = _contextoOperacao.Get<OperacaoCaixa>(operacaoExcluida.Codigo);
            _contextoOperacao.Delete<OperacaoCaixa>(operacao);
            _contextoOperacao.SaveChanges();
            return RedirectToAction("Sucesso", "Home");
        }
        public ActionResult OperacoesNestaData(DateTime? data)
        {
            IList<OperacaoCaixa> listaPorData = null;
            listaPorData = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.DataLancamento.Date == data).ToList();

            return View(listaPorData);
        }

        public ActionResult VerUsuario(string id, DateTime? data)
        {
            IList<OperacaoCaixa> listaPorNome = null;
            listaPorNome = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.UsuarioQueLancou.Nome == id && x.DataLancamento.Date == data).ToList();

            return View(listaPorNome);
        }

        public ActionResult VerPorFormaPagamento(string nomeForma, DateTime? data)
        {
            IList<OperacaoCaixa> listaPorForma = null;
            listaPorForma = _contextoOperacao.GetAll<OperacaoCaixa>()
                           .Where(x => x.FormaPagamentoUtilizada.NomeTipoFormaPagamento == nomeForma).ToList();

            return View(listaPorForma);
        }


    }
}

