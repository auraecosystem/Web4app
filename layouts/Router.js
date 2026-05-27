const routes = {
  "/": "Home",
  "/dashboard": "Dashboard"
};

export function navigate(path) {
  window.history.pushState({}, "", path);
  window.dispatchEvent(new Event("web4:route"));
}

export function getRoute() {
  return window.location.pathname;
}
