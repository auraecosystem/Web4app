import * as React from "https://esm.unpkg.com/react@18.3.1";
import { createRoot } from "https://esm.unpkg.com/react-dom@18.3.1/client";

/**
 * =========================
 * Web4 Micro Kernel
 * =========================
 */

const routes = {};
const listeners = new Set();
let currentRoute = location.hash || "#/";

/* -------------------------
   Router
--------------------------*/
function registerRoute(path, component) {
  routes[path] = component;
}

function navigate(path) {
  location.hash = path;
}

window.addEventListener("hashchange", () => {
  currentRoute = location.hash;
  render();
});

/* -------------------------
   Simple HMR system
   (polling-based ESM reload)
--------------------------*/
const moduleCache = new Map();

async function hotImport(url) {
  const cacheBustUrl = `${url}?t=${Date.now()}`;
  const mod = await import(cacheBustUrl);
  moduleCache.set(url, mod);
  return mod;
}

async function watchModule(url, callback, interval = 2000) {
  let lastVersion = Date.now();

  setInterval(async () => {
    const newMod = await hotImport(url);
    const newVersion = Date.now();

    if (newVersion !== lastVersion) {
      lastVersion = newVersion;
      callback(newMod);
    }
  }, interval);
}

/* -------------------------
   App Renderer
--------------------------*/
const root = createRoot(document.getElementById("root"));

function render() {
  const Component = routes[currentRoute] || routes["#/404"];
  root.render(React.createElement(Component));
}

/* -------------------------
   Web4 Component Registry
--------------------------*/
registerRoute("#/", () =>
  React.createElement("div", null,
    React.createElement("h1", null, "Web4 Kernel Running ⚡"),
    React.createElement("button", {
      onClick: () => navigate("#/about")
    }, "Go About")
  )
);

registerRoute("#/about", () =>
  React.createElement("div", null,
    React.createElement("h1", null, "About Route"),
    React.createElement("button", {
      onClick: () => navigate("#/")
    }, "Back Home")
  )
);

registerRoute("#/404", () =>
  React.createElement("h1", null, "Route not found")
);

/* -------------------------
   Boot
--------------------------*/
render();
