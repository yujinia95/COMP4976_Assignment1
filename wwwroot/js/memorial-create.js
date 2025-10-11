/**
 * Memorial Create Script
 * Handles form validation, photo upload, and character counting.
 */
(function () {
  const $ = (sel, root = document) => root.querySelector(sel);

  // Validation helpers
  function parseDate(input) {
    // yyyy-mm-dd -> Date (local)
    if (!input || !/^\d{4}-\d{2}-\d{2}$/.test(input)) return null;
    const [y, m, d] = input.split("-").map(Number);
    return new Date(y, m - 1, d, 12);
  }

  // Check if date is in the future (compared to today)
  function isFuture(date) {
    if (!date) return false;
    const today = new Date();
    today.setHours(23, 59, 59, 999);
    return date.getTime() > today.getTime();
  }

  // Biography counter
  function initBioCounter() {
    const bio = $("#bioInput");
    const counter = $("#bioCounter");
    if (!bio || !counter) return;

    const max = Number(bio.getAttribute("maxlength")) || 5000;

    const update = () => {
      let val = bio.value || "";
      if (val.length > max) {
        bio.value = val.slice(0, max);
        val = bio.value;
      }
      counter.textContent = `${val.length} / ${max}`;
    };

    bio.addEventListener("input", update);
    update();
  }

  // Dates interdependency checks
  function initDateChecks() {
    const dob = $("#dobInput");
    const dod = $("#dodInput");
    if (!dob || !dod) return;

    const validate = () => {
      const dobDate = parseDate(dob.value);
      const dodDate = parseDate(dod.value);

      dob.setCustomValidity("");
      dod.setCustomValidity("");

      if (dobDate && isFuture(dobDate)) {
        dob.setCustomValidity("Date of birth cannot be in the future.");
      }
      if (dodDate && isFuture(dodDate)) {
        dod.setCustomValidity("Date of death cannot be in the future.");
      }
      if (dobDate && dodDate && dodDate.getTime() < dobDate.getTime()) {
        dod.setCustomValidity(
          "Date of death must be on or after date of birth."
        );
      }
    };

    dob.addEventListener("change", validate);
    dod.addEventListener("change", validate);
    dob.addEventListener("input", validate);
    dod.addEventListener("input", validate);
  }

  // Click upload + drag & drop + basic size/type checks
  function initPhotoUpload() {
    const drop = $("#photoDrop");
    const input = $("#photoInput");
    const trigger = $("#triggerUpload");
    const clearBtn = $("#clearPhoto");
    const info = $("#photoInfo");
    if (!drop || !input) return;

    const MAX_MB = 5;
    const MAX_BYTES = MAX_MB * 1024 * 1024;
    const ACCEPTED = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    // Show file info
    function setInfo(file) {
      if (!info) return;
      const sizeMB = (file.size / (1024 * 1024)).toFixed(2);
      info.textContent = `${file.name} — ${sizeMB} MB`;
    }

    // Basic validation for photo
    function validateFile(file) {
      if (!file) return false;
      if (!ACCEPTED.includes(file.type)) {
        alert("Please upload a valid image file (JPEG, PNG, WEBP, or GIF).");
        return false;
      }
      if (file.size > MAX_BYTES) {
        alert(`Image is too large. Maximum size is ${MAX_MB} MB.`);
        return false;
      }
      return true;
    }

    // Handle file selection
    function handleFiles(fileList) {
      if (!fileList || fileList.length === 0) return;
      const file = fileList[0];
      if (!validateFile(file)) {
        input.value = "";
        return;
      }
      setInfo(file);
      if (clearBtn) clearBtn.classList.remove("d-none");
    }

    // Trigger button
    if (trigger) {
      trigger.addEventListener("click", () => input.click());
    }

    // Clear button
    if (clearBtn) {
      clearBtn.addEventListener("click", () => {
        input.value = "";
      });
    }

    // Input change
    input.addEventListener("change", (e) => handleFiles(e.target.files));

    // Dropzone interactions
    ["dragenter", "dragover"].forEach((ev) =>
      drop.addEventListener(ev, (e) => {
        e.preventDefault();
        e.stopPropagation();
        drop.classList.add("dragover");
      })
    );
    ["dragleave", "drop"].forEach((ev) =>
      drop.addEventListener(ev, (e) => {
        e.preventDefault();
        e.stopPropagation();
        drop.classList.remove("dragover");
      })
    );
    drop.addEventListener("drop", (e) => handleFiles(e.dataTransfer.files));

    // Keyboard “upload”
    drop.addEventListener("keydown", (e) => {
      if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        input.click();
      }
    });
  }

  // Bootstrap validation integration
  function initBootstrapValidation() {
    const forms = document.getElementsByClassName("needs-validation");

    Array.prototype.forEach.call(forms, function (form) {
      form.addEventListener(
        "submit",
        function (event) {
          const btn = form.querySelector('button[type="submit"]');

          // Show busy state on submit button
          if (btn) {
            btn.querySelector(".default")?.classList.add("d-none");
            btn.querySelector(".busy")?.classList.remove("d-none");
          }

          if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();

            // Reveal validation summary
            const summary =
              form.querySelector('[asp-validation-summary="ModelOnly"]') ||
              form.querySelector('.alert[role="alert"]');
            if (summary) summary.classList.remove("d-none");

            // Reset button state when invalid
            if (btn) {
              btn.querySelector(".default")?.classList.remove("d-none");
              btn.querySelector(".busy")?.classList.add("d-none");
            }
          }
          form.classList.add("was-validated");
        },
        false
      );
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initBioCounter();
    initDateChecks();
    initPhotoUpload();
    initBootstrapValidation();
  });
})();
