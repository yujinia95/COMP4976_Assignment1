/**
 * Memorial Details Script
 * Handles card reveal and biography toggle.
 */

(function () {

  // Fade-in cards
  function revealCards() {
    const cards = document.querySelectorAll(".card");
    setTimeout(() => cards.forEach((c) => c.classList.add("appear")), 100);
  }

  // Biography "Read more" with fade mask
  function initBioToggle() {
    const container = document.getElementById("bioContainer");
    const toggle = document.getElementById("toggleBio");
    const fade = document.getElementById("bioFade");
    if (!container || !toggle || !fade) return;

    // Determine if content exceeds the visual limit
    const needsToggle = container.scrollHeight - container.clientHeight > 12;
    if (needsToggle) {
      toggle.classList.remove("d-none");
      fade.classList.remove("d-none");
    }

    let expanded = false;
    toggle.addEventListener("click", () => {
      expanded = !expanded;
      container.classList.toggle("biography-content--expanded", expanded);

      // When expanded, allow natural height and hide fade
      fade.classList.toggle("d-none", expanded);
      toggle.setAttribute("aria-expanded", expanded ? "true" : "false");
      toggle.textContent = expanded ? "Show less" : "Read more";

      // If collapsed, scroll back to biography top for context
      if (!expanded)
        container.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    revealCards();
    initBioToggle();
    initUtilityButtons();
  });
})();
