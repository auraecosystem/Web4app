export const state = {
  app: { name: "Web4 Astro Framework v3" },

  user: {
    name: "Guest",
    status: "active"
  },

  data: {
    metricA: 42,
    metricB: 99
  },

  ai: {
    output: "Initializing..."
  },

  events: []
};

export function updateState(path, value) {
  const keys = path.split(".");
  let obj = state;

  for (let i = 0; i < keys.length - 1; i++) {
    obj = obj[keys[i]];
  }

  obj[keys[keys.length - 1]] = value;

  window.dispatchEvent(new Event("web4:update"));
}
