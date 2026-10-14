import { useState, useEffect } from "react";

function App() {
  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [search, setSearch] = useState("");
  const [token, setToken] = useState("");
  const [role, setRole] = useState("");
  const [id, setId] = useState(0);
  const [pname, setPname] = useState("");
  const [description, setDescription] = useState("");
  const [price, setPrice] = useState("");
  const [stock, setStock] = useState("");

  const api = "http://localhost:5279/api";

  function request(url, method = "GET", data = null) {
    return fetch(api + url, {
      method,
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + token
      },
      ...(data && { body: JSON.stringify(data) })
    });
  }

  function getProducts() {
    request("/products").then(r => r.json()).then(setProducts);
  }

  function getOrders() {
    request("/orders").then(r => r.json()).then(setOrders);
  }

  useEffect(() => {
    getProducts();
  }, []);

  function register() {
    request("/auth/register", "POST", { name, email, password })
      .then(r => alert(r.ok ? "Kontot skapades!" : "Registreringen misslyckades."));
  }

  function login() {
    request("/auth/login", "POST", { email, password })
      .then(r => {
        if (!r.ok) throw new Error();
        return r.json();
      })
      .then(d => {
        setToken(d.token);
        setRole(d.role);
        alert("Du är inloggad!");
      })
      .catch(() => alert("Fel e-post eller lösenord."));
  }

  function order(id) {
    request(`/orders?productId=${id}&quantity=1`, "POST")
      .then(r => {
        alert(r.ok ? "Beställningen skapades!" : "Beställningen misslyckades.");
        if (r.ok) {
          getProducts();
          getOrders();
        }
      });
  }

  function saveProduct() {
    const p = { name: pname, description, price: Number(price), stock: Number(stock) };
    request("/products" + (id ? "/" + id : ""), id ? "PUT" : "POST", p)
      .then(r => {
        alert(r.ok ? "Produkten sparades!" : "Det gick inte att spara.");
        if (r.ok) {
          setId(0);
          setPname("");
          setDescription("");
          setPrice("");
          setStock("");
          getProducts();
        }
      });
  }

  return (
    <main>
      <h1>JENSENOnline</h1>

      {!token ? <>
        <h2>Registrera eller logga in</h2>
        <input placeholder="Namn" onChange={e => setName(e.target.value)} />
        <input placeholder="E-post" onChange={e => setEmail(e.target.value)} />
        <input type="password" placeholder="Lösenord" onChange={e => setPassword(e.target.value)} />
        <button onClick={register}>Registrera</button>
        <button onClick={login}>Logga in</button>
      </> : <>
        <p>Inloggad: {role}</p>
        <button onClick={() => { setToken(""); setRole(""); setOrders([]); }}>Logga ut</button>
      </>}

      <h2>Produkter</h2>
      <input placeholder="Sök produkt" onChange={e => setSearch(e.target.value)} />

      {products.filter(p => p.name.toLowerCase().includes(search.toLowerCase())).map(p => (
        <article key={p.id}>
          <h3>{p.name}</h3>
          <p>{p.description}</p>
          <p>Pris: {p.price} kr | Lager: {p.stock}</p>
          {token && <button onClick={() => order(p.id)}>Beställ</button>}
          {role === "Admin" && <button onClick={() => {
            setId(p.id);
            setPname(p.name);
            setDescription(p.description);
            setPrice(p.price);
            setStock(p.stock);
          }}>Ändra</button>}
        </article>
      ))}

      {role === "Admin" && <>
        <h2>{id ? "Ändra produkt" : "Lägg till produkt"}</h2>
        <input placeholder="Produktnamn" value={pname} onChange={e => setPname(e.target.value)} />
        <input placeholder="Beskrivning" value={description} onChange={e => setDescription(e.target.value)} />
        <input type="number" placeholder="Pris" value={price} onChange={e => setPrice(e.target.value)} />
        <input type="number" placeholder="Lager" value={stock} onChange={e => setStock(e.target.value)} />
        <button onClick={saveProduct}>Spara</button>
      </>}

      {token && <>
        <h2>Mina beställningar</h2>
        <button onClick={getOrders}>Visa beställningar</button>
        {orders.map(o => <p key={o.id}>Order {o.id}: Produkt {o.productId}, antal {o.quantity}</p>)}
      </>}
    </main>
  );
}

export default App;