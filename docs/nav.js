// Site navigation for the static pages in docs/.
// Every page loads this file with <script src="./nav.js" defer>. It adds the
// header bar, turns the page's own contents list into a site sidebar with that
// list nested under the current page, and adds previous/next links.
// A plain script, not a module, so it also works when a page is opened from disk.
//
// To add a page: add it to SITE below. Set planned: true for a page that
// isn't written yet; it shows in the sidebar without a link.

const SITE = [
  { group: 'Start here', pages: [
    { file: 'index.html', title: 'Home' },
    { file: 'architecture.html', title: 'Architecture Overview' },
  ] },
  { group: 'Bounded contexts', pages: [
    { file: 'shared-kernel.html', title: 'Shared Kernel' },
    { file: 'property-registry.html', title: 'Property Registry' },
    { file: 'home-buyers.html', title: 'Home Buyers', planned: true },
    { file: 'homeownership.html', title: 'Homeownership', planned: true },
    { file: 'stewardship.html', title: 'Stewardship', planned: true },
    { file: 'trust-management.html', title: 'Trust Management', planned: true },
    { file: 'identity.html', title: 'Identity', planned: true },
    { file: 'documents.html', title: 'Documents', planned: true },
  ] },
  { group: 'Topics', pages: [
    { file: 'auth-flow.html', title: 'Authentication flow' },
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

  // Sidebar: reuse the page's <nav class="toc">, keep its <ol> of sections.
  const layout = document.querySelector('.layout');
  let sidebar = document.querySelector('nav.toc');
  const pageToc = sidebar ? sidebar.querySelector('ol') : null;
  if (!sidebar) {
    sidebar = el('nav', { class: 'toc' });
    layout.prepend(sidebar);
  }
  sidebar.replaceChildren();
  sidebar.id = 'site-nav';
  sidebar.setAttribute('aria-label', 'Site');

  for (const g of SITE) {
    const list = el('ul');
    for (const p of g.pages) {
      if (p.planned) {
        list.append(el('li', { class: 'planned' }, p.title, el('span', { class: 'tag' }, 'not written yet')));
      } else if (p.file === here) {
        const li = el('li', { class: 'current' }, el('a', { href: './' + p.file, 'aria-current': 'page' }, p.title));
        if (pageToc) { pageToc.classList.add('page-toc'); li.append(pageToc); }
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

  // Mark the section being read in the nested contents list.
  // The current section is the last one whose top has passed the header,
  // or the last section once the page is scrolled to the bottom.
  if (pageToc) {
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
