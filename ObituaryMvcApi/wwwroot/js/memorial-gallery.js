/**
 * Memorial Gallery Script
 * Handles search functionality and highlighting.
 */

(function () {

  // Helpers: escape special characters for RegExp
  function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  }

  // If highlight the searched word inside obituary cards
  function highlightSearchTerms(term) {
    if (!term || !term.trim()) return;
    const cards = document.querySelectorAll(
      ".memorial-card .memorial-card__title"
    );
    const regex = new RegExp("(" + escapeRegExp(term.trim()) + ")", "gi");

    // Highlight matches
    cards.forEach((card) => {
      const text = card.textContent;
      if (text.toLowerCase().includes(term.toLowerCase())) {
        card.innerHTML = text.replace(
          regex,
          '<mark class="bg-warning">$1</mark>'
        );
      }
    });
  }

  window.clearSearchAndReload = function () {
    
    // Back to the Obituaries index without query
    const url =
      document.querySelector("form.memorial-search")?.getAttribute("action") ||
      window.location.pathname;
    const base = url.split("?")[0];
    window.location.href = base;
  };

  document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form.memorial-search");
    const input = document.getElementById("searchInput");
    const clearBtn = document.getElementById("clearSearch");

    // Switch button state on submit
    if (form) {
      form.addEventListener("submit", function () {
        const submit = form.querySelector(".memorial-search__submit");
        if (submit) {
          submit.querySelector(".default")?.classList.add("d-none");
          submit.querySelector(".busy")?.classList.remove("d-none");
          submit.disabled = true;
        }
      });
    }

    // Clear search by button
    if (clearBtn) {
      clearBtn.addEventListener("click", clearSearchAndReload);
    }

    // ESC to clear
    if (input) {
      input.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
          if (this.value) {
            this.value = "";
            this.focus();
          } else {
            clearSearchAndReload();
          }
        }
      });

      // Autofocus when search active
      if (
        (typeof IsSearchActive !== "undefined" && IsSearchActive === true) ||
        document.body.innerHTML.includes("Found") ||
        document.body.innerHTML.includes("No memorials found")
      ) {
        input.focus();

        const len = input.value.length;
        input.setSelectionRange(len, len);
      }
    }

    // Highlight term
    const serverTerm =
      document.querySelector('form.memorial-search input[name="searchTerm"]')
        ?.value || "";
    highlightSearchTerms(serverTerm);
  });
})();
