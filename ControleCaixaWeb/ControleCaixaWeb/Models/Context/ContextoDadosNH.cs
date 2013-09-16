using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;

namespace ControleCaixaWeb.Models.Context
{
    public class ContextoDadosNH:IContextoDados
    {

        /// <summary>
        /// declara uma constante para a conexão
        /// </summary>
        private static readonly string CONNECTION_STRING_KEY = "conexao";

        /// <summary>
        /// Configura a conexão com o banco de dados
        /// usando configuração usando Fluently 
        /// e usando a lambda expression,
        /// mapeamento objeto relacional de uma classe chamada usuarios
        /// usando lambda expression da classe usuario
        /// </summary>
        private static readonly Configuration configuracao = Fluently.Configure( )
                         .Database(PostgreSQLConfiguration.PostgreSQL82.ConnectionString(c => c.FromConnectionStringWithKey(CONNECTION_STRING_KEY)))
                         .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ContextoDadosNH>( ))
                         .BuildConfiguration( );


        private static readonly ISessionFactory fabricaDeSessoes = configuracao.BuildSessionFactory( );
        private readonly ISession _sessao;

        /// <summary>
        /// Toda vez que a classe é instanciada uma nova sessão é aberta
        /// </summary>
        public ContextoDadosNH( )
        {
            _sessao = fabricaDeSessoes.OpenSession( );
        }
        /// <summary>
        /// obtem uma lista com todos os objetos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>( ) where T : class
        {
            return this._sessao.Query<T>( );
        }
        /// <summary>
        /// retorna o objeto cujo o id é passado como parametro
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(long id) where T : class
        {
            return this._sessao.Get<T>(id);
        }
        /// <summary>
        /// Adiciona um objeto no banco de dados
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Add<T>(T entity) where T : class
        {
            this._sessao.Save(entity);
        }
        /// <summary>
        /// Deleta um objeto do banco de dados
        /// o objeto é passado previamete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Delete<T>(T entity) where T : class
        {
            this._sessao.Delete(entity);
            
        }

        /// <summary>
        /// este metodo tem a funcao de atualizar uma classe e consiste em um espelho no banco de dados
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(T entity) where T : class
        {
            if (entity != null)
            {
                this._sessao.Update(entity);
            }
            else
            {
                this._sessao.Dispose( );
            }

        }
        /// <summary>
        /// vai ser responsavel, por pegar o script de geracao do banco
        /// e este vai executar os script do banco de dados
        /// os parametros  mostrar console
        /// false, true, false (banco, console)
        /// </summary>
        public void setup( )
        {
            SchemaExport exporter = new SchemaExport(configuracao);
            exporter.Execute(false, true, false, this._sessao.Connection, global::System.Console.Out);
        }
        /// <summary>
        /// salva as configurações em cada metodo
        /// </summary>
        public void SaveChanges( )
        {
            this._sessao.Flush( );
        }
        /// <summary>
        /// libera o objeto da memoria
        /// </summary>
        public void Dispose( )
        {
            this._sessao.Dispose( );
        }
    }
}