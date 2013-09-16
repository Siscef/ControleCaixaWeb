using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ControleCaixaWeb.Models
{
    public class Papel
    {
        public virtual long Codigo { get; set; }
        [Required(ErrorMessage="O campo Nome Papel é Obrigatório")]
        public virtual string NomePapel { get; set; }
        public virtual IList<CadastrarUsuario> FuncoesDoUsuario { get; set; }
        public virtual IList<Usuario> FuncaoDoUsuario { get; set; }

    }
}