/**
 * Memorial Edit Script
 * Handles form validation, photo upload, and character counting.
 */
(function () {
  const $ = (sel, root = document) => root.querySelector(sel);

  // Unsaved changes guard
  let dirty = false;
  function markDirty() {
    dirty = true;
  }

  // Warn user if they try to leave with unsaved changes
  function initDirtyGuard() {
    const form = $("#editForm");
    if (!form) return;
    form.addEventListener("input", markDirty, true);
    form.addEventListener("change", markDirty, true);
    window.addEventListener("beforeunload", function (e) {
      if (!dirty) return;
      e.preventDefault();
      e.returnValue = "";
    });
    form.addEventListener("submit", () => {
      dirty = false;
    });
  }

  // Date validation
  function parseDate(input) {

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
    ["change", "input"].forEach((ev) => {
      dob.addEventListener(ev, validate);
      dod.addEventListener(ev, validate);
    });
  }

  // Biography counter
  function initBioCounter() {

    const bio = $("#bioInput");
    const counter = $("#bioCounter");
    if (!bio || !counter) return;
    const max = Number(bio.getAttribute("maxlength")) || 5000;
    const update = () => {

      const len = (bio.value || "").length;
      counter.textContent = `${len} / ${max}`;

    };
    bio.addEventListener("input", update);
    update();

  }

  // Photo upload
  function initPhotoUpload() {
    const drop = $("#photoDrop");
    const input = $("#photoInput");
    const trigger = $("#triggerUpload");
    const clearBtn = $("#clearPhoto");
    const info = $("#photoInfo");
    const removeExisting = $("#removePhoto");
    if (!drop || !input) return;

    const MAX_MB = 5;
    const MAX_BYTES = MAX_MB * 1024 * 1024;
    const ACCEPTED = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    // Reset file input and
    function resetSelection() {
      input.value = "";
      if (info) info.textContent = "";
      if (clearBtn) clearBtn.classList.add("d-none");
    }

    // Validate file type and size
    function validateFile(file) {
      if (!file) return false;
      if (!ACCEPTED.includes(file.type)) {
        alert("Please upload a valid image (JPEG, PNG, WEBP, or GIF).");
        return false;
      }
      if (file.size > MAX_BYTES) {
        alert(`Image is too large. Maximum size is ${MAX_MB} MB.`);
        return false;
      }
      return true;
    }

    // Handle file selection
    function handleFiles(list) {
      if (!list || list.length === 0) return;
      const file = list[0];
      if (!validateFile(file)) {
        resetSelection();
        return;
      }
      previewFile(file);
      setInfo(file);
      if (clearBtn) clearBtn.classList.remove("d-none");
      if (removeExisting) removeExisting.checked = false; // if user selects a new file, uncheck remove
      markDirty();
    }

    // When user checks "remove existing photo", clear any selected file and show info
    if (removeExisting) {
      removeExisting.addEventListener('change', function () {
        if (removeExisting.checked) {
          resetSelection();
          if (info) info.textContent = 'Current photo will be removed when you save.';
        }
        else {
          if (info) info.textContent = '';
        }
        markDirty();
      });
    }

    if (trigger) trigger.addEventListener("click", () => input.click());
    if (clearBtn) clearBtn.addEventListener("click", () => resetSelection());
    input.addEventListener("change", (e) => handleFiles(e.target.files));

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

    drop.addEventListener("keydown", (e) => {
      if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        input.click();
      }
    });
  }

  // Bootstrap validation 
  function initBootstrapValidation() {

    const forms = document.getElementsByClassName("needs-validation");
    Array.prototype.forEach.call(forms, function (form) {
      form.addEventListener("submit", function (event) {

        // busy state
        const btn = form.querySelector("#saveBtn");
        if (btn) {
          btn.querySelector(".default")?.classList.add("d-none");
          btn.querySelector(".busy")?.classList.remove("d-none");
        }
        if (form.checkValidity() === false) {
          event.preventDefault();
          event.stopPropagation();
          const summary =
            form.querySelector('[asp-validation-summary="ModelOnly"]') ||
            form.querySelector('.alert[role="alert"]');
          if (summary) summary.classList.remove("d-none");

          // reset busy state
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
    initDirtyGuard();
    initDateChecks();
    initBioCounter();
    initPhotoUpload();
    initBootstrapValidation();
  });
})();
