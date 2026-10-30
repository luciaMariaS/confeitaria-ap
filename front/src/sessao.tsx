import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { guardarSessao, lerSessao, limparSessao, registrarAoExpirar } from "./api";
import type { Sessao } from "./api";

interface ContextoSessao {
  sessao: Sessao | null;
  aviso: string;
  entrar: (sessao: Sessao) => void;
  sair: () => void;
}

const Contexto = createContext<ContextoSessao | null>(null);
const AVISO = "Sua sessão expirou. Entre novamente para continuar.";

export function SessaoProvider({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(() => lerSessao());
  const [aviso, setAviso] = useState("");

  const entrar = useCallback((nova: Sessao) => {
    guardarSessao(nova);
    setAviso("");
    setSessao(nova);
  }, []);

  const sair = useCallback(() => {
    limparSessao();
    setSessao(null);
  }, []);

  const expirar = useCallback(() => {
    limparSessao();
    setSessao(null);
    setAviso(AVISO);
  }, []);

  useEffect(() => {
    registrarAoExpirar(expirar);
  }, [expirar]);

  useEffect(() => {
    if (!sessao) return;
    const restante = new Date(sessao.expiraEm).getTime() - Date.now();
    const id = window.setTimeout(expirar, Math.max(restante, 0));
    return () => window.clearTimeout(id);
  }, [sessao, expirar]);

  const valor = useMemo(() => ({ sessao, aviso, entrar, sair }), [sessao, aviso, entrar, sair]);

  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useSessao(): ContextoSessao {
  const contexto = useContext(Contexto);
  if (!contexto) throw new Error("useSessao precisa estar dentro de SessaoProvider.");
  return contexto;
}