// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll('[data-carousel-direction="previous"]').forEach((button) => {
	const carousel = document.querySelector(button.dataset.carouselTarget);
	if (!carousel) return;

	const controls = document.querySelectorAll(`[data-carousel-target="${button.dataset.carouselTarget}"]`);
	const updateControls = () => {
		const maxScroll = carousel.scrollWidth - carousel.clientWidth;
		controls.forEach((control) => {
			control.disabled = control.dataset.carouselDirection === "previous"
				? carousel.scrollLeft <= 0
				: carousel.scrollLeft >= maxScroll - 1;
		});
	};

	controls.forEach((control) => {
		control.addEventListener("click", () => {
			const direction = control.dataset.carouselDirection === "next" ? 1 : -1;
			carousel.scrollBy({
				left: direction * carousel.clientWidth * 0.8,
				behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth"
			});
		});
	});

	carousel.addEventListener("scroll", updateControls, { passive: true });
	window.addEventListener("resize", updateControls);
	updateControls();
});
