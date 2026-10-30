import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, Navigate, useLocation } from "react-router";
import { api, mensagemDe } from "../api";
import type { TokenResposta } from "../api";
import { useSessao } from "../sessao";

export default function Login() {
  const { sessao, entrar } = useSessao();
  const local = useLocation();
  const destino = (local.state as { de?: string } | null)?.de ?? "/encomendas";

  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState(false);
  const [verSenha, setVerSenha] = useState(false);

  useEffect(() => {
    document.title = "Entrar · Atelier Encanto";
  }, []);

  if (sessao) return <Navigate to={destino} replace />;

  async function enviar(e: FormEvent) {
    e.preventDefault();
    setErro("");
    setEnviando(true);
    try {
      const resposta = await api<TokenResposta>("/api/auth/login", {
        metodo: "POST",
        corpo: { email, senha },
      });
      entrar({
        token: resposta.token,
        nome: resposta.nome,
        papel: resposta.papel,
        expiraEm: resposta.expiraEm,
      });
    } catch (erroCapturado) {
      setErro(mensagemDe(erroCapturado));
      setEnviando(false);
    }
  }

  return (
    <section className="login">
      <div className="login-lateral">
        <h2>Boas memórias começam aqui.</h2>
        <p>
          Seus favoritos, suas encomendas e cada detalhe do próximo doce
          encontro.
        </p>
        <p className="login-selo">Feito à mão. Feito para você.</p>
      </div>
      <div className="login-area">
        <form className="formulario" onSubmit={enviar}>
          <div>
            <p className="eyebrow">Seu cantinho no atelier</p>
            <h1>Que bom ter você aqui.</h1>
            <p className="suave">
              Entre para acompanhar suas encomendas e celebrar momentos.
            </p>
          </div>

          <div>
            <label htmlFor="email">E-mail</label>
            <input
              id="email"
              type="email"
              placeholder="seuemail@exemplo.com"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div className="campo-senha">
              <input
                id="senha"
                type={verSenha ? "text" : "password"}
                placeholder="Sua senha"
                autoComplete="current-password"
                required
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
              />
              <button type="button" className="mostrar" onClick={() => setVerSenha((v) => !v)}>
                {verSenha ? "Ocultar" : "Mostrar"}
              </button>
            </div>

          {erro && (
            <p className="alerta alerta-erro" role="alert">
              {erro}
            </p>
          )}

          <button
            type="submit"
            className="botao botao-principal botao-largo"
            disabled={enviando}
          >
            {enviando ? "Entrando…" : "Entrar na minha conta"}
          </button>

          <p className="suave centro">
            Ainda não tem conta?{" "}
            <Link to="/cadastro" state={local.state}>
              Criar minha conta
            </Link>
          </p>

        </form>
      </div>
    </section>
  );
}
