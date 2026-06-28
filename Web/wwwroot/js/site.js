window.votify = {
    scrollToElement: function (selector) {
        try {
            var el = document.querySelector(selector);
            if (el) {
                el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
        catch (e) {
            console && console.error && console.error(e);
        }
    }
};
