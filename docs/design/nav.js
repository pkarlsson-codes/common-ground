// Site navigation for the static pages in docs/design/.
// Every page loads this file with <script src="./nav.js" defer>. It adds the
// header bar, a site sidebar with the current page's sections nested under it
// (built from each <section id> and its <h2>), previous/next links, and fills
// the context cards on the home page.
// A plain script, not a module, so it also works when a page is opened from disk.
//
// To add a page: add it to SITE below. Set planned: true for a page that
// isn't written yet; it shows in the sidebar and on the home page without a link.
// `about` is the one-line description on the page's home page card.

const SITE = [
  { group: 'Start here', pages: [
    { file: 'index.html', title: 'Home' },
    { file: 'architecture.html', title: 'Architecture Overview' },
  ] },
  { group: 'Bounded contexts', pages: [
    { file: 'shared-kernel.html', title: 'Shared Kernel', about: 'Base classes, shared value objects, errors, the clock, and the Marten and Wolverine helpers every context uses.' },
    { file: 'property-registry.html', title: 'Property Registry', about: 'Land parcels, homes, development projects, and where each home stands in being sold.' },
    { file: 'home-buyers.html', title: 'Home Buyers', about: 'Households, applications, the waiting list, choosing a household for a home, income limits.' },
    { file: 'homeownership.html', title: 'Homeownership', about: 'Purchase workflow, mortgages, ground leases, lease fee notices, resales.', planned: true },
    { file: 'stewardship.html', title: 'Stewardship', about: 'Stewardship cases and foreclosure prevention.', planned: true },
    { file: 'trust-management.html', title: 'Trust Management', about: 'The trust\'s settings, policy, members and invitations.', planned: true },
    { file: 'identity.html', title: 'Identity', about: 'Users, linked to their Keycloak account.', planned: true },
    { file: 'documents.html', title: 'Documents', about: 'Uploaded files attached to any record.', planned: true },
  ] },
  { group: 'Topics', pages: [
    { file: 'auth-flow.html', title: 'Authentication flow' },
    { file: 'seeding.html', title: 'Seeding' },
    { file: 'glossary.html', title: 'Glossary', about: 'Each bounded context\'s language: its terms, their meanings, and the words it avoids.' },
    { file: 'glossary-decisions.html', title: 'Glossary Decisions', about: 'Why each glossary term was chosen over its rivals.' },
  ] },
];

(function () {
  const here = decodeURIComponent(location.pathname.split('/').pop()) || 'index.html';
  const written = SITE.flatMap(g => g.pages.filter(p => !p.planned).map(p => ({ ...p, group: g.group })));
  const current = written.find(p => p.file === here);
  const el = (tag, attrs = {}, ...children) => {
    const e = document.createElement(tag);
    for (const [k, v] of Object.entries(attrs)) e.setAttribute(k, v);
    e.append(...children);
    return e;
  };

  document.documentElement.classList.add('has-site-nav');

  // Sidebar, with this page's sections nested under its entry.
  const layout = document.querySelector('.layout');
  const sidebar = el('nav', { class: 'toc', id: 'site-nav', 'aria-label': 'Site' });
  layout.prepend(sidebar);
  const pageToc = el('ol', { class: 'page-toc' });
  for (const h of document.querySelectorAll('main section[id] > h2')) {
    pageToc.append(el('li', {}, el('a', { href: '#' + h.parentElement.id }, h.textContent.replace(/^\d+\.\s*/, ''))));
  }

  for (const g of SITE) {
    const list = el('ul');
    for (const p of g.pages) {
      if (p.planned) {
        list.append(el('li', { class: 'planned' }, p.title, el('span', { class: 'tag' }, 'not written yet')));
      } else if (p.file === here) {
        const li = el('li', { class: 'current' }, el('a', { href: './' + p.file, 'aria-current': 'page' }, p.title));
        if (pageToc.children.length) li.append(pageToc);
        list.append(li);
      } else {
        list.append(el('li', {}, el('a', { href: './' + p.file }, p.title)));
      }
    }
    sidebar.append(el('p', { class: 'nav-group' }, g.group), list);
  }

  // Header bar: home link, where you are, and a menu button for narrow screens.
  const menu = el('button', { class: 'menu-button', type: 'button', 'aria-expanded': 'false', 'aria-controls': 'site-nav' }, 'Menu');
  const crumb = el('span', { class: 'crumb' });
  if (current && current.file !== 'index.html') crumb.append(el('span', { class: 'crumb-group' }, current.group + ' / '), current.title);
  const header = el('header', { class: 'site-header' },
    el('div', { class: 'site-header-inner' }, el('a', { class: 'brand', href: './index.html' }, 'CommonGround docs'), crumb, menu));
  document.body.prepend(header);

  const setOpen = open => {
    document.documentElement.classList.toggle('nav-open', open);
    menu.setAttribute('aria-expanded', String(open));
  };
  menu.addEventListener('click', () => setOpen(!document.documentElement.classList.contains('nav-open')));
  sidebar.addEventListener('click', e => { if (e.target.closest('a')) setOpen(false); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') setOpen(false); });

  // Previous and next page, in sidebar order.
  const main = document.querySelector('main');
  const i = written.findIndex(p => p.file === here);
  if (main && i >= 0) {
    const pager = el('nav', { class: 'pager', 'aria-label': 'Previous and next page' });
    const link = (p, label, cls) => el('a', { class: cls, href: './' + p.file }, el('span', { class: 'pager-label' }, label), p.title);
    if (i > 0) pager.append(link(written[i - 1], 'Previous', 'prev'));
    if (i < written.length - 1) pager.append(link(written[i + 1], 'Next', 'next'));
    main.append(pager);
  }

  // Home page cards: <div class="ctx" data-page="file.html"></div> gets the
  // page's title and description, as a link once the page is written.
  const byFile = Object.fromEntries(SITE.flatMap(g => g.pages).map(p => [p.file, p]));
  for (const card of document.querySelectorAll('[data-page]')) {
    const p = byFile[card.dataset.page];
    if (!p) { console.error('nav.js: no SITE entry for', card.dataset.page); continue; }
    const body = [el('strong', {}, p.title), el('span', {}, p.about || '')];
    card.replaceWith(p.planned
      ? el('div', { class: 'ctx planned' }, ...body, el('em', {}, 'Page not written yet'))
      : el('a', { class: 'ctx', href: './' + p.file }, ...body));
  }

  // Mermaid diagrams render after the browser has jumped to #section, and push
  // the section down. Re-scroll to it while the page grows, until the reader
  // scrolls or 5 seconds pass.
  const target = location.hash && document.getElementById(decodeURIComponent(location.hash.slice(1)));
  if (target && main) {
    const ro = new ResizeObserver(() => target.scrollIntoView());
    ro.observe(main);
    const stop = () => ro.disconnect();
    for (const ev of ['wheel', 'touchstart', 'keydown', 'mousedown']) window.addEventListener(ev, stop, { once: true, passive: true });
    setTimeout(stop, 5000);
  }

  // Mark the section being read in the nested contents list.
  // The current section is the last one whose top has passed the header,
  // or the last section once the page is scrolled to the bottom.
  {
    const targets = [...pageToc.querySelectorAll('a[href^="#"]')]
      .map(a => ({ a, s: document.getElementById(a.getAttribute('href').slice(1)) }))
      .filter(t => t.s);
    let queued = false;
    const update = () => {
      queued = false;
      const atBottom = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 2;
      let pick = atBottom ? targets[targets.length - 1] : null;
      if (!pick) for (const t of targets) if (t.s.getBoundingClientRect().top <= 100) pick = t;
      targets.forEach(t => t.a.classList.toggle('reading', t === pick));
    };
    const queue = () => { if (!queued) { queued = true; requestAnimationFrame(update); } };
    window.addEventListener('scroll', queue, { passive: true });
    window.addEventListener('resize', queue);
    update();
  }
})();
