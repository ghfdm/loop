// ============================================================
// Loop — protótipo de front-end (dados em memória, sem back-end)
// ============================================================

let nextId = 100;

const amenityInfo = {
  coberta:     { label: "Coberta",           icon: `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 11l9-6 9 6"/><path d="M5 10v9h14v-9"/></svg>` },
  cameras:     { label: "Câmeras 24h",       icon: `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="7" width="13" height="10" rx="2"/><path d="M16 10l5-3v10l-5-3"/></svg>` },
  portao:      { label: "Portão eletrônico", icon: `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="4" y="4" width="16" height="16" rx="2"/><path d="M9 4v16"/></svg>` },
  carregador:  { label: "Carregador EV",     icon: `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M13 2L4 14h6l-1 8 9-12h-6l1-8z"/></svg>` }
};

const vagas = [
  {
    id: 1, titulo: "Garagem coberta - Vila Madalena", tipo: "Garagem",
    endereco: "Rua Girassol, 210, Vila Madalena, São Paulo",
    horarios: "Seg-Sex 7h-22h", preco: 8, status: "aprovada",
    minha: false, top: 22, left: 30,
    distancia: 0.6, nota: 4.9, avaliacoes: 47,
    anfitriao: "Marina S.", amenidades: ["coberta", "cameras", "portao"]
  },
  {
    id: 2, titulo: "Vaga descoberta - Bela Vista", tipo: "Vaga",
    endereco: "Rua Major Sertório, 456, Bela Vista, São Paulo",
    horarios: "Todos os dias 6h-23h", preco: 6, status: "aprovada",
    minha: false, top: 55, left: 62,
    distancia: 1.2, nota: 4.6, avaliacoes: 23,
    anfitriao: "Diego A.", amenidades: ["cameras"]
  },
  {
    id: 3, titulo: "Garagem ampla - Moema", tipo: "Garagem",
    endereco: "Av. Ibirapuera, 900, Moema, São Paulo",
    horarios: "24h", preco: 10, status: "aprovada",
    minha: true, top: 70, left: 40,
    distancia: 2.4, nota: 4.8, avaliacoes: 61,
    anfitriao: "Você", amenidades: ["coberta", "portao", "carregador"]
  },
  {
    id: 4, titulo: "Vaga privativa - Santana", tipo: "Vaga",
    endereco: "Rua Voluntários da Pátria, 1500, Santana, São Paulo",
    horarios: "Seg-Sáb 8h-20h", preco: 5, status: "em_analise",
    minha: true, top: 15, left: 68,
    distancia: 3.1, nota: null, avaliacoes: 0,
    anfitriao: "Você", amenidades: ["portao"]
  }
];

const reservas = [
  {
    id: 1, titulo: "Garagem coberta - Vila Madalena",
    data: "05/09/2026 · 14h-17h", status: "confirmada"
  },
  {
    id: 2, titulo: "Vaga descoberta - Bela Vista",
    data: "28/08/2026 · 09h-11h", status: "concluida"
  },
  {
    id: 3, titulo: "Garagem ampla - Moema",
    data: "30/08/2026 · 19h-22h", status: "cancelada"
  }
];

const disputas = [
  {
    id: 1, titulo: "Garagem coberta - Vila Madalena",
    motivo: "Motorista relatou vaga ocupada na chegada.", status: "aberta"
  },
  {
    id: 2, titulo: "Vaga descoberta - Bela Vista",
    motivo: "Proprietário relatou atraso na devolução da vaga.", status: "resolvida"
  }
];

const statusLabel = {
  aprovada: "Aprovada",
  em_analise: "Em análise",
  pausada: "Pausada",
  rejeitada: "Rejeitada",
  confirmada: "Confirmada",
  concluida: "Concluída",
  cancelada: "Cancelada",
  aberta: "Aberta",
  resolvida: "Resolvida"
};

const statusBadgeClass = {
  aprovada: "aprovada",
  em_analise: "analise",
  pausada: "pausada",
  rejeitada: "rejeitada",
  confirmada: "aprovada",
  concluida: "concluida",
  cancelada: "cancelada",
  aberta: "analise",
  resolvida: "aprovada"
};

const disponibilidadeLabel = {
  aprovada: "Disponível agora",
  em_analise: "Aguardando aprovação",
  pausada: "Pausada",
  rejeitada: "Indisponível"
};

// ---------------- ícones auxiliares ----------------
const pinSvg = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 21s7-6.1 7-11.5A7 7 0 0 0 5 9.5C5 14.9 12 21 12 21z"/><circle cx="12" cy="9.5" r="2.4"/></svg>`;
const clockSvg = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" width="15" height="15"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3.5 2"/></svg>`;
const routeSvg = `<svg class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" width="15" height="15"><circle cx="6" cy="6" r="2.2"/><circle cx="18" cy="18" r="2.2"/><path d="M8 7l8 10"/></svg>`;
const starSvg = `<svg viewBox="0 0 24 24" fill="currentColor" width="14" height="14"><path d="M12 2l2.9 6.3 6.9.7-5.2 4.6 1.6 6.8L12 16.9l-6.2 3.5 1.6-6.8L2.2 9l6.9-.7z"/></svg>`;

function iconLg(svg) { return svg.replace("viewBox", 'width="14" height="14" viewBox'); }

function amenityRowHtml(list) {
  if (!list || !list.length) return "";
  return `<div class="amenity-row">${list.map(a => {
    const info = amenityInfo[a];
    if (!info) return "";
    return `<span class="amenity-chip">${info.icon}${info.label}</span>`;
  }).join("")}</div>`;
}

function ratingHtml(v) {
  if (!v.nota) {
    return `<div class="vaga-rating"><span class="count">Sem avaliações ainda</span></div>`;
  }
  return `<div class="vaga-rating">
      <span class="stars">${starSvg}</span>
      <strong>${v.nota.toFixed(1)}</strong>
      <span class="count">(${v.avaliacoes})</span>
      <span class="dot-sep">·</span>
      ${iconLg(routeSvg)}
      <span class="count">${v.distancia} km</span>
    </div>`;
}

function initials(name) {
  return name.split(" ").map(p => p[0]).slice(0, 2).join("").toUpperCase();
}

// ============================================================
// AUTENTICAÇÃO (em memória — sem back-end real)
// ============================================================
const roleLabel = { motorista: "Motorista", proprietario: "Proprietário" };

const users = [
  { nome: "Ana Motorista", email: "motorista@loop.com", senha: "123456", tipo: "motorista" },
  { nome: "Marina S.", email: "proprietario@loop.com", senha: "123456", tipo: "proprietario" }
];

let currentUser = null;

const authScreen = document.getElementById("auth-screen");
const appShell = document.getElementById("app-shell");

// ---------------- alternância entre abas Entrar / Criar conta ----------------
document.querySelectorAll(".auth-tab").forEach(tab => {
  tab.addEventListener("click", () => {
    document.querySelectorAll(".auth-tab").forEach(t => t.classList.toggle("is-active", t === tab));
    document.getElementById("login-form").classList.toggle("is-active", tab.dataset.tab === "entrar");
    document.getElementById("signup-form").classList.toggle("is-active", tab.dataset.tab === "cadastro");
    document.getElementById("login-error").textContent = "";
    document.getElementById("signup-error").textContent = "";
  });
});

// ---------------- login ----------------
document.getElementById("login-form").addEventListener("submit", (e) => {
  e.preventDefault();
  const email = document.getElementById("login-email").value.trim().toLowerCase();
  const senha = document.getElementById("login-senha").value;
  const user = users.find(u => u.email.toLowerCase() === email && u.senha === senha);

  if (!user) {
    document.getElementById("login-error").textContent = "E-mail ou senha inválidos.";
    return;
  }
  document.getElementById("login-error").textContent = "";
  enterApp(user);
});

// ---------------- botões de demonstração ----------------
document.querySelectorAll("[data-demo]").forEach(btn => {
  btn.addEventListener("click", () => {
    const user = users.find(u => u.tipo === btn.dataset.demo);
    if (user) enterApp(user);
  });
});

// ---------------- cadastro ----------------
document.getElementById("signup-form").addEventListener("submit", (e) => {
  e.preventDefault();
  const nome = document.getElementById("signup-nome").value.trim();
  const email = document.getElementById("signup-email").value.trim().toLowerCase();
  const senha = document.getElementById("signup-senha").value;
  const tipo = document.querySelector("input[name='tipo-conta']:checked").value;

  if (!nome || !email || senha.length < 4) return;

  if (users.some(u => u.email.toLowerCase() === email)) {
    document.getElementById("signup-error").textContent = "Já existe uma conta com esse e-mail.";
    return;
  }

  const novoUsuario = { nome, email, senha, tipo };
  users.push(novoUsuario);
  document.getElementById("signup-error").textContent = "";
  e.target.reset();
  enterApp(novoUsuario);
});

function enterApp(user) {
  currentUser = user;

  // liga as vagas fictícias "minhas" ao nome de quem acabou de entrar
  vagas.filter(v => v.minha).forEach(v => { v.anfitriao = user.nome; });

  document.getElementById("user-avatar").textContent = initials(user.nome);
  document.getElementById("user-name").textContent = user.nome;
  document.getElementById("user-role").textContent = roleLabel[user.tipo] || user.tipo;

  // proprietário e motorista veem menus relevantes ao seu perfil
  document.querySelectorAll(".nav-item[data-view='anunciar'], .nav-item[data-view='minhas-vagas']")
    .forEach(el => { el.style.display = user.tipo === "proprietario" ? "" : "none"; });
  document.querySelector(".nav-item[data-view='minhas-reservas']").style.display =
    user.tipo === "motorista" ? "" : "none";

  authScreen.style.display = "none";
  appShell.style.display = "";

  showView(user.tipo === "proprietario" ? "minhas-vagas" : "buscar");
  renderDestaques();
  renderMinhasVagas();
  renderMinhasReservas();
  renderAdministracao();
}

document.getElementById("logout-btn").addEventListener("click", () => {
  currentUser = null;
  document.getElementById("login-form").reset();
  document.getElementById("signup-form").reset();
  document.getElementById("login-error").textContent = "";
  appShell.style.display = "none";
  authScreen.style.display = "";
  document.querySelector(".auth-tab[data-tab='entrar']").click();
});

// ---------------- navegação entre telas ----------------
const navItems = document.querySelectorAll(".nav-item");
const views = document.querySelectorAll(".view");

function showView(name) {
  views.forEach(v => v.classList.toggle("is-active", v.id === "view-" + name));
  navItems.forEach(n => n.classList.toggle("is-active", n.dataset.view === name));
  window.scrollTo({ top: 0, behavior: "instant" in window ? "instant" : "auto" });
}

navItems.forEach(btn => {
  btn.addEventListener("click", () => showView(btn.dataset.view));
});

// ============================================================
// TELA: BUSCAR VAGAS (destaques)
// ============================================================
function renderDestaques() {
  const grid = document.getElementById("destaque-grid");
  const aprovadas = vagas.filter(v => v.status === "aprovada")
    .sort((a, b) => (b.nota || 0) - (a.nota || 0))
    .slice(0, 3);
  grid.innerHTML = aprovadas.map(vagaCardHtml).join("");
}

function vagaCardHtml(v) {
  return `
    <div class="vaga-card">
      <div class="vaga-thumb">
        <span class="tipo-tag">${v.tipo}</span>
        <span class="disp-tag"><span class="live-dot"></span>Disponível agora</span>
      </div>
      <div class="vaga-body">
        <h4>${v.titulo}</h4>
        <div class="vaga-meta">${iconLg(pinSvg)}${v.endereco}</div>
        <div class="vaga-meta">${clockSvg}${v.horarios}</div>
        ${ratingHtml(v)}
        ${amenityRowHtml(v.amenidades)}
        <div class="vaga-footer">
          <div class="vaga-price"><strong>R$ ${v.preco.toFixed(2)}</strong><span>por hora</span></div>
          <div class="vaga-host">
            <span class="host-avatar">${initials(v.anfitriao)}</span>
            ${v.anfitriao}
          </div>
        </div>
      </div>
    </div>`;
}

document.getElementById("home-search-btn").addEventListener("click", () => {
  const term = document.getElementById("home-search-input").value;
  document.getElementById("results-search-input").value = term;
  showView("resultados");
  renderResultados(term);
});

// ============================================================
// TELA: RESULTADOS
// ============================================================
function renderResultados(term = "") {
  const q = term.trim().toLowerCase();
  const lista = vagas.filter(v =>
    v.status === "aprovada" &&
    (q === "" || v.endereco.toLowerCase().includes(q) || v.titulo.toLowerCase().includes(q))
  );

  document.getElementById("results-count").textContent =
    `${lista.length} vaga${lista.length === 1 ? "" : "s"} disponível${lista.length === 1 ? "" : "eis"} para essa busca`;

  const listEl = document.getElementById("results-list");
  listEl.innerHTML = lista.length
    ? lista.map(resultItemHtml).join("")
    : `<div class="empty-state">Nenhuma vaga encontrada para essa busca.</div>`;

  renderMapMock(lista);
}

function resultItemHtml(v) {
  return `
    <div class="result-item">
      <div class="result-thumb"><span class="disp-tag"><span class="live-dot"></span>Livre</span></div>
      <div class="result-info">
        <h4>${v.titulo}</h4>
        <div class="vaga-meta">${iconLg(pinSvg)}${v.endereco}</div>
        <div class="vaga-meta">${clockSvg}${v.horarios}</div>
        ${amenityRowHtml(v.amenidades)}
      </div>
      <div class="result-price">
        <strong>R$ ${v.preco.toFixed(2)}</strong>
        <span>por hora</span>
        ${ratingHtml(v)}
      </div>
    </div>`;
}

function renderMapMock(lista) {
  const map = document.getElementById("map-mock");
  const bg = `
    <svg viewBox="0 0 100 100" preserveAspectRatio="none">
      <rect width="100" height="100" fill="#E7EBDE"/>
      <g stroke="#D1D6C2" stroke-width="1.4">
        <line x1="0" y1="20" x2="100" y2="20"/>
        <line x1="0" y1="48" x2="100" y2="48"/>
        <line x1="0" y1="76" x2="100" y2="76"/>
        <line x1="25" y1="0" x2="25" y2="100"/>
        <line x1="58" y1="0" x2="58" y2="100"/>
        <line x1="82" y1="0" x2="82" y2="100"/>
      </g>
    </svg>`;
  const pins = lista.map(v => `
    <div class="map-pin" style="top:${v.top}%; left:${v.left}%; color:#3F6B4A;" title="${v.titulo}">
      ${pinSvg}
    </div>`).join("");
  map.innerHTML = bg + pins;
}

document.getElementById("results-search-btn").addEventListener("click", () => {
  renderResultados(document.getElementById("results-search-input").value);
});
document.getElementById("results-search-input").addEventListener("keydown", (e) => {
  if (e.key === "Enter") renderResultados(e.target.value);
});

// ============================================================
// TELA: ANUNCIAR VAGA
// ============================================================
document.getElementById("anunciar-form").addEventListener("submit", (e) => {
  e.preventDefault();
  const titulo = document.getElementById("f-titulo").value.trim();
  const endereco = document.getElementById("f-endereco").value.trim();
  const preco = parseFloat(document.getElementById("f-preco").value);

  if (!titulo || !endereco || isNaN(preco)) return;

  const amenidades = Array.from(document.querySelectorAll("input[name='amenidade']:checked")).map(i => i.value);

  vagas.push({
    id: nextId++,
    titulo,
    tipo: document.getElementById("f-tipo").value,
    endereco,
    horarios: document.getElementById("f-horarios").value.trim() || "A combinar",
    preco,
    status: "em_analise",
    minha: true,
    top: 20 + Math.random() * 55,
    left: 20 + Math.random() * 55,
    distancia: +(Math.random() * 4 + 0.3).toFixed(1),
    nota: null,
    avaliacoes: 0,
    anfitriao: "Você",
    amenidades
  });

  e.target.reset();
  const feedback = document.getElementById("form-feedback");
  feedback.classList.add("is-visible");
  setTimeout(() => feedback.classList.remove("is-visible"), 3200);

  renderMinhasVagas();
  renderAdministracao();
});

// ============================================================
// TELA: MINHAS VAGAS
// ============================================================
function renderMinhasVagas() {
  const wrap = document.getElementById("minhas-vagas-list");
  const minhas = vagas.filter(v => v.minha);

  wrap.innerHTML = minhas.length
    ? minhas.map(v => `
      <div class="list-card">
        <div class="info">
          <h4>${v.titulo}</h4>
          <div class="vaga-meta">${iconLg(pinSvg)}${v.endereco} · R$ ${v.preco.toFixed(2)}/h</div>
          <div class="vaga-meta">${disponibilidadeLabel[v.status] || ""}${v.nota ? ` · ${v.nota.toFixed(1)} ★ (${v.avaliacoes})` : ""}</div>
          ${amenityRowHtml(v.amenidades)}
        </div>
        <span class="badge badge--${statusBadgeClass[v.status]}">${statusLabel[v.status]}</span>
        <div class="actions">
          ${v.status === "aprovada"
            ? `<button class="btn btn--outline btn--sm" data-action="pausar" data-id="${v.id}">Pausar</button>`
            : v.status === "pausada"
            ? `<button class="btn btn--outline btn--sm" data-action="ativar" data-id="${v.id}">Ativar</button>`
            : `<button class="btn btn--outline btn--sm" disabled>Aguardando análise</button>`}
          <button class="btn btn--danger btn--sm" data-action="excluir" data-id="${v.id}">Excluir</button>
        </div>
      </div>`).join("")
    : `<div class="empty-state">Você ainda não anunciou nenhuma vaga.</div>`;
}

document.getElementById("minhas-vagas-list").addEventListener("click", (e) => {
  const btn = e.target.closest("button[data-action]");
  if (!btn) return;
  const id = Number(btn.dataset.id);
  const vaga = vagas.find(v => v.id === id);
  if (!vaga) return;

  if (btn.dataset.action === "pausar") vaga.status = "pausada";
  if (btn.dataset.action === "ativar") vaga.status = "aprovada";
  if (btn.dataset.action === "excluir") {
    if (!confirm("Excluir este anúncio?")) return;
    vagas.splice(vagas.indexOf(vaga), 1);
  }
  renderMinhasVagas();
  renderDestaques();
  renderAdministracao();
});

// ============================================================
// TELA: MINHAS RESERVAS
// ============================================================
function renderMinhasReservas() {
  const wrap = document.getElementById("minhas-reservas-list");
  wrap.innerHTML = reservas.length
    ? reservas.map(r => `
      <div class="list-card">
        <div class="info">
          <h4>${r.titulo}</h4>
          <div class="vaga-meta">${clockSvg}${r.data}</div>
        </div>
        <span class="badge badge--${statusBadgeClass[r.status]}">${statusLabel[r.status]}</span>
        <div class="actions">
          ${r.status === "confirmada"
            ? `<button class="btn btn--danger btn--sm" data-action="cancelar" data-id="${r.id}">Cancelar</button>`
            : ""}
        </div>
      </div>`).join("")
    : `<div class="empty-state">Você ainda não fez nenhuma reserva.</div>`;
}

document.getElementById("minhas-reservas-list").addEventListener("click", (e) => {
  const btn = e.target.closest("button[data-action='cancelar']");
  if (!btn) return;
  const reserva = reservas.find(r => r.id === Number(btn.dataset.id));
  if (reserva) reserva.status = "cancelada";
  renderMinhasReservas();
});

// ============================================================
// TELA: ADMINISTRAÇÃO
// ============================================================
function renderAdministracao() {
  const pendentesWrap = document.getElementById("admin-pendentes-list");
  const pendentes = vagas.filter(v => v.status === "em_analise");

  pendentesWrap.innerHTML = pendentes.length
    ? pendentes.map(v => `
      <div class="admin-row">
        <div class="info">
          <h4>${v.titulo}</h4>
          <p>${v.endereco} · R$ ${v.preco.toFixed(2)}/h · ${v.horarios}</p>
          ${amenityRowHtml(v.amenidades)}
        </div>
        <div class="actions">
          <button class="btn btn--primary btn--sm" data-action="aprovar" data-id="${v.id}">Aprovar</button>
          <button class="btn btn--danger btn--sm" data-action="rejeitar" data-id="${v.id}">Rejeitar</button>
        </div>
      </div>`).join("")
    : `<div class="empty-state">Nenhuma vaga aguardando aprovação.</div>`;

  const disputasWrap = document.getElementById("admin-disputas-list");
  disputasWrap.innerHTML = disputas.map(d => `
    <div class="admin-row">
      <div class="info">
        <h4>${d.titulo}</h4>
        <p>${d.motivo}</p>
      </div>
      <div class="actions">
        <span class="badge badge--${statusBadgeClass[d.status]}">${statusLabel[d.status]}</span>
        ${d.status === "aberta"
          ? `<button class="btn btn--outline btn--sm" data-action="resolver" data-id="${d.id}">Marcar como resolvida</button>`
          : ""}
      </div>
    </div>`).join("");
}

document.getElementById("admin-pendentes-list").addEventListener("click", (e) => {
  const btn = e.target.closest("button[data-action]");
  if (!btn) return;
  const vaga = vagas.find(v => v.id === Number(btn.dataset.id));
  if (!vaga) return;
  if (btn.dataset.action === "aprovar") vaga.status = "aprovada";
  if (btn.dataset.action === "rejeitar") vaga.status = "rejeitada";
  renderAdministracao();
  renderMinhasVagas();
  renderDestaques();
});

document.getElementById("admin-disputas-list").addEventListener("click", (e) => {
  const btn = e.target.closest("button[data-action='resolver']");
  if (!btn) return;
  const disputa = disputas.find(d => d.id === Number(btn.dataset.id));
  if (disputa) disputa.status = "resolvida";
  renderAdministracao();
});

// ---------------- inicialização ----------------
renderDestaques();
renderResultados();
renderMinhasVagas();
renderMinhasReservas();
renderAdministracao();