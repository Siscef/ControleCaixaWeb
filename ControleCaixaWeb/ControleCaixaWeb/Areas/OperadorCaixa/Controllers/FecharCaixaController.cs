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
    public class FecharCaixaController : Controller
    {
        private IContextoDados _contextoFecharCaixa = new ContextoDadosNH();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListaDosFechamentosCaixa()
        {
            IList<FechamentoCaixa> listaDosFechamentos = null;
            listaDosFechamentos = _contextoFecharCaixa.GetAll<FechamentoCaixa>().ToList();
            return View(listaDosFechamentos);
        }



        public ActionResult Detalhes(int id)
        {
            FechamentoCaixa FecharCaixa = _contextoFecharCaixa.Get<FechamentoCaixa>(id);

            return View(FecharCaixa);
        }



        public ActionResult LancarFechamentoCaixa()
        {

            return View();
        }



        [HttpPost]
        public ActionResult LancarFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
        {
            lock (_contextoFecharCaixa)
            {

                if (ModelState.IsValid)
                {
                    IList<FechamentoCaixa> ListaVerificaInsercaoDuplicada = _contextoFecharCaixa.GetAll<FechamentoCaixa>()
                                                                           .Where(x => x.CaixaAbertura == fechamentoCaixa.CaixaAbertura && x.CaixaFechamento == fechamentoCaixa.CaixaFechamento && x.Faturamento == fechamentoCaixa.Faturamento)
                                                                           .ToList();
                    if (ListaVerificaInsercaoDuplicada.Count() >= 1)
                    {
                        foreach (var itemListaVerificacao in ListaVerificaInsercaoDuplicada)
                        {
                            if (itemListaVerificacao.DataHoraInsercao == null)
                            {
                                FechamentoCaixa PrimeiroFechamentoCaixa = new FechamentoCaixa();
                                string NomeUsuario = User.Identity.Name;

                                long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuario);

                                PrimeiroFechamentoCaixa.FaturamentoUsuario = _contextoFecharCaixa.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuario).FirstOrDefault();
                                PrimeiroFechamentoCaixa.FaturamentoEstabelecimento = _contextoFecharCaixa.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimento).FirstOrDefault();
                                PrimeiroFechamentoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                PrimeiroFechamentoCaixa.DataHoraInsercao = DateTime.Now;
                                PrimeiroFechamentoCaixa.CaixaFechamento = fechamentoCaixa.CaixaFechamento;
                                PrimeiroFechamentoCaixa.CaixaAbertura = fechamentoCaixa.CaixaAbertura;
                                PrimeiroFechamentoCaixa.Faturamento = fechamentoCaixa.Faturamento;
                                PrimeiroFechamentoCaixa.DataLancamento = fechamentoCaixa.DataLancamento;
                                _contextoFecharCaixa.Add<FechamentoCaixa>(PrimeiroFechamentoCaixa);
                                _contextoFecharCaixa.SaveChanges();
                                _contextoFecharCaixa.Dispose();
                                return RedirectToAction("Sucesso", "Home");

                            }
                            else
                            {
                                TimeSpan diferenca =Convert.ToDateTime(DateTime.Now)- Convert.ToDateTime(itemListaVerificacao.DataHoraInsercao.ToString("yyyy/MM/dd HH:mm:ss"));

                                if (diferenca.TotalMinutes <= 4)
                                {
                                    double TempoDecorrido = 4-Math.Round(diferenca.TotalMinutes, 0) ;
                                    ViewBag.Mensagem = "Atenção! " + User.Identity.Name + " Faz: " + Math.Round(diferenca.TotalMinutes,0) + " minuto(s)" +
                                                       " que você fez essa operação, se ela realmente existe tente daqui a: " + TempoDecorrido.ToString() + " minuto(s)"; 
                                    return View();

                                }
                                else
                                {
                                    FechamentoCaixa SegundoFechamentoCaixa = new FechamentoCaixa();
                                    string NomeUsuario = User.Identity.Name;

                                    long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuario);

                                    SegundoFechamentoCaixa.FaturamentoUsuario = _contextoFecharCaixa.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuario).FirstOrDefault();
                                    SegundoFechamentoCaixa.FaturamentoEstabelecimento = _contextoFecharCaixa.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimento).FirstOrDefault();
                                    SegundoFechamentoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                                    SegundoFechamentoCaixa.DataHoraInsercao = DateTime.Now;
                                    SegundoFechamentoCaixa.CaixaFechamento = fechamentoCaixa.CaixaFechamento;
                                    SegundoFechamentoCaixa.CaixaAbertura = fechamentoCaixa.CaixaAbertura;
                                    SegundoFechamentoCaixa.Faturamento = fechamentoCaixa.Faturamento;
                                    SegundoFechamentoCaixa.DataLancamento = fechamentoCaixa.DataLancamento;
                                    _contextoFecharCaixa.Add<FechamentoCaixa>(SegundoFechamentoCaixa);
                                    _contextoFecharCaixa.SaveChanges();
                                    _contextoFecharCaixa.Dispose();
                                    return RedirectToAction("Sucesso", "Home");
                                }


                            }

                        }

                    }
                    else
                    {
                        FechamentoCaixa TerceiroFechamentoCaixa = new FechamentoCaixa();
                        string NomeUsuario = User.Identity.Name;

                        long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuario);

                        TerceiroFechamentoCaixa.FaturamentoUsuario = _contextoFecharCaixa.GetAll<CadastrarUsuario>().Where(x => x.Nome == NomeUsuario).FirstOrDefault();
                        TerceiroFechamentoCaixa.FaturamentoEstabelecimento = _contextoFecharCaixa.GetAll<Estabelecimento>().Where(x => x.Codigo == codigoEstabelecimento).FirstOrDefault();
                        TerceiroFechamentoCaixa.UsuarioDataHoraInsercao = "Lançado por: " + User.Identity.Name + " Data: " + DateTime.Now;
                        TerceiroFechamentoCaixa.DataHoraInsercao = DateTime.Now;
                        TerceiroFechamentoCaixa.CaixaFechamento = fechamentoCaixa.CaixaFechamento;
                        TerceiroFechamentoCaixa.CaixaAbertura = fechamentoCaixa.CaixaAbertura;
                        TerceiroFechamentoCaixa.Faturamento = fechamentoCaixa.Faturamento;
                        TerceiroFechamentoCaixa.DataLancamento = fechamentoCaixa.DataLancamento;
                        _contextoFecharCaixa.Add<FechamentoCaixa>(TerceiroFechamentoCaixa);
                        _contextoFecharCaixa.SaveChanges();
                        _contextoFecharCaixa.Dispose();
                        return RedirectToAction("Sucesso", "Home");

                    }


                }

            }
            ViewBag.Usuario = new SelectList(_contextoFecharCaixa.GetAll<CadastrarUsuario>(), "Codigo", "Nome", fechamentoCaixa.FaturamentoUsuario);
            ViewBag.Estabelecimento = new SelectList(_contextoFecharCaixa.GetAll<Estabelecimento>(), "Codigo", "RazaoSocial", fechamentoCaixa.FaturamentoEstabelecimento);

            return View();
        }



        public ActionResult AlterarFechamentoCaixa(int id)
        {
            FechamentoCaixa fecharCaixaAlterar = _contextoFecharCaixa.Get<FechamentoCaixa>(id);
            return View(fecharCaixaAlterar);
        }



        [HttpPost]
        public ActionResult AlterarFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
        {
            FechamentoCaixa fecharCaixaAlterada = _contextoFecharCaixa.Get<FechamentoCaixa>(fechamentoCaixa.Codigo);

            TryUpdateModel(fecharCaixaAlterada);
            fecharCaixaAlterada.UsuarioDataHoraInsercao = "Alterado por: " + User.Identity.Name + " Data: " + DateTime.Now;
            fecharCaixaAlterada.DataHoraInsercao = DateTime.Now;
            _contextoFecharCaixa.SaveChanges();
            return RedirectToAction("Sucesso", "Home");
        }



        public ActionResult ExcluirFechamentoCaixa(int id)
        {
            FechamentoCaixa fecharCaixaExcluir = _contextoFecharCaixa.Get<FechamentoCaixa>(id);
            return View(fecharCaixaExcluir);
        }



        [HttpPost]
        public ActionResult ExcluirFechamentoCaixa(FechamentoCaixa fechamentoCaixa)
        {
            FechamentoCaixa fecharCaixaExluida = _contextoFecharCaixa.Get<FechamentoCaixa>(fechamentoCaixa.Codigo);
            _contextoFecharCaixa.Delete<FechamentoCaixa>(fecharCaixaExluida);
            _contextoFecharCaixa.SaveChanges();

            return RedirectToAction("Sucesso", "Home");
        }

        public ActionResult VerMeuCaixa()
        {
            return View();
        }

        public ActionResult VerNestaData(DateTime? id)
        {
            string NomeUsuario = User.Identity.Name;
            IList<FechamentoCaixa> listaVerMeuCaixaNumaData = null;

            listaVerMeuCaixaNumaData = _contextoFecharCaixa.GetAll<FechamentoCaixa>().Where(x => x.FaturamentoUsuario.Nome == NomeUsuario && x.DataLancamento.Date == id).OrderByDescending(x => x.DataLancamento).ToList();


            var valores = (from c in _contextoFecharCaixa.GetAll<OperacaoCaixa>().Where(x => x.DataLancamento.Date == id && x.UsuarioQueLancou.Nome == NomeUsuario && x.Valor > 0)
                           select c.Valor).Sum();

            var caixaFechamento = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>().Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Nome == NomeUsuario)
                                   select l.CaixaFechamento).Sum();
            var caixaAbertura = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>().Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Nome == NomeUsuario)
                                 select l.CaixaAbertura).Sum();
            var faturamento = (from l in _contextoFecharCaixa.GetAll<FechamentoCaixa>().Where(x => x.DataLancamento.Date == id && x.FaturamentoUsuario.Nome == NomeUsuario)
                               select l.Faturamento).Sum();

            ViewBag.listaParaResultadoFechamento = (((valores + caixaFechamento) - caixaAbertura) - faturamento);

            return View(listaVerMeuCaixaNumaData);
        }


        public ActionResult FechamentoCaixaData()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FechamentoCaixaData(ValidarData Datas)
        {
            if (Datas.DataFinal < Datas.DataInicial)
            {
                ViewBag.DataErrada = "A data final não pode ser menor que a data inicial";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    string NomeUsuario = User.Identity.Name;
                    long codigoEstabelecimento = BuscaEstabelecimento(NomeUsuario);
                    ViewBag.DataInicio = Datas.DataInicial;
                    ViewBag.DataFim = Datas.DataFinal;
                    ViewBag.Loja = (from c in _contextoFecharCaixa.GetAll<Estabelecimento>()
                                    .Where(x => x.Codigo == codigoEstabelecimento)
                                    select c.RazaoSocial).First();

                    IList<FechamentoCaixa> listaVerMeuCaixa = null;
                    listaVerMeuCaixa = _contextoFecharCaixa.GetAll<FechamentoCaixa>().Where(x => x.FaturamentoUsuario.Nome == NomeUsuario && x.DataLancamento.Date >= Datas.DataInicial && x.DataLancamento.Date <= Datas.DataFinal).OrderByDescending(x => x.DataLancamento).ToList();

                    return View("VerMeuCaixa", listaVerMeuCaixa);
                }
            }

            return View();
        }

       
        private long BuscaEstabelecimento(string nomeUsuario)
        {
            long codEstabelecimento = (from c in _contextoFecharCaixa.GetAll<CadastrarUsuario>()
                                       .Where(x => x.Nome == nomeUsuario)
                                       select c.EstabelecimentoTrabalho.Codigo).First();
            return codEstabelecimento;
        }



    }
}
