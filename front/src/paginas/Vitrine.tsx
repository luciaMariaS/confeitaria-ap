import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useSearchParams } from "react-router";
import { api, mensagemDe } from "../api";
import type { Pagina, Produto } from "../api";
import { moeda } from "../formato";

const TAMANHO = 8;

const FOTOS: Record<string, string> = {
  Bolo: "/img/bolo.png",
  Torta: "/img/bolo-cacau.png",
  Sobremesa: "/img/bolo.png",
  Doce: "/img/brigadeiro.png",
  Cookie: "/img/caixa-afeto.png",
  Cupcake: "/img/caixa-afeto.png",
};

const FOTO_POR_ID: Record<number, string> = {
  1: "/img/bolo-cacau.png",
  6: "/img/brigadeiro.png",
  8: "/img/caixa-afeto.png",
};

export default function Vitrine() {
  const [params, setParams] = useSearchParams();
  const busca = params.get("busca") ?? "";
  const pagina = Math.max(Number(params.get("pagina")) || 1, 1);

  const [texto, setTexto] = useState(busca);
  const [dados, setDados] = useState<Pagina<Produto> | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState("");
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    document.title = "Catálogo · Atelier Encanto";
  }, []);

  useEffect(() => {
    setTexto(busca);
  }, [busca]);

  useEffect(() => {
    let ativo = true;
    setCarregando(true);
    setErro("");
    api<Pagina<Produto>>(
      `/api/produtos?busca=${encodeURIComponent(busca)}&pagina=${pagina}&tamanho=${TAMANHO}`,
    )
      .then((resposta) => {
        if (ativo) setDados(resposta);
      })
      .catch((e: unknown) => {
        if (ativo) setErro(mensagemDe(e));
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });
    return () => {
      ativo = false;
    };
  }, [busca, pagina, tentativa]);

  function pesquisar(e: FormEvent) {
    e.preventDefault();
    const proximo = new URLSearchParams();
    if (texto.trim()) proximo.set("busca", texto.trim());
    setParams(proximo);
  }

  function limpar() {
    setTexto("");
    setParams(new URLSearchParams());
  }

  function irPara(n: number) {
    const proximo = new URLSearchParams(params);
    if (n <= 1) proximo.delete("pagina");
    else proximo.set("pagina", String(n));
    setParams(proximo);
  }

  return (
    <>
      <section className="hero">
        <div className="container hero-grade">
          <div>
            <p className="eyebrow">Feito para celebrar</p>
            <h1>Um doce para cada história.</h1>
            <p className="lead">
              Artesanal, do primeiro ingrediente ao último detalhe. Bolos, doces
              e presentes para transformar encontros em boas memórias.
            </p>
            <div className="hero-acoes">
              <a className="botao botao-principal" href="#catalogo">
                Encontre seu bolo
              </a>
              <Link className="botao botao-claro" to="/encomendas">
                Fazer uma encomenda
              </Link>
            </div>
          </div>
          <img
            className="hero-arte"
            src="/img/hero.png"
            alt="Bolo decorado com frutas vermelhas"
          />
        </div>
      </section>

      <section className="container secao" id="catalogo">
        <div className="secao-topo">
          <div>
            <p className="eyebrow">Escolha seu próximo momento</p>
            <h2>Os favoritos do atelier</h2>
          </div>
          <form className="busca" onSubmit={pesquisar} role="search">
            <label className="so-leitor" htmlFor="busca">
              Buscar produtos
            </label>
            <input
              id="busca"
              type="search"
              placeholder="Busque bolos, doces e presentes"
              value={texto}
              onChange={(e) => setTexto(e.target.value)}
            />
            <button type="submit" className="botao botao-principal">
              Buscar
            </button>
          </form>
        </div>

        {dados && !carregando && !erro && (
          <p className="contagem">
            {dados.total} {dados.total === 1 ? "produto" : "produtos"}
            {busca && ` para “${busca}”`}
          </p>
        )}

        {carregando && (
          <div
            className="grade"
            aria-busy="true"
            aria-label="Carregando produtos"
          >
            {Array.from({ length: 4 }, (_, i) => (
              <div className="cartao" key={i}>
                <div className="esqueleto sk-arte" />
                <div className="cartao-corpo">
                  <div className="esqueleto sk-linha curta" />
                  <div className="esqueleto sk-linha" />
                  <div className="esqueleto sk-linha media" />
                </div>
              </div>
            ))}
          </div>
        )}

        {!carregando && erro && (
          <div className="vazio">
            <p className="alerta alerta-erro" role="alert">
              {erro}
            </p>
            <button
              type="button"
              className="botao botao-principal"
              onClick={() => setTentativa((t) => t + 1)}
            >
              Tentar de novo
            </button>
          </div>
        )}

        {!carregando && !erro && dados && dados.itens.length === 0 && (
          <div className="vazio">
            <h3>Nada encontrado</h3>
            <p>Não achamos nenhum doce para essa busca. Tente outro termo.</p>
            <button
              type="button"
              className="botao botao-principal"
              onClick={limpar}
            >
              Limpar busca
            </button>
          </div>
        )}

        {!carregando && !erro && dados && dados.itens.length > 0 && (
          <>
            <div className="grade">
              {dados.itens.map((produto) => (
                <article className="cartao" key={produto.id}>
                  <img
                    className="cartao-arte"
                   src={FOTO_POR_ID[produto.id] ?? FOTOS[produto.categoria] ?? "/img/bolo.png"}
                    alt={produto.nome}
                    loading="lazy"
                    onError={(e) => {
                      e.currentTarget.onerror = null;
                      e.currentTarget.src =
                        FOTOS[produto.categoria] ?? "/img/bolo.png";
                    }}
                  />

                  <div className="cartao-corpo">
                    <span className="etiqueta">{produto.categoria}</span>
                    <h3>{produto.nome}</h3>
                    <p>{produto.descricao}</p>
                    <div className="cartao-rodape">
                      <strong className="preco">{moeda(produto.preco)}</strong>
                      <Link
                        className="botao botao-claro"
                        to={`/encomendas?produto=${produto.id}`}
                      >
                        Encomendar
                      </Link>
                    </div>
                  </div>
                </article>
              ))}
            </div>

            {dados.totalPaginas > 1 && (
              <nav className="paginacao" aria-label="Paginação">
                <button
                  type="button"
                  className="botao botao-claro"
                  disabled={dados.pagina <= 1}
                  onClick={() => irPara(dados.pagina - 1)}
                >
                  Anterior
                </button>
                <span>
                  Página {dados.pagina} de {dados.totalPaginas}
                </span>
                <button
                  type="button"
                  className="botao botao-claro"
                  disabled={dados.pagina >= dados.totalPaginas}
                  onClick={() => irPara(dados.pagina + 1)}
                >
                  Próxima
                </button>
              </nav>
            )}
          </>
        )}
      </section>
    </>
  );
}
