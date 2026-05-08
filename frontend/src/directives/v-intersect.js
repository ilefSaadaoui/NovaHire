export const vIntersect = {
    mounted(el, binding) {
        el.classList.add('before-enter');

        const observer = new IntersectionObserver(
            (entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        el.classList.add('enter');
                        // Check if binding value is true to trigger only once
                        if (binding.value === undefined || binding.value === true) {
                            observer.unobserve(el);
                        }
                    } else {
                        // Un-observe logic if we want to reverse animations
                        if (binding.value === false) {
                            el.classList.remove('enter');
                        }
                    }
                });
            },
            {
                rootMargin: '0px 0px -100px 0px',
                threshold: 0.1
            }
        );

        observer.observe(el);
    }
};
