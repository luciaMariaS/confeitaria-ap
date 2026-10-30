import { Link, Navigate, Outlet, Route, Routes, useLocation } from "react-router";
import Layout from "./componentes/Layout";
import Cadastro from "./paginas/Cadastro";
import Encomendas from "./paginas/Encomendas";
import Login from "./paginas/Login";
import Vitrine from "./paginas/Vitrine";
import { useSessao } from "./sessao";


function Protegida() {
  const { sessao } = useSessao();
  const local = useLocation();
  if (!sessao) {
    return <Navigate to="/login" replace state={{ de: local.pathname + local.search }} />;
  }
  return <Outlet />;
}

function NaoEncontrada() {
  return (
    <section className="container secao vazio">
      <h1>Página não encontrada</h1>
      <p>O endereço que você abriu não existe.</p>
      <Link className="botao botao-principal" to="/">
        Voltar ao catálogo
      </Link>
    </section>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Vitrine />} />
        <Route path="login" element={<Login />} />
        <Route path="cadastro" element={<Cadastro />} />
        <Route element={<Protegida />}>
          <Route path="encomendas" element={<Encomendas />} />
        </Route>
        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  );
}