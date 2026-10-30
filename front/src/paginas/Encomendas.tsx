import { useCallback, useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useSearchParams } from "react-router";
import { api, mensagemDe } from "../api";
import type { Encomenda, Pagina, Produto } from "../api";
import { dataHora, moeda } from "../formato";

export default function Encomendas() {
  const [params] = useSearchParams();

  const [produtos, setProdutos] = useState<Produto[]>([]);
  const [lista, setLista] = useState<Pagina<Encomenda> | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erroLista, setErroLista] = useState("");

  const [produtoId, setProdutoId] = useState(params.get("produto") ?? "");
  const [quantidade, setQuantidade] = useState("1");
  const [retirada, setRetirada] = useState("");
  const [observacoes, setObservacoes] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");

  useEffect(() => {
    document.title = "Minhas encomendas · Atelier Encanto";
  }, []);

  const carregar = useCallback(async () => {
    setErroLista("");
    try {
      setLista(await api<Pagina<Encomenda>>("/api/encomendas?pagina=1&tamanho=20"));
    } catch (e) {
      setErroLista(mensagemDe(e));
    } finally {
      setCarregando(false);
    }
  }, []);

  useEffect(() => {
    void carregar();
  }, [carregar]);

  useEffect(() => {
    api<Pagina<Produto>>("/api/produtos?pagina=1&tamanho=50")
      .then((resposta) => {
        setProdutos(resposta.itens);
        setProdutoId((atual) => atual || String(resposta.itens[0]?.id ?? ""));
      })
      .catch((e: unknown) => setErro(mensagemDe(e)));
  }, []);

  async function enviar(e: FormEvent) {
    e.preventDefault();
    setErro("");
    setSucesso("");
    if (!retirada) {
      setErro("Escolha a data e a hora da retirada.");
      return;
    }
    setEnviando(true);
    try {
      await api<Encomenda>("/api/encomendas", {
        metodo: "POST",
        corpo: {
          produtoId: Number(produtoId),
          quantidade: Number(quantidade),
          dataRetirada: new Date(retirada).toISOString(),
          observacoes: observacoes.trim() || null,
        },
      });
      setSucesso("Encomenda registrada! Você já pode acompanhá-la ao lado.");
      setObservacoes("");
      await carregar();
    } catch (erroCapturado) {
      setErro(mensagemDe(erroCapturado));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <section className="container secao">
      <p className="eyebrow">Sua área</p>
      <h1 className="titulo-pagina">Minhas encomendas</h1>

      <div className="duas-colunas">
        <form className="painel formulario" onSubmit={enviar}>
          <h2>Nova encomenda</h2>

          <div>
            <label htmlFor="produto">Produto</label>
            <select id="produto" required value={produtoId} onChange={(e) => setProdutoId(e.target.value)}>
              {produtos.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nome} — {moeda(p.preco)}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="quantidade">Quantidade</label>
            <input
              id="quantidade"
              type="number"
              min={1}
              max={100}
              required
              value={quantidade}
              onChange={(e) => setQuantidade(e.target.value)}
            />
          </div>

          <div>
            <label htmlFor="retirada">Data e hora da retirada</label>
            <input
              id="retirada"
              type="datetime-local"
              required
              value={retirada}
              onChange={(e) => setRetirada(e.target.value)}
            />
            <p className="dica">Cada data tem um limite de encomendas.</p>
          </div>

          <div>
            <label htmlFor="observacoes">Observações (opcional)</label>
            <textarea
              id="observacoes"
              rows={3}
              maxLength={500}
              value={observacoes}
              onChange={(e) => setObservacoes(e.target.value)}
            />
          </div>

          {erro && (
            <p className="alerta alerta-erro" role="alert">
              {erro}
            </p>
          )}
          {sucesso && (
            <p className="alerta alerta-ok" role="status">
              {sucesso}
            </p>
          )}

          <button type="submit" className="botao botao-principal botao-largo" disabled={enviando}>
            {enviando ? "Enviando…" : "Fazer encomenda"}
          </button>
        </form>

        <div>
          {carregando && (
            <div className="pilha" aria-busy="true">
              <div className="esqueleto sk-bloco" />
              <div className="esqueleto sk-bloco" />
            </div>
          )}

          {!carregando && erroLista && (
            <p className="alerta alerta-erro" role="alert">
              {erroLista}
            </p>
          )}

          {!carregando && !erroLista && lista && lista.itens.length === 0 && (
            <div className="vazio">
              <h3>Nenhuma encomenda ainda</h3>
              <p>Quando você fizer a primeira, ela aparece aqui.</p>
            </div>
          )}

          {!carregando && lista && lista.itens.length > 0 && (
            <ul className="pilha lista-limpa">
              {lista.itens.map((item) => (
                <li className="painel encomenda" key={item.id}>
                  <div>
                    <h3>{item.produtoNome}</h3>
                    <p className="suave">
                      {item.quantidade} {item.quantidade === 1 ? "unidade" : "unidades"} · retirada em{" "}
                      {dataHora(item.dataRetirada)}
                    </p>
                    {item.observacoes && <p className="suave">“{item.observacoes}”</p>}
                  </div>
                  <div className="encomenda-lado">
                    <strong>{moeda(item.valorTotal)}</strong>
                    <span className={item.status === "Cancelada" ? "status status-cancelada" : "status"}>
                      {item.status}
                    </span>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
}