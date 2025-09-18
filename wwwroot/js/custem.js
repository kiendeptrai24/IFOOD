  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    function scrollCarousel(direction) {
        const carousel = document.getElementById('bestsellerCarousel');
        const cardWidth = carousel.querySelector('.bestseller-card').offsetWidth;
        const scrollAmount = cardWidth + 30; // card width + gap
        
        carousel.scrollBy({
            left: direction * scrollAmount * 2,
            behavior: 'smooth'
        });
    }
    function addToCart(productId) {
        // Implement add to cart logic here
        console.log('Adding product to cart:', productId);
        
        // You can add a toast notification here
        showToast('Product added to cart!');
    }

    function showToast(message) {
        // Create toast element if it doesn't exist
        let toast = document.getElementById('toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'toast';
            toast.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: linear-gradient(45deg, #ff6b6b, #ff8e53);
                color: white;
                padding: 15px 20px;
                border-radius: 10px;
                box-shadow: 0 5px 15px rgba(0,0,0,0.2);
                z-index: 10000;
                transform: translateX(100%);
                transition: transform 0.3s ease;
            `;
            document.body.appendChild(toast);
        }
        
        toast.textContent = message;
        toast.style.transform = 'translateX(0)';
        
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
        }, 3000);
    }

    function BuyItems(button) {
        let productId = button.getAttribute("data-id");

        Swal.fire({
            title: "Choose payment method",
            text: "Please select the payment method you would like to use.",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "COD",
            cancelButtonText: "MoMo",
            showDenyButton: true,
            denyButtonText: "ZaloPay",
            html: `
                <button id="other-payment" class="swal2-confirm swal2-styled" style="margin-top: 10px;">
                    Other E-wallets
                </button>
            `,
            didOpen: () => {
                document.getElementById("other-payment").addEventListener("click", function () {
                    processOrder(productId, "Other");
                    Swal.close();
                });
            }
        }).then((result) => {
            if (result.isConfirmed) {
                processOrder(productId, 2); // COD
            } else if (result.dismiss === Swal.DismissReason.cancel) {
                processOrder(productId, 0); // MoMo
            } else if (result.isDenied) {
                processOrder(productId, 1); // ZaloPay
            }
        });
    }

    function processOrder(productId, paymentMethod) {
        console.log("Placing order with ProductID:", productId, "Payment method:", paymentMethod);

        fetch('/Order/CreateOrder', {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ productId, paymentMethod })
        })
        .then(response => response.json())
        .then(data => {
            if (data.payUrl) {
                window.location.href = data.payUrl;
            }
        });
    }

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    document.addEventListener('DOMContentLoaded', function() {
        const productCards = document.querySelectorAll('.product-card');
        productCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';
            card.style.transition = `opacity 0.6s ease ${index * 0.1}s, transform 0.6s ease ${index * 0.1}s`;
            observer.observe(card);
        });


        const filterBtns = document.querySelectorAll('.filter-btn');
        filterBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                filterBtns.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
            });
        });
    });