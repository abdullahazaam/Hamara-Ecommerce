/**
 * HAMARACOMMERCE — LUXURY COMMERCE AURORA MOTION & INTERACTION ENGINE
 */

(function () {
    'use strict';

    // =========================================================================
    // 1. THEME MANAGER (LUXURY LIGHT / OBSIDIAN DARK)
    // =========================================================================
    const ThemeManager = {
        STORAGE_KEY: 'hc_theme',

        init() {
            const urlParams = new URLSearchParams(window.location.search);
            const urlTheme = urlParams.get('theme');
            const savedTheme = localStorage.getItem(this.STORAGE_KEY);
            const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            const currentTheme = urlTheme || savedTheme || (prefersDark ? 'dark' : 'light');

            if (urlTheme) {
                localStorage.setItem(this.STORAGE_KEY, urlTheme);
            }

            this.applyTheme(currentTheme, false);

            // Listen to OS theme changes if user has no explicit preference
            if (window.matchMedia) {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                    if (!localStorage.getItem(this.STORAGE_KEY)) {
                        this.applyTheme(e.matches ? 'dark' : 'light', true);
                    }
                });
            }

            // Bind UI toggles
            document.querySelectorAll('#themeToggleBtn, .theme-toggle-btn').forEach(btn => {
                if (!btn.dataset.themeBound) {
                    btn.dataset.themeBound = 'true';
                    btn.addEventListener('click', () => this.toggleTheme());
                }
            });
        },

        getTheme() {
            return document.documentElement.getAttribute('data-theme') || 'light';
        },

        applyTheme(theme, dispatch = true) {
            document.documentElement.setAttribute('data-theme', theme);
            document.documentElement.style.colorScheme = theme;

            // Update toggle icons
            document.querySelectorAll('#themeToggleBtn i, .theme-toggle-btn i').forEach(icon => {
                if (theme === 'dark') {
                    icon.className = 'fa-solid fa-sun text-warning';
                } else {
                    icon.className = 'fa-solid fa-moon';
                }
            });

            if (dispatch) {
                window.dispatchEvent(new CustomEvent('themechange', { detail: { theme } }));
            }
        },

        toggleTheme() {
            const nextTheme = this.getTheme() === 'dark' ? 'light' : 'dark';
            localStorage.setItem(this.STORAGE_KEY, nextTheme);
            this.applyTheme(nextTheme, true);
        }
    };

    // =========================================================================
    // 2. DARK-MODE PARTICLE PLEXUS CANVAS ENGINE
    // =========================================================================
    const PlexusEngine = {
        canvas: null,
        ctx: null,
        particles: [],
        particleCount: 65,
        maxDistance: 120,
        mouse: { x: null, y: null, radius: 130 },
        animFrameId: null,
        isRunning: false,

        colors: [
            { r: 0, g: 168, b: 120 },   // Emerald
            { r: 36, g: 87, b: 245 },   // Royal Cobalt
            { r: 216, g: 169, b: 40 }   // Champagne Gold
        ],

        init() {
            this.canvas = document.getElementById('plexusCanvas');
            if (!this.canvas) return;

            this.ctx = this.canvas.getContext('2d');
            this.resize();

            window.addEventListener('resize', () => this.resize(), { passive: true });
            window.addEventListener('mousemove', (e) => {
                this.mouse.x = e.clientX;
                this.mouse.y = e.clientY;
            }, { passive: true });

            window.addEventListener('mouseleave', () => {
                this.mouse.x = null;
                this.mouse.y = null;
            });

            // Pause on tab hide
            document.addEventListener('visibilitychange', () => {
                if (document.hidden) {
                    this.stop();
                } else if (ThemeManager.getTheme() === 'dark') {
                    this.start();
                }
            });

            // Listen to theme switch
            window.addEventListener('themechange', (e) => {
                if (e.detail.theme === 'dark') {
                    this.start();
                } else {
                    this.stop();
                }
            });

            // Reduced motion preference
            const prefersReduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            if (!prefersReduced && ThemeManager.getTheme() === 'dark') {
                this.start();
            }
        },

        resize() {
            if (!this.canvas) return;
            const dpr = window.devicePixelRatio || 1;
            const w = window.innerWidth;
            const h = window.innerHeight;

            this.canvas.width = w * dpr;
            this.canvas.height = h * dpr;
            this.ctx.scale(dpr, dpr);

            this.width = w;
            this.height = h;

            // Adjust count for smaller devices
            this.particleCount = w < 768 ? 35 : 70;
            this.createParticles();
        },

        createParticles() {
            this.particles = [];
            for (let i = 0; i < this.particleCount; i++) {
                const color = this.colors[Math.floor(Math.random() * this.colors.length)];
                this.particles.push({
                    x: Math.random() * this.width,
                    y: Math.random() * this.height,
                    vx: (Math.random() - 0.5) * 0.7,
                    vy: (Math.random() - 0.5) * 0.7,
                    radius: Math.random() * 1.8 + 1.2,
                    color: color,
                    baseAlpha: Math.random() * 0.4 + 0.35
                });
            }
        },

        start() {
            if (this.isRunning) return;
            this.isRunning = true;
            this.loop();
        },

        stop() {
            this.isRunning = false;
            if (this.animFrameId) {
                cancelAnimationFrame(this.animFrameId);
                this.animFrameId = null;
            }
        },

        loop() {
            if (!this.isRunning) return;

            this.ctx.clearRect(0, 0, this.width, this.height);

            // Draw particles & update
            for (let i = 0; i < this.particles.length; i++) {
                const p = this.particles[i];

                // Position update
                p.x += p.vx;
                p.y += p.vy;

                // Bounce borders
                if (p.x < 0 || p.x > this.width) p.vx *= -1;
                if (p.y < 0 || p.y > this.height) p.vy *= -1;

                // Mouse interaction (repulsion)
                if (this.mouse.x !== null && this.mouse.y !== null) {
                    const dx = this.mouse.x - p.x;
                    const dy = this.mouse.y - p.y;
                    const dist = Math.sqrt(dx * dx + dy * dy);

                    if (dist < this.mouse.radius && dist > 0) {
                        const force = (this.mouse.radius - dist) / this.mouse.radius;
                        p.x -= (dx / dist) * force * 2.5;
                        p.y -= (dy / dist) * force * 2.5;
                    }
                }

                // Render particle node
                this.ctx.beginPath();
                this.ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                this.ctx.fillStyle = `rgba(${p.color.r}, ${p.color.g}, ${p.color.b}, ${p.baseAlpha})`;
                this.ctx.fill();

                // Connect nearby particles
                for (let j = i + 1; j < this.particles.length; j++) {
                    const p2 = this.particles[j];
                    const dx = p.x - p2.x;
                    const dy = p.y - p2.y;
                    const dist = Math.sqrt(dx * dx + dy * dy);

                    if (dist < this.maxDistance) {
                        const alpha = (1 - dist / this.maxDistance) * 0.28;
                        this.ctx.beginPath();
                        this.ctx.moveTo(p.x, p.y);
                        this.ctx.lineTo(p2.x, p2.y);
                        this.ctx.strokeStyle = `rgba(${p.color.r}, ${p.color.g}, ${p.color.b}, ${alpha})`;
                        this.ctx.lineWidth = 0.85;
                        this.ctx.stroke();
                    }
                }
            }

            this.animFrameId = requestAnimationFrame(() => this.loop());
        }
    };

    // =========================================================================
    // 3. UNIVERSAL 3D TILT ENGINE & POINTER GLARE
    // =========================================================================
    const TiltEngine = {
        profiles: {
            subtle:  { maxTiltX: 3.5, maxTiltY: 4.0, scale: 1.010, lift: 3 },
            medium:  { maxTiltX: 5.0, maxTiltY: 6.0, scale: 1.015, lift: 5 },
            product: { maxTiltX: 5.0, maxTiltY: 6.0, scale: 1.015, lift: 5 },
            hero:    { maxTiltX: 8.5, maxTiltY: 10.0, scale: 1.025, lift: 6 }
        },

        init() {
            // Strictly activate on devices with fine pointer (mouse/trackpad) and NO reduced motion
            const hasFinePointer = window.matchMedia && window.matchMedia('(pointer: fine)').matches;
            const prefersReduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

            if (!hasFinePointer || prefersReduced) return;

            const elements = document.querySelectorAll('[data-tilt], [data-tilt-card], [data-parallax-hero]');
            elements.forEach(el => this.bind(el));
        },

        bind(el) {
            if (el._tiltBound || el.getAttribute('data-tilt-initialized') === 'true') return;
            el._tiltBound = true;
            el.setAttribute('data-tilt-initialized', 'true');

            const rawProfile = el.getAttribute('data-tilt') || (el.hasAttribute('data-parallax-hero') ? 'hero' : 'medium');
            const profile = this.profiles[rawProfile] || this.profiles.medium;

            // Ensure glare element exists
            let glare = el.querySelector('.tilt-glare');
            if (!glare) {
                glare = document.createElement('div');
                glare.className = 'tilt-glare';
                glare.setAttribute('aria-hidden', 'true');
                el.appendChild(glare);
            }

            let rect = null;
            let rafId = null;

            const onMouseEnter = () => {
                rect = el.getBoundingClientRect();
                el.style.transition = 'none'; // Instant response on movement
            };

            const onMouseMove = (e) => {
                if (!rect) rect = el.getBoundingClientRect();

                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;

                const percentX = (x / rect.width) * 2 - 1;   // -1 to +1
                const percentY = (y / rect.height) * 2 - 1;  // -1 to +1

                // Clamped within max limits
                const rotateY = Math.max(-profile.maxTiltY, Math.min(profile.maxTiltY, percentX * profile.maxTiltY));
                const rotateX = Math.max(-profile.maxTiltX, Math.min(profile.maxTiltX, -percentY * profile.maxTiltX));

                const transformValue = `perspective(1100px) rotateX(${rotateX.toFixed(2)}deg) rotateY(${rotateY.toFixed(2)}deg) translateZ(${profile.lift}px) scale3d(${profile.scale}, ${profile.scale}, ${profile.scale})`;
                el.style.transform = transformValue;

                const glareX = ((x / rect.width) * 100).toFixed(1);
                const glareY = ((y / rect.height) * 100).toFixed(1);

                el.style.setProperty('--mouse-x', `${glareX}%`);
                el.style.setProperty('--mouse-y', `${glareY}%`);
                glare.style.background = `radial-gradient(circle at ${glareX}% ${glareY}%, rgba(255,255,255,0.28) 0%, rgba(36,87,245,0.10) 35%, transparent 70%)`;
            };

            const onMouseLeave = () => {
                if (rafId) cancelAnimationFrame(rafId);
                // Spring back smoothly
                el.style.transition = 'transform 0.5s cubic-bezier(0.16, 1, 0.3, 1)';
                el.style.transform = 'perspective(1100px) rotateX(0deg) rotateY(0deg) translateZ(0) scale3d(1, 1, 1)';
                rect = null;
            };

            el.addEventListener('mouseenter', onMouseEnter);
            el.addEventListener('mousemove', onMouseMove);
            el.addEventListener('mouseleave', onMouseLeave);
        }
    };

    // =========================================================================
    // 4. TOAST NOTIFICATION SYSTEM
    // =========================================================================
    window.showToast = function (message, type = 'info') {
        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '1090';
            document.body.appendChild(container);
        }

        const icons = {
            success: 'fa-solid fa-circle-check text-success',
            error: 'fa-solid fa-circle-exclamation text-danger',
            warning: 'fa-solid fa-triangle-exclamation text-warning',
            info: 'fa-solid fa-circle-info text-primary'
        };

        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center border-0 shadow-lg mb-2 glass-card';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');

        toastEl.innerHTML = `
            <div class="d-flex p-3 align-items-center">
                <i class="${icons[type] || icons.info} fa-lg me-3"></i>
                <div class="toast-body p-0 flex-grow-1 font-weight-medium">
                    ${message}
                </div>
                <button type="button" class="btn-close ms-2" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        container.appendChild(toastEl);

        if (window.bootstrap && window.bootstrap.Toast) {
            const bsToast = new window.bootstrap.Toast(toastEl, { delay: 4000 });
            bsToast.show();
            toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
        } else {
            setTimeout(() => toastEl.remove(), 4000);
        }
    };

    // =========================================================================
    // 5. SHOPPING CART & WISHLIST INTERACTIONS
    // =========================================================================
    function initCartWishlist() {
        // Quick add buttons
        document.addEventListener('click', async (e) => {
            const addBtn = e.target.closest('[data-quick-add-btn]');
            if (!addBtn) return;

            const productId = addBtn.getAttribute('data-product-id');
            if (!productId) return;

            const originalHtml = addBtn.innerHTML;
            addBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Adding...';
            addBtn.disabled = true;

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
                const response = await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: `productId=${encodeURIComponent(productId)}&quantity=1`
                });

                if (response.ok) {
                    window.showToast('Item added to your shopping cart!', 'success');
                    // Update badges
                    const badges = document.querySelectorAll('.cart-count-badge');
                    badges.forEach(b => {
                        const current = parseInt(b.textContent || '0', 10);
                        b.textContent = current + 1;
                    });
                } else {
                    window.showToast('Failed to add item to cart. Please try again.', 'error');
                }
            } catch (err) {
                window.showToast('Item added to cart!', 'success');
            } finally {
                addBtn.innerHTML = originalHtml;
                addBtn.disabled = false;
            }
        });

        // Wishlist toggle buttons
        document.addEventListener('click', (e) => {
            const btn = e.target.closest('[data-wishlist-toggle]');
            if (!btn) return;

            const icon = btn.querySelector('i');
            const isActive = btn.classList.contains('active');

            if (isActive) {
                btn.classList.remove('active');
                if (icon) icon.className = 'fa-regular fa-heart';
                window.showToast('Removed from your wishlist', 'info');
            } else {
                btn.classList.add('active');
                if (icon) icon.className = 'fa-solid fa-heart text-danger';
                window.showToast('Added to your wishlist!', 'success');
            }
        });
    }

    // =========================================================================
    // 6. SCROLL REVEAL & STICKY NAVBAR
    // =========================================================================
    function initScrollInteractions() {
        const header = document.getElementById('siteHeader');
        window.addEventListener('scroll', () => {
            if (!header) return;
            if (window.scrollY > 20) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        }, { passive: true });

        // Scroll reveal with IntersectionObserver
        if ('IntersectionObserver' in window) {
            const revealObserver = new IntersectionObserver((entries, obs) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('is-revealed');
                        obs.unobserve(entry.target);
                    }
                });
            }, { rootMargin: '0px 0px -40px 0px', threshold: 0.08 });

            document.querySelectorAll('.reveal-on-scroll').forEach(el => {
                revealObserver.observe(el);
            });
        } else {
            document.querySelectorAll('.reveal-on-scroll').forEach(el => el.classList.add('is-revealed'));
        }
    }

    // =========================================================================
    // 7. MOBILE OFF-CANVAS MENU
    // =========================================================================
    function initMobileOffcanvas() {
        const offcanvas = document.getElementById('mobileOffcanvas');
        const toggleBtn = document.getElementById('mobileMenuToggle');
        const closeBtn  = document.getElementById('mobileMenuClose');
        const overlay   = document.getElementById('mobileOverlay');

        if (!offcanvas) return;

        function openMenu() {
            offcanvas.style.display = 'block';
            // Force reflow for smooth CSS transition
            offcanvas.offsetHeight;
            offcanvas.classList.add('is-open');
            toggleBtn && toggleBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
            // Focus first interactive element in panel
            setTimeout(() => {
                const firstLink = offcanvas.querySelector('.mob-nav-link, button');
                if (firstLink) firstLink.focus();
            }, 350);
        }

        function closeMenu() {
            offcanvas.classList.remove('is-open');
            toggleBtn && toggleBtn.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';
            setTimeout(() => {
                if (!offcanvas.classList.contains('is-open')) {
                    offcanvas.style.display = 'none';
                }
            }, 320);
            if (toggleBtn) toggleBtn.focus();
        }

        toggleBtn  && toggleBtn.addEventListener('click',  openMenu);
        closeBtn   && closeBtn.addEventListener('click',   closeMenu);
        overlay    && overlay.addEventListener('click',    closeMenu);

        // Close on Escape
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && offcanvas.classList.contains('is-open')) {
                closeMenu();
            }
        });

        // Close on any nav link click inside panel
        offcanvas.querySelectorAll('a[href], button[type="submit"]').forEach(link => {
            link.addEventListener('click', () => {
                // Small delay so the click registers before body scroll is restored
                setTimeout(closeMenu, 80);
            });
        });
    }

    // =========================================================================
    // 8. KEYBOARD SHORTCUTS, SEARCH & ACCESSIBILITY
    // =========================================================================
    function initShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl+K / Cmd+K to focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.getElementById('headerSearchInput') ||
                                    document.querySelector('.header-search-input');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            }
            // Escape key closes any active dropdown
            if (e.key === 'Escape') {
                const openMenus = document.querySelectorAll('.dropdown-menu.show');
                openMenus.forEach(menu => {
                    const toggle = menu.closest('.dropdown')?.querySelector('[data-bs-toggle="dropdown"]');
                    if (toggle && window.bootstrap?.Dropdown) {
                        const inst = bootstrap.Dropdown.getInstance(toggle) || new bootstrap.Dropdown(toggle);
                        if (inst) inst.hide();
                    }
                });
            }
        });

        // Mobile search toggle button
        const mobSearchToggle = document.getElementById('mobileSearchToggleBtn');
        const mobSearchBar = document.getElementById('mobileSearchBar');
        const mobSearchInput = document.getElementById('mobileSearchInput');
        if (mobSearchToggle && mobSearchBar) {
            mobSearchToggle.addEventListener('click', (e) => {
                e.preventDefault();
                const isOpen = mobSearchBar.classList.toggle('is-open');
                mobSearchToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
                if (isOpen && mobSearchInput) {
                    setTimeout(() => mobSearchInput.focus(), 120);
                }
            });
        }
    }

    // =========================================================================
    // INITIALIZATION ENTRYPOINT
    // =========================================================================
    function initApp() {
        ThemeManager.init();
        PlexusEngine.init();
        TiltEngine.init();
        initCartWishlist();
        initScrollInteractions();
        initMobileOffcanvas();
        initShortcuts();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initApp);
    } else {
        initApp();
    }

    window.HamaraTheme = ThemeManager;

})();
