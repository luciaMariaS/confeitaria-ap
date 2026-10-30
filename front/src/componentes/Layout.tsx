import { Link, NavLink, Outlet } from "react-router";
import { useSessao } from "../sessao";

export default function Layout() {
  const { sessao, aviso, sair } = useSessao();

  return (
    <>
      <a className="pular" href="#conteudo">
        Pular para o conteúdo
      </a>
      <div className="faixa-topo">Doces artesanais feitos por encomenda</div>

      <header className="cabecalho">
        <div className="container cabecalho-interno">
          <Link to="/" className="marca">
            <span className="marca-nome">Atelier Encanto</span>
            <span className="marca-sub">Confeitaria artesanal</span>
          </Link>
          <nav className="menu" aria-label="Principal">
            <NavLink to="/" end>
              Catálogo
            </NavLink>
            <NavLink to="/encomendas">Minhas encomendas</NavLink>
          </nav>
          <div className="usuario">
            {sessao ? (
              <>
                <span>Olá, {sessao.nome.split(" ")[0]}</span>
                <button type="button" className="botao botao-claro" onClick={sair}>
                  Sair
                </button>
              </>
            ) : (
              <Link className="botao botao-claro" to="/login">
                Entrar
              </Link>
            )}
          </div>
        </div>
      </header>

      <main id="conteudo">
        {aviso && (
          <div className="container">
            <p className="alerta alerta-info" role="status">
              {aviso}
            </p>
          </div>
        )}
        <Outlet />
      </main>

      <footer className="rodape">
        <div className="container rodape-interno">
          <div>
            <p className="rodape-marca">Atelier Encanto</p>
            <p className="rodape-texto">
              Pequenos lotes, ingredientes de verdade e carinho em cada encomenda.
            </p>
          </div>
          <p className="rodape-aviso">
            Projeto escolar — empresa fictícia, não é um serviço real. Não use seu e-mail nem sua
            senha de verdade.
          </p>
        </div>
      </footer>
    </>
  );
}