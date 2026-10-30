const moedaBR = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export const moeda = (valor: number) => moedaBR.format(valor);

export const dataHora = (iso: string) =>
  new Date(iso).toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });