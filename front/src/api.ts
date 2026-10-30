const BASE = ((import.meta.env.VITE_API_URL as string | undefined) ?? "").replace(/\/$/, "");
const CHAVE = "confeitaria.sessao";

export interface Sessao {
  token: string;
  nome: string;
  papel: string;
  expiraEm: string;
}

export interface TokenResposta {
  token: string;
  expiraEm: string;
  nome: string;
  papel: string;
}

export interface Produto {
  id: number;
  nome: string;
  descricao: string;
  preco: number;
  categoria: string;
  ativo: boolean;
}

export interface Encomenda {
  id: number;
  produtoId: number;
  produtoNome: string;
  usuarioId: number;
  quantidade: number;
  valorTotal: number;
  dataRetirada: string;
  observacoes: string;
  status: string;
  criadoEm: string;
}

export interface Pagina<T> {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalPaginas: number;
}

export class ErroApi extends Error {
  status: number;

  constructor(status: number, mensagem: string) {
    super(mensagem);
    this.status = status;
  }
}

let aoExpirar: (() => void) | null = null;

export function registrarAoExpirar(funcao: () => void) {
  aoExpirar = funcao;
}

export function lerSessao(): Sessao | null {
  try {
    const bruto = localStorage.getItem(CHAVE);
    if (!bruto) return null;
    const sessao = JSON.parse(bruto) as Sessao;
    if (new Date(sessao.expiraEm).getTime() <= Date.now()) {
      localStorage.removeItem(CHAVE);
      return null;
    }
    return sessao;
  } catch {
    return null;
  }
}

export function guardarSessao(sessao: Sessao) {
  localStorage.setItem(CHAVE, JSON.stringify(sessao));
}

export function limparSessao() {
  localStorage.removeItem(CHAVE);
}

export function mensagemDe(erro: unknown): string {
  return erro instanceof Error ? erro.message : "Algo deu errado. Tente novamente.";
}

async function mensagemDeErro(resposta: Response): Promise<string> {
  try {
    const dados = (await resposta.json()) as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };
    if (dados.errors) {
      const lista = Object.values(dados.errors).flat();
      if (lista.length > 0) return lista.join(" ");
    }
    return dados.detail ?? dados.title ?? `Erro ${resposta.status}.`;
  } catch {
    if (resposta.status === 401) return "Entre na sua conta para continuar.";
    if (resposta.status === 403) return "Você não tem permissão para fazer isso.";
    return `Erro ${resposta.status}.`;
  }
}

interface Opcoes {
  metodo?: string;
  corpo?: unknown;
}

export async function api<T>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  if (!BASE) throw new ErroApi(0, "A variável VITE_API_URL não está configurada.");

  const sessao = lerSessao();
  const cabecalhos: Record<string, string> = { Accept: "application/json" };
  if (opcoes.corpo !== undefined) cabecalhos["Content-Type"] = "application/json";
  if (sessao) cabecalhos.Authorization = `Bearer ${sessao.token}`;

  let resposta: Response;
  try {
    resposta = await fetch(`${BASE}${caminho}`, {
      method: opcoes.metodo ?? "GET",
      headers: cabecalhos,
      body: opcoes.corpo !== undefined ? JSON.stringify(opcoes.corpo) : undefined,
    });
  } catch {
    throw new ErroApi(
      0,
      "Não foi possível falar com a API. Ela pode estar acordando (leva cerca de 1 minuto): tente de novo.",
    );
  }

  if (!resposta.ok) {
    if (resposta.status === 401 && sessao) aoExpirar?.();
    throw new ErroApi(resposta.status, await mensagemDeErro(resposta));
  }

  if (resposta.status === 204) return undefined as T;
  return (await resposta.json()) as T;
}