let _ref;
let _observer;

export function initMarkdownViewer(ref) {
    _ref = ref;
    observeLazyImages();
}

export function disposeMarkdownViewer() {
    if (_observer) {
        _observer.disconnect();
        _observer = null;
    }
    _ref = null;
}

function observeLazyImages() {
    const images = document.querySelectorAll('.markdown-body img[data-src]');
    if (!images.length) return;

    _observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                const src = img.getAttribute('data-src');
                img.setAttribute('src', src);
                img.removeAttribute('data-src');
                _observer.unobserve(img);
            }
        });
    });

    images.forEach(img => {
        _observer.observe(img);
    });
}
