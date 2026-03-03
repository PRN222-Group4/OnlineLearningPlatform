// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Simple AJAX nav helper: when a link has the `ajax-nav` class, load the target page
// and replace the main content area instead of a full page reload.
document.addEventListener('DOMContentLoaded', function () {
    const mainSelector = 'main[role="main"]';
    const mainEl = document.querySelector(mainSelector);
    if (!mainEl) return;

    async function loadUrlIntoMain(url, replaceState = false) {
        try {
            const res = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });
            if (!res.ok) {
                // fallback to full navigation on error
                window.location.href = url;
                return;
            }
            const text = await res.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(text, 'text/html');
            const newMain = doc.querySelector(mainSelector) || doc.querySelector('main');
            if (!newMain) {
                // fallback to full navigation if fragment not found
                window.location.href = url;
                return;
            }
            // replace main content
            mainEl.innerHTML = newMain.innerHTML;

            // update document title
            const newTitle = doc.querySelector('title')?.innerText;
            if (newTitle) document.title = newTitle;

            // update history
            if (replaceState) {
                history.replaceState({ url }, '', url);
            } else {
                history.pushState({ url }, '', url);
            }
        } catch (e) {
            console.error('AJAX nav error', e);
            window.location.href = url;
        }
    }

    // delegate clicks on links with .ajax-nav
    document.body.addEventListener('click', function (e) {
        const a = e.target.closest && e.target.closest('a.ajax-nav');
        if (!a) return;
        const href = a.getAttribute('href') || a.getAttribute('data-href');
        if (!href) return;
        // Only intercept same-origin navigations
        const url = new URL(href, window.location.origin);
        if (url.origin !== window.location.origin) return;
        e.preventDefault();
        loadUrlIntoMain(url.href);
    });

    // handle logout forms via AJAX to remove cookie and update header/main
    document.body.addEventListener('submit', async function (e) {
        const form = e.target;
        if (!form || !form.classList.contains('logout-form')) return;
        e.preventDefault();
        try {
            const res = await fetch(form.action, { method: 'POST', credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (res.ok) {
                // replace header
                try {
                    const hres = await fetch('/api/ui/header', { credentials: 'same-origin' });
                    if (hres.ok) {
                        const html = await hres.text();
                        const container = document.getElementById('headerUserArea');
                        if (container) container.outerHTML = html;
                    }
                } catch (e) { }

                // reload main content home
                const mainSelector = 'main[role="main"]';
                const mainEl = document.querySelector(mainSelector);
                if (mainEl) {
                    const home = await fetch('/', { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' });
                    if (home.ok) {
                        const text = await home.text();
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(text, 'text/html');
                        const newMain = doc.querySelector(mainSelector) || doc.querySelector('main');
                        if (newMain) mainEl.innerHTML = newMain.innerHTML;
                        const newTitle = doc.querySelector('title')?.innerText;
                        if (newTitle) document.title = newTitle;
                } else {
                        // fallback: force full reload
                        window.location.href = '/';
                    }
                } else {
                    window.location.href = '/';
                }
            }
        } catch (err) {
            console.error('Logout failed', err);
            window.location.href = '/Account/Logout';
        }
    });

    // handle back/forward
    window.addEventListener('popstate', function (e) {
        const state = e.state;
        const url = (state && state.url) || window.location.href;
        loadUrlIntoMain(url, true);
    });
});
