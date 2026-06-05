/**
 * Lightweight Web4 Reactive Template Parser Engine
 */
export class Web4Compiler {
  constructor(state = {}) {
    this.state = state;
    this.eventListeners = {};
  }

  // Safely read properties from nested objects like 'user.status'
  resolveStatePath(path, stateObj) {
    return path.split('.').reduce((obj, key) => (obj && obj[key] !== undefined) ? obj[key] : '', stateObj);
  }

  // Core compiling pipeline
  compile(templateString) {
    let output = templateString;

    // 1. Process Conditionals: {% if condition %} ... {% else %} ... {% endif %}
    const ifRegex = /\{%\s*if\s+([^%]+)\s*%\}([\s\S]*?)(?:\{%\s*else\s*%\}([\s\S]*?))?\{%\s*endif\s*%\}/g;
    output = output.replace(ifRegex, (match, conditionExpr, trueBody, falseBody) => {
      const parts = conditionExpr.trim().split(/\s*==\s*/);
      const val1 = this.resolveStatePath(parts[0], this.state);
      const val2 = parts[1] ? parts[1].replace(/['"]/g, '') : true;
      
      const isTrue = String(val1) === String(val2);
      return isTrue ? trueBody : (falseBody || '');
    });

    // 2. Parse Reactive Event System: {% on event="name" %} ... {% endon %}
    const eventRegex = /\{%\s*on\s+event="([^"]+)"\s*%\}([\s\S]*?)\{%\s*endon\s*%\}/g;
    output = output.replace(eventRegex, (match, eventName, innerTemplate) => {
      // Return a structural placeholder that handles reactive mutations down the line
      return `<div data-web4-event="${eventName}" class="web4-event-container">${innerTemplate}</div>`;
    });

    // 3. Evaluate Data-Binding: {{ object.property }}
    const interpolationRegex = /\{\{\s*([^}]+)\s*\}\}/g;
    output = output.replace(interpolationRegex, (match, expression) => {
      const resolvedValue = this.resolveStatePath(expression.trim(), this.state);
      return resolvedValue !== undefined ? resolvedValue : '';
    });

    return output;
  }
}
