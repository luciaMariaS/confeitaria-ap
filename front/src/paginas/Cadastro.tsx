import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, Navigate, useLocation } from "react-router";
import { api, mensagemDe } from "../api";
import type { TokenResposta } from "../api";
import { useSessao } from "../sessao";

export default function Cadastro() {
  const { sessao, entrar } = useSessao();
  const local = useLocation();
  const destino = (local.state as { de?: string } | null)?.de ?? "/encomendas";

  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [repeticao, setRepeticao] = useState("");
  const [erro, setErro] = useState("");
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    document.title = "Criar conta · Atelier Encanto";
  }, []);

  if (sessao) return <Navigate to={destino} replace />;

  async function enviar(e: FormEvent) {
    e.preventDefault();
    setErro("");
    if (senha.length < 8) {
      setErro("A senha precisa ter pelo menos 8 caracteres.");
      return;
    }
    if (senha !== repeticao) {
      setErro("As senhas não são iguais.");
      return;
    }
    setEnviando(true);
    try {
      const resposta = await api<TokenResposta>("/api/auth/registro", {
        metodo: "POST",
        corpo: { nome, email, senha },
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
        <h2>Seu lugar no atelier.</h2>
        <p>Crie sua conta para fazer encomendas e acompanhar cada uma delas.</p>
      </div>
      <div className="login-area">
        <form className="formulario" onSubmit={enviar}>
          <div>
            <p className="eyebrow">Novo por aqui</p>
            <h1>Criar minha conta</h1>
            <p className="suave">Projeto escolar: use um e-mail inventado, não o seu de verdade.</p>
          </div>

          <div>
            <label htmlFor="nome">Nome</label>
            <input id="nome" required minLength={2} autoComplete="name" value={nome} onChange={(e) => setNome(e.target.value)} />
          </div>

          <div>
            <label htmlFor="email">E-mail</label>
            <input id="email" type="email" required autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>

          <div>
            <label htmlFor="senha">Senha</label>
            <input id="senha" type="password" required minLength={8} autoComplete="new-password" value={senha} onChange={(e) => setSenha(e.target.value)} />
            <p className="dica">Mínimo de 8 caracteres.</p>
          </div>

          <div>
            <label htmlFor="repeticao">Repita a senha</label>
            <input id="repeticao" type="password" required autoComplete="new-password" value={repeticao} onChange={(e) => setRepeticao(e.target.value)} />
          </div>

          {erro && (
            <p className="alerta alerta-erro" role="alert">
              {erro}
            </p>
          )}

          <button type="submit" className="botao botao-principal botao-largo" disabled={enviando}>
            {enviando ? "Criando…" : "Criar minha conta"}
          </button>

          <p className="suave centro">
            Já tem conta? <Link to="/login" state={local.state}>Entrar</Link>
          </p>
        </form>
      </div>
    </section>
  );
}