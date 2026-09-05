const report = () => {
    const el = (sel) => document.querySelector(sel);
    const rect = (sel) => {
        const e = el(sel);
        if (!e) return 'NOT FOUND';
        const r = e.getBoundingClientRect();
        return { tag: e.tagName, id: e.id, class: e.className, left: Math.round(r.left), top: Math.round(r.top), width: Math.round(r.width), height: Math.round(r.height), display: getComputedStyle(e).display };
    };
    return JSON.stringify({
        windowWidth: window.innerWidth,
        headerInner: rect('.site-header-inner'),
        hamburger: rect('#mobileMenuToggle'),
        brand: rect('.brand-logo'),
        controls: rect('.header-controls'),
        searchBtn: rect('#mobileSearchToggleBtn'),
        themeBtn: rect('#themeToggleBtn'),
        cartBtn: rect('.header-cart-btn'),
        signinBtn: rect('.header-signin-btn')
    }, null, 2);
};
console.log("===REPORT_START===");
console.log(report());
console.log("===REPORT_END===");
