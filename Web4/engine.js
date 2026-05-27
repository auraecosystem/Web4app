export function resolve(expr, ctx) {
  const parts = expr.split(".");
  let value = ctx[parts[0]];

  for (let i = 1; i < parts.length; i++) {
    value = value?.[parts[i]];
  }

  return typeof value === "function" ? value() : value;
}

export function render(template, ctx) {
  return template.replace(/\{\{\s*(.*?)\s*\}\}/g, (_, expr) => {
    return resolve(expr.trim(), ctx);
  });
}
