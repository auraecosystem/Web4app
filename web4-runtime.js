export class Web4Runtime {
  constructor() {
    this.state = {};
    this.modules = {};
    this.listeners = {};
  }

  // =========================
  // 🧠 STATE ENGINE
  // =========================
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
    return path.split(".").reduce((a, k) => a?.[k], this.state);
  }

  // =========================
  // ⚡ EVENT SYSTEM
  // =========================
  emit(event, data) {
    (this.listeners[event] || []).forEach(fn => fn(data));
  }

  on(event, fn) {
    this.listeners[event] = this.listeners[event] || [];
    this.listeners[event].push(fn);
  }

  // =========================
  // 🔌 MODULE SYSTEM
  // =========================
  register(name, module) {
    this.modules[name] = module;
    module.init?.(this);
  }

  run(name, input) {
    return this.modules[name]?.execute?.(input, this);
  }

  // =========================
  // 🤖 AI CORE
  // =========================
  async ai(prompt) {
    return `AI → ${prompt}`;
  }
}
