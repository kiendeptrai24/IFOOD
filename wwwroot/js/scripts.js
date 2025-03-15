// /*!
// * Start Bootstrap - Creative v7.0.7 (https://startbootstrap.com/theme/creative)
// * Copyright 2013-2023 Start Bootstrap
// * Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-creative/blob/master/LICENSE)
// */
// //
// // Scripts
// // 

// window.addEventListener('DOMContentLoaded', event => {

//     // Navbar shrink function
//     var navbarShrink = function () {
//         const navbarCollapsible = document.body.querySelector('#mainNav');
//         if (!navbarCollapsible) {
//             return;
//         }
//         if (window.scrollY === 0) {
//             navbarCollapsible.classList.remove('navbar-shrink')
//         } else {
//             navbarCollapsible.classList.add('navbar-shrink')
//         }

//     };

//     // Shrink the navbar 
//     navbarShrink();

//     // Shrink the navbar when page is scrolled
//     document.addEventListener('scroll', navbarShrink);

//     // Activate Bootstrap scrollspy on the main nav element
//     const mainNav = document.body.querySelector('#mainNav');
//     if (mainNav) {
//         new bootstrap.ScrollSpy(document.body, {
//             target: '#mainNav',
//             rootMargin: '0px 0px -40%',
//         });
//     };

//     // Collapse responsive navbar when toggler is visible
//     const navbarToggler = document.body.querySelector('.navbar-toggler');
//     const responsiveNavItems = [].slice.call(
//         document.querySelectorAll('#navbarResponsive .nav-link')
//     );
//     responsiveNavItems.map(function (responsiveNavItem) {
//         responsiveNavItem.addEventListener('click', () => {
//             if (window.getComputedStyle(navbarToggler).display !== 'none') {
//                 navbarToggler.click();
//             }
//         });
//     });

//     // Activate SimpleLightbox plugin for portfolio items
//     new SimpleLightbox({
//         elements: '#portfolio a.portfolio-box'
//     });

// });
// window.addEventListener('DOMContentLoaded', event => {

//     // Navbar shrink function
//     var navbarShrink = function () {
//         const navbarCollapsible = document.body.querySelector('#mainNav');
//         const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

//         if (!navbarCollapsible) {
//             return;
//         }

//         // Kiểm tra nếu đang ở trang Home/Index
//         var isHomePage = window.location.pathname === "/" || window.location.pathname.toLowerCase() === "/home/index";

//         if (window.scrollY === 0) {
//             navbarCollapsible.classList.remove('navbar-shrink');

//             // Nếu ở trang Home, đổi màu chữ thành trắng khi ở trên cùng
//             if (isHomePage) {
//                 navLinks.forEach(link => link.style.color = "#ffffff"); // Chữ trắng
//             }
//         } else {
//             navbarCollapsible.classList.add('navbar-shrink');

//             // Khi cuộn xuống, chữ navbar luôn đen
//             navLinks.forEach(link => link.style.color = "#000000");
//         }
//     };

//     // Shrink the navbar
//     navbarShrink();

//     // Shrink the navbar when page is scrolled
//     document.addEventListener('scroll', navbarShrink);

//     // Activate Bootstrap scrollspy on the main nav element
//     const mainNav = document.body.querySelector('#mainNav');
//     if (mainNav) {
//         new bootstrap.ScrollSpy(document.body, {
//             target: '#mainNav',
//             rootMargin: '0px 0px -40%',
//         });
//     }

//     // Collapse responsive navbar when toggler is visible
//     const navbarToggler = document.body.querySelector('.navbar-toggler');
//     const responsiveNavItems = [].slice.call(
//         document.querySelectorAll('#navbarResponsive .nav-link')
//     );
//     responsiveNavItems.map(function (responsiveNavItem) {
//         responsiveNavItem.addEventListener('click', () => {
//             if (window.getComputedStyle(navbarToggler).display !== 'none') {
//                 navbarToggler.click();
//             }
//         });
//     });

//     // Activate SimpleLightbox plugin for portfolio items
//     new SimpleLightbox({
//         elements: '#portfolio a.portfolio-box'
//     });

// });


// // window.addEventListener('DOMContentLoaded', event => {

// //     // Lấy phần tử navbar và danh sách các liên kết trong navbar
// //     const navbarCollapsible = document.body.querySelector('#mainNav');
// //     const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

// //     if (!navbarCollapsible) {
// //         return;
// //     }

// //     // Kiểm tra xem có đang ở Home/Index không
// //     var isHomePage = window.location.pathname === "/" || window.location.pathname.toLowerCase() === "/home/index";

// //     // Hàm thay đổi màu chữ navbar
// //     var updateNavbarTextColor = function () {
// //         if (window.scrollY === 0 && isHomePage) {
// //             navbarCollapsible.classList.remove('navbar-shrink');
// //             navLinks.forEach(link => link.style.color = "#ffffff"); // Chữ trắng khi ở trên cùng trang Home
// //         } else {
// //             navbarCollapsible.classList.add('navbar-shrink');
// //             navLinks.forEach(link => link.style.color = "#000000"); // Chữ đen khi cuộn xuống hoặc ở trang khác
// //         }
// //     };

// //     // Nếu không phải trang Home, đổi chữ đen ngay lập tức
// //     if (!isHomePage) {
// //         navbarCollapsible.classList.add('navbar-shrink');
// //         navLinks.forEach(link => link.style.color = "#000000"); // Chữ đen ngay từ đầu
// //     }

// //     // Áp dụng sự kiện cuộn trang
// //     document.addEventListener('scroll', updateNavbarTextColor);

// //     // Gọi hàm ngay khi tải trang
// //     updateNavbarTextColor();

// // });

document.getElementById("menuToggle").addEventListener("click", function () {
    let menu = document.getElementById("navbarResponsive");
    if (menu.classList.contains("show")) {
        menu.classList.remove("show"); // Ẩn menu
    } else {
        menu.classList.add("show"); // Hiện menu
    }
});

