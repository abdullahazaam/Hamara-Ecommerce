/**
 * HAMARACOMMERCE — LUXURY COMMERCE AURORA MOTION & INTERACTION ENGINE
 */

(function () {
    'use strict';

    // Helper: Escape HTML to avoid injection
    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // Helper: Retrieve CSRF token
    function getCsrfToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

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
                <div class="toast-body p-0 flex-grow-1 font-weight-medium" style="color:var(--text-primary); font-size: 0.9rem;">
                    ${escapeHtml(message)}
                </div>
                <button type="button" class="btn-close ms-2" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        container.appendChild(toastEl);

        if (window.bootstrap && window.bootstrap.Toast) {
            const bsToast = new window.bootstrap.Toast(toastEl, { delay: 4200 });
            bsToast.show();
            toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
        } else {
            setTimeout(() => {
                toastEl.style.opacity = '0';
                toastEl.style.transition = 'opacity 0.3s ease';
                setTimeout(() => toastEl.remove(), 350);
            }, 4200);
        }
    };

    function updateCartBadges(count) {
        const badges = document.querySelectorAll('.cart-count-badge');
        badges.forEach(b => {
            b.textContent = count;
            if (count > 0) {
                b.classList.remove('d-none');
            } else {
                b.classList.add('d-none');
            }
        });
    }

    function updateWishlistBadges(count) {
        const badges = document.querySelectorAll('.wishlist-count-badge');
        badges.forEach(b => {
            b.textContent = count;
            if (count > 0) {
                b.classList.remove('d-none');
            } else {
                b.classList.add('d-none');
            }
        });
    }

    // =========================================================================
    // 5. SHOPPING CART & WISHLIST INTERACTIONS (AJAX + Server Confirmation)
    // =========================================================================
    function initCartWishlist() {
        // Quick add to cart buttons
        document.addEventListener('click', async (e) => {
            const addBtn = e.target.closest('[data-quick-add-btn]');
            if (!addBtn) return;

            const productId = addBtn.getAttribute('data-product-id');
            if (!productId) return;

            const originalHtml = addBtn.innerHTML;
            addBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Adding...';
            addBtn.disabled = true;

            try {
                const token = getCsrfToken();
                const response = await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: `productId=${encodeURIComponent(productId)}&quantity=1`
                });

                if (!response.ok) {
                    window.showToast(`Server error (${response.status}). Could not add item to cart.`, 'error');
                    return;
                }

                const data = await response.json();
                if (data.success) {
                    window.showToast(data.message || 'Item added to your shopping cart!', 'success');
                    if (data.warning) {
                        window.showToast(data.warning, 'warning');
                    }
                    if (typeof data.itemCount === 'number') {
                        updateCartBadges(data.itemCount);
                    }
                } else {
                    window.showToast(data.message || 'Failed to add item to cart.', 'error');
                }
            } catch (err) {
                console.error('QuickAdd error:', err);
                window.showToast('Network error. Unable to add item to cart.', 'error');
            } finally {
                addBtn.innerHTML = originalHtml;
                addBtn.disabled = false;
            }
        });

        // Wishlist toggle buttons (Server Confirmed State)
        document.addEventListener('click', async (e) => {
            const btn = e.target.closest('[data-wishlist-toggle]');
            if (!btn) return;

            const productId = btn.getAttribute('data-product-id');
            if (!productId) return;

            const icon = btn.querySelector('i');
            const token = getCsrfToken();

            btn.disabled = true;

            try {
                const response = await fetch('/Account/ToggleWishlist', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: `productId=${encodeURIComponent(productId)}`
                });

                if (response.status === 401) {
                    window.showToast('Please sign in to save items to your wishlist.', 'warning');
                    setTimeout(() => {
                        window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                    }, 1400);
                    return;
                }

                if (!response.ok) {
                    window.showToast(`Error (${response.status}): Could not update wishlist.`, 'error');
                    return;
                }

                const data = await response.json();

                if (data.requiresAuth) {
                    window.showToast(data.message || 'Please sign in to save items to your wishlist.', 'warning');
                    setTimeout(() => {
                        window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                    }, 1400);
                    return;
                }

                if (data.success) {
                    if (data.isAdded) {
                        btn.classList.add('active');
                        if (icon) icon.className = 'fa-solid fa-heart text-danger';
                        window.showToast(data.message || 'Added to your wishlist!', 'success');
                    } else {
                        btn.classList.remove('active');
                        if (icon) icon.className = 'fa-regular fa-heart';
                        window.showToast(data.message || 'Removed from your wishlist', 'info');
                    }

                    if (typeof data.count === 'number') {
                        updateWishlistBadges(data.count);
                    }
                } else {
                    window.showToast(data.message || 'Unable to update wishlist.', 'error');
                }
            } catch (err) {
                console.error('Wishlist error:', err);
                window.showToast('Network error. Could not update wishlist.', 'error');
            } finally {
                btn.disabled = false;
            }
        });
    }

    // =========================================================================
    // 6. PRODUCT DETAILS SHOWCASE (Thumbnails, Variants, Steppers, Sticky Bar)
    // =========================================================================
    function initProductDetails() {
        // A. Image Thumbnails Switcher
        const mainImage = document.getElementById('productMainImage');
        const thumbButtons = document.querySelectorAll('.gallery-thumb-btn');

        if (mainImage && thumbButtons.length > 0) {
            thumbButtons.forEach(btn => {
                btn.addEventListener('click', () => {
                    const newSrc = btn.getAttribute('data-img-src');
                    if (!newSrc) return;

                    mainImage.style.opacity = '0.3';
                    setTimeout(() => {
                        mainImage.src = newSrc;
                        mainImage.style.opacity = '1';
                    }, 150);

                    thumbButtons.forEach(b => {
                        b.classList.remove('border-primary', 'shadow-sm');
                        b.style.borderColor = 'var(--border-default)';
                    });
                    btn.classList.add('border-primary', 'shadow-sm');
                    btn.style.borderColor = 'var(--brand-primary)';
                });
            });
        }

        // B. Quantity Steppers
        const qtyInputs = [
            document.getElementById('productQuantityInput'),
            document.getElementById('stickyQuantityInput')
        ].filter(Boolean);

        function setQuantity(val) {
            qtyInputs.forEach(input => {
                const min = parseInt(input.min || '1', 10);
                const max = parseInt(input.max || '999', 10);
                let clamped = Math.max(min, Math.min(max, val));
                input.value = clamped;
            });
        }

        document.addEventListener('click', (e) => {
            const minusBtn = e.target.closest('[data-step-minus]');
            const plusBtn  = e.target.closest('[data-step-plus]');

            if (minusBtn) {
                const input = minusBtn.closest('.quantity-stepper')?.querySelector('input[type="number"]') || qtyInputs[0];
                if (input) {
                    const current = parseInt(input.value || '1', 10);
                    setQuantity(current - 1);
                }
            } else if (plusBtn) {
                const input = plusBtn.closest('.quantity-stepper')?.querySelector('input[type="number"]') || qtyInputs[0];
                if (input) {
                    const current = parseInt(input.value || '1', 10);
                    setQuantity(current + 1);
                }
            }
        });

        qtyInputs.forEach(input => {
            input.addEventListener('change', () => {
                const val = parseInt(input.value || '1', 10);
                setQuantity(isNaN(val) ? 1 : val);
            });
        });

        // C. Variant Select Synchronization
        const variantSelect = document.getElementById('productVariantSelect');
        const priceDisplay = document.getElementById('productPriceDisplay');
        const skuDisplay = document.getElementById('productSkuDisplay');
        const stockContainer = document.getElementById('productStockBadgeContainer');
        const primaryBtn = document.getElementById('primaryAddToCartBtn');
        const stickyBtn = document.getElementById('stickyAddToCartBtn');
        const stickyBarPrice = document.getElementById('stickyBarPrice');
        const stickyBarImage = document.getElementById('stickyBarImage');
        const stickyVariantInput = document.getElementById('stickyVariantId');

        if (variantSelect) {
            variantSelect.addEventListener('change', () => {
                const selectedOpt = variantSelect.options[variantSelect.selectedIndex];
                if (!selectedOpt) return;

                const priceFormatted = selectedOpt.dataset.priceFormatted;
                const sku = selectedOpt.dataset.sku;
                const stock = parseInt(selectedOpt.dataset.stock || '0', 10);
                const image = selectedOpt.dataset.image;
                const variantId = selectedOpt.value;

                // 1. Update Price
                if (priceFormatted) {
                    if (priceDisplay) priceDisplay.textContent = priceFormatted;
                    if (stickyBarPrice) stickyBarPrice.textContent = priceFormatted;
                }

                // 2. Update SKU
                if (skuDisplay && sku) {
                    skuDisplay.textContent = `SKU: ${sku}`;
                }

                // 3. Update Image if variant has a distinct image
                if (image && mainImage) {
                    mainImage.src = image;
                    if (stickyBarImage) stickyBarImage.src = image;
                }

                // 4. Update Variant ID in sticky form
                if (stickyVariantInput) {
                    stickyVariantInput.value = variantId || '';
                }

                // 5. Update Stock Badge
                if (stockContainer) {
                    if (stock > 10) {
                        stockContainer.innerHTML = `
                            <span class="badge bg-success-subtle text-success border border-success rounded-pill px-3 py-1.5 small">
                                <i class="fa-solid fa-circle-check me-1"></i>In Stock (${stock} available)
                            </span>`;
                    } else if (stock > 0) {
                        stockContainer.innerHTML = `
                            <span class="badge bg-warning-subtle text-warning border border-warning rounded-pill px-3 py-1.5 small">
                                <i class="fa-solid fa-fire me-1"></i>Low Stock: Only ${stock} left!
                            </span>`;
                    } else {
                        stockContainer.innerHTML = `
                            <span class="badge bg-danger-subtle text-danger border border-danger rounded-pill px-3 py-1.5 small">
                                <i class="fa-solid fa-ban me-1"></i>Currently Out of Stock
                            </span>`;
                    }
                }

                // 6. Update Button States and Quantity Limit
                const hasStock = stock > 0;
                if (primaryBtn) primaryBtn.disabled = !hasStock;
                if (stickyBtn)  stickyBtn.disabled  = !hasStock;

                qtyInputs.forEach(input => {
                    input.max = Math.max(1, stock);
                    if (parseInt(input.value || '1', 10) > stock && hasStock) {
                        input.value = stock;
                    }
                });
            });
        }

        // D. Sticky Purchase Bar Visibility on Scroll
        const stickyBar = document.getElementById('stickyPurchaseBar');
        const mainForm = document.getElementById('mainAddToCartForm');

        if (stickyBar && mainForm) {
            const checkStickyBar = () => {
                const rect = mainForm.getBoundingClientRect();
                if (rect.bottom < 60) {
                    stickyBar.classList.add('is-visible');
                } else {
                    stickyBar.classList.remove('is-visible');
                }
            };

            window.addEventListener('scroll', checkStickyBar, { passive: true });
            checkStickyBar();
        }

        // E. AJAX Form Submission for Main and Sticky Product Forms
        [mainForm, document.getElementById('stickyAddToCartForm')].filter(Boolean).forEach(form => {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();

                const submitBtn = form.querySelector('button[type="submit"]');
                const origHtml = submitBtn ? submitBtn.innerHTML : '';
                if (submitBtn) {
                    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i> Adding...';
                    submitBtn.disabled = true;
                }

                try {
                    const formData = new FormData(form);
                    const body = new URLSearchParams(formData).toString();
                    const token = getCsrfToken();

                    const response = await fetch(form.action || '/Cart/AddToCart', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded',
                            'X-Requested-With': 'XMLHttpRequest',
                            'Accept': 'application/json',
                            'RequestVerificationToken': token
                        },
                        body: body
                    });

                    if (!response.ok) {
                        window.showToast(`Server error (${response.status}). Could not add item.`, 'error');
                        return;
                    }

                    const data = await response.json();
                    if (data.success) {
                        window.showToast(data.message || 'Item added to your shopping cart!', 'success');
                        if (data.warning) {
                            window.showToast(data.warning, 'warning');
                        }
                        if (typeof data.itemCount === 'number') {
                            updateCartBadges(data.itemCount);
                        }
                    } else {
                        window.showToast(data.message || 'Could not add item to cart.', 'error');
                    }
                } catch (err) {
                    console.error('AddToCart error:', err);
                    window.showToast('Network error. Unable to add item to cart.', 'error');
                } finally {
                    if (submitBtn) {
                        submitBtn.innerHTML = origHtml;
                        submitBtn.disabled = false;
                    }
                }
            });
        });
    }

    // =========================================================================
    // 7. CONTACT SUPPORT FORM (AJAX + Honest Error Handling)
    // =========================================================================
    function initContactForm() {
        const form = document.getElementById('contactSupportForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const submitBtn = document.getElementById('contactSubmitBtn');
            const origHtml = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1.5"></i>Sending...';
                submitBtn.disabled = true;
            }

            try {
                const formData = new FormData(form);
                const token = getCsrfToken();

                const response = await fetch(form.action || '/Home/SubmitContact', {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: formData
                });

                if (!response.ok) {
                    window.showToast(`Server error (${response.status}). Failed to send message.`, 'error');
                    return;
                }

                const data = await response.json();
                if (data.success) {
                    window.showToast(data.message || 'Thank you! Your message has been sent.', 'success');
                    form.reset();
                } else {
                    window.showToast(data.message || 'Please verify all required fields.', 'error');
                }
            } catch (err) {
                console.error('Contact submit error:', err);
                window.showToast('Network error. Please try again or call us directly.', 'error');
            } finally {
                if (submitBtn) {
                    submitBtn.innerHTML = origHtml;
                    submitBtn.disabled = false;
                }
            }
        });
    }

    // =========================================================================
    // 8. SEARCH AUTOCOMPLETE (Debounced + Keyboard Navigation)
    // =========================================================================
    function initSearchAutocomplete() {
        const pairs = [
            {
                input: document.getElementById('headerSearchInput'),
                dropdown: document.getElementById('headerSearchResults')
            },
            {
                input: document.getElementById('mobileSearchInput'),
                dropdown: document.getElementById('mobileSearchResults')
            }
        ];

        pairs.forEach(({ input, dropdown }) => {
            if (!input || !dropdown) return;

            let debounceTimer = null;
            let activeIndex = -1;
            let currentResults = [];

            function closeDropdown() {
                dropdown.style.display = 'none';
                dropdown.innerHTML = '';
                activeIndex = -1;
                currentResults = [];
                input.setAttribute('aria-expanded', 'false');
            }

            function highlightItem(index) {
                const items = dropdown.querySelectorAll('.search-suggest-item');
                items.forEach((item, idx) => {
                    if (idx === index) {
                        item.classList.add('active');
                        item.setAttribute('aria-selected', 'true');
                        item.scrollIntoView({ block: 'nearest' });
                    } else {
                        item.classList.remove('active');
                        item.removeAttribute('aria-selected');
                    }
                });
            }

            async function performSearch(query) {
                if (!query || query.length < 2) {
                    closeDropdown();
                    return;
                }

                dropdown.style.display = 'block';
                dropdown.innerHTML = `
                    <div class="p-3 text-center small text-muted">
                        <i class="fa-solid fa-spinner fa-spin me-2"></i>Searching catalog...
                    </div>`;

                try {
                    const res = await fetch(`/Shop/SearchApi?term=${encodeURIComponent(query)}`);
                    if (!res.ok) {
                        dropdown.innerHTML = `<div class="p-3 text-center small text-danger">Search error. Please press Enter to browse.</div>`;
                        return;
                    }

                    const data = await res.json();
                    currentResults = Array.isArray(data) ? data : [];
                    activeIndex = -1;

                    if (currentResults.length === 0) {
                        dropdown.innerHTML = `
                            <div class="p-3 text-center small" style="color:var(--text-muted);">
                                No matching products found for "<strong>${escapeHtml(query)}</strong>"
                            </div>
                            <div class="search-suggest-footer">
                                <a href="/Shop?search=${encodeURIComponent(query)}" class="small fw-bold">
                                    Search all catalog for "${escapeHtml(query)}" &rarr;
                                </a>
                            </div>`;
                        return;
                    }

                    let html = currentResults.map(p => `
                        <a href="/Shop/Details/${encodeURIComponent(p.id)}" class="search-suggest-item" role="option">
                            <img src="${escapeHtml(p.image || '/images/placeholder.svg')}" alt="${escapeHtml(p.title)}" class="search-suggest-thumb" />
                            <div class="search-suggest-info">
                                <div class="search-suggest-title">${escapeHtml(p.title)}</div>
                                <div class="search-suggest-meta">
                                    <span class="search-suggest-price">${escapeHtml(p.price)}</span>
                                    ${p.category ? `<span class="badge bg-secondary-subtle text-secondary small ms-1">${escapeHtml(p.category)}</span>` : ''}
                                </div>
                            </div>
                        </a>
                    `).join('');

                    html += `
                        <div class="search-suggest-footer">
                            <a href="/Shop?search=${encodeURIComponent(query)}" class="small fw-bold">
                                View all results for "<strong>${escapeHtml(query)}</strong>" &rarr;
                            </a>
                        </div>`;

                    dropdown.innerHTML = html;
                    input.setAttribute('aria-expanded', 'true');
                } catch (err) {
                    console.error('SearchApi fetch error:', err);
                    dropdown.innerHTML = `<div class="p-3 text-center small text-danger">Network error fetching suggestions.</div>`;
                }
            }

            input.addEventListener('input', () => {
                clearTimeout(debounceTimer);
                const query = input.value.trim();
                if (query.length < 2) {
                    closeDropdown();
                    return;
                }
                debounceTimer = setTimeout(() => performSearch(query), 300);
            });

            input.addEventListener('keydown', (e) => {
                const items = dropdown.querySelectorAll('.search-suggest-item');
                const itemCount = items.length;

                if (e.key === 'ArrowDown') {
                    if (dropdown.style.display !== 'none' && itemCount > 0) {
                        e.preventDefault();
                        activeIndex = (activeIndex + 1) % itemCount;
                        highlightItem(activeIndex);
                    }
                } else if (e.key === 'ArrowUp') {
                    if (dropdown.style.display !== 'none' && itemCount > 0) {
                        e.preventDefault();
                        activeIndex = (activeIndex - 1 + itemCount) % itemCount;
                        highlightItem(activeIndex);
                    }
                } else if (e.key === 'Enter') {
                    if (dropdown.style.display !== 'none' && activeIndex >= 0 && items[activeIndex]) {
                        e.preventDefault();
                        items[activeIndex].click();
                    }
                } else if (e.key === 'Escape') {
                    closeDropdown();
                }
            });

            document.addEventListener('click', (e) => {
                if (!input.contains(e.target) && !dropdown.contains(e.target)) {
                    closeDropdown();
                }
            });
        });
    }

    // =========================================================================
    // 9. FLASH DEALS COUNTDOWN TIMER (Timestamp-based)
    // =========================================================================
    function initFlashTimer() {
        const container = document.getElementById('flashCountdown');
        const display = document.getElementById('flashTimerDisplay');
        if (!container || !display) return;

        const endTimeStr = container.getAttribute('data-end-time');
        if (!endTimeStr) return;

        const targetTime = new Date(endTimeStr).getTime();
        if (isNaN(targetTime)) return;

        function updateCountdown() {
            const now = Date.now();
            const diff = targetTime - now;

            if (diff <= 0) {
                display.textContent = 'Deal Ended';
                container.classList.remove('text-warning', 'border-warning');
                container.classList.add('text-danger', 'border-danger');
                return;
            }

            const totalSecs = Math.floor(diff / 1000);
            const hours = Math.floor(totalSecs / 3600);
            const minutes = Math.floor((totalSecs % 3600) / 60);
            const seconds = totalSecs % 60;

            const pad = (n) => String(n).padStart(2, '0');
            display.textContent = `${pad(hours)}h ${pad(minutes)}m ${pad(seconds)}s`;
        }

        updateCountdown();
        setInterval(updateCountdown, 1000);
    }

    // =========================================================================
    // 10. SCROLL REVEAL & STICKY NAVBAR
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

        const backToTop = document.getElementById('backToTop');
        if (backToTop) {
            window.addEventListener('scroll', () => {
                if (window.scrollY > 300) {
                    backToTop.classList.add('is-visible');
                } else {
                    backToTop.classList.remove('is-visible');
                }
            }, { passive: true });

            backToTop.addEventListener('click', () => {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            });
        }
    }

    // =========================================================================
    // 11. MOBILE OFF-CANVAS MENU
    // =========================================================================
    function initMobileOffcanvas() {
        const offcanvas = document.getElementById('mobileOffcanvas');
        const toggleBtn = document.getElementById('mobileMenuToggle');
        const closeBtn  = document.getElementById('mobileMenuClose');
        const overlay   = document.getElementById('mobileOverlay');

        if (!offcanvas) return;

        function openMenu() {
            offcanvas.style.display = 'block';
            offcanvas.offsetHeight;
            offcanvas.classList.add('is-open');
            toggleBtn && toggleBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
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

        toggleBtn && toggleBtn.addEventListener('click', openMenu);
        closeBtn  && closeBtn.addEventListener('click', closeMenu);
        overlay   && overlay.addEventListener('click', closeMenu);

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && offcanvas.classList.contains('is-open')) {
                closeMenu();
            }
        });

        offcanvas.querySelectorAll('a[href], button[type="submit"]').forEach(link => {
            link.addEventListener('click', () => {
                setTimeout(closeMenu, 80);
            });
        });
    }

    // =========================================================================
    // 12. KEYBOARD SHORTCUTS, SEARCH & ACCESSIBILITY
    // =========================================================================
    function initShortcuts() {
        document.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.getElementById('headerSearchInput') ||
                                    document.querySelector('.header-search-input');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            }
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
        initProductDetails();
        initContactForm();
        initSearchAutocomplete();
        initFlashTimer();
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
