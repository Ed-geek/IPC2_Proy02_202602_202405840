const $ = id => document.getElementById(id);
const api = "/api/catalog";

// ── Tabs ──
document.querySelectorAll("nav button").forEach(b => {
  b.onclick = () => {
    document.querySelectorAll(".tab").forEach(t => t.classList.remove("active"));
    $(b.dataset.tab).classList.add("active");
  };
});
document.querySelector("nav button").click();

// ── Helper ──
async function call(url, opts) {
  const r = await fetch(url, opts);
  const j = await r.json().catch(() => ({}));
  if (!r.ok) throw new Error(j.message || r.statusText);
  return j;
}

// ── Init ──
async function initSystem() {
  const j = await call(`${api}/init`, { method: "POST" });
  $("initOut").textContent = j.message;
}

// ── XML ──
async function uploadXml() {
  const f = $("xmlFile").files[0];
  if (!f) { $("xmlOut").textContent = "Selecciona un archivo."; return; }

  const fd = new FormData();
  fd.append("file", f);

  try {
    const j = await call(`${api}/load-xml`, { method: "POST", body: fd });
    const lines = [
      "Categorias agregadas : " + j.categoriesAdded,
      "Categorias rechazadas: " + j.categoriesRejected,
      "Libros agregados     : " + j.booksAdded,
      "Libros rechazados    : " + j.booksRejected,
      "",
      "Detalle:"
    ];
    const details = j.details || [];
    for (let i = 0; i < details.length; i++) lines.push(details[i]);
    $("xmlOut").textContent = lines.join("\n");
  } catch (e) {
    $("xmlOut").textContent = e.message;
  }
}

// ── Categorias ──
async function addCategory() {
  try {
    const j = await call(`${api}/categories`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name:   $("catName").value,
        parent: $("catParent").value || null
      })
    });
    alert(j.message);
  } catch (e) { alert(e.message); }
}

async function showTree() {
  const start = $("catStart").value || "";
  try {
    const j = await call(`${api}/categories/dot?start=${encodeURIComponent(start)}`);
    $("treeOut").innerHTML = '<img src="' + j.url + '" alt="Arbol">';
  } catch (e) { alert(e.message); }
}

async function showCategoryBooks() {
  const name = $("catBooks").value;
  if (!name) return;
  try {
    const j = await call(`${api}/categories/${encodeURIComponent(name)}/books/dot`);
    $("treeOut").innerHTML = '<img src="' + j.url + '" alt="Libros">';
  } catch (e) { alert(e.message); }
}

// ── Libros ──
async function addBook() {
  try {
    const j = await call(`${api}/books`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        isbn:     parseInt($("isbn").value, 10),
        title:    $("title").value,
        author:   $("author").value,
        category: $("category").value
      })
    });
    alert(j.message);
  } catch (e) { alert(e.message); }
}

async function findBook() {
  try {
    const isbn = $("searchIsbn").value;
    const j = await call(`${api}/books/${isbn}`);
    $("booksOut").textContent = JSON.stringify(j, null, 2);
  } catch (e) { $("booksOut").textContent = e.message; }
}

async function deleteBook() {
  try {
    const isbn = $("searchIsbn").value;
    const j = await call(`${api}/books/${isbn}`, { method: "DELETE" });
    $("booksOut").textContent = j.message;
  } catch (e) { $("booksOut").textContent = e.message; }
}

async function getMin() {
  try {
    const j = await call(`${api}/books/min`);
    $("booksOut").textContent = JSON.stringify(j, null, 2);
  } catch (e) { $("booksOut").textContent = e.message; }
}

async function getMax() {
  try {
    const j = await call(`${api}/books/max`);
    $("booksOut").textContent = JSON.stringify(j, null, 2);
  } catch (e) { $("booksOut").textContent = e.message; }
}

// ═══════════════════════════════════════════════════════
// ENLACE DE BOTONES  
// ═══════════════════════════════════════════════════════
function bind(id, fn) {
  const el = document.getElementById(id);
  if (el) {
    el.addEventListener("click", fn);
  } else {
    console.warn("Boton no encontrado:", id);
  }
}

bind("btnInit",         initSystem);
bind("btnUpload",       uploadXml);
bind("btnAddCat",       addCategory);
bind("btnShowTree",     showTree);
bind("btnShowCatBooks", showCategoryBooks);
bind("btnAddBook",      addBook);
bind("btnFind",         findBook);
bind("btnDelete",       deleteBook);
bind("btnMin",          getMin);
bind("btnMax",          getMax);

console.log("app.js cargado correctamente");