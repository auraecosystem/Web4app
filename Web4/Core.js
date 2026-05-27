export class Web4 {
  constructor() {
    this.state = {};
    this.events = [];
    this.plugins = [];
  }

  // ======================
  // 🧠 STATE SYSTEM
  // ======================
  set(path, value) {
    const keys = path.split(".");
    let obj = this.state;

    for (let i = 0; i < keys.length - 1; i++) {
      obj[keys[i]] = obj[keys[i]] || {};
      obj = obj[keys[i]];
    }

    obj[keys[keys.length - 1]] = value;
    this.emit("state:update", { path, value });
  }

  get(path) {
    return path.split(".").reduce((acc, k) => acc?.[k], this.state);
  }

  // ======================
  // ⚡ EVENT SYSTEM
  // ======================
  emit(type, payload) {
    const event = { type, payload, time: Date.now() };
    this.events.push(event);
    window.dispatchEvent(new CustomEvent(type, { detail: payload }));
  }

  on(type, handler) {
    window.addEventListener(type, (e) => handler(e.detail));
  }

  // ======================
  // 🤖 AI LAYER (hook)
  // ======================
  async ai(prompt) {
    return `AI: ${prompt}`;
  }

  // ======================
  // 🔌 PLUGIN SYSTEM
  // ======================
  register(plugin) {
    this.plugins.push(plugin);
    plugin.init(this);
  }
}
